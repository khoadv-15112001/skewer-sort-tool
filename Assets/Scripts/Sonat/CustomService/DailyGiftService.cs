using UnityEngine;
using SonatFramework.Systems;
using Cysharp.Threading.Tasks;
using SonatFramework.Systems.GameDataManagement;
using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using SonatFramework.Systems.InventoryManagement.GameResources;
using GrillSort.RealTime;
using SonatFramework.Scripts.Helper;

namespace GrillSort.DailyGift
{
    public abstract class DailyGiftService : SonatServiceSo, IServiceInitializeAsync
    {
        [Header("Config")]
        public DailyGiftConfig dailyGiftConfig;

        private readonly Service<DataService> dataService = new();
        private readonly Service<RealTimeService> realTimeService = new();

        public DailyGiftData data;
        public abstract string DataKey { get; }
        public Action onDataChanged;

        private int _finishDay;
        private IntDataPref _isUnlocked;

        public async UniTaskVoid InitializeAsync()
        {
            await LoadConfig();
            await LoadData();

            _finishDay = dailyGiftConfig.GetFinishDay();
            realTimeService.Instance.OnNextDay += UpdateData;
        }

        #region Data
        private async UniTask LoadData()
        {
            data = dataService.Instance.GetData<DailyGiftData>(DataKey);
            if (data == null)
            {
                data = new DailyGiftData();
                ResetData();
            }

            _isUnlocked = new IntDataPref(DataKey + "_isUnlocked", 0);
        }

        private async UniTask LoadConfig()
        {

        }

        private void UpdateData()
        {
            if (CheckContinueStreak() == false)
            {
                ResetData();
            }
            NextDay();
        }

        protected void SaveData()
        {
            dataService.Instance.SetData(DataKey, data);
            onDataChanged?.Invoke();
        }

        private void ResetData()
        {
            data.currentStreakDay = 0;
            data.lastLoginSecond = 0;
            data.claimDays.Clear();

            SaveData();
        }

        private void NextDay()
        {
            data.lastLogin = realTimeService.Instance.GetCurrentTime();
            data.currentStreakDay += 1;
            if (dailyGiftConfig.CheckGift(data.currentStreakDay))
            {
                data.claimDays.Add(data.currentStreakDay);
            }

            SaveData();
        }
        #endregion
        public void CheatNextDay()
        {

            if (data.currentStreakDay >= dailyGiftConfig.GetFinishDay())
            {
                ResetData();
                return;
            }

            NextDay();
        }
        private bool CheckContinueStreak()
        {
            int days = realTimeService.Instance.GetDaysPassed(data.lastLogin, realTimeService.Instance.GetCurrentTime());

            if (days >= dailyGiftConfig.maxLoginStreakGapDays) return false;
            if (data.currentStreakDay + 1 > _finishDay) return false;
            return true;
        }

        public bool CanClaim(int streakDay = -1)
        {
            if (streakDay == -1)
            {
                return data.claimDays.Count > 0;
            }

            foreach (var claimDay in data.claimDays)
            {
                if (claimDay == streakDay)
                {
                    return true;
                }
            }
            return false;
        }

        public bool ClaimDailyGift(int streakDay)
        {
            data.claimDays.Remove(streakDay);
            SaveData();

            return true;
        }

        public void ClaimAllDailyGift()
        {
            data.claimDays.Clear();
            SaveData();
            onDataChanged?.Invoke();
        }

        public RewardData GetAllReward()
        {
            RewardData rewardData = new RewardData();
            rewardData.resourceDatas = new();
            foreach (var day in data.claimDays)
            {
                var gift = dailyGiftConfig.GetGiftForDay(day);
                rewardData.resourceDatas.AddRange(gift.reward.resourceDatas);
            }
            return rewardData;
        }

        public class DailyGiftData
        {
            public int currentStreakDay = 0;
            public long lastLoginSecond;
            [JsonIgnore]
            public DateTime lastLogin
            {
                get => DateTimeOffset.FromUnixTimeSeconds(lastLoginSecond).ToLocalTime().DateTime;
                set => lastLoginSecond = ((DateTimeOffset)value).ToUnixTimeSeconds();
            }
            public List<int> claimDays = new List<int>();
        }

        public bool CanUnlock()
        {
            return dailyGiftConfig.liveOpsPackData.CheckCondition();
        }

        public bool IsUnlocked()
        {
            return _isUnlocked.Value == 1;
        }

        public void Unlock()
        {
            _isUnlocked.Value = 1;
            ResetData();
            NextDay();
        }

    }
}