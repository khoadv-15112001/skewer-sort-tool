#if using_admob
using System;
using GoogleMobileAds.Api;

namespace Sonat.AdsModule.Admob
{
    public class AdUnitInterstitialAdmob : AdUnitAdmob
    {
        public override AdType AdType => AdType.Interstitial;

        private InterstitialAd interstitialAd;

        public override void Initialize(AdUnitId adUnitId)
        {
            base.Initialize(adUnitId);
        }

        public override void RequestAds()
        {
            if (adState is AdState.Requesting or AdState.Loaded or AdState.Showing or AdState.Hidden) return;

            if (!PreCheck())
            {
                RetryRequestAds();
                return;
            }

            interstitialAd?.Destroy();
            adState = AdState.Requesting;
            AdRequest adRequest = new AdRequest();
            InterstitialAd.Load(adId, adRequest,
                (ad, error) =>
                {
                    // if error is not null, the load request failed.
                    if (error != null || ad == null)
                    {
                        HandleOnAdFailedToLoad(error);
                        return;
                    }

                    interstitialAd = ad;
                    HandleOnAdLoaded(interstitialAd.GetResponseInfo());
                    interstitialAd.OnAdFullScreenContentOpened += HandleOnAdOpened;
                    interstitialAd.OnAdFullScreenContentClosed += HandleOnAdClosed;
                    interstitialAd.OnAdFullScreenContentFailed += HandleOnAdFailedToShow;
                    interstitialAd.OnAdClicked += HandleOnAdClicked;
                    interstitialAd.OnAdPaid += HandleOnAdPaid;
                });
        }

        public override bool Ready()
        {
            return interstitialAd != null && interstitialAd.CanShowAd() && adState is AdState.Loaded or AdState.Hidden;
        }

        public override void Show(AdPlacement placement, Action<bool> callback)
        {
            this.Placement = placement;
            active = true;
            if (!Ready())
            {
                callback?.Invoke(false);
                return;
            }
            
            SonatAds.SetFullScreenAd(true);
            adState = AdState.Showing;
            SonatSdkManager.DelayCall(SonatAds.Config.TimeShowLoading, () =>
            {
                interstitialAd.Show();
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
            if (adState == AdState.Destroyed) return;
            interstitialAd?.Destroy();
            adState = AdState.Destroyed;
        }
        

    }
}
#endif