#if sonat_sdk
using Sonat;
#endif
using System;
using System.Collections.Generic;
using System.Linq;
using Sonat.Enums;
#if sonat_sdk_v2
using Sonat.TrackingModule;
#endif
using SonatFramework.Scripts.Helper;
using SonatFramework.Scripts.SonatSDKAdapterModule;
using SonatFramework.Scripts.Utils;
using SonatFramework.Systems.EventBus;
using UnityEngine;

namespace SonatFramework.Systems.TrackingModule
{
    [CreateAssetMenu(fileName = "SonatTrackingService", menuName = "Sonat Services/Tracking Service")]
    public class SonatTrackingService : TrackingService, IServiceInitialize
    {
        [SerializeField] protected Service<GameplayAnalyticsService> gameplayAnalytics = new();
        [SerializeField] protected GameResourceTrackingData gameResourceTrackingData;
        protected Dictionary<string, GameResourceTrackingConfig> gameResourceTrackingConfigs = new(StringComparer.Ordinal);

        private bool initialized;

        public virtual void Initialize()
        {
            if (initialized) return;
            initialized = true;

            new EventBinding<LevelStartedEvent>(OnStartLevel);
            new EventBinding<LevelEndedEvent>(OnEndLevel);
            new EventBinding<LevelStuckEvent>(OnLevelStuck);
            new EventBinding<LevelContinueEvent>(OnPlayContinue);
            new EventBinding<PhaseStartedEvent>(OnStartPhase);
            new EventBinding<UseBoosterEvent>(OnUseBooster);
            new EventBinding<EarnResourceEvent>(LogEarnCurrency);
            new EventBinding<SpendResourceEvent>(LogSpendCurrency);

            new EventBinding<UpdatePlacementEvent>(OnUpdatePlacement);
            new EventBinding<UpdateScreenEvent>(OnUpdateScreen);
            new EventBinding<SwitchPlacementEvent>(OnSwitchPlacement);
            new EventBinding<ClickShortcutEvent>(OnClickShortcut);
            new EventBinding<LevelQuitEvent>(OnLevelQuit);
        }

        protected virtual void OnUpdatePlacement(UpdatePlacementEvent eventData)
        {
            this.placement = eventData.placement;
        }

        protected virtual void OnUpdateScreen(UpdateScreenEvent eventData)
        {
            this.screen = eventData.screen;
        }

        protected virtual void OnSwitchPlacement(SwitchPlacementEvent eventData)
        {
            switch (eventData.to)
            {
                case GamePlacement.Gameplay:
                    this.location = "IG";
                    break;
                case GamePlacement.Home:
                    this.location = "OG";
                    break;
                default:
                    this.location = "IG";
                    break;
            }
        }

        public override void OnShowPopup(string uiName, string uiType, string uiClass, string openBy, string action = "open")
        {
            var log = new SonatLogShowUi()
            {
                level = GetLevelLog(),
                location = this.location,
                placement = this.placement,
                screen = this.screen,
                ui_name = uiName,
                ui_type = uiType,
                ui_class = uiClass,
                open_by = openBy,
                action = action,
            };

            log.Post();
        }

        public override void TrackingScreenView()
        {
            var log = new SonatLogScreenView()
            {
                screen_name = $"{location}:{screen}:{placement}",
                level = GetLevelLog()
            };

            log.AddExtraParameter("location", this.location);
            log.AddExtraParameter("placement", this.placement);
            log.AddExtraParameter("screen", this.screen);
            log.Post();
        }


        protected virtual void OnStartLevel(LevelStartedEvent eventData)
        {
            SonatUtils.ExecuteNextFrame(LogLevelStartTurn);
            SonatUtils.ExecuteNextFrame(LogLevelStart);
        }

        protected virtual void OnStartPhase(PhaseStartedEvent eventData)
        {
        }

        protected virtual void OnEndLevel(LevelEndedEvent eventData)
        {
            SonatUtils.ExecuteNextFrame(() =>
            {
                LogLevelEndTurn();
                if (eventData.success)
                {
                    LogLevelEnd();
#if !sonat_sdk_v2
                int level = eventData.level;
                if ((level <= 30 || (level <= 100 && level % 10 == 0)))
                {
                    LogLevelComplete(level);
                }
#endif
                }
            });
        }


        protected virtual void OnLevelStuck(LevelStuckEvent eventData)
        {
            SonatUtils.ExecuteNextFrame(LogLevelEnd);
        }

        protected virtual void OnLevelQuit(LevelQuitEvent eventData)
        {
            SonatUtils.ExecuteNextFrame(LogLevelEnd);
        }

        protected virtual void OnPlayContinue(LevelContinueEvent eventData)
        {
            SonatUtils.ExecuteNextFrame(LogLevelStart);
        }

        protected virtual void OnUseBooster(UseBoosterEvent eventData)
        {
            SonatUtils.ExecuteNextFrame(() => { LogUseBooster(eventData.booster.ToString()); });
        }

        protected virtual void OnClickShortcut(ClickShortcutEvent eventData)
        {
#if sonat_sdk
            new SonatLogClickIconShortcut()
            {
                shortcut = eventData.shortcut,
                location = location,
                screen = screen,
                placement = placement
            }.Post();
#endif
        }


        public override void LogShowAds()
        {
            IntDataPref showAdsCount = new IntDataPref("SHOW_ADS_COUNT");
            showAdsCount.Value++;

            if (showAdsCount.Value == 1)
            {
                //SendEventAF("paid_ad_impression");
                SonatSDKAdapter.SendEventFireBase("paid_ad_impression");
            }
            else if (showAdsCount.Value <= 50 && showAdsCount.Value % 5 == 0)
            {
                //SendEventAF($"paid_ad_{showVideoCount}_impression");
                SonatSDKAdapter.SendEventFireBase($"paid_ad_{showAdsCount.Value:D2}_impression");
            }
        }


        public override void LogShowRewardAds()
        {
#if !sonat_sdk_v2
            IntDataPref ShowRWCount = new IntDataPref("SHOW_REWARD_COUNT");
            ShowRWCount.Value++;
            if (ShowRWCount.Value == 1)
            {
                SonatSDKAdapter.SendEventFireBase("complete_rwd");
            }
            else if (ShowRWCount.Value <= 15)
            {
                SonatSDKAdapter.SendEventFireBase($"complete_rwd_{ShowRWCount:D2}");
            }

            LogShowAds();
#endif
        }

        public override void LogShowInterAds()
        {
#if !sonat_sdk_v2
            IntDataPref showRWCount = new IntDataPref("SHOW_In_COUNT");
            showRWCount.Value++;
            if (showRWCount.Value == 1)
            {
                SonatSDKAdapter.SendEventFireBase("complete_rwd");
            }
            else if (showRWCount.Value <= 15)
            {
                SonatSDKAdapter.SendEventFireBase($"complete_rwd_{showRWCount:D2}");
                //if (ShowRWCount.Value == 7 && GameManager.DayPlay() == 0)
                //{
                //	SendEventFireBase($"complete_rwd_07_D0");
                //}
            }
#endif
        }

        public override void LogLevelComplete(int level)
        {
            SonatSDKAdapter.SendEventFireBase($"complete_level_{level:D3}");
        }

        public override void LogLevelStart()
        {
#if sonat_sdk
            new SonatLogLevelStart()
            {
                mode = GetGameModeLog(),
                level = GetLevelLog(),
                is_first_play = gameplayAnalytics.Instance.levelPlayData.isFirstPlay,
                continue_with = gameplayAnalytics.Instance.levelPlayData.continueWith,
                start_count = gameplayAnalytics.Instance.levelPlayData.startCount,
                continue_times = gameplayAnalytics.Instance.levelPlayData.startCount - 1,
            }.Post(true);
#endif
        }

        public override void LogLevelStartTurn()
        {
// #if sonat_sdk
//             new SonatLogLevelStartTurn()
//             {
//                 mode = GetGameModeLog(),
//                 level = GetLevelLog(),
//                 is_first_play = gameplayAnalytics.Instance.levelPlayData.isFirstPlay,
//                 continue_with = gameplayAnalytics.Instance.levelPlayData.continueWith,
//                 start_count = gameplayAnalytics.Instance.levelPlayData.startCount
//             }.Post(true);
// #endif
        }

        public override void LogLevelEnd()
        {
#if sonat_sdk
            new SonatLogLevelEnd()
            {
                level = GetLevelLog(),
                mode = GetGameModeLog(),
                use_booster_count = gameplayAnalytics.Instance.levelPlayData.useBoosterCount,
                play_time = (int)gameplayAnalytics.Instance.levelPlayData.FullTimePlayLevel,
                move_count = gameplayAnalytics.Instance.levelPlayData.moveCount,
                is_first_play = gameplayAnalytics.Instance.levelPlayData.isFirstPlay,
                lose_cause = gameplayAnalytics.Instance.levelPlayData.loseCause,
                success = gameplayAnalytics.Instance.levelPlayData.isWin,
                continue_with = gameplayAnalytics.Instance.levelPlayData.continueWith,
                start_count = gameplayAnalytics.Instance.levelPlayData.startCount,
            }.Post();
#endif
        }

        public override void LogLevelEndTurn()
        {
// #if sonat_sdk
//             new SonatLogLevelEndTurn()
//             {
//                 level = GetLevelLog(),
//                 mode = GetGameModeLog(),
//                 use_booster_count = gameplayAnalytics.Instance.levelPlayData.useBoosterCount,
//                 play_time = (int)gameplayAnalytics.Instance.levelPlayData.FullTimePlayLevel,
//                 move_count = gameplayAnalytics.Instance.levelPlayData.moveCount,
//                 is_first_play = gameplayAnalytics.Instance.levelPlayData.isFirstPlay,
//                 lose_cause = gameplayAnalytics.Instance.levelPlayData.loseCause,
//                 success = gameplayAnalytics.Instance.levelPlayData.isWin,
//                 continue_with = gameplayAnalytics.Instance.levelPlayData.continueWith,
//                 start_count = gameplayAnalytics.Instance.levelPlayData.startCount,
//             }.Post();
// #endif
        }

        public override void LogLevelUp()
        {
#if sonat_sdk
            new SonatLogLevelUp()
            {
                level = GetLevelLog(),
            }.Post(false);
#endif
        }

        public override void LogUseBooster(string boosterName)
        {
#if sonat_sdk
            new SonatLogUseBooster()
            {
                mode = GetGameModeLog(),
                level = GetLevelLog(),
                name = boosterName.ToLower()
            }.Post(true);
#endif
        }

        public override void LogShortcut(string shortcutName)
        {
#if sonat_sdk
            new SonatLogClickIconShortcut()
            {
                shortcut = shortcutName,
                placement = placement,
                location = location,
                screen = screen
            }.Post();
#endif
        }

        public override void LogEarnCurrency(EarnResourceEvent eventData)
        {
#if sonat_sdk
            GameResourceTrackingConfig trackingConfig = GetGameResourceTrackingConfig(eventData.resource.ToString());
            new SonatLogEarnVirtualCurrency()
            {
                virtual_currency_name = trackingConfig?.GetTrackingName() ?? eventData.resource.ToLogString(),
                value = eventData.value,
                virtual_currency_type = eventData.resource.ResourceType().ToLogString(),
                spend_item_type = eventData.spendType.ToLogString(),
                spend_item_id = eventData.spendId.ToLogString(),
                level = GetLevelLog(),
                source = eventData.source.ToLogString(),
                is_first_buy = eventData.isFirstBuy,
                mode = GetGameModeLog(),
                placement = placement,
                location = location,
                screen = screen,
                valueGameCurrency = trackingConfig?.GetValueCurrency(eventData.value) ?? 0,
            }.Post();
#endif
        }

        public void LogEarnCurrency(string currencyName, string currencyType, int value, string spendItemType,
            string spendItemId, string source, bool isFirstBuy, int price = 0)
        {
#if sonat_sdk
            GameResourceTrackingConfig trackingConfig = GetGameResourceTrackingConfig(currencyName);
            new SonatLogEarnVirtualCurrency()
            {
                virtual_currency_name = trackingConfig?.GetTrackingName() ?? currencyName,
                value = value,
                virtual_currency_type = currencyType,
                spend_item_type = spendItemType,
                spend_item_id = spendItemId,
                level = GetLevelLog(),
                source = source,
                is_first_buy = isFirstBuy,
                mode = GetGameModeLog(),
                placement = placement,
                location = location,
                screen = screen,
                valueGameCurrency = trackingConfig?.GetValueCurrency(value) ?? 0
            }.Post();
#endif
        }

        public void LogSpendCurrency(string currencyName, string currencyType, int value, string earnItemType,
            string earnItemId, int price = 0)
        {
#if sonat_sdk
            GameResourceTrackingConfig trackingConfig = GetGameResourceTrackingConfig(currencyName);
            new SonatLogSpendVirtualCurrency()
            {
                virtual_currency_name = trackingConfig?.GetTrackingName() ?? currencyName,
                virtual_currency_type = currencyType,
                value = value,
                earn_item_type = earnItemType,
                earn_item_id = earnItemId,
                level = GetLevelLog(),
                mode = GetGameModeLog(),
                placement = placement,
                location = location,
                screen = screen,
                valueGameCurrency = trackingConfig?.GetValueCurrency(value) ?? 0
            }.Post();
#endif
        }

        public override void LogSpendCurrency(SpendResourceEvent eventData)
        {
#if sonat_sdk
            GameResourceTrackingConfig trackingConfig = GetGameResourceTrackingConfig(eventData.resource.ToString());
            new SonatLogSpendVirtualCurrency()
            {
                virtual_currency_name = trackingConfig?.GetTrackingName() ?? eventData.resource.ToLogString(),
                virtual_currency_type = eventData.resource.ResourceType().ToLogString(),
                value = eventData.value,
                earn_item_type = eventData.earnType.ToLogString(),
                earn_item_id = eventData.earnId.ToLogString(),
                level = GetLevelLog(),
                mode = GetGameModeLog(),
                placement = placement,
                location = location,
                screen = screen,
                valueGameCurrency = trackingConfig?.GetValueCurrency(eventData.value) ?? 0,
            }.Post();
#endif
        }

        public void LogTutorialBegin(string placement, int step, int tutorialId)
        {
            var log = new SonatLogTutorialBegin(placement, step);
            log.AddExtraParameter("tutorial_id", tutorialId.ToString());
            log.Post();
        }

        public void LogTutorialComplete(string placement, int step, int tutorialId)
        {
            var log = new SonatLogTutorialComplete(placement, step);
            log.AddExtraParameter("tutorial_id", tutorialId.ToString());
            log.Post();
        }

        protected virtual int GetLevelLog()
        {
            int level = gameplayAnalytics.Instance.levelPlayData.level;
            return level;
        }

        protected virtual string GetGameModeLog()
        {
            GameMode gameMode = gameplayAnalytics.Instance.levelPlayData.gameMode;
            return SonatUtils.ConvertToLogParam(gameMode.ToString());
        }

        public virtual GameResourceTrackingConfig GetGameResourceTrackingConfig(string gameResourceName)
        {
            if (gameResourceTrackingData == null) return null;
            if (gameResourceTrackingConfigs.TryGetValue(gameResourceName, out GameResourceTrackingConfig config))
            {
                return config;
            }

            config = gameResourceTrackingData.gameResourceTrackingConfigs.FirstOrDefault(e => e.gameResourceName == gameResourceName);
            if (config == null) return null;
            gameResourceTrackingConfigs.Add(gameResourceName, config);
            return config;
        }
    }
}