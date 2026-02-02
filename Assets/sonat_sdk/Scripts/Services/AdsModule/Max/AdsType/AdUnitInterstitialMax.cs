#if using_max

using System;
using Sonat.Debugger;
#if using_aps
using AmazonAds;
#endif

namespace Sonat.AdsModule.Max
{
    public class AdUnitInterstitialMax : AdUnitMax
    {
        public override AdType AdType => AdType.Interstitial;

        public override void Initialize(AdUnitId adUnitId)
        {
            base.Initialize(adUnitId);
            MaxSdkCallbacks.Interstitial.OnAdLoadedEvent += HandleOnAdLoaded;
            MaxSdkCallbacks.Interstitial.OnAdLoadFailedEvent += HandleOnAdFailedToLoad;
            MaxSdkCallbacks.Interstitial.OnAdDisplayedEvent += HandleOnAdOpened;
            MaxSdkCallbacks.Interstitial.OnAdClickedEvent += HandleAdClickedEvent;
            MaxSdkCallbacks.Interstitial.OnAdHiddenEvent += HandleOnAdClosed;
            MaxSdkCallbacks.Interstitial.OnAdDisplayFailedEvent += HandleOnAdFailedToShow;
            MaxSdkCallbacks.Interstitial.OnAdRevenuePaidEvent += HandleAdPaidEvent;
        }

        public override void RequestAds()
        {
            if (adState is AdState.Requesting or AdState.Loaded or AdState.Showing or AdState.Hidden) return;

            if (!PreCheck())
            {
                RetryRequestAds();
                return;
            }

            adState = AdState.Requesting;
            
#if !using_aps || UNITY_EDITOR
            MaxSdk.LoadInterstitial(adId);
#else
        LoadInterWithAPS();
#endif
        }

        public override bool Ready()
        {
            return MaxSdk.IsInterstitialReady(adId) && adState is AdState.Loaded or AdState.Hidden;
        }

        public override void Show(AdPlacement placement, Action<bool> callback)
        {
            this.Placement = placement;
            active = true;
            if (!Ready())
            {
                SonatDebugType.Ads.Log($"{AdType} is not ready - {adState}");
                callback?.Invoke(false);
                return;
            }
        
            SonatAds.SetFullScreenAd(true);
            adState = AdState.Showing;
            SonatSdkManager.DelayCall(SonatAds.Config.TimeShowLoading, () =>
            {
                MaxSdk.ShowInterstitial(adId, Placement.ToString());
                callback?.Invoke(true);
            });
            
        }

        public override void Hide()
        {
            adState = AdState.Hidden;
            active = false;
        }

        public override void DestroyAd()
        {
            active = false;
            //if (adState == AdState.Destroyed) return;
            adState = AdState.Destroyed;
        }


#if using_aps
        private bool IsFirstLoad = true;
        private bool staticInterLoaded = false;
        private bool videoInterLoaded = false;

        private void LoadInterWithAPS()
        {
            if (IsFirstLoad && (!string.IsNullOrEmpty(apsId1) || !string.IsNullOrEmpty(apsId2)))
            {
                IsFirstLoad = false;
                var interstitialAd = new APSInterstitialAdRequest(apsId1);
                var interstitialVideoAd = new APSVideoAdRequest(320, 480, apsId2);
                interstitialAd.onSuccess += (adResponse) =>
                {
                    MaxSdk.SetInterstitialLocalExtraParameter(adId, "amazon_ad1_response", adResponse.GetResponse());
                    staticInterLoaded = true;
                    if (videoInterLoaded && staticInterLoaded)
                        MaxSdk.LoadInterstitial(adId);
                };
                interstitialAd.onFailedWithError += (adError) =>
                {
                    MaxSdk.SetInterstitialLocalExtraParameter(adId, "amazon_ad1_error", adError.GetAdError());
                    staticInterLoaded = true;
                    if (videoInterLoaded && staticInterLoaded)
                        MaxSdk.LoadInterstitial(adId);
                };

                interstitialVideoAd.onSuccess += (adResponse) =>
                {
                    MaxSdk.SetInterstitialLocalExtraParameter(adId, "amazon_ad_response", adResponse.GetResponse());
                    videoInterLoaded = true;
                    if (videoInterLoaded && staticInterLoaded)
                        MaxSdk.LoadInterstitial(adId);
                };
                interstitialVideoAd.onFailedWithError += (adError) =>
                {
                    MaxSdk.SetInterstitialLocalExtraParameter(adId, "amazon_ad_error", adError.GetAdError());
                    videoInterLoaded = true;
                    if (videoInterLoaded && staticInterLoaded)
                        MaxSdk.LoadInterstitial(adId);
                };
                interstitialAd.LoadAd();
                interstitialVideoAd.LoadAd();
            }
            else
            {
                MaxSdk.LoadInterstitial(adId);
            }
        }

#endif
    }
}

#endif