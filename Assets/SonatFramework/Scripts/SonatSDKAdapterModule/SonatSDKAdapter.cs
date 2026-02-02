#if !sonat_sdk_v2
using System;
using System.Collections.Generic;
using Sonat.Enums;
using SonatFramework.Scripts.Utils;
using SonatFramework.Systems;
using SonatFramework.Systems.GameDataManagement;
using SonatFramework.Systems.LevelManagement;
using SonatFramework.Systems.TrackingModule;
using SonatFramework.Systems.UserData;
#if sonat_sdk
using Sonat;
#endif

namespace SonatFramework.Scripts.SonatSDKAdapterModule
{
    public static class SonatSDKAdapter
    {
        private static readonly Service<TrackingService> trackingService = new();
        private static readonly Service<GameplayAnalyticsService> gameplayAnalytics = new();
        private static readonly Service<UserDataService> userDataService = new();
        private static readonly Service<DataService> dataService = new();

        public static void ShowRewardAds(Action onWatchedAds, string itemType = "", string itemId = "")
        {
#if sonat_sdk
		if (IsRewardAdsReady())
		{
			trackingService.Instance.LogShowRewardAds();
		}
		else
		{
			PopupToast.Cretate("No video available!");
			return;
		}
		Kernel.Resolve<AdsManager>().ShowVideoAds(onWatchedAds, new SonatLogVideoRewarded()
		{
			placement = trackingService.Instance.placement,
			level = gameplayAnalytics.Instance.levelPlayData.level,
			mode = gameplayAnalytics.Instance.levelPlayData.gameMode.ToString().ToLogString(),
			item_type = itemType,
			item_id = itemId
		});

#else
            onWatchedAds?.Invoke();
#endif
        }

        public static bool IsRewardAdsReady()
        {
#if sonat_sdk
		return Kernel.Resolve<AdsManager>().IsVideoAdsReady();
#else
            return false;
#endif
        }

        public static bool CanShowInterAds()
        {
#if sonat_sdk
		return Kernel.Resolve<AdsManager>().IsInterstitialAdsReady() && Kernel.Resolve<AdsManager>()
			.IsInterstitialAdsValid(gameplayAnalytics.Instance.levelPlayData.level, true, false) != null;
#else
            return false;
#endif
        }

        public static bool ShowInterAds(string plecement, Action callback = null)
        {
#if sonat_sdk
			//long interAdsInterval = RemoteConfigHelper.GetValueInt("inter_ads_interval");
			//if (AdsManager.PlayTimesOrLevel % interAdsInterval == 0)
			bool conditionLevel = gameplayAnalytics.Instance.levelPlayData.gameMode == GameMode.Classic;


			bool showed =
				Kernel.Resolve<AdsManager>().ShowInterstitial(plecement.CreateDefaultLogInterstitial(), false, conditionLevel, actionAfterAds: callback);
			if (showed)
			{
				trackingService.Instance.LogShowInterAds();
			}
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
			Kernel.Resolve<AdsManager>().ShowBanner();
		else
			Kernel.Resolve<AdsManager>().HideBanner();
#endif
        }

        public static bool IsNativeAdsReady()
        {
#if sonat_sdk
			#if use_max
			return Kernel.Resolve<AdsManager>().IsMrecBannerReady();
			#elif use_admob
			return !IsNoads() && Kernel.Resolve<AdsManager>().IsMrecBannerReady();
			#endif
#endif
            return false;
        }

        public static void ShowNativeAds()
        {
#if sonat_sdk
#if use_max
			Kernel.Resolve<AdsManager>().ShowMrecBanner();
			#elif use_admob
			Kernel.Resolve<AdsManager>().ShowMrecBanner();
			#endif
#endif
        }

        public static void HideNavtiveAds()
        {
#if sonat_sdk
#if use_max
			Kernel.Resolve<AdsManager>().HideMrecBanner();
			#elif use_admob
			Kernel.Resolve<AdsManager>().HideMrecBanner();
			#endif
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

		SonatLogBuyShopItemIapInput log = new SonatLogBuyShopItemIapInput(trackingService.Instance.placement, item_type, 1);
		Kernel.Resolve<BasePurchaser>().Buy((int)iapKey, (success) =>
		{
			packBuying = ShopItemKey.None;
			if (success)
			{
				callback.Invoke(true);
				//OnBuyPack?.Invoke(iapKey);
			}
			else
			{
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
		if (noAds)
			Kernel.Resolve<AdsManager>().EnableNoAds();
		else
			Kernel.Resolve<AdsManager>().DisableNoAds();
#endif
        }

        public static bool IsNoads()
        {
#if sonat_sdk
		return Kernel.Resolve<AdsManager>().IsNoAds();
#else
            return false;
#endif
        }

        public static bool IsAppOpenAdsReady()
        {
#if sonat_sdk
			return Kernel.Resolve<AdsManager>().IsAppOpenAdsReady();
#else
            return false;
#endif
        }

        public static void ShowAppOpenAds()
        {
#if sonat_sdk
			Kernel.Resolve<AdsManager>().ShowAppOpenAds();
#endif
        }

        public static bool CheckPackBought(ShopItemKey packId)
        {
#if sonat_sdk && use_iap
		return Kernel.Resolve<Purchaser>().CheckPackBought((int)packId);
#else
            return false;
#endif
        }


        public static void SendEventAf(string s, Dictionary<string, string> pairs)
        {
#if sonat_sdk
		Kernel.Resolve<AppFlyerController>().SendEvent(s, pairs);
#endif
        }

        public static void SendEventAf(string s)
        {
#if sonat_sdk
		Kernel.Resolve<AppFlyerController>().SendEvent(s);
#endif
        }

        public static void SendEventFireBase(string s, bool logAf = true)
        {
#if sonat_sdk
		Kernel.Resolve<FireBaseController>().LogEvent(s);
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
		var product = Kernel.Resolve<BasePurchaser>().StoreProductDescriptors.Find(x => x.key == (int)iapKey);
		if (product != null)
		{
			return product.StoreProductId;
		}
		return iapKey.ToString();
#else
            return "";
#endif
        }

        public static string GetProductPrice(ShopItemKey key)
        {
#if sonat_sdk
		return Kernel.Resolve<BasePurchaser>().GetPriceText((int)key);
#else
            return "0$";
#endif
        }

        public static int GetRemoteInt(string key, int defaultValue = 0)
        {
#if sonat_sdk
		int value = (int)RemoteConfigHelper.GetValueInt(key, defaultValue);
#else
            int value = defaultValue;
#endif
            return value;
        }

        public static float GetRemoteFloat(string key, float defaultValue = 0)
        {
#if sonat_sdk
		float value = (float)RemoteConfigHelper.GetValueDouble(key);
#else
            float value = 0;
#endif
            if (value != 0) return value;
            return defaultValue;
        }

        public static bool GetRemoteBool(string key, bool defaultValue = false)
        {
#if sonat_sdk
		if ((SharedRemoteConfigController.FetchStatus == ConfigFetchStatus.Fetched ||
	 SharedRemoteConfigController.FetchStatus == ConfigFetchStatus.FetchedFail)
	&& RemoteConfigController.RemoteHasValue(key))
			return RemoteConfigController.GetValue(key).BooleanValue;

		var defaultConfig = Kernel.Resolve<FireBaseController>().remoteConfigController.GetDefault(key);
		if (defaultConfig == null)
		{
#if DISPLAY_LOG
            UIDebugLog.LogError("err : not found this default config :" + key);
#endif
			if (dataService.Instance.HasKey($"remote_value_{key}"))
			{
				return dataService.Instance.GetInt($"remote_value_{key}", 0) != 0;
			}
			return defaultValue;
		}
		bool value = defaultConfig.GetDefaultBoolean(true);
#else
            bool value = true;
#endif
            return value;
        }

        public static string GetRemoteString(string key)
        {
#if sonat_sdk
		string value = RemoteConfigHelper.GetValueString(key);
#else
            string value = "";
#endif
            return value;
        }

        public static bool IsPurchaserInited()
        {
#if sonat_sdk
        return Kernel.Resolve<BasePurchaser>().IsInitialized();
#else
            return false;
#endif
        }

        public static void Restore(Action onsuccess)
        {
            if (!IsPurchaserInited()) return;
#if sonat_sdk
#if UNITY_ANDROID
		bool success = Kernel.Resolve<BasePurchaser>().RestorePurchase();
		if (success)
			onsuccess?.Invoke();
#elif UNITY_IOS
		Kernel.Resolve<BasePurchaser>().RestorePurchasesIOS(() =>
		{
			onsuccess?.Invoke();
		});
#endif
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
}
#endif