#if using_max
using System;

namespace Sonat.AdsModule.Max
{
    public class AdUnitAppOpenMax : AdUnitMax
    {
        public override AdType AdType => AdType.AppOpenAd;

        public override void Initialize(AdUnitId adUnitId)
        {
            base.Initialize(adUnitId);
            
            MaxSdkCallbacks.AppOpen.OnAdLoadedEvent += HandleOnAdLoaded;
            MaxSdkCallbacks.AppOpen.OnAdLoadFailedEvent += HandleOnAdFailedToLoad;
            MaxSdkCallbacks.AppOpen.OnAdDisplayedEvent += HandleOnAdOpened;
            MaxSdkCallbacks.AppOpen.OnAdClickedEvent += HandleAdClickedEvent;
            MaxSdkCallbacks.AppOpen.OnAdHiddenEvent += HandleOnAdClosed;
            MaxSdkCallbacks.AppOpen.OnAdDisplayFailedEvent += HandleOnAdFailedToShow;
            MaxSdkCallbacks.AppOpen.OnAdRevenuePaidEvent += HandleAdPaidEvent;
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
            MaxSdk.LoadAppOpenAd(adId);
        }

        public override bool Ready()
        {
            return MaxSdk.IsAppOpenAdReady(adId) && adState is AdState.Loaded or AdState.Hidden;
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
            MaxSdk.ShowAppOpenAd(adId);
            callback?.Invoke(true);
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
    }
}
#endif
