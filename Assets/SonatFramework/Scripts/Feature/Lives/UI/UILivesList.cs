using System.Collections.Generic;
using Sonat.Enums;
using SonatFramework.Systems;
using SonatFramework.Systems.InventoryManagement;
using SonatFramework.Systems.ObjectPooling;
using UnityEngine;

namespace SonatFramework.Scripts.Feature.Lives.UI
{
    public class UILivesList : MonoBehaviour
    {
        public Transform liveContainer;
        private readonly Service<InventoryService> inventoryService = new();
        private readonly Service<LivesService> livesService = new ();
        private readonly Service<PoolingContainerService> poolingService = new();
        private readonly GameResource resource = GameResource.Lives;
        private List<Transform> liveItems;

        protected virtual void OnDestroy()
        {
            inventoryService.Instance.OnResourceUpdate -= OnResourceUpdate;
        }

        public virtual void Setup()
        {
            poolingService.Instance.CleanContainer(liveContainer);
            liveItems = new List<Transform>();
            for (var i = 0; i < livesService.Instance.config.maxLives; i++)
            {
                var item = poolingService.Instance.CreateObject<Transform>(liveContainer);
                liveItems.Add(item);
            }

            OnResourceUpdate(GameResource.Lives);
            inventoryService.Instance.OnResourceUpdate += OnResourceUpdate;
        }

        protected virtual void OnResourceUpdate(GameResource res)
        {
            if (res != resource && res != GameResource.MAX) return;
            var lives = inventoryService.Instance.GetResource(GameResource.Lives);
            SetLive(lives);
        }

        private void SetLive(int count)
        {
            for (var i = 0; i < liveItems.Count; i++) liveItems[i].GetChild(0).gameObject.SetActive(i < count);
        }
    }
}