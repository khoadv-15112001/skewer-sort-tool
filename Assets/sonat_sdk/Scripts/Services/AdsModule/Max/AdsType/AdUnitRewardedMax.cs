#if using_max

using System;
using Sonat.Debugger;
#if using_aps
using AmazonAds;
#endif

namespace Sonat.AdsModule.Max
{
    public class AdUnitRewardedMax : AdUnitMax
    {
        public override AdType AdType => AdType.Rewarded;

        private Action<bool> callback;

        public override void Initialize(AdUnitId adUnitId)
        {
            base.Initialize(adUnitId);
            MaxSdkCallbacks.Rewarded.OnAdLoadedEvent += HandleOnAdLoaded;
            MaxSdkCallbacks.Rewarded.OnAdLoadFailedEvent += HandleOnAdFailedToLoad;
            MaxSdkCallbacks.Rewarded.OnAdDisplayedEvent += HandleOnAdOpened;
            MaxSdkCallbacks.Rewarded.OnAdClickedEvent += HandleAdClickedEvent;
            MaxSdkCallbacks.Rewarded.OnAdHiddenEvent += HandleOnAdClosed;
            MaxSdkCallbacks.Rewarded.OnAdDisplayFailedEvent += HandleOnAdFailedToShow;
            MaxSdkCallbacks.Rewarded.OnAdRevenuePaidEvent += HandleAdPaidEvent;
            MaxSdkCallbacks.Rewarded.OnAdReceivedRewardEvent += OnAdReceivedRewardEvent;
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
            MaxSdk.LoadRewardedAd(adId);
#else
        LoadRewardVideoWithAPS();
#endif
        }

        public override bool Ready()
        {
            return MaxSdk.IsRewardedAdReady(adId) && adState is AdState.Loaded or AdState.Hidden;
        }

        public override void Show(AdPlacement placement, Action<bool> callback)
        {
            this.Placement = placement;
            active = true;
            if (!Ready())
            {
                SonatDebugType.Ads.Log($"{AdType} is not ready - {adState}");
                callback(false);
                return;
            }

            SonatAds.SetFullScreenAd(true);
            this.callback = callback;
            adState = AdState.Showing;
            SonatSdkManager.DelayCall(SonatAds.Config.TimeShowLoading, () =>
            {
                MaxSdk.ShowRewardedAd(adId);
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

        private void OnAdReceivedRewardEvent(string id, MaxSdkBase.Reward reward, MaxSdkBase.AdInfo adInfo)
        {
            callback?.Invoke(true);
            EarnedRewardData data = new EarnedRewardData(this);
            OnEarnedReward?.Invoke(data);
        }


#if using_aps
        private bool IsFirstLoadReward = true;
        private void LoadRewardVideoWithAPS()
        {
            if (IsFirstLoadReward && !string.IsNullOrEmpty(apsId1))
            {
                IsFirstLoadReward = false;

                var rewardedVideoAd = new APSVideoAdRequest(320, 480, apsId1);
                rewardedVideoAd.onSuccess += (adResponse) =>
                {
                    MaxSdk.SetRewardedAdLocalExtraParameter(adId, "amazon_ad_response", adResponse.GetResponse());
                    MaxSdk.LoadRewardedAd(adId);
                };
                rewardedVideoAd.onFailedWithError += (adError) =>
                {
                    MaxSdk.SetRewardedAdLocalExtraParameter(adId, "amazon_ad_error", adError.GetAdError());
                    MaxSdk.LoadRewardedAd(adId);
                };

                rewardedVideoAd.LoadAd();
            }
            else
            {
                MaxSdk.LoadRewardedAd(adId);
            }
        }
#endif
    }
}
#endif