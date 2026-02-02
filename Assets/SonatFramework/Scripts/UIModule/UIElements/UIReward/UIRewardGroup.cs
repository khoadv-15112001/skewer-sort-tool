using System.Collections.Generic;
using System.Linq;
using Sonat.Enums;
using SonatFramework.Systems.InventoryManagement.GameResources;
using UnityEngine;

namespace SonatFramework.Scripts.UIModule.UIElements
{
    public class UIRewardGroup : MonoBehaviour
    {
        private bool inited;
        private List<UIItemGroup> itemGroups;
        private RewardData rewardData;

        private void Init()
        {
            if (inited) return;
            itemGroups = GetComponentsInChildren<UIItemGroup>().ToList();
            foreach (var uiItemGroup in itemGroups) uiItemGroup.CleanContainer();
            itemGroups.Sort((x, y) => x.order.CompareTo(y.order));
            inited = true;
        }

        public void SetData(RewardData rewardData)
        {
            Init();
            this.rewardData = rewardData;
            foreach (var resourceData in this.rewardData.resourceDatas)
            {
                var uiItemGroup = GetBestItemGroup(resourceData.resource);
                if (uiItemGroup != null) uiItemGroup.AddResource(resourceData);
            }
        }

        public RewardData GetData()
        {
            return rewardData;
        }

        private UIItemGroup GetBestItemGroup(GameResource resource)
        {
            foreach (var uiItemGroup in itemGroups)
                if (uiItemGroup.CanDisplay(resource))
                    return uiItemGroup;

            return null;
        }

        public void Clear()
        {
            itemGroups = GetComponentsInChildren<UIItemGroup>().ToList();
            foreach (var uiItemGroup in itemGroups) uiItemGroup.CleanContainer();
            itemGroups.Sort((x, y) => x.order.CompareTo(y.order));
            inited = false;
        }
    }
}