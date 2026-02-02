using System;
using System.Collections.Generic;
using System.Linq;
using Sonat.AdsModule.Admob;
using Sonat.Data;
using Sonat.Debugger;
using Sonat.FirebaseModule;
using Sonat.TrackingModule;
using UnityEngine;
#if using_admob_native
#endif

namespace Sonat.AdsModule
{
    [CreateAssetMenu(menuName = "SonatSDK/Services/Ads Service", fileName = nameof(SonatAds))]
    public class SonatAds : SonatService
    {
        private static SonatAds instance;

        public bool testAds;
        public override SonatServiceType ServiceType => SonatServiceType.AdsService;
        public override bool Ready { get; set; }

        private static bool consentReady;

        public static bool ConsentReady
        {
            get => consentReady;
            set
            {
                consentReady = value;
                PlayerPrefs.SetInt("ConsentReady", value ? 1 : 0);
            }
        }

        public List<SonatMediation> mediations;
        public SonatAdsConfig config;
        public static SonatAdsConfig Config => instance.config;
        private Dictionary<AdPlacement, List<AdUnit>> adUnits = new();

        private readonly Dictionary<AdPlacement, float> lastTimeShowAds = new();

        public readonly Dictionary<AdPlacement, string> adShowPlacement = new();

        private SonatAdsEvents adsEvents;
        private int mediationInited = 0;
        public static event Action<bool> OnNoAdsUpdate;

        public static event Action<bool> OnFullScreenAd;
        public static bool blockOnFocusAds;
        public static bool needShowAppOpenAds = true;

        public static Func<AdPlacement, bool> externConditionShowAds;
        public static Action<AdPlacement> OnAdShowComplete;

        public override void Initialize(Action<ISonatService> onInitialized)
        {
            instance = this;
            base.Initialize(onInitialized);
            adsEvents = new SonatAdsEvents(this);
#if UNITY_IOS
            SonatATT sonatAtt = new SonatATT();
            sonatAtt.CheckRequest();
#endif

            consentReady = PlayerPrefs.GetInt("ConsentReady", 0) == 1;
            needShowAppOpenAds = needShowAppOpenAds && config.AutoShowAppOpenAds;
            SonatSdkUtils.DoActionDelay(InitializeMediations, 0.5f);
        }


        protected virtual void InitializeMediations()
        {
            SonatDebugType.Ads.Log("Start initializing Mediations");
            if (config.PriorityMediation != MediationType.None)
            {
                if (config.ForceMediationPlacement)
                {
                    var mediation = mediations.Find(e => e.MediationType == config.PriorityMediation);
                    if (mediation != null)
                    {
                        mediation.OnInitialized += OnMediationInitialized;
                        mediation.Initialize(this);
                        return;
                    }
                }

                mediations.Sort((a, b) => PointForMediation(b.MediationType).CompareTo(PointForMediation(a.MediationType)));
            }

            mediationInited = 0;
            foreach (var mediation in mediations)
            {
                mediation.OnInitialized += OnMediationInitialized;
                mediation.Initialize(this);
            }
        }

        private int PointForMediation(MediationType mediationType)
        {
            if (config.PriorityMediation == mediationType) return 100;
            return 0;
        }

        public virtual void AddUnit(AdPlacement placement, AdUnit adUnit, bool newUnit)
        {
            if (!adUnits.TryGetValue(placement, out var units))
            {
                units = new List<AdUnit>();
                adUnits.Add(placement, units);
            }

            if (adUnit.Mediation == config.MediationForPlacement(placement))
            {
                units.Insert(0, adUnit);
            }
            else
            {
                units.Add(adUnit);
            }

            if (placement == AdPlacement.Banner)
            {
                adUnit.active = config.AutoShowBanner && CheckPassLevel(AdPlacement.Banner);
            }
            else if (placement == AdPlacement.AppOpen)
            {
                adUnit.active = config.AutoShowAppOpenAds;
            }

            if (newUnit)
            {
                RegisterAdUnit(placement, adUnit);
            }
        }

        public void RegisterAdUnit(AdPlacement adPlacement, AdUnit adUnit)
        {
            adUnit.OnAdLoaded += adsEvents.OnAdLoaded;
            adUnit.OnAdOpened += adsEvents.OnAdOpened;
            adUnit.OnAdClosed += adsEvents.OnAdClosed;
            adUnit.OnAdLoadFailed += adsEvents.OnAdFailedToLoad;
            adUnit.OnAdShowFailed += adsEvents.OnAdFailedToShow;
            adUnit.OnAdClicked += adsEvents.OnAdClicked;
            adUnit.OnEarnedReward += adsEvents.OnEarnedReward;
            adUnit.OnAdPaid += adsEvents.OnAdPaid;

            RequestAd(adPlacement, adUnit);
        }

        private void RequestAd(AdPlacement adPlacement, AdUnit adUnit)
        {
            float timeDelay = 0;
            switch (adPlacement)
            {
                case AdPlacement.Banner:
                    timeDelay = 1;
                    break;
                case AdPlacement.OnFocus:
                    timeDelay = 4;
                    break;
                case AdPlacement.Interstitial:
                    timeDelay = 5;
                    break;
                case AdPlacement.Rewarded:
                    timeDelay = 3;
                    break;
                case AdPlacement.Native:
                    timeDelay = 2;
                    break;
                case AdPlacement.AdsBreak:
                    timeDelay = 10;
                    break;
            }

            SonatSdkUtils.DoActionDelay(adUnit.RequestAds, timeDelay);
        }


        #region Handler

        protected virtual void OnMediationInitialized(SonatMediation mediation)
        {
            SonatDebugType.Ads.Log($"Mediation {mediation.MediationType} Initialized!");
            mediationInited++;
            if (mediationInited == mediations.Count)
            {
                Ready = true;
                OnInitialized?.Invoke(this);
            }
        }

        #endregion


        #region Public Static Functions

        public static bool IsNoAds()
        {
            return UserData.IsNoads.BoolValue;
        }

        public static void SetNoAds(bool value)
        {
            UserData.IsNoads.BoolValue = value;
            OnNoAdsUpdate?.Invoke(value);
            if (value) ActiveNoAds();
        }

        public static bool IsAdsReady(AdPlacement placement)
        {
            if (instance == null) return false;
            return instance._IsAdsReady(placement);
        }

        public static void LoadAd(AdPlacement placement)
        {
            if (instance == null) return;
            instance._LoadAd(placement);
        }

        public static bool ShowAd(AdPlacement adPlacement, string placementLog, Action<bool> callback, SonatLogBase log)
        {
            if (instance == null) return false;
            instance.adShowPlacement.TrySetValue(adPlacement, placementLog);
            return instance._ShowAd(adPlacement, callback, log);
        }

        public static bool ShowInterstitial(Action<bool> callback, SonatLogShowInterstitial log)
        {
            if (instance == null) return false;
            string placement = log != null ? log.placement : "";
            instance.adShowPlacement.TrySetValue(AdPlacement.Interstitial, placement);
            return instance._ShowAd(AdPlacement.Interstitial, callback, log);
        }

        public static bool ShowRewardedAds(Action<bool> callback, SonatLogVideoRewarded log)
        {
            if (instance == null) return false;
            instance.adsEvents.rewardedVideoLog = log;
            string placement = log != null ? log.placement : "";
            instance.adShowPlacement.TrySetValue(AdPlacement.Rewarded, placement);
            return instance._ShowAd(AdPlacement.Rewarded, callback, null);
        }

        public static bool ShowBanner(Action<bool> callback = null, string placement = "banner")
        {
            if (instance == null) return false;
            instance.adShowPlacement.TrySetValue(AdPlacement.Banner, placement);
            return instance._ShowAd(AdPlacement.Banner, callback);
        }

        public static bool ShowNative(Action<bool> callback = null, string placement = "native")
        {
            if (instance == null) return false;
            instance.adShowPlacement.TrySetValue(AdPlacement.Native, placement);
            return instance._ShowAd(AdPlacement.Native, callback);
        }

        public static bool ShowAdsBreak(Action<bool> callback, SonatLogBase log)
        {
            if (instance == null) return false;
            instance.adShowPlacement.TrySetValue(AdPlacement.AdsBreak, "ads_break");
            return instance._ShowAd(AdPlacement.AdsBreak, callback, log);
        }

        public static bool ShowNative(GameObject adObject, Action<bool> callback = null, string placement = "native")
        {
            if (instance == null) return false;
            instance.adShowPlacement.TrySetValue(AdPlacement.Native, placement);
#if using_admob_native
            if (adObject == null)
#endif
            return ShowNative(callback, placement);
#if using_admob_native
            return instance._ShowAdmobNativeAd(adObject, callback);
#endif
        }

        public static void HideAd(AdPlacement placement)
        {
            if (instance == null) return;
            instance._HideAd(placement);
        }

        public static void DestroyAd(AdPlacement placement)
        {
            if (instance == null) return;
            instance._DestroyAd(placement);
        }

        public static float GetBannerHeight()
        {
            if (instance == null) return 0;
            AdUnit adUnit = instance.GetReadyUnit(AdPlacement.Banner);
            if (adUnit == null) return 0;
            IAdUnitBanner banner = adUnit as IAdUnitBanner;
            return banner?.GetBannerHeight() ?? 0;
        }

        public static AdUnit GetAdUnit(AdPlacement placement)
        {
            if (instance == null) return null;
            return instance.GetReadyUnit(placement);
        }


        public static SonatMediation GetMediation(MediationType mediationType)
        {
            if (instance == null) return null;
            return instance.mediations.Find(e => e.MediationType == mediationType);
        }

        public static bool CanShowAds(AdPlacement placement)
        {
            return instance.CheckCanShow(placement) && instance.GetReadyUnit(placement) != null;
        }

        public static void SetFullScreenAd(bool show)
        {
            if (show)
            {
                blockOnFocusAds = true;
                //SonatSdkUtils.DoActionDelay(() => blockOnFocusAds = false, 5);
            }

            OnFullScreenAd?.Invoke(show);
        }

        public static bool CheckAdPlacement(AdUnit adUnit, AdPlacement placement)
        {
            if (instance == null) return false;
            return instance._CheckAdPlacement(adUnit, placement);
        }

        public static float LastTimeShowAds(AdPlacement placement)
        {
            if(instance == null) return 0;
            return instance.GetLastTimeShowAds(placement);
        }

        #endregion

        #region Internal Functions

        private static void ActiveNoAds()
        {
            DestroyAd(AdPlacement.Banner);
            DestroyAd(AdPlacement.Native);
        }

        private bool _IsAdsReady(AdPlacement placement)
        {
            AdUnit adUnit = GetReadyUnit(placement);
            return adUnit != null;
        }

        private bool _CheckAdPlacement(AdUnit adUnit, AdPlacement placement)
        {
            if (adUnits.TryGetValue(placement, out var ads))
            {
                return ads.Contains(adUnit);
            }

            return false;
        }

        private void _LoadAd(AdPlacement placement)
        {
            if (adUnits.TryGetValue(placement, out var units))
            {
                foreach (var adUnit in units)
                {
                    adUnit.RequestAds();
                }
            }
        }

        private bool _ShowAd(AdPlacement placement, Action<bool> callback, SonatLogBase log = null)
        {
            if (!CheckCanShow(placement))
            {
                callback?.Invoke(false);
                return false;
            }

            AdUnit adUnit = GetReadyUnit(placement);
            if (adUnit == null)
            {
                callback?.Invoke(false);
                return false;
            }

            SetLastTimeShowAds(placement);
            adUnit.Show(placement, callback);
            log?.Post(true);
            return true;
        }

        private void _HideAd(AdPlacement placement)
        {
            if (adUnits.TryGetValue(placement, out var units))
            {
                foreach (var adUnit in units)
                {
                    adUnit.Hide();
                }
            }
        }

        private void _DestroyAd(AdPlacement placement)
        {
            if (adUnits.TryGetValue(placement, out var units))
            {
                foreach (var adUnit in units)
                {
                    adUnit.DestroyAd();
                }

                units.Clear();
            }
        }

        private void ClearAds()
        {
            foreach (var ads in adUnits)
            {
                foreach (var ad in ads.Value)
                {
                    ad.DestroyAd();
                }
            }
        }

#if using_admob_native
        private bool _ShowAdmobNativeAd(GameObject adObject, Action<bool> callback)
        {
            if (!CheckCanShow(AdPlacement.Native))
            {
                callback?.Invoke(false);
                return false;
            }

            AdUnit adUnit = null;
            if (adUnits.TryGetValue(AdPlacement.Native, out var units))
            {
                adUnit = units.FirstOrDefault(e => e.Mediation == MediationType.Admob && e.Ready() && e.AdType == AdType.NativeAds);
            }

            if (adUnit == null)
            {
                callback?.Invoke(false);
                return false;
            }

            SetLastTimeShowAds(AdPlacement.Native);
            ((AdUnitNativeAdmob)adUnit).ShowNativeAds(adObject, callback);
            return true;
        }
#endif

        #endregion

        private AdUnit GetReadyUnit(AdPlacement placement)
        {
            if (adUnits.TryGetValue(placement, out var units))
            {
                foreach (var adUnit in units)
                {
                    if (adUnit.Ready())
                    {
                        return adUnit;
                    }
                }
            }

            return null;
        }


        #region Validate

        private bool CheckCanShow(AdPlacement placement)
        {
            if (placement != AdPlacement.Rewarded && IsNoAds())
            {
                SonatDebugType.Ads.Log("Cant Show Ads - No Ads");
                return false;
            }

            if (!CheckPassTimeGap(placement))
            {
                return false;
            }

            if (!CheckPassLevel(placement))
            {
                SonatDebugType.Ads.Log("Cant Show Ads - Level too low");
                return false;
            }

            if (!CheckExternalConditionShowAds(placement))
            {
                return false;
            }

            return true;
        }

        private bool CheckPassLevel(AdPlacement placement)
        {
            switch (placement)
            {
                case AdPlacement.Interstitial:
                    return UserData.GetLevel() >= config.LevelStartShowInter;
                case AdPlacement.Native:
                    return UserData.GetLevel() >= config.LevelStartShowNativeBanner;
                case AdPlacement.Banner:
                    return UserData.GetLevel() >= config.LevelStartShowBanner;
                default:
                    return UserData.GetLevel() >= SonatFirebase.remote.GetRemoteInt($"level_start_show_{placement}".ToLower(), 0);
            }
        }

        private bool CheckExternalConditionShowAds(AdPlacement placement)
        {
            return externConditionShowAds == null || externConditionShowAds.Invoke(placement);
        }

        private bool CheckPassTimeGap(AdPlacement placement)
        {
            switch (placement)
            {
                case AdPlacement.Interstitial:
                    return PassTimeGap(AdPlacement.Interstitial,
                        new List<AdPlacement>() { AdPlacement.Rewarded, AdPlacement.OnFocus, AdPlacement.AdsBreak });
                case AdPlacement.OnFocus:
                    return PassTimeGap(AdPlacement.OnFocus,
                        new List<AdPlacement>() { AdPlacement.Interstitial, AdPlacement.Rewarded, AdPlacement.AdsBreak });
                case AdPlacement.AdsBreak:
                    return PassTimeGap(AdPlacement.AdsBreak,
                        new List<AdPlacement>() { AdPlacement.Rewarded, AdPlacement.OnFocus, AdPlacement.Interstitial });
                default:
                    return true;
            }
        }


        private bool PassTimeGap(AdPlacement placement, List<AdPlacement> otherPlacements)
        {
            int timeGapMain = config.GetTimeGap(placement); // ex: "time_gap_interstitial"
            float currentTime = Time.time;
            if (currentTime - GetLastTimeShowAds(placement) < timeGapMain)
            {
                SonatDebugType.Ads.Log($"Time gap: {placement} and last {placement} < {timeGapMain}s");
                return false;
            }

            foreach (var otherPlacement in otherPlacements)
            {
                int timeGap = config.GetTimeGap(placement, otherPlacement, timeGapMain);
                if (currentTime - GetLastTimeShowAds(otherPlacement) < timeGap)
                {
                    SonatDebugType.Ads.Log($"Time gap: {placement} and {otherPlacement} < {timeGap}s");
                    return false;
                }
            }

            return true;
        }


        private float GetLastTimeShowAds(AdPlacement placement)
        {
            return lastTimeShowAds.GetValueOrDefault(placement, -100);
        }

        private void SetLastTimeShowAds(AdPlacement placement)
        {
            lastTimeShowAds.TrySetValue(placement, Time.time);
        }

        #endregion


        #region OnFocusAds

        public override void OnApplicationPause(bool pauseStatus)
        {
            if (pauseStatus)
                lastTimeLostFocus = Time.realtimeSinceStartup;
        }

        private float lastTimeLostFocus;


        public override void OnApplicationFocus(bool hasFocus)
        {
            if (hasFocus)
            {
                Time.timeScale = 1;
            }

            SonatDebugType.Ads.Log("OnApplicationFocus: " + hasFocus);

            //  return;
            if (!hasFocus)
            {
                lastTimeLostFocus = Time.realtimeSinceStartup;
                return;
            }

            if (blockOnFocusAds)
            {
                blockOnFocusAds = false;
                return;
            }

            if (!Ready)
            {
                return;
            }

            if (RemoteConfigKey.turn_off_focus_ads.GetBool())
            {
                SonatDebugType.Ads.Log("On focus ads is off");
                return;
            }

            if (SonatFirebase.FirebaseRemoteReady)
            {
                var current = Time.realtimeSinceStartup;
                var diff = current - lastTimeLostFocus;
                if (diff > RemoteConfigKey.seconds_to_dispose_ads.GetInt(999999))
                {
                    ClearAds();
                    SonatDebugType.Ads.Log("Dispose all ads");
                    return;
                }
            }


            if (Time.time < 5)
            {
                SonatDebugType.Ads.Log("Show on focus ads fail: startup too soon < 3s");
                return;
            }


            if (Time.realtimeSinceStartup - lastTimeLostFocus > RemoteConfigKey.max_seconds_out_focus.GetInt(3600))
            {
                SonatDebugType.Ads.Log("Show on focus ads fail : time out focus too long");
                return;
            }
            else if (Time.realtimeSinceStartup - lastTimeLostFocus < RemoteConfigKey.min_seconds_out_focus.GetInt(10))
            {
                SonatDebugType.Ads.Log($"Show on focus ads fail : time out focus too short {Time.realtimeSinceStartup - lastTimeLostFocus}s");
                return;
            }

            ShowAdsOnFocus();
        }

        private void ShowAdsOnFocus()
        {
            SonatSdkUtils.DoActionDelay(() => { _ShowAd(AdPlacement.OnFocus, null, new SonatLogShowInterstitial() { placement = "on_focus" }); },
                0.1f);
        }

        #endregion
    }
}