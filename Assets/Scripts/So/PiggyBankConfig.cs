using System;
using System.Collections;
using System.Collections.Generic;
using Sonat.Enums;
using SonatFramework.Systems.InventoryManagement.GameResources;
using UnityEngine;

namespace GrillSort.PiggyBank
{
    [CreateAssetMenu(fileName = "PiggyBankConfig", menuName = "Sonat Configs Custom/PiggyBankConfig", order = 0)]
    public class PiggyBankConfig : ScriptableObject
    {
        public int reward = 5;
        public List<PiggyTierConfig> piggyTiers;
        public LiveOpsPackData liveOpsPackData;

        public PiggyTierConfig GetPiggyTierConfig(int piggyTierIdx)
        {
            if (piggyTierIdx < 0 || piggyTierIdx >= piggyTiers.Count)
            {
                piggyTierIdx = piggyTiers.Count - 1;
            }
            return piggyTiers[piggyTierIdx];
        }
    }

    [Serializable]
    public class  PiggyTierConfig
    {
        [Serializable]
        public class PointAndReward
        {
            public int point;
            public RewardData reward;
            public int iconSpriteIdx;
            public ShopItemKey shopItemKey;
        }
        public List<PointAndReward> piggyRewards;


        #region functions
        public int GetNumPiggyTier()
        {
            return piggyRewards.Count;
        }

        public int GetMaxPoint()
        {
            return piggyRewards[piggyRewards.Count - 1].point;
        }

        public int GetRewardCoins(int rewardIdx)
        {
            if (rewardIdx < 0 || rewardIdx >= piggyRewards.Count)
            {
                return 0;
            }
            return piggyRewards[rewardIdx].reward.resourceDatas[0].quantity;
        }

        public int GetRewardIdxByPoints(int points)
        {
            for (int i = piggyRewards.Count - 1; i >= 0; i--)
            {
                if (points >= piggyRewards[i].point)
                {
                    return i;
                }
            }
            return -1;
        }

        public ShopItemKey GetRewardShopKey(int rewardIdx)
        {
            if (rewardIdx < 0 || rewardIdx >= piggyRewards.Count)
            {
                rewardIdx = 0;
            }
            return piggyRewards[rewardIdx].shopItemKey;
        }
        #endregion
    }
}
