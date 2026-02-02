using UnityEngine;
using SonatFramework.Systems;
using Cysharp.Threading.Tasks;
using System.Threading.Tasks;
using System;
using SonatFramework.Systems.GameDataManagement;
using GrillSort.RealTime;
using SonatFramework.Scripts.Helper;
using SonatFramework.Scripts.SonatSDKAdapterModule;
using SonatFramework.Systems.ConfigManagement;
using Unity.VisualScripting.FullSerializer;
using SonatFramework.Scripts.Utils;

namespace GrillSort.LuckySpin
{

    [CreateAssetMenu(fileName = "LuckySpinService", menuName = "My Services/Lucky Spin Service")]
    public class LuckySpinService : SonatServiceSo, IServiceInitializeAsync
    {
        [Header("Config")]
        [SerializeField] public LuckySpinConfig luckySpinConfig;

        private readonly Service<DataService> dataService = new();
        private readonly Service<RealTimeService> realTimeService = new();

        public LuckySpinData data;
        public static string DataKey = "LuckySpin_Data";
        private string CooldownKey = "LuckySpinCooldown";

        public Action<LuckySpinData> onDataChanged;

        private IntDataPref _unlockLuckySpin;
        private LongDataPref _cooldown;
        public long Cooldown { get => _cooldown.Value; }
        public void AddTimeCooldown()
        {
            _cooldown.Value = realTimeService.Instance.GetCurrentTimeUnix() + luckySpinConfig.cooldown;
        }
        internal void ResetCooldown()
        {
            _cooldown.Value = 0;
        }

        internal long GetCooldownTime()
        {
            return _cooldown.Value - realTimeService.Instance.GetCurrentTimeUnix();
        }
        public async UniTaskVoid InitializeAsync()
        {
            await LoadData();
            await LoadConfig();

            realTimeService.Instance.OnNextDay += ResetData;
        }

        #region Data/ Config
        private async Task LoadConfig()
        {
            luckySpinConfig.cooldown = (long)SonatSDKAdapter.GetRemoteFloat("cooldown_lucky_spin", luckySpinConfig.cooldown);

            // throw new NotImplementedException();
        }

        private async Task LoadData()
        {
            data = dataService.Instance.GetData<LuckySpinData>(DataKey);
            if (data == null)
            {
                data = new LuckySpinData();
            }
            _cooldown = new LongDataPref(CooldownKey);
            _unlockLuckySpin = new IntDataPref($"{DataKey}_unlockLuckySpin", 0);

            if (Cooldown <= realTimeService.Instance.GetCurrentTimeUnix())
                ResetCooldown();
        }

        public void ResetData()
        {
            data.currentSpinCount = 0;
            SaveData();
        }

        private void SaveData()
        {
            dataService.Instance.SetData(DataKey, data);
            onDataChanged?.Invoke(data);
        }


        public LuckySpinData GetData()
        {
            return data;
        }
        #endregion

        public void Unlock()
        {
            _unlockLuckySpin.Value = 1;
        }

        public bool IsUnlocked()
        {
            return _unlockLuckySpin.Value == 1;
        }

        public void AddSpin()
        {
            data.currentSpinCount++;

            if (data.currentSpinCount < luckySpinConfig.maxSpinCount)
                AddTimeCooldown();

            SaveData();
        }

        public bool CheckFreeSpin()
        {
            var userDay = MySonatFramework.userDataService.UserDay;
            return userDay == 0 && data.currentSpinCount == 0;
        }

        public bool CanUnlock()
        {
            var level = MySonatFramework.userDataService.GetLevel();
            return luckySpinConfig.liveOpsPackData.CheckCondition();
        }

        internal object GetEndCooldown()
        {
            return SonatUtils.GetTimeByFormat(_cooldown.Value > 0?GetCooldownTime() : realTimeService.Instance.GetRemainingTimeInDay(), TxtTimeFormat.SmartFull);
        }
    }


    public class LuckySpinData
    {
        public int currentSpinCount;
    }
}