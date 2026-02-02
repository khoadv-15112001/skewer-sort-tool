using System.Collections.Generic;
using Sonat.AdsModule.Admob;
using Sonat.AdsModule.Max;

namespace Sonat.AdsModule
{
    public static class AdUnitFactory
    {
        private static Dictionary<string, AdUnit> adUnitsById = new Dictionary<string, AdUnit>();

        public static AdUnit CreateAdUnit(MediationType mediationType, AdUnitId adUnitId, out bool newUnit, bool forceNew = false)
        {
            if (!forceNew && adUnitsById.TryGetValue(adUnitId.id, out AdUnit adUnit))
            {
                newUnit = false;
                return adUnit;
            }

            newUnit = true;
            switch (mediationType)
            {
                case MediationType.Admob:
                    return CreateAdUnitAdmob(adUnitId);
                case MediationType.Max:
                    return CreateAdUnitMax(adUnitId);
            }

            return null;
        }

        private static AdUnit CreateAdUnitAdmob(AdUnitId adUnitId)
        {
            AdUnit adUnit = null;
            switch (adUnitId.adType)
            {
#if using_admob
                case AdType.Banner:
                    adUnit = new AdUnitBannerAdmob();
                    break;
                case AdType.Interstitial:
                    adUnit = new AdUnitInterstitialAdmob();
                    break;
                case AdType.Rewarded:
                    adUnit = new AdUnitRewardedAdmob();
                    break;
                case AdType.AppOpenAd:
                    adUnit = new AdUnitAppOpenAdmob();
                    break;
                case AdType.LargeBanner:
                    adUnit = new AdUnitLargeBannerAdmob();
                    break;
                case AdType.CollapsibleBanner:
                    adUnit = new AdUnitCollapsibleBannerAdmob();
                    break;
#if using_admob_native
                case AdType.NativeAds:
                    adUnit = new AdUnitNativeAdmob();
break;
#endif
#endif
                default:
                    adUnit = null;
                    break;
            }

            if (adUnit != null)
            {
                adUnit.Initialize(adUnitId);
                adUnitsById.Add(adUnitId.id, adUnit);
            }

            return adUnit;
        }

        private static AdUnit CreateAdUnitMax(AdUnitId adUnitId)
        {
            AdUnit adUnit = null;
            switch (adUnitId.adType)
            {
#if using_max
                case AdType.Banner:
                    adUnit = new AdUnitBannerMax();
                    break;
                case AdType.Interstitial:
                    adUnit = new AdUnitInterstitialMax();
                    break;
                case AdType.Rewarded:
                    adUnit = new AdUnitRewardedMax();
                    break;
                case AdType.AppOpenAd:
                    adUnit = new AdUnitAppOpenMax();
                    break;
                case AdType.MREC:
                    adUnit = new AdUnitMRECMax();
                    break;
                case AdType.NativeAds:
#endif
                default:
                    adUnit = null;
                    break;
            }

            if (adUnit != null)
            {
                adUnit.Initialize(adUnitId);
                adUnitsById.Add(adUnitId.id, adUnit);
            }

            return adUnit;
        }
    }
}