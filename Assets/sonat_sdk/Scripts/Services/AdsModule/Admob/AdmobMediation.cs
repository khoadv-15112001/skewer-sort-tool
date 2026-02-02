using System.Collections;
#if using_admob
using GoogleMobileAds.Api;
#endif
using Sonat.Debugger;
using Sonat.FirebaseModule;
using Sonat.TrackingModule;
using UnityEngine;

namespace Sonat.AdsModule.Admob
{
    [CreateAssetMenu(menuName = "SonatSDK/Mediation/Admob Mediation", fileName = nameof(AdmobMediation))]
    public class AdmobMediation : SonatMediation
    {
        public override MediationType MediationType => MediationType.Admob;
#if using_admob
        private GDPRScript gdprScript = new GDPRScript();
#endif

        public override void Initialize(SonatAds ads)
        {
            base.Initialize(ads);
#if using_admob && !using_max
            SonatAds.ConsentReady = gdprScript.consenDone;
            gdprScript.InitConsent();
#endif
            SonatSdkManager.instance.StartCoroutine(Wait());
        }

        private IEnumerator Wait()
        {
            float t = 5;
            while (t > 0 && !SonatFirebase.analytic.DependencyStatusAvailable)
            {
                yield return null;
                t -= Time.deltaTime;
            }

            SonatDebugType.Ads.Log("Waiting for Consent");
            while (!SonatAds.ConsentReady)
                yield return null;

            SonatDebugType.Ads.Log("Consent Ready");
            SetupAdmob();
        }

        private void SetupAdmob()
        {
#if using_admob
#if UNITY_IOS
            MobileAds.SetiOSAppPauseOnBackground(true);
#endif
            RequestConfiguration requestConfiguration = new RequestConfiguration();
            MobileAds.SetRequestConfiguration(requestConfiguration);
            MobileAds.Initialize(HandleInitCompleteAction);
            SonatFirebase.analytic.LogEvent("admob_sdk_init_start", new LogParameter("flow_seq", "1"));
#endif
        }

#if using_admob
        private void HandleInitCompleteAction(InitializationStatus initStatus)
        {
            SonatDebugType.Ads.Log("Admob initialized");
            SonatFirebase.analytic.LogEvent("admob_sdk_init_complete", new LogParameter("flow_seq", "2"));
            CreateAdUnits();
            OnInitialized?.Invoke(this);
        }
#endif

        protected override void CreateAdUnit(AdUnitId adUnitId)
        {
            if (SonatAds.IsNoAds() && adUnitId.placement is not AdPlacement.Rewarded) return;

            if (ads.config.ForceMediationPlacement)
            {
                MediationType mediationForPlacement = ads.config.MediationForPlacement(adUnitId.placement);
                if (mediationForPlacement != MediationType && mediationForPlacement != MediationType.All) return;
            }

            int unitCreate = 1;
            if (adUnitId.placement == AdPlacement.Interstitial)
            {
                unitCreate = ads.config.InterCache;
            }
            else if (adUnitId.placement == AdPlacement.Rewarded)
            {
                unitCreate = ads.config.RewardCache;
            }

            for (int i = 0; i < unitCreate; i++)
            {
                AdUnit adUnit = AdUnitFactory.CreateAdUnit(MediationType.Admob, adUnitId, out var newUnit, i > 0);
                if (adUnit == null) continue;
                ads.AddUnit(adUnitId.placement, adUnit, newUnit);
            }
        }
    }
}