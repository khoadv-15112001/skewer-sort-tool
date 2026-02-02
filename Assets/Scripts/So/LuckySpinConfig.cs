using System;
using System.Collections;
using System.Collections.Generic;
using SonatFramework.Systems.InventoryManagement.GameResources;
using UnityEngine;

namespace GrillSort.LuckySpin
{
    [CreateAssetMenu(fileName = "LuckySpinConfig", menuName = "Sonat Configs Custom/LuckySpinConfig", order = 0)]
    public class LuckySpinConfig : ScriptableObject
    {
        public int maxSpinCount = 3;
        public long cooldown= 3600;
        public RewardData accumulatedReward;
        public List<SpinRewardData> rewards;
        public LiveOpsPackData liveOpsPackData;

#if UNITY_EDITOR
        public void OnValidate()
        {
            if (rewards != null)
            {
                var totalWeight = 0f;
                for (int i = 0; i < rewards.Count - 1; i++)
                {
                    totalWeight += rewards[i].weight;
                }

                rewards[rewards.Count - 1].weight = 1 - totalWeight;
            }
        }
#endif
    }

    [Serializable]
    public class SpinRewardData
    {
        public ResourceData reward;
        [Range(0, 1)]
        public float weight;
    }
}
