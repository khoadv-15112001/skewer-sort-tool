using System.Collections.Generic;
using Newtonsoft.Json;
using Sirenix.OdinInspector;
using Sonat.Debugger;
using Sonat.Enums;
using Sonat.FirebaseModule;
using SonatFramework.Scripts.SonatSDKAdapterModule;
using SonatFramework.Systems.EventBus;
using SonatFramework.Systems.GameDataManagement;
using SonatFramework.Systems.InventoryManagement;
using SonatFramework.Systems.UserData;
using UnityEngine;

namespace SonatFramework.Systems.BoosterManagement
{
    [CreateAssetMenu(fileName = "SonatBoosterService", menuName = "Sonat Services/Booster Service")]
    public class SonatBoosterService : BoosterService, IServiceWaitingSDKInitialize
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

        private string configKey;
        private bool allBoosterUnlocked = false;

        private readonly Dictionary<GameResource, BoosterConfig> configs = new();


        public void OnSonatSDKInitialize()
        {
            int level = userDataService.Instance.GetLevel();
            allBoosterUnlocked = true;
            foreach (var boosterConfig in boostersConfig.configs)
            {
                var currentConfig = GetBoosterConfig(boosterConfig.booster);
                var boosterData = new BoosterData
                {
                    boosterType = currentConfig.booster,
                    unlocked = currentConfig.levelUnlock == 0 || dataService.Instance.GetBool($"{currentConfig.booster}_DATA", false),
                    quantity = inventoryService.Instance.GetResource(currentConfig.booster)
                };
                boostersData.Add(currentConfig.booster, boosterData);
                if (!boosterData.unlocked)
                {
                    if (currentConfig.levelUnlock < level)
                    {
                        UnlockBooster(boosterData.boosterType);
                    }
                    else
                    {
                        allBoosterUnlocked = false;
                    }
                }
            }

            inventoryService.Instance.OnResourceUpdate += OnResourceUpdated;
            new EventBinding<LevelStartedEvent>(OnLevelStarted);
        }

        #region EventHandle

        private void OnLevelStarted(LevelStartedEvent levelStartedEvent)
        {
            if (allBoosterUnlocked || levelStartedEvent.gameMode != GameMode.Classic) return;
            allBoosterUnlocked = true;
            foreach (var data in boostersData)
            {
                if (!data.Value.unlocked)
                {
                    if (configs[data.Key].levelUnlock == levelStartedEvent.level)
                        UnlockBooster(data.Value.boosterType);
                    else
                    {
                        allBoosterUnlocked = false;
                    }
                }
            }
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

        public override BoosterConfig GetBoosterConfig(GameResource boosterType)
        {
            if (configs.TryGetValue(boosterType, out var config)) return config;
            var defaultConfig = boostersConfig.configs.Find(e => e.booster == boosterType);
            config = SonatSDKAdapter.GetRemoteConfig($"{boosterType}_config", defaultConfig);

            // // sử dụng BoosterConfigConverter cho BoosterConfig thì gọi luôn
            // config = SonatSDKAdapter.GetValueBySegment($"{boosterType}_config_by_segment", Sonat.Data.UserData.UserCampaignSegment.Value, config);

            // Để không làm ảnh hưởng key cũ thì chỉ làm dùng BoosterConfigConverter trong key segment này
            var converter = new BoosterConfigConverter();
            var serializedConfig = JsonConvert.SerializeObject(config, converter);
            var strConfig = SonatSDKAdapter.GetValueBySegment(
                $"{boosterType}_config_by_segment",
                Sonat.Data.UserData.UserCampaignSegment.Value,
                serializedConfig
            );
            config = JsonConvert.DeserializeObject<BoosterConfig>(strConfig, converter);
            SonatDebugType.RemoteConfig.Log($"boosterType: {boosterType}, config(json): {config.ToString()}");

            config.booster = boosterType;
            configs.Add(boosterType, config);
            return config;
        }

        public override bool BuyBooster(GameResource boosterType, int quantity)
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
                source = "non_iap",
                price = config.price
            };

            AddBooster(boosterType, quantity, logEarn);
            return true;
        }

        public override void AddBooster(GameResource boosterType, int quantity, EarnResourceLogData log)
        {
            inventoryService.Instance.AddResource(boosterType, quantity, log);
        }


        public override bool IsBoosterUnlock(GameResource booster)
        {
            return GetBoosterData(booster).unlocked;
        }

        public override void UnlockBooster(GameResource boosterType)
        {
            dataService.Instance.SetBool($"{boosterType}_DATA", true);
            boostersData[boosterType].unlocked = true;
            BoosterConfig config = GetBoosterConfig(boosterType);
            int quantity = config.defaultValue + boostersData[boosterType].quantity;
            boostersData[boosterType].quantity = quantity;
            inventoryService.Instance.SetResource(boosterType, quantity);
            onUnlockBooster?.Invoke(boosterType);
            LogEarnOnUnlock(boosterType, config.defaultValue);
        }

        private void LogEarnOnUnlock(GameResource boosterType, int value)
        {
            var logData = new EarnResourceLogData()
            {
                spendType = "unlock",
                spendId = "unlock",
            };
            EarnResourceEvent eventData = new EarnResourceEvent(boosterType, value, logData);
            EventBus<EarnResourceEvent>.Raise(eventData);
        }

        public override BoosterData GetBoosterData(GameResource boosterType)
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


        public override bool CanUseBooster(GameResource boosterType)
        {
            if (!IsBoosterUnlock(boosterType)) return false;
            return inventoryService.Instance.CanReduce(boosterType, 1);
        }


        public override void UseBoosterSuccess(GameResource boosterType)
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

    }


    public class BoosterData
    {
        public GameResource boosterType;
        public int quantity;
        public bool unlocked;
    }
}