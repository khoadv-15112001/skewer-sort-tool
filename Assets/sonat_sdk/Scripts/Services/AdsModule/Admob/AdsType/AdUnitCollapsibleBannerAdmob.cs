#if using_admob
using GoogleMobileAds.Api;

namespace Sonat.AdsModule.Admob
{
    public class AdUnitCollapsibleBannerAdmob: AdUnitBannerAdmob
    {
        public override AdType AdType => AdType.CollapsibleBanner;
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
            request.Extras.Add("collapsible", "bottom");
            bannerView.LoadAd(request);
        }
    }
}
#endif