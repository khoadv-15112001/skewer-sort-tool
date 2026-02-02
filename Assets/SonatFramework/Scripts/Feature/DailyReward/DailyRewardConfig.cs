using System;
using System.Collections.Generic;
using SonatFramework.Systems.InventoryManagement.GameResources;
using UnityEngine;

namespace SonatFramework.Scripts.Feature.DailyReward
{
    [CreateAssetMenu(fileName = "DailyRewardConfig", menuName = "Sonat/Daily Reward/Config")]
    public class DailyRewardConfig : ScriptableObject, IFeatureConfig
    {
        public List<DailyReward> rewards;
        public List<DailyReward> milestones;

        public DailyReward GetDailyReward(int day, bool isMilestone)
        {
            return isMilestone ? milestones.Find(e => e.day == day) : rewards.Find(e => e.day == day);
        }

        public int MaxDay()
        {
            return milestones.Count;
        }
    }

    [Serializable]
    public class DailyReward
    {
        public int day;
        public RewardData rewardData;
    }
}