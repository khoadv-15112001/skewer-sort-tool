namespace Sonat.AdsModule
{
    public enum MediationType
    {
        None,
        Admob,
        Max,
        IronSource,
        All
    }

    public enum NetworkType
    {
    }

    public enum AdType
    {
        Interstitial,
        Rewarded,
        Banner,
        LargeBanner,
        NativeAds,
        AppOpenAd,
        CollapsibleBanner,
        MREC,
#if using_aps
        InterstitialVideo,
#endif
    }

    public enum AdPlacement
    {
        Interstitial,
        Rewarded,
        Banner,
        Native,
        AppOpen,
        OnFocus,
        AdsBreak
    }

    public enum AdState
    {
        NotStart,
        Requesting,
        Failed,
        Loaded,
        Showing,
        Closed,
        Hidden,
        Destroyed,
    }
}