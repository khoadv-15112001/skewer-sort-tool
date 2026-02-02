using System;
using System.Collections.Generic;
using System.Linq;
using MyGame.Modules.CardCollection;
using Sonat.Enums;
using SonatFramework.Scripts.Feature.Lives;
using SonatFramework.Systems.EventBus;
using SonatFramework.Systems.GameDataManagement;
using SonatFramework.Systems.InventoryManagement.GameResources;
using UnityEngine;

namespace SonatFramework.Systems.InventoryManagement
{
    [CreateAssetMenu(fileName = "SonatInventoryService", menuName = "Sonat Services/Inventory Service")]
    public class SonatInventoryService : InventoryService, IServiceInitialize
    {
        private readonly Dictionary<GameResource, int> lastResources = new();
        private readonly Dictionary<GameResource, int> currentResources = new();

        private const string GameResourcePrefixKey = "Game_Resource_";
        [SerializeField] private Service<DataService> dataService = new();


        public void Initialize()
        {
            new EventBinding<AddItemEvent>(OnCollectEvent);
            new EventBinding<ReduceItemEvent>(OnReduceEvent);
        }

        public override int GetResource(GameResource resource)
        {
            if (currentResources.TryGetValue(resource, out var value)) return value;
            value = dataService.Instance.GetInt($"{GameResourcePrefixKey}{resource}", 0);
            currentResources.Add(resource, value);
            return value;
        }

        public override int SetResource(GameResource resource, int value)
        {
            dataService.Instance.SetInt($"{GameResourcePrefixKey}{resource}", value);
            if (!currentResources.TryAdd(resource, value))
            {
                currentResources[resource] = value;
            }

            return value;
        }


        public override int GetResourceView(GameResource resource)
        {
            if (lastResources.TryGetValue(resource, out var value))
            {
                return value;
            }

            value = GetResource(resource);
            lastResources.Add(resource, value);
            return value;
        }

        public override int AddResource(GameResource resource, int value, EarnResourceLogData logData = null,
            bool noti = false)
        {
            int lastResource = GetResource(resource);
            int curr = lastResource + value;
            SetResource(resource, curr);
            if (noti)
            {
                NotiUpdateResource(resource);
            }
            else
            {
                SetLastResource(resource, lastResource);
            }

            if (logData != null)
            {
                EarnResourceEvent eventData = new EarnResourceEvent(resource, value, logData);
                EventBus<EarnResourceEvent>.Raise(eventData);
            }

            return curr;
        }

        public override int ReduceResource(GameResource resource, int value, SpendResourceLogData logData = null,
            bool noti = true)
        {
            int lastResource = GetResource(resource);
            int curr = lastResource - value;
            SetResource(resource, curr);
            if (noti)
            {
                NotiUpdateResource(resource);
                EventBus<ReduceItemEvent>.Raise(new ReduceItemEvent() { resource = resource, quantity = value });
            }
            else
            {
                SetLastResource(resource, lastResource);
            }

            if (logData != null)
            {
                SpendResourceEvent eventData = new SpendResourceEvent(resource, value, logData);
                EventBus<SpendResourceEvent>.Raise(eventData);
            }

            return curr;
        }


        public override void AddReward(RewardData rewardData, EarnResourceLogData logData = null, bool noti = false)
        {
            foreach (var resourceData in rewardData.resourceDatas)
            {
                switch (resourceData.resource)
                {
                    case GameResource.Lives:
                        SonatSystem.GetService<LivesService>().AddUnlimitedLives(resourceData.quantity, logData);
                        break;
                    case GameResource.LivesService_SingleLive:
                        var maxLives = SonatSystem.GetService<LivesService>().config.maxLives;
                        var currentLives = GetResource(GameResource.Lives);
                        var addLives = Math.Min(maxLives, currentLives + resourceData.quantity) - currentLives;
                        AddResource(GameResource.Lives, addLives, logData, noti);
                        break;
                    default:
                        if (GameResourceHelper.ResourceType(resourceData.resource) == GameResourceType.Card)
                        {
                            var cardCollectionService = MySonatFramework.GetService<CardCollectionService>();
                            if (cardCollectionService == null || cardCollectionService.IsUnlocked() == false)
                            {
                                continue;
                            }
                        }
                        AddResource(resourceData.resource, resourceData.quantity, logData, noti);
                        break;
                }
            }

            if (noti)
            {
                OnClaimReward?.Invoke(rewardData);
            }
        }

        public override bool CanReduce(GameResource resource, int quantity)
        {
            return GetResource(resource) >= quantity;
        }


        private void SetLastResource(GameResource resource, int value)
        {
            if (!lastResources.TryAdd(resource, value))
            {
                lastResources[resource] = value;
            }
        }

        private void OnCollectEvent(AddItemEvent addEvent)
        {
            NotiUpdateResource(addEvent.resource);
        }

        private void OnReduceEvent(ReduceItemEvent reduceEvent)
        {
            NotiUpdateResource(reduceEvent.resource);
        }

        public override void NotiUpdateResource(GameResource resource = GameResource.MAX)
        {
            if (resource == GameResource.MAX)
            {
                if (lastResources != null)
                {
                    var keys = lastResources.Keys.ToList();
                    foreach (var key in keys)
                    {
                        SetLastResource(key, GetResource(key));
                        OnResourceUpdate?.Invoke(key);
                    }
                }
            }
            else
            {
                SetLastResource(resource, GetResource(resource));
                OnResourceUpdate?.Invoke(resource);
            }

        }

    }
}