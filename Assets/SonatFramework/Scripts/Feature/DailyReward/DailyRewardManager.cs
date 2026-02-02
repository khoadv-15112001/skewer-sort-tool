using System;
using Cysharp.Threading.Tasks;
using SonatFramework.Systems;
using SonatFramework.Systems.GameDataManagement;
using SonatFramework.Systems.InventoryManagement;
using SonatFramework.Systems.LoadObject;
using SonatFramework.Systems.TimeManagement;

namespace SonatFramework.Scripts.Feature.DailyReward
{
    public class DailyRewardManager : SonatFeature<DailyRewardConfig, DailyRewardData>
    {
        private LoadObjectServiceAsync loadObjectServiceAsync;

        public override async UniTaskVoid InitializeAsync()
        {
            await LoadConfig();
            await LoadData();
            CheckDailyReward();
        }

        protected override async UniTask LoadConfig()
        {
            configs = await loadObjectServiceAsync.LoadAsync<DailyRewardConfig>(configKey);
        }

        protected override UniTask LoadData()
        {
            data = dataService.Instance.GetData<DailyRewardData>(dataKey);
            return UniTask.CompletedTask;
        }

        protected override void SaveData()
        {
            dataService.Instance.SetData(dataKey, data);
        }


        private void CheckDailyReward()
        {
            var daysSinceLastLogin = CalculateDaysSinceLastLogin();
            if (daysSinceLastLogin == 0) return;
            if (daysSinceLastLogin == 1)
            {
                data.currentStreak++;
                if (data.currentStreak > configs.MaxDay())
                {
                    data.currentStreak = 1;
                    data.dayClaimed.Clear();
                    data.milestoneClaimed.Clear();
                }
            }
            else if (daysSinceLastLogin > 1)
            {
                ResetStreak();
            }

            data.lastTimeLogin = timeService.Instance.GetUnixTimeSeconds();
            SaveData();
        }


        private int CalculateDaysSinceLastLogin()
        {
            if (data.lastTimeLogin == 0) return 0;
            var lastLoginDay = DateTimeOffset.FromUnixTimeSeconds(data.lastTimeLogin);
            return (timeService.Instance.GetCurrentTime() - lastLoginDay).Days;
        }

        private void ResetStreak()
        {
            data.currentStreak = 0;
            data.dayClaimed.Clear();
            data.milestoneClaimed.Clear();
        }

        public bool CheckDayClaimed(int day, bool isMilestone = false)
        {
            return isMilestone ? data.milestoneClaimed.Contains(day) : data.dayClaimed.Contains(day);
        }

        public bool CheckDayCanClaim(int day, bool isMilestone = false)
        {
            return data.currentStreak > day && !CheckDayClaimed(day, isMilestone);
        }

        public DailyReward ClaimReward(int day, bool isMilestone = false)
        {
            if (!CheckDayCanClaim(day, isMilestone)) return null;

            var dailyReward = configs.GetDailyReward(day, isMilestone);
            inventory.Instance.AddReward(dailyReward.rewardData, new EarnResourceLogData
            {
                spendType = "feature",
                spendId = "daily_reward"
            });
            AddDayClaim(day, isMilestone);
            return dailyReward;
        }

        public void AddDayClaim(int day, bool isMilestone = false)
        {
            if (isMilestone)
                data.milestoneClaimed.Add(day);
            else
                data.dayClaimed.Add(day);
            SaveData();
        }

        #region Services

        private readonly Service<InventoryService> inventory = new();
        private readonly Service<DataService> dataService = new();
        private readonly Service<TimeService> timeService = new();

        #endregion

        #region KEY

        private string configKey;
        private string dataKey;

        #endregion
    }
}