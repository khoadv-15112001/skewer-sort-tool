using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using Sonat.Enums;
using SonatFramework.Scripts.Feature.Shop;
using SonatFramework.Systems;
using SonatFramework.Systems.InventoryManagement.GameResources;
using UnityEngine;

namespace GrillSort.EndlessTreasure
{
    [CreateAssetMenu(fileName = "EndlessTreasureConfig", menuName = "Sonat Configs Custom/EndlessTreasureConfig", order = 0)]
    public class EndlessTreasureConfig : ScriptableObject
    {
        public bool active;
        public int appearLevel = 11;
        // public int appearDay = 0;
        public int startLoop = 3;
        public int expireInDays = 7;
        public long cooldown = 7200; //2h
        public List<PackData> listPacks;
        public LiveOpsPackData liveOpsPackData;
        
        private readonly Service<ShopService> shopService = new();
        public RewardData GetReward(int packIdx)
        {
            var key = GetPackKey(packIdx);
            if (key == ShopItemKey.None)
            {
                return listPacks[packIdx].rewardData;
            }
            else
            {
                return shopService.Instance.GetPackData(key)?.rewardData;
            }
        }

        public ShopItemKey GetPackKey(int packIdx)
        {
            return listPacks[packIdx].shopItemKey;
        }

        public int GetMaxPack()
        {
            return listPacks.Count;
        }

        void OnValidate()
        {
            for (int i = 0; i < listPacks.Count; i++)
            {
                listPacks[i].packIdx = i;
            }
        }
    }

    [Serializable]
    public class PackData
    {
        [GUIColor(0.2f, 1f, 0.2f)]
        [ReadOnly]
        public int packIdx;
        public ShopItemKey shopItemKey = ShopItemKey.None;
        [ShowIf("@shopItemKey == ShopItemKey.None")]
        public RewardData rewardData;
    }
}
