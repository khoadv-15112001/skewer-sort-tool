using System;
using Sonat.Enums;
using SonatFramework.Systems.InventoryManagement.GameResources;
using UnityEngine;

namespace SonatFramework.Systems.InventoryManagement
{
    public abstract class InventoryService : SonatServiceSo
    {
        public Action<GameResource> OnResourceUpdate { get; set; }
        public Action<RewardData> OnClaimReward { get; set; }

        public abstract int GetResource(GameResource resource);
        public abstract int GetResourceView(GameResource resource);

        public abstract int AddResource(GameResource resource, int value, EarnResourceLogData logData = null,
            bool noti = true);

        public abstract int ReduceResource(GameResource resource, int value, SpendResourceLogData logData = null,
            bool noti = true);

        public abstract void AddReward(RewardData rewardData, EarnResourceLogData logData = null, bool noti = false);

        public abstract int SetResource(GameResource resource, int value);

        public abstract bool CanReduce(GameResource resource, int quantity);

        public abstract void NotiUpdateResource(GameResource resource = GameResource.MAX);
    }

    public class EarnResourceLogData
    {
        public string spendType;
        public string spendId;
        public bool isFirstBuy = false;
        public string source = "non_iap";
        public int price;
    }

    public class SpendResourceLogData
    {
        public string earnType;
        public string earnId;
        public string source = "non_iap";
        public int price;
    }


}