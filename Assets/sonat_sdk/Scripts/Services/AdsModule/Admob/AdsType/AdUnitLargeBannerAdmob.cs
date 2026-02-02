#if using_admob
using GoogleMobileAds.Api;
using UnityEngine;

namespace Sonat.AdsModule.Admob
{
    public class AdUnitLargeBannerAdmob: AdUnitBannerAdmob
    {
        public override AdType AdType => AdType.LargeBanner;

        public override void Initialize(AdUnitId adUnitId)
        {
            base.Initialize(adUnitId);
            active = false;
        }

        protected override AdSize GetAdSize()
        {
            return AdSize.MediumRectangle;
        }

        protected override void HandleOnAdLoaded(ResponseInfo responseInfo)
        {
            
            float dp = bannerView.GetWidthInPixels() / 300;
            float dpWidth = Screen.width / dp;
            float dpHeight = Screen.height / dp;
            float offsetDP = 80;
            bannerView.SetPosition((int)((dpWidth - AdSize.MediumRectangle.Width) / 2), (int)(dpHeight - AdSize.MediumRectangle.Height - offsetDP));
            base.HandleOnAdLoaded(responseInfo);
            
        }
    }
}
#endif