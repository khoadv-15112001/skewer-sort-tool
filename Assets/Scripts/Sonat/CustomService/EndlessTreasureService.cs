using System;
using Cysharp.Threading.Tasks;
using GrillSort.RealTime;
using Sonat.Enums;
using SonatFramework.Scripts.Helper;
using SonatFramework.Scripts.SonatSDKAdapterModule;
using SonatFramework.Scripts.Utils;
using SonatFramework.Systems;
using SonatFramework.Systems.EventBus;
using SonatFramework.Systems.GameDataManagement;
using SonatFramework.Systems.InventoryManagement.GameResources;
using Unity.VisualScripting;
using UnityEngine;

namespace GrillSort.EndlessTreasure
{
    [CreateAssetMenu(fileName = "EndlessTreasureService", menuName = "My Services/EndlessTreasureService")]
    public class EndlessTreasureService : SonatServiceSo, IServiceInitialize
    {
        public EndlessTreasureConfig config;

        public EndlessTreasureData data;
        private readonly Service<DataService> dataService = new();
        private string DataKey = "EndlessTreasureData";
        private string CooldownKey = "EndlessTreasureCooldown";
        private readonly Service<RealTimeService> realTimeService = new();

        private IntDataPref _isUnlocked;
        private LongDataPref _cooldown;
        public Action<bool> OnChangeData;

        public long Cooldown { get => _cooldown.Value; }
        public void AddTimeCooldown()
        {
            _cooldown.Value = realTimeService.Instance.GetCurrentTimeUnix() + config.cooldown;
        }
        internal void ResetCooldown()
        {
            _cooldown.Value = 0;
        }

        internal long GetCooldownTime()
        {
            return _cooldown.Value - realTimeService.Instance.GetCurrentTimeUnix();
        }
        public void Initialize()
        {
            LoadConfig();
            LoadData();

            if (CheckNextDay()) // có cả trường hợp khi khởi tạo
            {
                Reset();
            }

            CheckLoop().Forget();

            // Tính thời gian từ khi xuất hiện
            // Xuất hiện theo level
            new EventBinding<LevelEndedEvent>(OnLevelEnded);

            // // Bắt đầu xuất hiện theo d
            // var currentDay = MySonatFramework.GetService<UserDataService>().UserDay;
            // var session = MySonatFramework.GetService<UserDataService>().SessionToday;

            // if (config.appearDay > 0 && currentDay == config.appearDay && session == 1)
            // {
            //     Reset();
            // }

            if (Cooldown <= realTimeService.Instance.GetCurrentTimeUnix())
                ResetCooldown();
        }

        private bool CheckNextDay()
        {
            var currentTime = realTimeService.Instance.GetCurrentTime();
            return currentTime > data.nextResetTime;
        }

        private void OnLevelEnded(LevelEndedEvent @event)
        {
            if (!config.active) return;

            if (IsUnlocked() == true)
            {
                return;
            }
            if (@event.success && @event.level + 1 == config.appearLevel && CanUnlock())
            {
                Unlock();
            }
        }

        private void LoadConfig()
        {
            config.appearLevel = SonatSDKAdapter.GetRemoteInt("endless_treasure_appear_level", config.appearLevel);
            config.cooldown = (long)SonatSDKAdapter.GetRemoteFloat("cooldown_endless_treasure", config.cooldown);
            config.active = SonatSDKAdapter.GetRemoteBool("active_endless_treasure", config.active);
            // config.appearDay = SonatSDKAdapter.GetRemoteInt("endless_treasure_appear_day", config.appearDay);
        }

        private void LoadData()
        {
            _isUnlocked = new IntDataPref($"{DataKey}_isUnlocked", 0);
            _cooldown = new LongDataPref($"{CooldownKey}");
            data = dataService.Instance.GetData<EndlessTreasureData>(DataKey);
            if (data == null)
            {
                data = new EndlessTreasureData();
            }
        }

        private void SaveData(bool reset = false)
        {
            dataService.Instance.SetData(DataKey, data);
            if (reset)
            {
                OnChangeData?.Invoke(true);
            }
            else
            {
                OnChangeData?.Invoke(false);
            }
        }


        private async UniTaskVoid CheckLoop()
        {
            var timeToReset = GetTimeToReset();
            await UniTask.Delay((int)timeToReset * 1000, DelayType.Realtime);
            Reset();
            CheckLoop().Forget();

        }

        public void Reset()
        {
            data.nextResetTime = realTimeService.Instance.GetCurrentTime().AddDays(config.expireInDays);
            data.currentPackIdx = 0;
            SaveData(true);
        }

        public RewardData GetRewardInPack(int packIdx)
        {
            return config.GetReward(packIdx);
        }

        public void ClaimPack(int packIdx)
        {
            //Debug.Log($"anhnt: ClaimPack: {packIdx}");
            data.currentPackIdx += 1;

            if (config.GetPackKey(data.currentPackIdx) != ShopItemKey.None)
                _cooldown.Value = 0;
            else
                AddTimeCooldown();

            if (data.currentPackIdx >= config.GetMaxPack())
            {
                data.currentPackIdx = config.startLoop;
            }

            SaveData(false);
        }

        public ShopItemKey GetPackKey(int packIdx)
        {
            return config.GetPackKey(packIdx);
        }

        public long GetTimeToReset()
        {
            return (long)(data.nextResetTime - realTimeService.Instance.GetCurrentTime()).TotalSeconds;
        }

        public int GetCurrentIdxByOrderNumber(int orderNumber)
        {
            var idx = data.currentPackIdx + orderNumber;
            if (idx >= config.startLoop)
            {
                idx = (idx - config.startLoop) % (config.GetMaxPack() - config.startLoop) + config.startLoop;
            }
            return idx;
        }

        public bool IsUnlocked()
        {
            if (!config.active) return false;

            if (_isUnlocked.Value == 1)
            {
                return true;
            }
            return config.liveOpsPackData.CheckCondition();
        }

        public void Unlock()
        {
            _isUnlocked.Value = 1;
            Reset();
        }

        public bool CanUnlock()
        {
            return config.active && _isUnlocked.Value == 0 && config.liveOpsPackData.CheckCondition();
        }

        internal string GetEndCooldown()
        {
            return SonatUtils.GetTimeByFormat(GetCooldownTime(), TxtTimeFormat.SmartFull);
        }
    }

    public class EndlessTreasureData
    {
        public int currentPackIdx = 0;
        public DateTime nextResetTime = DateTime.MinValue;
    }
}
