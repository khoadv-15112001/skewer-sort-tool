#if sonat_sdk_v2
using System;
using System.Collections.Generic;
using Sonat.DebugViewModule;
using Sonat.Enums;
using Sonat.TrackingModule;
using SonatFramework.Scripts.Utils;
using SonatFramework.Systems;
using SonatFramework.Systems.GameDataManagement;
using SonatFramework.Systems.LevelManagement;
using SonatFramework.Systems.TrackingModule;
using SonatFramework.Systems.UserData;
#if sonat_sdk
using Sonat;
using Sonat.AdsModule;
using Sonat.AppsFlyerModule;
using Sonat.Data;
using Sonat.FirebaseModule;
using Sonat.IapModule;
using SonatFramework.Scripts.UIModule;
#endif

namespace SonatFramework.Scripts.SonatSDKAdapterModule
{
    public static class SonatSDKAdapter
    {
        private static readonly Service<TrackingService> trackingService = new();
        private static readonly Service<GameplayAnalyticsService> gameplayAnalytics = new();
        private static readonly Service<UserDataService> userDataService = new();
        private static readonly Service<DataService> dataService = new();
        private static LevelShowInterDaily levelShowInterDaily = new();

        public static bool IsInternetConnection()
        {
#if sonat_sdk
            return SonatSdkManager.IsInternetConnection();
#else
            return Application.internetReachability != NetworkReachability.NotReachable;
#endif
        }

        public static void SetExternalConditionShowAds(Func<AdPlacement, bool> condition)
        {
#if sonat_sdk
            SonatAds.externConditionShowAds = condition;
#endif
        }

        public static void SetCheckDailyShowAds()
        {
#if sonat_sdk
            SonatAds.externConditionShowAds = CheckLevelDailyShowAds;
#endif
        }

        private static bool CheckLevelDailyShowAds(AdPlacement adPlacement)
        {
            switch (adPlacement)
            {
                case AdPlacement.Interstitial:
                    return levelShowInterDaily.CheckLevelDaily();
                default:
                    return true;
            }
        }

        public static void ShowRewardAds(Action onWatchedAds, string itemType = "", string itemId = "")
        {
#if sonat_sdk
            var log = new SonatLogVideoRewarded()
            {
                placement = trackingService.Instance.placement,
                level = gameplayAnalytics.Instance.levelPlayData.level,
                mode = gameplayAnalytics.Instance.levelPlayData.gameMode.ToString().ToLogString(),
                item_type = itemType,
                item_id = itemId
            };

            SonatAds.ShowRewardedAds((success) =>
            {
                if (success)
                {
                    onWatchedAds?.Invoke();
                }
                else
                {
                    PopupToast.Cretate("No video available!");
                }
            }, log);

#else
            onWatchedAds?.Invoke();
#endif
        }

        public static bool IsRewardAdsReady()
        {
#if sonat_sdk
            return SonatAds.IsAdsReady(AdPlacement.Rewarded);
#else
            return false;
#endif
        }

        public static bool CanShowInterAds()
        {
#if sonat_sdk
            return SonatAds.CanShowAds(AdPlacement.Interstitial);
#else
            return false;
#endif
        }

        public static bool ShowInterAds(string plecement, Action callback = null)
        {
#if sonat_sdk
            int level = SonatSystem.GetService<UserDataService>().GetLevel();
            if (level < GetValueBySegment($"level_start_show_interstitial_segment", UserData.UserCampaignSegment.Value, 9))
            {
                callback?.Invoke();
                return false;
            }

            var log = new SonatLogShowInterstitial()
            {
                placement = plecement
            };
            bool showed = SonatAds.ShowInterstitial((success) => { callback?.Invoke(); }, log);
            return showed;
            //else callback?.Invoke();
#else
            callback?.Invoke();
            return false;
#endif
        }

        public static void SetBanner(bool state)
        {
#if sonat_sdk
            if (state)
                SonatAds.ShowBanner();
            else
                SonatAds.HideAd(AdPlacement.Banner);
#endif
        }

        public static bool IsNativeAdsReady()
        {
#if sonat_sdk
            return SonatAds.IsAdsReady(AdPlacement.Native);
#endif
            return false;
        }

        public static void ShowNativeAds()
        {
#if sonat_sdk

            SonatAds.ShowNative();
#endif
        }

        public static void HideNavtiveAds()
        {
#if sonat_sdk
            SonatAds.HideAd(AdPlacement.Native);
#endif
        }

        private static ShopItemKey packBuying = ShopItemKey.None;

        public static void BuyPack(ShopItemKey iapKey, Action<bool> callback, string item_type)
        {
            if (!IsPurchaserInited()) return;

#if sonat_sdk
            if (packBuying != ShopItemKey.None)
            {
                //PopupToast.Cretate("buy_processing");
                return;
            }

            packBuying = iapKey;

            SonatBuyLogData log = new SonatBuyLogData(trackingService.Instance.placement, item_type, 1);
            log.screen = trackingService.Instance.screen;
            log.location = trackingService.Instance.location;
            PopupLoadingIap popupLoadingIap = PanelManager.Instance.OpenPanel<PopupLoadingIap>();
            SonatIap.Buy((int)iapKey, (success) =>
            {
                packBuying = ShopItemKey.None;
                if (success)
                {
                    callback.Invoke(true);
                    //OnBuyPack?.Invoke(iapKey);
                }
                else
                {
                    popupLoadingIap?.Close();
                    callback?.Invoke(false);
                    //OnCancelShopItem();
                }
            }, log);
#else
            callback?.Invoke(true);
#endif
        }

        public static void SetNoAds(bool noAds)
        {
#if sonat_sdk
            SonatAds.SetNoAds(noAds);
#endif
        }

        public static bool IsNoads()
        {
#if sonat_sdk
            return SonatAds.IsNoAds();
#else
            return false;
#endif
        }

        public static bool IsAppOpenAdsReady()
        {
#if sonat_sdk
            return SonatAds.IsAdsReady(AdPlacement.AppOpen);
#else
            return false;
#endif
        }

        public static void ShowAppOpenAds()
        {
#if sonat_sdk
            SonatAds.ShowAd(AdPlacement.AppOpen, "app_open", null, null);
#endif
        }

        public static bool CheckPackBought(ShopItemKey packId)
        {
#if sonat_sdk
            return SonatIap.CheckItemBought((int)packId);
#else
            return false;
#endif
        }

        public static List<int> GetPacksPurchased()
        {
#if sonat_sdk
            return SonatIap.GetPackPurchased();
#else
            return null;
#endif
        }


        public static void SendEventAf(string s, Dictionary<string, string> pairs)
        {
#if sonat_sdk
            SonatAppsFlyer.SendEvent(s, pairs);
#endif
        }

        public static void SendEventAf(string s)
        {
#if sonat_sdk
            SonatAppsFlyer.SendEvent(s);
#endif
        }

        public static void SendEventFireBase(string s, bool logAf = true)
        {
#if sonat_sdk
            SonatFirebase.analytic.LogEvent(s);
            if (logAf)
            {
                SendEventAf(s);
            }

            OnScreenDebugLog.Log("LogEvent:" + s + "\t ");
#endif
        }

        public static string FindProductId(ShopItemKey iapKey)
        {
#if sonat_sdk
            var product = SonatIap.GetStoreProductDescriptor((int)iapKey);
            if (product != null)
            {
                return product.StoreProductId;
            }

            return iapKey.ToString();
#else
            return "";
#endif
        }

        public static ShopItemKey ConvertToProductID(string packageName)
        {
#if sonat_sdk
            var key = SonatIap.ConvertProductIDToShopKey(packageName);
            if (key >= 0)
            {
                return (ShopItemKey)key;
            }

            return ShopItemKey.None;
#else
            return ShopItemKey.None;
#endif
        }

        public static string GetProductPrice(ShopItemKey key)
        {
#if sonat_sdk
            return SonatIap.GetPriceText((int)key);
#else
            return "0$";
#endif
        }

        public static int GetRemoteInt(string key, int defaultValue = 0)
        {
#if sonat_sdk
            int value = SonatFirebase.remote.GetRemoteInt(key, defaultValue);
#else
            int value = defaultValue;
#endif
            return value;
        }

        public static float GetRemoteFloat(string key, float defaultValue = 0)
        {
#if sonat_sdk
            float value = SonatFirebase.remote.GetRemoteFloat(key, defaultValue);
#else
            float value = 0;
#endif
            return value;
        }

        public static bool GetRemoteBool(string key, bool defaultValue = false)
        {
#if sonat_sdk
            bool value = SonatFirebase.remote.GetRemoteBool(key, defaultValue);
#else
            bool value = true;
#endif
            return value;
        }

        public static string GetRemoteString(string key, string defaultValue = "")
        {
#if sonat_sdk
            string value = SonatFirebase.remote.GetRemoteString(key, defaultValue);
#else
            string value = "";
#endif
            return value;
        }

        public static T GetRemoteConfig<T>(string key, T defaultValue = default(T))
        {
#if sonat_sdk
            return SonatFirebase.remote.GetRemoteConfig<T>(key, defaultValue);
#else
            return defaultValue;
#endif
        }

        public static T GetValueByLevel<T>(string key, T defaultValue = default(T))
        {
#if sonat_sdk
            return SonatFirebase.remote.GetValueByLevel(key, defaultValue);
#else
            return defaultValue;
#endif
        }

        public static T GetValueByLevel<T>(string key, int level, T defaultValue = default(T))
        {
#if sonat_sdk
            return SonatFirebase.remote.GetValueByLevel(key, level, defaultValue);
#else
            return defaultValue;
#endif
        }

        public static T GetValueByLevelSegment<T>(string key, string segment, int level, T defaultValue = default(T))
        {
#if sonat_sdk
            return SonatFirebase.remote.GetValueByLevelSegment(key, segment, level, defaultValue);
#else
            return defaultValue;
#endif
        }
        
        public static T GetValueBySegment<T>(string key, string segment, T defaultValue = default(T))
        {
#if sonat_sdk
            return SonatFirebase.remote.GetValueBySegment(key, segment, defaultValue);
#else
            return defaultValue;
#endif
        }


        public static bool IsPurchaserInited()
        {
#if sonat_sdk
            return SonatSdkManager.SonatServices.GetService<SonatIap>().Ready;
#else
            return false;
#endif
        }

        public static void Restore(Action<List<int>> onComplete)
        {
            if (!IsPurchaserInited()) return;
#if sonat_sdk
            SonatIap.RestorePurchases(onComplete);
#endif
        }

        public static T DownLoadLevel<T>(int level, GameMode gameMode, Func<int, GameMode, T> defaultFunction) where T : LevelData
        {
// #if sonat_sdk && !UNITY_STANDALONE_WIN && !UNITY_EDITOR
// 			return Kernel.Resolve<DownloadLevelManager>().GetData<T>(level, gameMode, defaultFunction);
// #else
            return defaultFunction?.Invoke(level, gameMode);
//#endif
        }
    }

    public class LevelShowInterDaily
    {
        private Dictionary<int, int> levelShowInter;
        private bool hasShowInter = false;
        private bool initialized = false;

        private void Initialize()
        {
            if (initialized) return;
            initialized = true;
            //if (SonatFirebase.remote.RemoteHasValue("level_show_inter_daily"))
            levelShowInter = SonatSDKAdapter.GetRemoteConfig<Dictionary<int, int>>("level_show_inter_daily", null);
            //else
            //{
            //  levelShowInter = null;
            //}
        }

        public bool CheckLevelDaily()
        {
            if (hasShowInter) return true;
            Initialize();
            if (levelShowInter == null)
            {
                hasShowInter = true;
                return true;
            }

            int day = SonatSystem.GetService<UserDataService>().UserDay;
            int levelCondition = 0;
            foreach (var config in levelShowInter)
            {
                if (config.Key > day)
                {
                    break;
                }

                levelCondition = config.Value;
            }

            hasShowInter = SonatSystem.GetService<UserDataService>().LevelPlayToday >= levelCondition;
            return hasShowInter;
        }
    }
}
#endif