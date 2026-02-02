using System;
using System.Collections;
using System.Collections.Generic;
using SonatFramework.Systems.InventoryManagement.GameResources;
using UnityEngine;

namespace GrillSort.DailyReward
{
    [CreateAssetMenu(fileName = "DailyRewardConfig", menuName = "Sonat Configs Custom/DailyRewardConfig", order = 0)]
    public class DailyRewardConfig : ScriptableObject
    {
        public List<RewardConfig> rewards;
    }

    [Serializable]
    public class RewardConfig
    {
        public int numAds = 0;
        public RewardData reward;
    }
}
