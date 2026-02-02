using System;
using System.Collections.Generic;
using SonatFramework.Systems.InventoryManagement.GameResources;
using UnityEngine;
using UnityEngine.Serialization;

namespace SonatFramework.Scripts.Feature
{
    [CreateAssetMenu(menuName = "Sonat/ChestProgress/Config", fileName = "ChestProgressConfig")]
    public class ChestRewardProgressConfig : ScriptableObject, IFeatureConfig
    {
        public bool active = false;
        public int levelStart = 10;

        [FormerlySerializedAs("chestProgressDataDefault")]
        public ChestConfig chestConfigDefault;

        public List<ChestConfig> ChestRewards;

        public virtual ChestConfig GetChestRewardData(int index)
        {
            if (index < 0 || index >= ChestRewards.Count) return chestConfigDefault;
            return ChestRewards[index];
        }
    }

    [Serializable]
    public class ChestConfig
    {
        public int levelRequired = 5;
        public RewardData reward;
    }

    public class ChestRewardProgressData : IFeatureData
    {
        public int currentChestIndex;
        public int currentProgress;
    }
}