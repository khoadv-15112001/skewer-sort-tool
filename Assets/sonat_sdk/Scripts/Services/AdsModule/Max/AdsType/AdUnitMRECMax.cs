#if using_max
using System;
using Sonat.Debugger;
using UnityEngine;

namespace Sonat.AdsModule.Max
{
    public class AdUnitMRECMax : AdUnitMax
    {
        public override AdType AdType => AdType.MREC;

        protected float mrecPosX = 0;
        protected float mrecPosY;
        private MaxSdkBase.AdViewPosition mrecPos = MaxSdkBase.AdViewPosition.BottomCenter;

        public override void Initialize(AdUnitId adUnitId)
        {
            base.Initialize(adUnitId);
            mrecPosY = SonatAds.Config.nativePosY;
            
            MaxSdkCallbacks.MRec.OnAdLoadedEvent += HandleOnAdLoaded;
            MaxSdkCallbacks.MRec.OnAdLoadFailedEvent += HandleOnAdFailedToLoad;
            MaxSdkCallbacks.MRec.OnAdClickedEvent += HandleAdClickedEvent;
            MaxSdkCallbacks.MRec.OnAdRevenuePaidEvent += HandleAdPaidEvent;
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
                registed = true;
            
                if (mrecPos == MaxSdkBase.AdViewPosition.BottomCenter && mrecPosY != 0)
                {
                    //MRECs are sized to 300x250 on phones and tablets
                    float density = MaxSdkUtils.GetScreenDensity();
                    Vector2 bannerSize = new Vector2(300.0f, 250.0f);

                    float dpWidth = Screen.safeArea.width / density;
                    float dpheight = Screen.safeArea.height / density;

                    //Banners are automatically sized to 320×50 on phones and 728×90 on tablets
                    float offsetHeight = (mrecPosY * (Screen.height / 1920.0f)) + (MaxSdkUtils.IsTablet() ? 90.0f : 50.0f);

                    MaxSdk.CreateMRec(adId, (dpWidth - bannerSize.x) / 2, (dpheight - bannerSize.y - offsetHeight));
                }
                else if (mrecPosX == 0 && mrecPosY == 0)
                {
                    MaxSdk.CreateMRec(adId, mrecPos);
                }
                else
                {
                    MaxSdk.CreateMRec(adId, mrecPosX, mrecPosY);
                }
            }

            adState = AdState.Requesting;
        
        }

        public override bool Ready()
        {
            return adState is AdState.Loaded or AdState.Hidden;
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

            SonatDebugType.Ads.Log("Start Show MREC");
            MaxSdk.ShowMRec(adId);
            adState = AdState.Showing;

            callback?.Invoke(true);
        }

        public override void Hide()
        {
            active = false;
            if (adState == AdState.Hidden) return;
            MaxSdk.HideMRec(adId);
            adState = AdState.Hidden;
        }

        public override void DestroyAd()
        {
            active = false;
            if (adState == AdState.Destroyed) return;
            MaxSdk.DestroyMRec(adId);
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
                adState = AdState.Showing;
                active = true;
            }
        }

        #endregion
    }
}
#endif