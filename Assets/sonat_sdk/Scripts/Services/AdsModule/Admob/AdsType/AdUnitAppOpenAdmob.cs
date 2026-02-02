#if using_admob
using System;
using GoogleMobileAds.Api;

namespace Sonat.AdsModule.Admob
{
    public class AdUnitAppOpenAdmob : AdUnitAdmob
    {
        public override AdType AdType => AdType.AppOpenAd;

        private AppOpenAd appOpenAd;

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
            
            appOpenAd?.Destroy();
            adState = AdState.Requesting;
            AdRequest adRequest = new AdRequest();
            AppOpenAd.Load(adId, adRequest,
                (ad, error) =>
                {
                    // if error is not null, the load request failed.
                    if (error != null || ad == null)
                    {
                        HandleOnAdFailedToLoad(error);
                        return;
                    }

                    appOpenAd = ad;
                    
                    HandleOnAdLoaded(appOpenAd.GetResponseInfo());
                    appOpenAd.OnAdFullScreenContentOpened += HandleOnAdOpened;
                    appOpenAd.OnAdFullScreenContentClosed += HandleOnAdClosed;
                    appOpenAd.OnAdFullScreenContentFailed += HandleOnAdFailedToShow;
                    appOpenAd.OnAdClicked += HandleOnAdClicked;
                    appOpenAd.OnAdPaid += HandleOnAdPaid;
                });
        }

        public override bool Ready()
        {
            return appOpenAd != null && appOpenAd.CanShowAd() && adState is AdState.Loaded or AdState.Hidden;
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

            adState = AdState.Showing;
            appOpenAd.Show();
            callback?.Invoke(true);
        }

        public override void Hide()
        {
            active = false;
            adState = AdState.Hidden;
        }

        public override void DestroyAd()
        {
            active = false;
            if (adState == AdState.Destroyed) return;
            appOpenAd?.Destroy();
            adState = AdState.Destroyed;
        }

        
    }
}
#endif