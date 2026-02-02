using System;
using System.Linq;
using Cysharp.Threading.Tasks;
using UnityEngine;
using SonatFramework.Systems;
using SonatFramework.Systems.InventoryManagement.GameResources;
using GrillSort.RealTime;
using SonatFramework.Scripts.Helper;
using SonatFramework.Scripts.SonatSDKAdapterModule;
using SonatFramework.Scripts.UIModule;
using SonatFramework.Systems.UserData;
using SonatFramework.Systems.InventoryManagement;
using Sonat.Data;

namespace GrillSort.Services
{
    [CreateAssetMenu(fileName = "VideoBarService", menuName = "My Services/Video Bar Service")]
    public class VideoBarService : SonatServiceSo, IServiceInitialize
    {
        public string Prefix = "video_bar";
        public VideoBarConfig _config;

        // Data
        private IntDataPref _currentCount;        // số video đã xem hôm nay
        private LongDataPref _nextResetTime;      // thời gian reset tiếp theo (unix)
        private IntDataPref _lastDay;             // lưu DayOfYear để auto reset
        private IntDataPref _tutShown;             // tut
        private bool _initialized;

        // References
        private readonly Service<RealTimeService> _realTimeService = new();
        private readonly Service<UserDataService> _userDataService = new();

        // Events cho UI
        public event Action OnProgressChanged;
        public event Action OnCooldownChanged;
        
        #region Initialize
        public void Initialize()
        {
            _config = SonatSDKAdapter.GetRemoteConfig<VideoBarConfig>($"{Prefix}_config", _config);
            _config.active = SonatSDKAdapter.GetRemoteConfig<VideoBarConfig>($"{Prefix}_config", _config);

            _currentCount = new IntDataPref($"{Prefix}_currentCount", 0);
            _nextResetTime = new LongDataPref($"{Prefix}_nextResetTime");
            _lastDay = new IntDataPref($"{Prefix}_lastDay", -1);
            _tutShown = new IntDataPref($"{Prefix}_tut_shown", 0);
            _realTimeService.Instance.OnNextDay += ResetDaily;

            TickService.Register(UpdateCooldown, TickService.TickPhase.TickRealtime);

            CheckDailyReset();
            _initialized = true;
        }
        #endregion

        #region Core Logic

        private void CheckDailyReset()
        {
            int currentDay = _realTimeService.Instance.GetCurrentTime().DayOfYear;
            if (_lastDay.Value != currentDay)
            {
                ResetDaily();
            }
        }

        private void ResetDaily()
        {
            _currentCount.Value = 0;
            _lastDay.Value = _realTimeService.Instance.GetCurrentTime().DayOfYear;
            _nextResetTime.Value = GetEndOfDayUnix();

            Debug.Log("[VideoBarService] Daily reset done.");
            OnProgressChanged?.Invoke();
            OnCooldownChanged?.Invoke();
        }

        public bool CanShow()
        {
            if (!_config.active || !RwdGlobalHelper.RwdGlobalEnable) return false;

            int level = _userDataService.Instance.GetLevel();
            int userDay = _userDataService.Instance.UserDay;

            return level >= _config.levelStart && userDay >= _config.retentionDay;
        }

        public bool HasMoreAds() => _currentCount.Value < _config.maxWatchPerDay;

        private long GetEndOfDayUnix()
        {
            long now = _realTimeService.Instance.GetCurrentTimeUnix();
            long remain = _realTimeService.Instance.GetRemainingTimeInDay();
            return now + remain;
        }

        public async UniTask<bool> WatchVideoAsync()
        {
            if (!SonatSDKAdapter.IsRewardAdsReady())
            {
                PopupToast.Cretate("No video avaiable!");
                return false;
            }

            if (!CanShow() || !HasMoreAds())
                return false;

            var tcs = new UniTaskCompletionSource();
            MySonatFramework.ShowRewardAds(() => tcs.TrySetResult(), Prefix, "video_bar");
            await tcs.Task;

            _currentCount.Value++;
            Debug.Log($"[VideoBarService] Watched {_currentCount.Value}/{_config.maxWatchPerDay} videos.");

            OnProgressChanged?.Invoke();

            TryGiveReward().Forget();

            if (!HasMoreAds())
            {
                _nextResetTime.Value = GetEndOfDayUnix();
                OnCooldownChanged?.Invoke();
            }

            return true;
        }

        private async UniTaskVoid TryGiveReward()
        {
            var rewardData = _config.datas.FirstOrDefault(d => d.watchCount == _currentCount.Value);
            if (rewardData == null) return;

            Debug.Log($"[VideoBarService] Reached milestone {rewardData.watchCount}. Granting reward.");

            var logData = new EarnResourceLogData
            {
                spendType = Prefix,
                spendId = Prefix,
                source = "non_iap"
            };
            MySonatFramework.inventoryService.AddReward(rewardData.rewards, logData);
            var uiData = new UIData();
            uiData.Add("Reward", rewardData.rewards);

            await UniTask.WaitForSeconds(0.5f);

            PanelManager.Instance.OpenPanel<PopupReward>(uiData);
        }

        private void UpdateCooldown()
        {
            if (!_initialized) return;

            if (!HasMoreAds())
                OnCooldownChanged?.Invoke();
        }

        #endregion

        #region Getters

        public int CurrentCount => _currentCount.Value;
        public int MaxCount => _config.maxWatchPerDay;
        public long NextResetTime => _nextResetTime.Value;
        public bool TutShown { get => _tutShown.BoolValue; set => _tutShown.BoolValue = value; }
        public VideoBarConfig Config => _config;

        #endregion
    }
}
