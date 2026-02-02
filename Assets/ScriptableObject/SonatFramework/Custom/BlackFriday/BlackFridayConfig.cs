using System;
using System.Collections.Generic;
using Sonat.Enums;
using SonatFramework.Systems.InventoryManagement.GameResources;
using UnityEngine;

namespace GrillSort.BlackFriday
{
    [CreateAssetMenu(fileName = "BlackFridayConfig", menuName = "Sonat Configs Custom/BlackFridayConfig")]
    [Serializable]
    public class BlackFridayConfig : ScriptableObject
    {
        [Header("event time")]
        public string startDate = "2025-11-17";
        public string endDate = "2025-11-30";

        [Header("price Spin")]
        public List<SpinPriceByUserType> spinPrices = new();

        [Header("discount")]
        public List<UserDiscountTable> discountTables = new();

        [Header("Reward")]
        public List<RewardByUserType> rewardsPerSpin = new();
    }

    [Serializable]
    public class SpinPriceByUserType
    {
        public UserGroup userType;
        public int firstSpinPrice = 0;
        public int spinPrice = 50;
    }

    [Serializable]
    public class UserDiscountTable
    {
        public UserGroup userType;
        public List<DiscountProbability> discountProbabilities = new();
    }

    [Serializable]
    public class DiscountProbability
    {
        public int discountPercent; // 60, 40, 30, 20
        public int weightFirst;
        public int weightSecond;
        public int weightLater;
    }

    [Serializable]
    public class BlackFridayPackData
    {
        public UserGroup userType;
        public int discountPercent;
        public ShopItemKey shopItemKey; // pack ứng với discount này
    }

    [Serializable]
    public class RewardByUserType
    {
        public UserGroup userType;
        public ResourceData reward = new ResourceData() { resource = GameResource.Coin, quantity = 2500 };
    }
}
