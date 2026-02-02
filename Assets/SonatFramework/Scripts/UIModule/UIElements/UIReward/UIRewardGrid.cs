using System;
using Sonat.Enums;
using SonatFramework.Systems;
using SonatFramework.Systems.InventoryManagement.GameResources;
using SonatFramework.Systems.ObjectPooling;
using UnityEngine;

namespace SonatFramework.Scripts.UIModule.UIElements
{
    public class UIRewardGrid : MonoBehaviour
    {
        [SerializeField] private Transform container;
        private readonly Service<PoolingContainerService> poolingService = new();
        private RewardData rewardData;
        private UIItemGroup uiItemGroup;

        private void Awake()
        {
            if (container == null) container = transform;
        }

        public void SetReward(RewardData rewardData)
        {
            this.rewardData = rewardData;
            ShowReward();
        }

        private void ShowReward()
        {
            poolingService.Instance.CleanContainer(container);
            CreateUIItemGroup();
            foreach (var resourceData in rewardData.resourceDatas)
            {
                if (!uiItemGroup.CanDisplay(resourceData.resource)) CreateUIItemGroup();
                uiItemGroup.AddResource(resourceData);
            }
        }

        private void CreateUIItemGroup()
        {
            uiItemGroup = poolingService.Instance.CreateObject<UIItemGroup>(container);
            uiItemGroup.CleanContainer();
        }

        public UIRewardItem GetRewardItem(GameResource resource)
        {
            if (container == null) return null;
            foreach (Transform child in container)
            {
                if (child.gameObject.activeSelf && child.TryGetComponent<UIItemGroup>(out var uIItemGroup))
                {
                    if (uIItemGroup.GetRewardItem(resource) != null)
                    {
                        return uIItemGroup.GetRewardItem(resource);
                    }
                }
            }

            return null;
        }
    }
}