using Cysharp.Threading.Tasks;
using GrillSort.OnlineService;
using SonatFramework.Scripts.Helper;
using SonatFramework.Systems;
using SonatFramework.Systems.EventBus;
using SonatFramework.Systems.InventoryManagement;
using SonatFramework.Systems.InventoryManagement.GameResources;
using SonatFramework.Systems.UserData;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace SkewerJam.Features.Leaderboard.Service
{

    [CreateAssetMenu(fileName = "LeaderboardService_SkewerJam", menuName = "Sonat Services/SkewerJam/Leaderboard Service")]
    public class LeaderboardHLWService : SonatServiceSo, IServiceInitialize
    {
        private const string DATA_KEY = "LEADERBOARD_HLW_DATA";
        private const string EVENT_ID = "hlw-grill-A-1025";
        public LeaderboardHLWConfig config;

        public static int limitTopLeaderboard;
        public static bool forceUnlock = false;

        private IntDataPref _rewardMilestoneClaimed;

        private readonly Service<OnlineService> _onlineService = new();
        private readonly Service<UserDataService> _userDataService = new();

        public int CurrentMilestoneIdx => _rewardMilestoneClaimed.Value;
        public int CurrentRank { get; private set; } = -1;

        public void Initialize()
        {
            LoadData();

            // new EventBinding<LevelEndedEvent>(OnLevelEnded);
            //new EventBinding<OnlineService.BIGetOrSignUpEvent>(OnBIGetOrSignUp);
            if (!PlayerPrefs.HasKey("BackupLeaderboardHLWScore"))
            {
                var pumpkin = MySonatFramework.GetService<InventoryService>().GetResource(Sonat.Enums.GameResource.Pumpkin);
                UpdateUserScore(pumpkin).Forget();
                PlayerPrefs.SetInt("BackupLeaderboardHLWScore", 1);
            }

            new EventBinding<AddItemEvent>(OnAddResourcePumpkin);

        }

        private void OnAddResourcePumpkin(AddItemEvent @event)
        {
            if (@event.resource == Sonat.Enums.GameResource.Pumpkin)
            {
                var pumpkin = MySonatFramework.GetService<InventoryService>().GetResource(Sonat.Enums.GameResource.Pumpkin);
                UpdateUserScore(pumpkin).Forget();
            }
        }

        private void LoadData()
        {
            _rewardMilestoneClaimed = new IntDataPref($"{DATA_KEY}_RewardMilestoneClaimed", -1);
        }

        public async UniTask<LeaderboardResponseData> FetchLeaderboard(int limit = 20)
        {
            LeaderboardRequestData_HLW leaderboardRequestData = new LeaderboardRequestData_HLW();
            leaderboardRequestData.limit = limit;

            try
            {
                return await GetLeaderboard(leaderboardRequestData);
            }
            catch (Exception ex)
            {
                Debug.LogError("Can't fetch leaderboard: " + ex.Message);
                return null;
            }
        }

        public async UniTask UpdateUserScore(int score)
        {
            await SubmitScore(score);
        }

        #region BI API
        private async UniTask<LeaderboardResponseData> GetLeaderboard(LeaderboardRequestData_HLW requestData)
        {
            var response = await _onlineService.Instance.Post<LeaderboardResponseData>($"leaderboard/v2/event/{EVENT_ID}", requestData);
            CurrentRank = response.position;
            return response;
        }

        private async UniTask<LeaderboardTrackingData> SubmitScore(int score)
        {
            var data = new Dictionary<string, int> { { "score", score } };
            return await _onlineService.Instance.Post<LeaderboardTrackingData>($"leaderboard/v2/tracking/event/{EVENT_ID}", data);
        }
        #endregion

        public bool CheckRewardMilestone()
        {

            if (_rewardMilestoneClaimed.Value >= config.rewardMilestone.Count - 1) return false;

            var pumpkin = MySonatFramework.GetService<InventoryService>().GetResource(Sonat.Enums.GameResource.Pumpkin);
            var neededPumpkin = config.rewardMilestone[_rewardMilestoneClaimed.Value + 1].pumpkin;
            return pumpkin >= neededPumpkin;
        }

        public bool CheckCompleteMilestone(int index)
        {
            return _rewardMilestoneClaimed.Value >= index;
        }

        public RewardData GetRewardMilestone()
        {
            return config.rewardMilestone[_rewardMilestoneClaimed.Value].reward;
        }

        public void UpdateMilestoneClaimed()
        {
            _rewardMilestoneClaimed.Value += 1;
        }

        public RewardData GetEventReward(int rank)
        {
            if (rank <= 0 || rank > config.rewardTop.Length)
            {
                return null;
            }
            return config.GetRewardTop(rank);
        }
    }

    public class LeaderboardRequestData_HLW
    {
        public int limit;
    }
}