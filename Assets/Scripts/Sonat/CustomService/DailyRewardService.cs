using UnityEngine;
using SonatFramework.Systems;
using Cysharp.Threading.Tasks;
using System.Threading.Tasks;
using SonatFramework.Systems.GameDataManagement;
using System;
using GrillSort.RealTime;
using SonatFramework.Scripts.SonatSDKAdapterModule;

namespace GrillSort.DailyReward
{

    [CreateAssetMenu(fileName = "DailyRewardService", menuName = "My Services/DailyRewardService")]
    public class DailyRewardService : SonatServiceSo, IServiceInitializeAsync
    {
        [SerializeField] public bool active = true;

        [Header("Config")]
        [SerializeField] public DailyRewardConfig dailyRewardConfig;

        private readonly Service<DataService> dataService = new();
        private readonly Service<RealTimeService> realTimeService = new();

        public DailyRewardData data;
        public static string DataKey = "DailyReward_Data";
        public Action<DailyRewardData> onDataChanged;

        public async UniTaskVoid InitializeAsync()
        {
            await LoadConfig();
            if (!active) return;
            await LoadData();

            realTimeService.Instance.OnNextDay += ResetData;
        }

        #region Data/ Config
        private async Task LoadConfig()
        {
            active = SonatSDKAdapter.GetRemoteBool("daily_reward_active", active);
        }

        private async Task LoadData()
        {
            data = dataService.Instance.GetData<DailyRewardData>(DataKey);
            if (data == null)
            {
                data = new DailyRewardData();
            }
        }

        public void ResetData()
        {
            data.currentRewardIndex = 1;
            data.currentNumAds = 0;
            data.canClaimFreeReward = true;
            SaveData();
        }

        private void SaveData()
        {
            dataService.Instance.SetData(DataKey, data);
            onDataChanged?.Invoke(data);
        }


        public DailyRewardData GetData()
        {
            return data;
        }
        #endregion

        public bool CanClaimReward(int dailyRewardIndex = -1)
        {
            if (data.currentRewardIndex >= dailyRewardConfig.rewards.Count)
            {
                return false;
            }
            if (dailyRewardIndex == -1)
            {
                if (data.canClaimFreeReward) return true;
                var currentIdx = data.currentRewardIndex;
                var rewardConfig = dailyRewardConfig.rewards[currentIdx];
                return data.currentNumAds >= rewardConfig.numAds;
            }
            else
            {
                var rewardConfig = dailyRewardConfig.rewards[dailyRewardIndex];
                return data.currentRewardIndex == dailyRewardIndex && data.currentNumAds >= rewardConfig.numAds;
            }
            return false;
        }

        public void ClaimReward(int dailyRewardIndex)
        {
            if (dailyRewardIndex == 0)
            {
                data.canClaimFreeReward = false;
            }
            else
            {
                data.currentRewardIndex++;
                data.currentNumAds = 0;
            }

            SaveData();
        }

        public void IncreaseAds()
        {
            data.currentNumAds += 1;
            SaveData();
        }
    }


    public class DailyRewardData
    {
        public int currentRewardIndex = 1;
        public int currentNumAds = 0;
        public bool canClaimFreeReward = true;
    }
}