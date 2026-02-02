#if using_admob
using System;
using GoogleMobileAds.Api;

namespace Sonat.AdsModule.Admob
{
    public class AdUnitRewardedAdmob : AdUnitAdmob
    {
        public override AdType AdType => AdType.Rewarded;

        private RewardedAd rewardedAd;
        private Action<bool> callback;

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
            
            rewardedAd?.Destroy();

            adState = AdState.Requesting;
            AdRequest adRequest = new AdRequest();
            RewardedAd.Load(adId, adRequest,
                (ad, error) =>
                {
                    // if error is not null, the load request failed.
                    if (error != null || ad == null)
                    {
                        HandleOnAdFailedToLoad(error);
                        return;
                    }

                    rewardedAd = ad;
                    HandleOnAdLoaded(rewardedAd.GetResponseInfo());
                    rewardedAd.OnAdFullScreenContentOpened += HandleOnAdOpened;
                    rewardedAd.OnAdFullScreenContentClosed += HandleOnAdClosed;
                    rewardedAd.OnAdFullScreenContentFailed += HandleOnAdFailedToShow;
                    rewardedAd.OnAdClicked += HandleOnAdClicked;
                    rewardedAd.OnAdPaid += HandleOnAdPaid;
                });
        }

        public override bool Ready()
        {
            return rewardedAd != null && rewardedAd.CanShowAd() && adState is AdState.Loaded or AdState.Hidden;
        }

        public override void Show(AdPlacement placement, Action<bool> callback)
        {
            this.Placement = placement;
            active = true;
            if (!Ready())
            {
                callback(false);
                return;
            }

            SonatAds.SetFullScreenAd(true);
            this.callback = callback;
            adState = AdState.Showing;
            SonatSdkManager.DelayCall(SonatAds.Config.TimeShowLoading, () =>
            {
                rewardedAd.Show(OnRewarded);
            });
            
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
            rewardedAd?.Destroy();
            adState = AdState.Destroyed;
        }


        private void OnRewarded(Reward reward)
        {
            callback?.Invoke(true);
            EarnedRewardData data = new EarnedRewardData(this);
            OnEarnedReward?.Invoke(data);
        }


    }
}
#endif