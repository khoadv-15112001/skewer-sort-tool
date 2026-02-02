using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using MyFramework.Sonat;
using Sonat.AdsModule;
using Sonat.Data;
using Sonat.Enums;
using Sonat.IapModule;
using Sonat.TrackingModule;
using SonatFramework.Scripts.Utils;
using SonatFramework.Systems.EventBus;
using SonatFramework.Systems.TrackingModule;
using UnityEngine;

namespace Sonat.CustomService
{
    [CreateAssetMenu(fileName = "CustomTrackingService", menuName = "Custom Services/Tracking Service")]
    public class CustomTrackingService : SonatTrackingService
    {
        [SerializeField] private HLWTrackingService hlwTrackingService;
        private int levelTime;
        private int orderComplete;
        private int completion;
        private int numberOfOrders;
        private int orderCount;
        private List<StringBuilder> levelFlows;
        public int category;
        public HLWTrackingService HLWTrackingService => hlwTrackingService;
        private PlayerPrefInt useBoosterCount;
        private int rwdInLevel;
        private int interInLevel = 0;

        public override void Initialize()
        {
            base.Initialize();
            hlwTrackingService.Init();
            GameplayController.OnCollectItem += OnCollectItem;
            useBoosterCount = new PlayerPrefInt("UseBoosterCount", 0);
            SonatAds.OnAdShowComplete += OnShowAdsComplete;
        }

        protected override void OnStartLevel(LevelStartedEvent eventData)
        {
            base.OnStartLevel(eventData);
            levelTime = GameplayController.instance.levelGenerator.LevelData.time;
            numberOfOrders = GameplayController.instance.levelGenerator.MaxOrder;
            orderComplete = 0;
            completion = 0;
            orderCount = 0;
            rwdInLevel = 0;
            interInLevel = 0;
            levelFlows = new List<StringBuilder>();
            levelFlows.Add(new StringBuilder());
        }

        private void OnCollectItem(int id)
        {
            completion++;
            AddLevelFlows(id.ToString());
        }

        public void OnCompleteOrder()
        {
            orderComplete++;
        }

        public void OnOrderAppear()
        {
            orderCount++;
        }

        private void OnShowAdsComplete(AdPlacement adPlacement)
        {
            switch (adPlacement)
            {
                case AdPlacement.Rewarded:
                {
                    rwdInLevel++;
                    if (useBoosterCount.Value > 8 && UserData.RewardedCount.Value > 8)
                    {
                        LogTrackingBoosterRwd(8, 8);
                    }

                    break;
                }
                case AdPlacement.Interstitial:
                    interInLevel++;
                    break;
            }
        }

        protected override void OnUseBooster(UseBoosterEvent eventData)
        {
            base.OnUseBooster(eventData);
            AddLevelFlows($"b{(byte)eventData.booster}");
            useBoosterCount.Value++;
            
            if (useBoosterCount.Value > 8 && UserData.RewardedCount.Value > 8)
            {
                LogTrackingBoosterRwd(8, 8);
            }
        }

        private void AddLevelFlows(string flow)
        {
            levelFlows ??= new List<StringBuilder>() { new StringBuilder() };
            levelFlows[^1].Append(flow);
            levelFlows[^1].Append(".");
            if (levelFlows[^1].Length > 98)
            {
                levelFlows.Add(new StringBuilder());
            }
        }

        public int GetTimeRemaining()
        {
            if (GameplayController.instance != null)
            {
                return (int)GameplayController.instance.timeManager.GetTimeRemaining();
            }

            return 0;
        }

        public int GetTimePlayLevel()
        {
            if (GameplayController.instance != null)
            {
                return (int)GameplayController.instance.timeManager.GetTimeRunning();
            }

            return 0;
        }

        public override void LogLevelStart()
        {
#if sonat_sdk
            var log = new SonatLogLevelStart()
            {
                mode = GetGameModeLog(),
                level = GetLevelLog(),
                is_first_play = gameplayAnalytics.Instance.levelPlayData.isFirstPlay,
                continue_with = gameplayAnalytics.Instance.levelPlayData.continueWith,
                start_count = gameplayAnalytics.Instance.levelPlayData.startCount,
            };

            double completionPercent = Math.Round(completion * 100.0f / numberOfOrders, 2);
            log.AddExtraParameter("completion", completionPercent.ToString(CultureInfo.InvariantCulture));
            log.AddExtraParameter("id", category.ToString());
            
            log.Post(true);
#endif
        }

        public override void LogLevelEnd()
        {
#if sonat_sdk
            var log = new SonatLogLevelEnd()
            {
                level = GetLevelLog(),
                mode = GetGameModeLog(),
                use_booster_count = gameplayAnalytics.Instance.levelPlayData.useBoosterCount,
                play_time = GetTimePlayLevel(),
                move_count = gameplayAnalytics.Instance.levelPlayData.moveCount,
                is_first_play = gameplayAnalytics.Instance.levelPlayData.isFirstPlay,
                lose_cause = gameplayAnalytics.Instance.levelPlayData.loseCause,
                success = gameplayAnalytics.Instance.levelPlayData.isWin,
                continue_with = gameplayAnalytics.Instance.levelPlayData.continueWith,
                start_count = gameplayAnalytics.Instance.levelPlayData.startCount,
            };


            log.AddExtraParameter("default_time", levelTime.ToString());
            log.AddExtraParameter("order_completed", orderComplete.ToString());

            double completionPercent = Math.Round(completion * 100.0f / numberOfOrders, 2);
            log.AddExtraParameter("completion", completionPercent.ToString(CultureInfo.InvariantCulture));
            log.AddExtraParameter("id", category.ToString());

            if (levelFlows != null)
            {
                for (int i = 0; i < levelFlows.Count; i++)
                {
                    log.AddExtraParameter($"flow{i + 1}", levelFlows[i].ToString());
                }
            }

            log.Post();
            orderComplete = 0;
#endif
        }

        protected override void OnEndLevel(LevelEndedEvent eventData)
        {
            base.OnEndLevel(eventData);
            if (eventData.success)
            {
                TrackingLevelBoosterRwd(eventData.level);
            }
        }

        public void OnOrderEnd(string success, int timeOrder, int timeComplete, float orderCompletion, int useBoosterCount)
        {
            float completionPercent = completion * 100.0f / numberOfOrders;
            var log = new SonatLogOrderEnd()
            {
                level = GetLevelLog(),
                mode = GetGameModeLog(),
                remainTime = GetTimeRemaining(),
                completeTime = timeComplete,
                completion = completionPercent,
                orderNumber = orderCount,
                success = success,
                placement = placement,
                orderCompletion = orderCompletion,
                defaultTime = timeOrder,
                useBoosterCount = useBoosterCount,
                startCount = gameplayAnalytics.Instance.levelPlayData.startCount,
                playTime = GetTimePlayLevel()
            };

            log.Post();
        }

        public override void LogSpendCurrency(SpendResourceEvent eventData)
        {
#if sonat_sdk
            var log = new SonatLogSpendVirtualCurrency()
            {
                virtual_currency_name = eventData.resource.ToLogString(),
                virtual_currency_type = eventData.resource.ResourceType().ToLogString(),
                value = eventData.value,
                earn_item_type = eventData.earnType.ToLogString(),
                earn_item_id = eventData.earnId.ToLogString(),
                level = GetLevelLog(),
                mode = GetGameModeLog(),
                placement = placement,
                location = location,
                screen = screen
            };

            float completionPercent = completion * 100.0f / numberOfOrders;
            log.AddExtraParameter("completion", completionPercent.ToString(CultureInfo.InvariantCulture));
            log.Post();
#endif
        }

        public void LogLevelStartFeature(string mode, string type, int start_count, int continue_count, Dictionary<string, string> extraParams = null)
        {
#if sonat_sdk
            var log = new SonatLogStartFeature()
            {
                level = MySonatFramework.userDataService.GetLevel(),
                mode = mode,
                start_count = start_count
            };

            log.AddExtraParameter("type", type);
            log.AddExtraParameter("continue_count", continue_count.ToString());

            if (extraParams != null)
            {
                foreach (var extraParam in extraParams)
                {
                    log.AddExtraParameter(extraParam.Key, extraParam.Value);
                }
            }

            log.Post();
#endif
        }

        public void LogLevelProgressFeature(string mode, string type, int start_count, int continue_count, string completion,
            string percentage_claim_resource, string milestone, int rank, int day_active = -1, Dictionary<string, string> extraParams = null)
        {
#if sonat_sdk
            var log = new SonatLogProgressFeature()
            {
                level = MySonatFramework.userDataService.GetLevel(),
                mode = mode,
                start_count = start_count,
            };

            log.AddExtraParameter("type", type);
            log.AddExtraParameter("continue_count", continue_count.ToString());
            log.AddExtraParameter("completion", completion.ToString(CultureInfo.InvariantCulture));
            log.AddExtraParameter("percentage_claim_resource", percentage_claim_resource);
            log.AddExtraParameter("milestone", !string.IsNullOrEmpty(milestone) ? milestone : "");
            log.AddExtraParameter("rank", rank.ToString());
            log.AddExtraParameter("day_active", day_active == -1 ? "" : day_active.ToString());

            if (extraParams != null)
            {
                foreach (var extraParam in extraParams)
                {
                    log.AddExtraParameter(extraParam.Key, extraParam.Value);
                }
            }

            log.Post();
#endif
        }

        public void LogLevelEndFeature(string mode, string type, int start_count, int continue_count, bool success, string completion,
            string percentage_claim_resource, string milestone, int rank, string end_cause, int day_active = -1, Dictionary<string, string> extraParams = null)
        {
#if sonat_sdk
            var log = new SonatLogEndFeature()
            {
                level = MySonatFramework.userDataService.GetLevel(),
                mode = mode,
                start_count = start_count,
                success = success
            };

            log.AddExtraParameter("type", type);
            log.AddExtraParameter("continue_count", continue_count.ToString());
            log.AddExtraParameter("completion", completion.ToString(CultureInfo.InvariantCulture));
            log.AddExtraParameter("percentage_claim_resource", percentage_claim_resource);
            log.AddExtraParameter("milestone", !string.IsNullOrEmpty(milestone) ? milestone : "");
            log.AddExtraParameter("rank", rank.ToString());
            log.AddExtraParameter("end_cause", end_cause);
            log.AddExtraParameter("day_active", day_active == -1 ? "" : day_active.ToString());

            if (extraParams != null)
            {
                foreach (var extraParam in extraParams)
                {
                    log.AddExtraParameter(extraParam.Key, extraParam.Value);
                }
            }

            log.Post();
#endif
        }

        protected override void OnPlayContinue(LevelContinueEvent eventData)
        {
            base.OnPlayContinue(eventData);
#if sonat_sdk
            new SonatLogEarnVirtualCurrency()
            {
                virtual_currency_name = "booster_revive",
                value = 1,
                virtual_currency_type = "booster",
                spend_item_type = eventData.by == "play_on_ads" ? "rwd" : "currency",
                spend_item_id = eventData.by == "play_on_ads" ? "rwd" : "coin",
                level = GetLevelLog(),
                source = "non_iap",
                is_first_buy = false,
                mode = GetGameModeLog(),
                placement = placement,
                location = location,
                screen = screen
            }.Post();

            new SonatLogSpendVirtualCurrency()
            {
                virtual_currency_name = "booster_revive",
                virtual_currency_type = "booster",
                value = 1,
                earn_item_type = "",
                earn_item_id = "",
                level = GetLevelLog(),
                mode = GetGameModeLog(),
                placement = placement,
                location = location,
                screen = screen
            }.Post();

#endif
        }

        private static List<int> levelTrackingBoosterRwd = new List<int>() { 6, 13, 15, 24, 40 };

        private void TrackingLevelBoosterRwd(int level)
        {
            if (level == 6)
            {
                if (gameplayAnalytics.Instance.levelPlayData.useBoosterCount > 1)
                {
                    LogTrackingLevelBooster(level, 1);
                }

                if (gameplayAnalytics.Instance.levelPlayData.useBoosterCount > 3 && rwdInLevel > 3)
                {
                    LogTrackingLevelBoosterRwd(level, 3, 3);
                }

                if (gameplayAnalytics.Instance.levelPlayData.useBoosterCount > 4 && rwdInLevel > 2)
                {
                    LogTrackingLevelBoosterRwd(level, 4, 2);
                }

                if (rwdInLevel > 10 && interInLevel > 3)
                {
                    LogTrackingLevelRwdInter(level, 10, 3);
                }

                return;
            }

            if (level == 13 && gameplayAnalytics.Instance.levelPlayData.useBoosterCount > 3 && rwdInLevel > 3)
            {
                LogTrackingLevelBoosterRwd(level, 3, 3);
                return;
            }

            if (level == 15 && gameplayAnalytics.Instance.levelPlayData.useBoosterCount > 0 && rwdInLevel > 0)
            {
                LogTrackingLevelBoosterRwd(level, 0, 0);
                return;
            }

            if (level == 24)
            {
                if (gameplayAnalytics.Instance.levelPlayData.useBoosterCount > 5 && rwdInLevel > 3)
                {
                    LogTrackingLevelBoosterRwd(level, 5, 3);
                }

                if (gameplayAnalytics.Instance.levelPlayData.useBoosterCount > 4 && rwdInLevel > 2)
                {
                    LogTrackingLevelBoosterRwd(level, 4, 2);
                }

                if (gameplayAnalytics.Instance.levelPlayData.useBoosterCount > 1 && rwdInLevel > 2)
                {
                    LogTrackingLevelBoosterRwd(level, 1, 2);
                }

                if (gameplayAnalytics.Instance.levelPlayData.useBoosterCount > 0 && rwdInLevel > 0)
                {
                    LogTrackingLevelBoosterRwd(level, 0, 0);
                }

                return;
            }

            if (level == 40 && gameplayAnalytics.Instance.levelPlayData.useBoosterCount > 2 && rwdInLevel > 2)
            {
                LogTrackingLevelBoosterRwd(level, 2, 2);
                return;
            }
        }

        private void LogTrackingLevelBoosterRwd(int level, int boosterCount, int rwdCount)
        {
            var value = SonatAnalyticTracker.sn_ltv_iaa + SonatIap.sn_ltv_iap;
            var eventName = $"complete_level_{level}_use_booster_{boosterCount}_rwd_{rwdCount}";
            var log = new CustomSonatLog(eventName, new List<LogParameter>()
            {
                new LogParameter("value", value.ToString(CultureInfo.InvariantCulture)),
            });
            
            Debug.Log($"<color=lime>[TRACKING] ✉️ {eventName}</color> | value: {value:F2}");
            
            log.Post();
        }

        private void LogTrackingLevelBooster(int level, int boosterCount)
        {
            var value = SonatAnalyticTracker.sn_ltv_iaa + SonatIap.sn_ltv_iap;
            var eventName = $"complete_level_{level}_use_booster_{boosterCount}";
            var log = new CustomSonatLog(eventName, new List<LogParameter>()
            {
                new LogParameter("value", value.ToString(CultureInfo.InvariantCulture)),
            });
            
            Debug.Log($"<color=lime>[TRACKING] ✉️ {eventName}</color> | value: {value:F2}");
            
            log.Post();
        }

        private void LogTrackingLevelRwdInter(int level, int rwdCount, int interCount)
        {
            var value = SonatAnalyticTracker.sn_ltv_iaa + SonatIap.sn_ltv_iap;
            var eventName = $"complete_level_{level}_rwd_{rwdCount}_inter_{interCount}";
            var log = new CustomSonatLog(eventName, new List<LogParameter>()
            {
                new LogParameter("value", value.ToString(CultureInfo.InvariantCulture)),
            });
            
            Debug.Log($"<color=lime>[TRACKING] ✉️ {eventName}</color> | value: {value:F2}");
            
            log.Post();
        }
        
        private void LogTrackingBoosterRwd(int boosterCount, int rwdCount)
        {
            if(PlayerPrefs.HasKey("TrackingBoosterReward")) return;
            PlayerPrefs.SetInt("TrackingBoosterReward", 1);
            var value = SonatAnalyticTracker.sn_ltv_iaa + SonatIap.sn_ltv_iap;
            var eventName = $"use_booster_{boosterCount}_rwd_{rwdCount}";
            var log = new CustomSonatLog(eventName, new List<LogParameter>()
            {
                new LogParameter("value", value.ToString(CultureInfo.InvariantCulture)),
            });
            
            Debug.Log($"<color=lime>[TRACKING] ✉️ {eventName}</color> | value: {value:F2}");
            
            log.Post();
        }
    }


    [Serializable]
    public class SonatLogProgressFeature : SonatLogBasicMode
    {
        public int start_count;
        public override string EventName => "level_progress_feature";

        protected override List<LogParameter> GetParameters()
        {
            var parameters = base.BaseLogs();
            parameters.Add(new LogParameter(ParameterEnum.start_count.ToString(), start_count));
            return parameters;
        }
    }
}