#if using_max

using System;
#if using_aps
using AmazonAds;
#endif

namespace Sonat.AdsModule.Max
{
    public class AdUnitBannerMax : AdUnitMax
    {
        public override AdType AdType => AdType.Banner;

        public override void Initialize(AdUnitId adUnitId)
        {
            base.Initialize(adUnitId);
            MaxSdkCallbacks.Banner.OnAdLoadedEvent += HandleOnAdLoaded;
            MaxSdkCallbacks.Banner.OnAdLoadFailedEvent += HandleOnAdFailedToLoad;
            MaxSdkCallbacks.Banner.OnAdClickedEvent += HandleAdClickedEvent;
            MaxSdkCallbacks.Banner.OnAdRevenuePaidEvent += HandleAdPaidEvent;
        }

        public override void RequestAds()
        {
            if (adState is AdState.Requesting or AdState.Loaded or AdState.Showing or AdState.Hidden) return;

            if (!PreCheck())
            {
                RetryRequestAds();
                return;
            }

            if (!registed)
            {
#if !using_aps || UNITY_EDITOR
                MaxSdk.CreateBanner(adId, MaxSdkBase.BannerPosition.BottomCenter);
#else
            LoadBannerWithAPS();
#endif
                registed = true;
            }
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

            MaxSdk.ShowBanner(adId);
            adState = AdState.Showing;

            callback?.Invoke(true);
        }

        public override void Hide()
        {
            active = false;
            if (adState == AdState.Hidden) return;
            MaxSdk.HideBanner(adId);
            adState = AdState.Hidden;
        }

        public override void DestroyAd()
        {
            active = false;
            if (adState == AdState.Destroyed) return;
            MaxSdk.DestroyBanner(adId);
            adState = AdState.Destroyed;
        }

        public float GetBannerHeight()
        {
            return MaxSdk.GetBannerLayout(adId).height;
        }

        #region Handle

        protected override void HandleOnAdLoaded(string id, MaxSdkBase.AdInfo adInfo)
        {
            if(id != this.adId) return;
            base.HandleOnAdLoaded(id, adInfo);

            if (!active)
            {
                Hide();
            }
            else
            {
                Show(Placement, null);
            }
        }

        #endregion

#if using_aps
        private void LoadBannerWithAPS()
        {
            int width;
            int height;
            string apsId;
            if (MaxSdkUtils.IsTablet())
            {
                width = 728;
                height = 90;
                apsId = apsId1;
            }
            else
            {
                width = 320;
                height = 50;
                apsId = apsId2;
            }

            if (string.IsNullOrEmpty(apsId))
            {
                CreateMaxBannerAd();
                return;
            }
            var apsBanner = new APSBannerAdRequest(width, height, apsId);
            apsBanner.onSuccess += (adResponse) =>
            {
                MaxSdk.SetBannerLocalExtraParameter(adId, "amazon_ad_response", adResponse.GetResponse());
                CreateMaxBannerAd();
            };
            apsBanner.onFailedWithError += (adError) =>
            {
                MaxSdk.SetBannerLocalExtraParameter(adId, "amazon_ad_error", adError.GetAdError());
                CreateMaxBannerAd();
            };

            apsBanner.LoadAd();
        }

        private void CreateMaxBannerAd()
        {
            MaxSdk.CreateBanner(adId, MaxSdkBase.BannerPosition.BottomCenter);
        }

#endif
    }
}

#endif