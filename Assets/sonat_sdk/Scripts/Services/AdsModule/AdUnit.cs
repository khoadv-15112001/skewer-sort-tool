using System;
using Sonat.Debugger;
using Sonat.FirebaseModule;
using UnityEngine;

namespace Sonat.AdsModule
{
    public abstract class AdUnit
    {
        public class Data
        {
            public string mediationAdapter;
            public string responseId;
        }
        
        public abstract MediationType Mediation { get; }
        public AdPlacement Placement { get; protected set; }
        public abstract AdType AdType { get; }
        protected string adId;
        protected AdState adState;
        public bool active = true;
        
#if using_aps
        protected string apsId1, apsId2;
        public void SetApsId(string apsId1, string apsId2)
        {
            this.apsId1 = apsId1;
            this.apsId2 = apsId2;
        }
#endif

        public Data data = new Data();

        #region Events

        public Action<AdLoadedData> OnAdLoaded;
        public Action<AdLoadFailedData> OnAdLoadFailed;
        public Action<AdOpenedData> OnAdOpened;
        public Action<AdShowFailedData> OnAdShowFailed;
        public Action<AdClosedData> OnAdClosed;
        public Action<AdClickedData> OnAdClicked;
        public Action<EarnedRewardData> OnEarnedReward;
        public Action<AdPaidData> OnAdPaid;

        #endregion

        public int retryAttempt;

        public virtual void Initialize(AdUnitId adUnitId)
        {
            this.adId = SonatFirebase.remote.GetValueByLevel($"by_level_{Mediation}_{AdType}_id".ToLower(),
                SonatFirebase.remote.GetRemoteString($"{Mediation}_{AdType}_id", adUnitId.id));
           this.Placement = adUnitId.placement;
            
            SonatDebugType.Ads.Log($"Created Ad Unit {Placement} - {Mediation}: {adId}");
        }

        protected virtual bool PreCheck()
        {
            if (string.IsNullOrEmpty(adId))
            {
                SonatDebugType.Ads.LogError($"Request {Placement} failed empty id!");
                return false;
            }

            if (!SonatSdkManager.IsInternetConnection())
            {
                SonatDebugType.Ads.LogError($"Request {Placement} failed no internet!");
                return false;
            }

            if (!SonatAds.ConsentReady)
            {
                SonatDebugType.Ads.LogError($"Request {Placement} Consent is not ready!");
                return false;
            }

            return true;
        }

        protected virtual void RetryRequestAds()
        {
            float delay = Mathf.Pow(2, Mathf.Clamp(retryAttempt, 1, 5));
            SonatSdkUtils.DoActionDelay(RequestAds, delay);
            retryAttempt++;
        }

        public abstract void RequestAds();
        public abstract bool Ready();
        public abstract void Show(AdPlacement placement, Action<bool> callback);
        public abstract void Hide();
        public abstract void DestroyAd();
    }
}