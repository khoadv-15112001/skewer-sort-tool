using Newtonsoft.Json;
using SonatFramework.Scripts.SonatSDKAdapterModule;
using SonatFramework.Systems;
using SonatFramework.Systems.InventoryManagement.GameResources;
using System;
using UnityEngine;
using UnityEngine.Events;
using GrillSort.RealTime;
using SonatFramework.Scripts.Helper;
using SonatFramework.Systems.InventoryManagement;
using SonatFramework.Scripts.UIModule;
using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using Sonat.Data;

namespace GrillSort.ClaimRrdService
{
    [CreateAssetMenu(fileName = "ClaimRwdService", menuName = "My Services/Claim Rwd Service")]
    public class ClaimRwdService : SonatServiceSo, IServiceInitialize, IServiceTick
    {
        private readonly string prefix = "claim_rwd_shop";

        public ClaimRwdConfig _config;

        private IntDataPref _currentLevel;
        private LongDataPref _nextTime;
        private IntDataPref _lastClaimDay;

        private readonly Service<RealTimeService> _realTimeService = new();

        public int CurrentLevel { get => _currentLevel.Value; internal set => _currentLevel.Value = value; }
        public long NextTime { get => _nextTime.Value; internal set => _nextTime.Value = value; }
        public ClaimRwdConfig Config { get => _config; }

        public void Initialize()
        {
            //Debug.Log("anhnt: " + JsonConvert.SerializeObject(_config));
            _config = SonatSDKAdapter.GetRemoteConfig<ClaimRwdConfig>($"{prefix}_config", _config);

            _currentLevel = new IntDataPref($"{prefix}_currentLevel", 1);
            _nextTime = new LongDataPref($"{prefix}_nextTime");
            _lastClaimDay = new IntDataPref($"{prefix}_lastDay", 0);

            ResetIfNewDay();

            _realTimeService.Instance.OnNextDay += ResetDaily;
        }

        public bool CanUnlock()
        {
            if (!RwdGlobalHelper.RwdGlobalEnable) return false;

            var level = MySonatFramework.userDataService.GetLevel();
            var userDay = MySonatFramework.userDataService.UserDay;
            return _config.active && level >= _config.levelStart && userDay >= _config.retentionDay;
        }

        public bool CanClaim()
        {
            if (!CanUnlock()) return false;
            long now = _realTimeService.Instance.GetCurrentTimeUnix();
            return now >= _nextTime.Value;
        }

        public bool IsFinishedToday() => _currentLevel.Value > _config.datas.Count;

        public async UniTask<(bool success, RewardData reward)> ClaimAsync()
        {
            if (!CanClaim())
                return (false, null);

            var data = _config.datas[_currentLevel.Value - 1];
            if (data == null)
            {
                Debug.LogError($"ClaimRwdService: Missing config for level {_currentLevel.Value}");
                return (false, null);
            }

            if (!SonatSDKAdapter.IsRewardAdsReady())
            {
                Debug.LogWarning("Reward Ads not ready");
                return (false, null);
            }

            // ✅ Khoá claim tạm thời ngay trước khi xem quảng cáo
            var now = _realTimeService.Instance.GetCurrentTimeUnix();
            _nextTime.Value = now + 1; // tránh spam click khi ad đang show

            bool adCompleted = false;

            // ✅ Chờ ShowRewardAds hoàn tất
            await UniTask.Create(async () =>
            {
                var tcs = new UniTaskCompletionSource();

                MySonatFramework.ShowRewardAds(() =>
                {
                    adCompleted = true;
                    tcs.TrySetResult();
                }, prefix, prefix);

                await tcs.Task;
            });

            if (!adCompleted)
                return (false, null);

            // ✅ Sau khi xem xong quảng cáo → thực hiện thưởng và cooldown
            data = _config.datas[_currentLevel.Value - 1]; // confirm lại level hiện tại
            var logData = new EarnResourceLogData
            {
                spendType = prefix,
                spendId = prefix,
                source = "non_iap"
            };
            MySonatFramework.inventoryService.AddReward(data.rewardData, logData);

            var uiData = new UIData();
            uiData.Add("Reward", data.rewardData);
            PanelManager.Instance.OpenPanel<PopupReward>(uiData);

            now = _realTimeService.Instance.GetCurrentTimeUnix();
            var endOfDay = now + _realTimeService.Instance.GetRemainingTimeInDay();

            if (_currentLevel.Value < _config.datas.Count)
            {
                var nextData = _config.datas[_currentLevel.Value]; // level kế tiếp
                var nextCooldown = nextData.cooldown;
                var next = now + nextCooldown;
                _nextTime.Value = Math.Min(next, endOfDay);

                _currentLevel.Value++;
            }
            else
            {
                _nextTime.Value = endOfDay;
                _currentLevel.Value = _config.datas.Count + 1;
            }

            _lastClaimDay.Value = _realTimeService.Instance.GetCurrentTime().DayOfYear;

            Debug.Log($"[ClaimRwdService] Claim done: level={_currentLevel.Value}, nextTime={_nextTime.Value}");
            return (true, data.rewardData);
        }

        private void ResetIfNewDay()
        {
            int today = _realTimeService.Instance.GetCurrentTime().DayOfYear;
            if (today != _lastClaimDay.Value)
            {
                ResetDaily();
            }
        }

        private void ResetDaily()
        {
            _currentLevel.Value = 1;
            _nextTime.Value = 0;
            _lastClaimDay.Value = _realTimeService.Instance.GetCurrentTime().DayOfYear;
        }
    }
}
