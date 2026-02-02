using Sonat.Debugger;

#if using_max
namespace Sonat.AdsModule.Max
{
    public abstract class AdUnitMax : AdUnit
    {
        public override MediationType Mediation => MediationType.Max;
        protected bool registed;

        #region Handle

        protected virtual void HandleOnAdLoaded(string id, MaxSdkBase.AdInfo adInfo)
        {
            if (id != this.adId) return;
            SonatDebugType.Ads.Log($"MAX {AdType} - {adId} loaded");
            adState = AdState.Loaded;
            retryAttempt = 0;
            this.data.responseId = adInfo.AdUnitIdentifier;
            this.data.mediationAdapter = adInfo.NetworkName;
            AdLoadedData eventData = new AdLoadedData(this);
            OnAdLoaded?.Invoke(eventData);
        }

        protected virtual void HandleOnAdFailedToLoad(string id, MaxSdkBase.ErrorInfo errorInfo)
        {
            if (id != this.adId) return;
            SonatDebugType.Ads.Log($"MAX {AdType} - {adId} failed to load {errorInfo.MediatedNetworkErrorCode} - {errorInfo.Message} - {errorInfo.Code}");
            adState = AdState.Failed;
            AdLoadFailedData data = new AdLoadFailedData(this, errorInfo.MediatedNetworkErrorMessage, errorInfo.MediatedNetworkErrorCode);
            OnAdLoadFailed?.Invoke(data);
            DestroyAd();
            RetryRequestAds();
        }

        // protected void HandleAdOpeningEvent(string adUnitId, MaxSdkBase.AdInfo adInfo)
        // {
        //
        // }

        protected virtual void HandleAdClickedEvent(string adUnitId, MaxSdkBase.AdInfo adInfo)
        {
            if (adUnitId != this.adId) return;
            SonatDebugType.Ads.Log($"MAX {AdType} - {adId} clicked");
            AdClickedData data = new AdClickedData(this);
            OnAdClicked?.Invoke(data);
        }


        protected virtual void HandleOnAdFailedToShow(string id, MaxSdkBase.ErrorInfo errorInfo, MaxSdkBase.AdInfo adInfo)
        {
            if (id != this.adId) return;
            SonatDebugType.Ads.Log($"MAX {AdType} - {adId} failed to show {errorInfo.MediatedNetworkErrorCode} - {errorInfo.Message} - {errorInfo.Code}");
            adState = AdState.Failed;
            AdShowFailedData data = new AdShowFailedData(this, errorInfo.MediatedNetworkErrorCode);
            OnAdShowFailed?.Invoke(data);
            DestroyAd();
            RetryRequestAds();
        }

        protected virtual void HandleOnAdOpened(string id, MaxSdkBase.AdInfo adInfo)
        {
            ;
            if (id != this.adId) return;
            SonatDebugType.Ads.Log($"MAX {AdType} - {adId} opened");
            AdOpenedData data = new AdOpenedData(this);
            OnAdOpened?.Invoke(data);
        }

        protected virtual void HandleOnAdClosed(string adUnitId, MaxSdkBase.AdInfo adInfo)
        {
            if (adUnitId != this.adId) return;
            adState = AdState.Closed;
            AdClosedData data = new AdClosedData(this);
            OnAdClosed?.Invoke(data);
            DestroyAd();
            RetryRequestAds();
        }

        protected virtual void HandleOnAdLeavingApplication(string id, MaxSdkBase.AdInfo adInfo)
        {
        }

        protected void HandleAdPaidEvent(string id, MaxSdkBase.AdInfo adInfo)
        {
            if (id != this.adId) return;
            AdPaidData data = new AdPaidData(this, adInfo.Revenue, adInfo.RevenuePrecision, "USD");
            OnAdPaid?.Invoke(data);
        }

        #endregion
    }
}
#endif