using Sonat.Debugger;
using Sonat.TrackingModule;
using UnityEngine;
#if using_aps
using AmazonAds;
#endif

namespace Sonat.AdsModule.Max
{
    [CreateAssetMenu(menuName = "SonatSDK/Mediation/Max Mediation", fileName = nameof(MaxMediation))]
    public class MaxMediation : SonatMediation
    {
        public bool showDebugger;
        public string sdkKey;
        public override MediationType MediationType => MediationType.Max;

        public override void Initialize(SonatAds ads)
        {
            base.Initialize(ads);
            SonatDebugType.Ads.Log("MaxSdk Start Initialize");
            SetupMAXMediation();
        }

        void SetupMAXMediation()
        {
#if using_aps && !UNITY_EDITOR
        Amazon.Initialize(amazonConfig.appId);
        Amazon.SetAdNetworkInfo(new AdNetworkInfo(DTBAdNetwork.MAX));
#endif
#if using_max
            MaxSdkCallbacks.OnSdkInitializedEvent += sdkConfiguration =>
            {
                // AppLovin SDK is initialized, start loading ads
                HandleInitCompleteAction();

                if (showDebugger)
                    MaxSdk.ShowMediationDebugger();
            };

            // MaxSdk.SetSdkKey(sdkKey);

            MaxSdk.InitializeSdk();
#endif
        }

        private void HandleInitCompleteAction()
        {
            Ready = true;
            SonatAds.ConsentReady = true;

            if (!PlayerPrefs.HasKey("TRACKING_GDPR_FIREBASE_MAX"))
            {
                PlayerPrefs.SetInt("TRACKING_GDPR_FIREBASE_MAX", 1);
                SonatSdkUtils.DoActionDelay(TrackingGDPR, 3);
            }

            SonatDebugType.Ads.Log("MaxSdk Initialized");
            CreateAdUnits();
            OnInitialized?.Invoke(this);
        }

        private void TrackingGDPR()
        {
#if using_max
            new SonatLogGDPR()
            {
                status = MaxSdk.HasUserConsent() ? "true" : "false"
            }.Post();
#endif
        }

        protected override void CreateAdUnit(AdUnitId adUnitId)
        {
            if (SonatAds.IsNoAds() && adUnitId.placement is not AdPlacement.Rewarded) return;

            if (ads.config.ForceMediationPlacement)
            {
                MediationType mediationForPlacement = ads.config.MediationForPlacement(adUnitId.placement);
                if (mediationForPlacement != MediationType && mediationForPlacement != MediationType.All) return;
            }

            AdUnit adUnit = AdUnitFactory.CreateAdUnit(MediationType.Max, adUnitId, out var newUnit);
            if (adUnit == null) return;

#if using_aps
            if (usingAps)
            {
                string apsId1, apsId2;
                switch (adUnit.AdType)
                {
                    case AdType.Interstitial:
                        apsId1 = amazonConfig.GetAdUnitId(AdType.Interstitial)?.id;
                        apsId2 = amazonConfig.GetAdUnitId(AdType.InterstitialVideo)?.id;
                        adUnit.SetApsId(apsId1, apsId2);
                        break;
                    case AdType.Rewarded:
                        apsId1 = amazonConfig.GetAdUnitId(AdType.Rewarded)?.id;
                        apsId2 = "";
                        adUnit.SetApsId(apsId1, apsId2);
                        break;
                    case AdType.Banner:
                        apsId1 = amazonConfig.GetAdUnitId(AdType.Banner)?.id;
                        apsId2 = amazonConfig.GetAdUnitId(AdType.LargeBanner)?.id;
                        adUnit.SetApsId(apsId1, apsId2);
                        break;
                }
            }

#endif

            ads.AddUnit(adUnitId.placement, adUnit, newUnit);
        }
    }
}