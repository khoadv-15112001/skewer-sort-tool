#if using_admob && using_admob_native
using System;
using GoogleMobileAds.Api;
using Sonat.Debugger;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace Sonat.AdsModule.Admob
{
    public class AdUnitNativeAdmob : AdUnitAdmob
    {
        protected NativeAd nativeView;
        private GameObject adObject;
        public override AdType AdType => AdType.NativeAds;

        public override void RequestAds()
        {
            if (adState is AdState.Requesting or AdState.Loaded or AdState.Showing or AdState.Hidden) return;

            if (!PreCheck())
            {
                RetryRequestAds();
                return;
            }

            nativeView?.Destroy();

            AdLoader adLoader = new AdLoader.Builder(adId)
                .ForNativeAd()
                .Build();

            adLoader.OnNativeAdLoaded += HandleOnNativeAdLoaded;
            adLoader.OnAdFailedToLoad += HandleOnNativeAdFailedToLoad;

            adState = AdState.Requesting;
            AdRequest request = new AdRequest();
            adLoader.LoadAd(request);
        }

        public override bool Ready()
        {
            return adState is AdState.Loaded or AdState.Hidden;
        }

        public override void Show(Action<bool> callback)
        {
            
        }
        
        public void ShowNativeAds(GameObject adObject, Action<bool> callback)
        {
            if (!Ready() || nativeView == null)
            {
                callback?.Invoke(false);
                return;
            }
            
            this.adObject.SetActive(true);
            this.adObject = adObject;
            TextMeshProUGUI text = adObject.GetComponentInChildren<TextMeshProUGUI>();
            Image icon = adObject.GetComponentInChildren<Image>();
            
            Texture2D iconTexture = nativeView.GetIconTexture();
            string headline = nativeView.GetHeadlineText();

            icon.sprite = Sprite.Create(iconTexture, new Rect(0.0f, 0.0f, iconTexture.width, iconTexture.height), new Vector2(0.5f, 0.5f), 100.0f);
            text.text = headline;

            if (!nativeView.RegisterIconImageGameObject(adObject))
            {
                SonatDebugType.Ads.LogError("RegisterIconImageGameObject Failed!");
                callback?.Invoke(false);
                return;
            }
            active = true;
            adState = AdState.Showing;
            callback?.Invoke(true);
        }

        public override void Hide()
        {
            active = false;
            if (adState == AdState.Hidden) return;
            adState = AdState.Hidden;
            if (adObject != null)
            {
                adObject.SetActive(false);
                adObject = null;
            }
            RetryRequestAds();
        }

        public override void DestroyAd()
        {
            active = false;
            if (adState == AdState.Destroyed) return;
            adState = AdState.Destroyed;
            if (adObject != null)
            {
                Object.Destroy(adObject);
                adObject = null;
            }
            RetryRequestAds();
        }
        

        #region Handle

        protected virtual void HandleOnNativeAdLoaded(object sender, NativeAdEventArgs args)
        {
            this.nativeView = args.nativeAd;
            var responseInfo = args.nativeAd.GetResponseInfo();
            HandleOnAdLoaded(responseInfo);
        }

        protected virtual void HandleOnNativeAdFailedToLoad(object sender, AdFailedToLoadEventArgs args)
        {
            var adError = args.LoadAdError;
            HandleOnAdFailedToLoad(adError);
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