using System;
using System.Collections.Generic;
using System.Linq;
using SonatFramework.Systems.InventoryManagement.GameResources;
using UnityEngine;

namespace SkewerJam.Features.Leaderboard.Service
{
    [CreateAssetMenu(fileName = "LeaderboardHLWConfig", menuName = "Sonat Configs Custom/LeaderboardHLWConfig")]
    public class LeaderboardHLWConfig : LeaderboardConfig
    {
        [Header("Reward Milestone")]
        public List<RewardMilestone> rewardMilestone;

        [Header("Reward Top")]
        public RewardData[] rewardTop;

        public int GetMaxPumpkinMilestone()
        {
            return rewardMilestone.Max(e => e.pumpkin);
        }

        public RewardData GetRewardTop(int rank)
        {
            return rewardTop[rank - 1];
        }
    }

    [Serializable]
    public class RewardMilestone
    {
        public int pumpkin;
        public RewardData reward;
    }
}