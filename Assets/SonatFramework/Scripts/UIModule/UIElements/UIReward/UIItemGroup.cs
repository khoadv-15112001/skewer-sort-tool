using System;
using System.Collections.Generic;
using Sonat.Enums;
using SonatFramework.Systems;
using SonatFramework.Systems.InventoryManagement.GameResources;
using SonatFramework.Systems.ObjectPooling;
using UnityEngine;

namespace SonatFramework.Scripts.UIModule.UIElements
{
    public class UIItemGroup : MonoBehaviour
    {
        [SerializeField] private Transform container;

        [SerializeField] private List<GameResource> resources;
        public int maxSlot = 1;
        public int order;
        private readonly Service<PoolingContainerService> poolingService = new();
        private readonly List<ResourceData> resourceDatas = new();

        public void CleanContainer()
        {
            poolingService.Instance.CleanContainer(container);
            resourceDatas.Clear();
        }

        public void AddResource(ResourceData resource)
        {
            resourceDatas.Add(resource);
            var item = poolingService.Instance.CreateObject<UIRewardItem>(container);
            item.Init(resource.resource, resource.quantity);
        }

        public bool CanDisplay(GameResource resource)
        {
            if (resourceDatas.Count >= maxSlot) return false;
            return resources.Contains(GameResource.MAX) || resources.Contains(resource);
        }

        public UIRewardItem GetRewardItem(GameResource resource)
        {
            foreach (Transform child in container)
            {
                if (child.gameObject.activeSelf && child.TryGetComponent<UIRewardItem>(out var uIRewardItem))
                {
                    if (uIRewardItem.Resource == resource)
                    {
                        return uIRewardItem;
                    }
                }
            }
            return null;
        }
    }
}