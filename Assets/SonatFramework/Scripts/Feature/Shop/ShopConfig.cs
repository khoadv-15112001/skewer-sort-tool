using System.Collections.Generic;
using Sonat.Enums;
using SonatFramework.Systems.ConfigManagement;
using SonatFramework.Systems.InventoryManagement.GameResources;
using UnityEngine;
using UnityEngine.Serialization;

namespace SonatFramework.Scripts.Feature.Shop
{
    [CreateAssetMenu(menuName = ("Sonat Configs/Shop Config"), fileName = "ShopConfig")]
    public class ShopConfig : ConfigSo
    {
        public List<ShopPack> packs;
    }

    [System.Serializable]
    public class ShopPack
    {
        public bool active = true;
        public ShopItemKey key;
        public RewardData rewardData;
        public bool noAds;
        public bool noAdsFree;
        public bool oneTimePurchase;
        [SerializeField] private int group;
        public int Group => group > 0 ? group : (int)key;
    }
}