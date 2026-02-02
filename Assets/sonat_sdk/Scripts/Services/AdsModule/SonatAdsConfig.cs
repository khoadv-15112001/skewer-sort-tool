using System;
using System.Collections.Generic;
using System.Linq;
using Sonat.FirebaseModule;
using UnityEngine;

namespace Sonat.AdsModule
{
    [Serializable]
    public class SonatAdsConfig
    {
        [SerializeField] private bool autoShowBanner = true;
        [SerializeField] private bool autoShowAppOpenAds = true;

        [SerializeField] private int levelStartShowBanner = 1;
        [SerializeField] private int levelStartShowInter = 1;
        [SerializeField] private int levelStartShowNative = 1;
        public float nativePosY = 5;

        [SerializeField] private int timeGapInterstitial = 30;
        [SerializeField] private int timeGapOnfocus = 30;

        [SerializeField] private float timeShowLoading = 0;

        [SerializeField] private MediationType priorityMediation = MediationType.None;
        [SerializeField] private List<MediationPlacement> mediationPlacements;
        [SerializeField] private bool forceMediationPrioritize = false;

        [Min(1)] [SerializeField] private int interAdsCache = 1;
        [Min(1)] [SerializeField] private int rewardAdsCache = 1;


        public int InterCache => RemoteConfigKey.inter_ads_cache.GetInt(interAdsCache);
        public int RewardCache => RemoteConfigKey.reward_ads_cache.GetInt(interAdsCache);

        public MediationType PriorityMediation => RemoteConfigKey.priority_mediation.Get(priorityMediation);
        public bool AutoShowBanner => RemoteConfigKey.auto_show_banner.GetBool(autoShowBanner);
        public bool AutoShowAppOpenAds => RemoteConfigKey.auto_show_app_open_ads.GetBool(autoShowAppOpenAds);
        public bool ForceMediationPlacement => RemoteConfigKey.force_mediation_prioritize.GetBool(forceMediationPrioritize);
        public int LevelStartShowBanner => RemoteConfigKey.level_start_show_banner.GetInt(levelStartShowBanner);
        public int LevelStartShowInter => RemoteConfigKey.level_start_show_interstitial.GetInt(levelStartShowInter);
        public float TimeShowLoading => RemoteConfigKey.time_show_loading_ads.GetFloat(timeShowLoading);

        public int LevelStartShowNativeBanner =>
            RemoteConfigKey.level_start_show_native_banner.GetInt(levelStartShowNative);


        private Dictionary<AdPlacement, MediationType> mediationForPlacement;


        /// <summary>
        /// Find Mediation for Placement
        /// </summary>
        /// <param name="placement"></param>
        /// <returns></returns>
        public MediationType MediationForPlacement(AdPlacement placement)
        {
            if (PriorityMediation != MediationType.None) return PriorityMediation;
            if (mediationForPlacement == null)
            {
                var cfg = RemoteConfigKey.mediation_placement.Get(mediationPlacements);
                mediationForPlacement = cfg?.ToDictionary(e => e.placement, e => e.mediationType);
            }

            return mediationForPlacement?.GetValueOrDefault(placement, MediationType.All) ?? MediationType.All;
        }

        public int GetTimeGap(AdPlacement placement)
        {
            if (placement != AdPlacement.Interstitial && placement != AdPlacement.OnFocus && placement != AdPlacement.AdsBreak) return 0;
            int timeGapDefault = placement == AdPlacement.OnFocus ? timeGapOnfocus : timeGapInterstitial;
            int timeGapMain = SonatFirebase.remote.GetRemoteInt($"time_gap_{placement.ToString().ToLower()}", timeGapDefault); // ex: "time_gap_interstitial"
            timeGapMain = SonatFirebase.remote.GetValueByLevel($"by_level_time_gap_{placement}".ToLower(), timeGapMain);
            return timeGapMain;
        }

        public int GetTimeGap(AdPlacement placement, AdPlacement otherPlacement, int timeGapDefault)
        {
            if (placement != AdPlacement.Interstitial && placement != AdPlacement.OnFocus && placement != AdPlacement.AdsBreak) return 0;
            int timeGap = SonatFirebase.remote.GetRemoteInt($"time_gap_{placement.ToString().ToLower()}_{otherPlacement.ToString().ToLower()}",
                timeGapDefault);
            timeGap = SonatFirebase.remote.GetValueByLevel($"by_level_time_gap_{placement}_{otherPlacement}".ToLower(), timeGap);
            return timeGap;
        }

    }

    [Serializable]
    public class MediationPlacement
    {
        public AdPlacement placement;
        public MediationType mediationType;
    }

    [Serializable]
    public class RemoteConfigManual<T>
    {
        private RemoteConfigKey key;
        [SerializeField] private T defaultValue;
        public T Value => key.Get(defaultValue);

        public RemoteConfigManual(RemoteConfigKey key, T defaultValue)
        {
            this.key = key;
            this.defaultValue = defaultValue;
        }
    }
}