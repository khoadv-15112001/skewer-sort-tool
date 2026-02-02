using System;
using System.Collections.Generic;
using Sonat.Enums;

namespace SonatFramework.Systems.InventoryManagement.GameResources
{
    [Serializable]
    public class RewardData
    {
        public List<ResourceData> resourceDatas;

        public void AddReward(ResourceData resourceData)
        {
            if(resourceDatas == null) resourceDatas = new List<ResourceData>();
            int index = resourceDatas.FindIndex(e => e.resource == resourceData.resource);
            if(index  < 0) resourceDatas.Add(resourceData);
            else resourceDatas[index].quantity += resourceData.quantity;
        }

        public void MultiplyReward(int multiplier)
        {
            if(resourceDatas == null) return;
            foreach (var resourceData in resourceDatas)
            {
                resourceData.quantity *= multiplier;
            }
        }

        public List<RewardDataJson> ToListRewardDataJson()
        {
            var result = new List<RewardDataJson>();
            if (resourceDatas == null) return result;

            foreach (var resourceData in resourceDatas)
            {
                result.Add(resourceData.ToRewardDataJson());
            }

            return result;
        }
    }

    [Serializable]
    public class ResourceData
    {
        public GameResource resource;
        public int quantity;
        
        public ResourceData(){}

        public ResourceData(GameResource resource, int quantity)
        {
            this.resource = resource;
            this.quantity = quantity;
        }

        public RewardDataJson ToRewardDataJson()
        {
            return new(resource.ToString(), quantity, 0);
        }
    }
}