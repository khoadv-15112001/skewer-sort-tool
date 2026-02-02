using UnityEngine;
using SonatFramework.Systems;
using Cysharp.Threading.Tasks;
using SonatFramework.Systems.BoosterManagement;
using Sonat.Enums;
using SonatFramework.Systems.InventoryManagement;
using SonatFramework.Systems.UserData;
using SonatFramework.Scripts.SonatSDKAdapterModule;
using SonatFramework.Systems.GameDataManagement;
using System.Collections.Generic;
using SonatFramework.Systems.EventBus;
using Sirenix.OdinInspector;
using SonatFramework.Scripts.UIModule;
using System;
using SonatFramework.Scripts.Utils;
using Sonat.Data;
using Sonat.TrackingModule;
using UnityEngine.Device;
using SonatFramework.Systems.TrackingModule;

namespace GrillSort.PreBooster
{
    [CreateAssetMenu(fileName = "PreBoosterService", menuName = "My Services/Pre Booster Service")]
    public class PreBoosterService : SonatServiceSo, IServiceWaitingSDKInitialize
    {
        #region Services

        [BoxGroup("SERVICES")]
        [Required]
        [SerializeField]
        private Service<InventoryService> inventoryService = new();

        [BoxGroup("SERVICES")]
        [Required]
        [SerializeField]
        private Service<DataService> dataService = new();

        private Service<UserDataService> userDataService = new();

        #endregion

        [BoxGroup("CONFIGS", true)]
        [SerializeField]
        private BoostersConfig boostersConfig;

        private readonly Dictionary<GameResource, BoosterData> boostersData = new();
        private readonly Dictionary<GameResource, BoosterConfig> configs = new();
        private readonly Dictionary<GameResource, bool> usingBooster = new();
        private readonly Dictionary<GameResource, bool> usedPreBooster = new();
        public Action<GameResource> onUnlockBooster;

        public bool unlockAllBooster = false;
        public int timeToAdd = 10;
        public int numPreBoosterMagnet = 1;

        public void OnSonatSDKInitialize()
        {
            unlockAllBooster = true;
            int level = userDataService.Instance.GetLevel();
            foreach (var boosterConfig in boostersConfig.configs)
            {
                boosterConfig.levelUnlock = SonatSDKAdapter.GetRemoteInt($"level_unlock_{boosterConfig.booster}", boosterConfig.levelUnlock);

                var boosterData = new BoosterData
                {
                    boosterType = boosterConfig.booster,
                    unlocked = boosterConfig.levelUnlock == 0 || dataService.Instance.GetBool($"{boosterConfig.booster}_DATA", false),
                    quantity = inventoryService.Instance.GetResource(boosterConfig.booster)
                };

                boostersData.Add(boosterConfig.booster, boosterData);
                if (!boosterData.unlocked)
                {
                    if (boosterConfig.levelUnlock <= level)
                    {
                        UnlockBooster(boosterData.boosterType);
                    }
                    else
                    {
                        unlockAllBooster = false;
                    }
                }
            }

            inventoryService.Instance.OnResourceUpdate += OnResourceUpdated;
            new EventBinding<LevelStartedEvent>(OnLevelStarted);
            new EventBinding<LevelEndedEvent>(OnLevelEnded);
            //new EventBinding<LevelUpEvent>(OnLevelUp);

            timeToAdd = SonatSDKAdapter.GetRemoteInt("pre_booster_freeze_time", 10);
            numPreBoosterMagnet = SonatSDKAdapter.GetRemoteInt("pre_booster_magnet_number", 1);
        }

        #region EventHandle

        private void OnLevelStarted(LevelStartedEvent levelStartedEvent)
        {
            if (levelStartedEvent.gameMode != GameMode.Classic) return;
            UsePreBoosters();
        }

        private void OnLevelEnded(LevelEndedEvent levelEndedEvent)
        {
            if (levelEndedEvent.success == true)
            {
                unlockAllBooster = true;
                foreach (var data in boostersData)
                {
                    if (data.Value == null) continue;
                    if (!data.Value.unlocked)
                    {
                        if (GetBoosterConfig(data.Key)?.levelUnlock == levelEndedEvent.level + 1)
                        {
                            UnlockBooster(data.Value.boosterType);
                        }
                        else
                        {
                            unlockAllBooster = false;
                        }
                    }
                }
            }

            // reset used pre booster
            foreach (var booster in usedPreBooster)
            {
                switch (booster.Key)
                {
                    // case GameResource.PreBoosterFreeze:
                    //     inventoryService.Instance.AddResource(GameResource.BoosterFreeze, 1);
                    //     break;
                    // case GameResource.PreBoosterMagnet:
                    //     inventoryService.Instance.AddResource(GameResource.BoosterMagnet, 1);
                    //     break;
                    case GameResource.PreBoosterDoubleStar:
                        MySonatFramework.GetService<StarChestService>().SetMultiplier(1);
                        break;
                }
            }

            usedPreBooster.Clear();
        }

        private void OnResourceUpdated(GameResource resource)
        {
            if (resource == GameResource.MAX)
            {
                foreach (var boosterData in boostersData)
                {
                    boostersData[boosterData.Key].quantity = inventoryService.Instance.GetResource(boosterData.Key);
                }
            }
            else
            {
                if (boostersData.TryGetValue(resource, out var boosterData))
                {
                    boosterData.quantity = inventoryService.Instance.GetResource(resource);
                    //onUnlockBooster?.Invoke(resource);
                }
            }
        }

        #endregion

        public BoosterConfig GetBoosterConfig(GameResource boosterType)
        {
            if (configs.TryGetValue(boosterType, out var config)) return config;
            var defaultConfig = boostersConfig.configs.Find(e => e.booster == boosterType);
            config = SonatSDKAdapter.GetRemoteConfig($"{boosterType}_config", defaultConfig);
            configs.Add(boosterType, config);
            return config;
        }

        public bool BuyBooster(GameResource boosterType, int quantity)
        {
            var config = GetBoosterConfig(boosterType);
            if (!inventoryService.Instance.CanReduce(config.priceCurrency, config.price * quantity)) return false;
            var logSpend = new SpendResourceLogData
            {
                earnType = boosterType.ResourceType().ToString(),
                earnId = boosterType.ToString(),
                source = "non_iap"
            };
            inventoryService.Instance.ReduceResource(config.priceCurrency, config.price * quantity, logSpend);

            var logEarn = new EarnResourceLogData()
            {
                spendType = config.priceCurrency.ResourceType().ToString(),
                spendId = config.priceCurrency.ToString(),
                source = "non_iap"
            };

            AddBooster(boosterType, quantity, logEarn);
            return true;
        }

        public void AddBooster(GameResource boosterType, int quantity, EarnResourceLogData log)
        {
            inventoryService.Instance.AddResource(boosterType, quantity, log);
        }


        public bool IsBoosterUnlock(GameResource booster)
        {
            return GetBoosterData(booster).unlocked;
        }

        public void UnlockBooster(GameResource boosterType)
        {
            dataService.Instance.SetBool($"{boosterType}_DATA", true);
            boostersData[boosterType].unlocked = true;
            BoosterConfig config = GetBoosterConfig(boosterType);
            int quantity = config.defaultValue + boostersData[boosterType].quantity;
            boostersData[boosterType].quantity = quantity;
            inventoryService.Instance.SetResource(boosterType, quantity);

            SonatUtils.ExecuteNextFrame(() =>
            {
                LogEarnOnUnlock(boosterType, config.defaultValue);
            }, 10);

            onUnlockBooster?.Invoke(boosterType);
        }

        //public void TryLogEarnOnUnlock()
        //{
        //    foreach (var data in boostersData)
        //    {
        //        if (data.Value == null) continue;

        //        if (GetBoosterConfig(data.Key)?.levelUnlock <= UserData.GetLevel())
        //        {
        //            LogEarnOnUnlock(data.Value.boosterType, GetBoosterConfig(data.Value.boosterType).defaultValue);
        //        }
        //    }
        //}

        private void LogEarnOnUnlock(GameResource boosterType, int value)
        {
            var logData = new EarnResourceLogData()
            {
                spendType = "tutorial",
                spendId = "tutorial",
            };

            EarnResourceEvent eventData = new EarnResourceEvent(boosterType, value, logData);

            LogEarnCurrency(eventData);
        }

        public void LogEarnCurrency(EarnResourceEvent eventData)
        {
            var trackingService = SonatSystem.GetService<SonatTrackingService>();
            var gameplayAnalytics = SonatSystem.GetService<GameplayAnalyticsService>();

            GameMode gameMode = gameplayAnalytics.levelPlayData.gameMode;
            var gameModeLog = SonatUtils.ConvertToLogParam(gameMode.ToString());

#if sonat_sdk
            new SonatLogEarnVirtualCurrency()
            {
                virtual_currency_name = eventData.resource.ToLogString(),
                value = eventData.value,
                virtual_currency_type = eventData.resource.ResourceType().ToLogString(),
                spend_item_type = eventData.spendType.ToLogString(),
                spend_item_id = eventData.spendId.ToLogString(),
                level = MySonatFramework.userDataService.GetLevel(),
                source = eventData.source.ToLogString(),
                is_first_buy = eventData.isFirstBuy,
                mode = gameModeLog,
                placement = trackingService.placement,
                location = trackingService.location,
                screen = trackingService.screen
            }.Post();
#endif
        }

        public BoosterData GetBoosterData(GameResource boosterType)
        {
            if (boostersData.TryGetValue(boosterType, out var data)) return data;
            data = new BoosterData
            {
                boosterType = boosterType,
                unlocked = dataService.Instance.GetBool($"{boosterType}_DATA", false),
                quantity = inventoryService.Instance.GetResource(boosterType)
            };
            boostersData.Add(boosterType, data);
            return data;
        }


        public bool CanUseBooster(GameResource boosterType)
        {
            if (!IsBoosterUnlock(boosterType)) return false;
            return inventoryService.Instance.CanReduce(boosterType, 1);
        }


        public void UseBoosterSuccess(GameResource boosterType)
        {
            var logSpend = new SpendResourceLogData
            {
                earnType = "_",
                earnId = "_",
                source = "non_iap"
            };
            inventoryService.Instance.ReduceResource(boosterType, 1, logSpend);

            EventBus<UseBoosterEvent>.Raise(new UseBoosterEvent() { booster = boosterType });
        }

        public void PrepareBooster(GameResource boosterType)
        {
            usingBooster[boosterType] = true;
        }

        public void UsePreBoosters()
        {
            var list = new List<GameResource>();
            foreach (var booster in usingBooster)
            {
                if (booster.Value)
                {
                    list.Add(booster.Key);
                    usedPreBooster[booster.Key] = true;
                    UseBoosterSuccess(booster.Key);
                }
            }

            usingBooster.Clear();

            if (list.Count > 0)
            {
                var uiData = new UIData();
                uiData.Add("listPreBooster", list);
                // SonatUtils.DelayCall(1.5f, () => //thời gian load là 1.5s
                // {
                ShowPopupUsePreBooster(uiData).Forget();
                // });
            }
        }

        private async UniTask ShowPopupUsePreBooster(UIData uiData)
        {
            UIFlowController.isShowedEffectPreBooster = true;
            await UniTask.WaitUntil(() => UIFlowController.CheckConditionShowEffectPreBooster());
            PanelManager.Instance.OpenPanel<PopupUsePreBooster>(uiData);
        }

        public int GetTimeToAdd()
        {
            return timeToAdd;
        }

        public int GetNumPreBoosterMagnet()
        {
            return numPreBoosterMagnet;
        }
    }
}