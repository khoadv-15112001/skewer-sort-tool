using System;
using Sonat.Debugger;
using Sonat.FirebaseModule;
using Sonat.TrackingModule;
using UnityEngine;

namespace Sonat.AdsModule
{
    public abstract class SonatMediation : ScriptableObject
    {
        public abstract MediationType MediationType { get; }

        protected SonatAds ads;

        public Action<SonatMediation> OnInitialized;

        protected AdDurationTracker adDurationTracker;
        public bool Ready { get; protected set; }

        public AdsConfig androidConfig;
        public AdsConfig iosConfig;
        public AdsConfig testConfig;

#if using_aps
        public bool usingAps;
        public AdsConfig amazonConfig;
#endif
        public AdsConfig AdsConfig => ads.testAds
            ? testConfig
            :
#if UNITY_IOS
            iosConfig;
#else
            androidConfig;
#endif

        public virtual void Initialize(SonatAds ads)
        {
            this.ads = ads;
            adDurationTracker = AdDurationTracker.Instance;
        }

        protected virtual void CreateAdUnits()
        {
            foreach (var adUnitId in AdsConfig.adUnitIds)
            {
                adUnitId.id = adUnitId.id.RemoveWhiteSpace();
                var adUnitIdValidate = SonatFirebase.remote.GetRemoteConfig<AdUnitId>($"remote_ad_unit_{adUnitId.placement}", adUnitId);
                SonatDebugType.Ads.Log($"Create Ad Unit {adUnitIdValidate.placement}: {adUnitIdValidate.id}");
                CreateAdUnit(adUnitIdValidate);
            }
        }

        protected abstract void CreateAdUnit(AdUnitId adUnitId);
    }
}