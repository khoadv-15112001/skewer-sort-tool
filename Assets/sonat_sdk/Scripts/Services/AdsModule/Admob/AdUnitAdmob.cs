#if using_admob
using GoogleMobileAds.Api;
using Sonat.Debugger;

namespace Sonat.AdsModule.Admob
{
    public abstract class AdUnitAdmob : AdUnit
    {

        public override MediationType Mediation => MediationType.Admob;

        #region Handler
        protected virtual void HandleOnAdLoaded(ResponseInfo responseInfo)
        {
            adState = AdState.Loaded;
            SonatDebugType.Ads.Log($"Admob {AdType} - {adId} loaded");
            retryAttempt = 0;
            this.data.responseId = responseInfo.GetResponseId();
            this.data.mediationAdapter = responseInfo.GetMediationAdapterClassName();
            AdLoadedData eventData = new AdLoadedData(this);
            OnAdLoaded?.Invoke(eventData);
        }

        protected virtual void HandleOnAdFailedToLoad(AdError adError)
        {
            adState = AdState.Failed;
            SonatDebugType.Ads.Log($"Admob {AdType} - {adId} failed to load with error {adError.GetMessage()}");
            AdLoadFailedData data = new AdLoadFailedData(this, adError.GetMessage(), adError.GetCode());
            OnAdLoadFailed?.Invoke(data);
            RetryRequestAds();
        }

        protected virtual void HandleOnAdOpened()
        {
            //throw new System.NotImplementedException();
            SonatDebugType.Ads.Log($"Admob {AdType} - {adId} opened");
            AdOpenedData data = new AdOpenedData(this);
            OnAdOpened?.Invoke(data);
        }

        protected virtual void HandleOnAdFailedToShow(AdError adError)
        {
            adState = AdState.Failed;
            SonatDebugType.Ads.Log($"Admob {AdType} - {adId} failed to show with error {adError.GetMessage()}");
            AdShowFailedData data = new AdShowFailedData(this, adError.GetCode());
            OnAdShowFailed?.Invoke(data);
            DestroyAd();
            RetryRequestAds();
        }

        protected virtual void HandleOnAdClosed()
        {
            adState = AdState.Closed;
            SonatDebugType.Ads.Log($"Admob {AdType} - {adId} closed");
            AdClosedData data = new AdClosedData(this);
            OnAdClosed?.Invoke(data);
            DestroyAd();
            RetryRequestAds();
        }

        protected virtual void HandleOnAdClicked()
        {
            AdClickedData data = new AdClickedData(this);
            OnAdClicked?.Invoke(data);
        }

        protected virtual void HandleOnAdPaid(AdValue adValue)
        {
            double revenue = adValue.Value / ((double)1000000f);
            AdPaidData data = new AdPaidData(this, revenue, adValue.Precision.ToString(), adValue.CurrencyCode);
            OnAdPaid?.Invoke(data);
        }
        

        #endregion

    }
}
#endif