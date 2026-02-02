#if using_admob
using System;
using GoogleMobileAds.Api;

namespace Sonat.AdsModule.Admob
{
    public class AdUnitBannerAdmob : AdUnitAdmob, IAdUnitBanner
    {
        public override AdType AdType => AdType.Banner;
        
        protected BannerView bannerView;

        public override void Initialize(AdUnitId adUnitId)
        {
            base.Initialize(adUnitId);
        }
        
        protected virtual AdSize GetAdSize()
        {
            return AdSize.GetCurrentOrientationAnchoredAdaptiveBannerAdSizeWithWidth(AdSize.FullWidth);
        }

        public override void RequestAds()
        {
            if (adState is AdState.Requesting or AdState.Loaded or AdState.Showing or AdState.Hidden) return;
            
            if (!PreCheck())
            {
                RetryRequestAds();
                return;
            }
            
            bannerView?.Destroy();

            bannerView = new BannerView(adId, GetAdSize(), AdPosition.Bottom);
            bannerView.OnBannerAdLoaded += OnBannerLoaded;
            bannerView.OnBannerAdLoadFailed += HandleOnAdFailedToLoad;
            bannerView.OnAdFullScreenContentOpened += HandleOnAdOpened;
            bannerView.OnAdFullScreenContentClosed += HandleOnAdClosed;
            bannerView.OnAdClicked += HandleOnAdClicked;
            bannerView.OnAdPaid += HandleOnAdPaid;

            adState = AdState.Requesting;
            AdRequest request = new AdRequest();
            bannerView.LoadAd(request);
        }

        public override bool Ready()
        {
            return true;
        }

        private bool CanShow()
        {
            return adState is AdState.Loaded or AdState.Hidden;
        }

        public override void Show(AdPlacement placement, Action<bool> callback)
        {
            this.Placement = placement;
            active = true;
            if (!CanShow())
            {
                callback?.Invoke(false);
                return;
            }

            adState = AdState.Showing;
            bannerView?.Show();

            callback?.Invoke(true);
        }

        public override void Hide()
        {
            active = false;
            if (adState == AdState.Hidden) return;
            bannerView?.Hide();
            adState = AdState.Hidden;
        }

        public override void DestroyAd()
        {
            active = false;
            if (adState == AdState.Destroyed) return;
            bannerView?.Destroy();
            adState = AdState.Destroyed;
        }

        public float GetBannerHeight()
        {
            return bannerView?.GetHeightInPixels() ?? 0;
        }

        #region Handle

        protected void OnBannerLoaded()
        {
            HandleOnAdLoaded(bannerView.GetResponseInfo());
        }

        protected override void HandleOnAdLoaded(ResponseInfo responseInfo)
        {
            base.HandleOnAdLoaded(responseInfo);
            
            if (!active)
            {
                Hide();
            }
            else
            {
                adState = AdState.Showing;
                active = true;
            }
        }
        

        #endregion

    }
}
#endif