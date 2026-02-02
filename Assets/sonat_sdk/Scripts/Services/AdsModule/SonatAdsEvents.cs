using System.Collections.Generic;
using Sonat.AppsFlyerModule;
using Sonat.Data;
using Sonat.Debugger;
using Sonat.FirebaseModule;
using Sonat.TrackingModule;

namespace Sonat.AdsModule
{
    public class SonatAdsEvents
    {
        private SonatAds ads;
        private AdDurationTracker adDurationTracker;
        public SonatLogVideoRewarded rewardedVideoLog;

        private string GetShowAdPlacement(AdPlacement placement)
        {
            if (ads.adShowPlacement.TryGetValue(placement, out string placementStr) && !string.IsNullOrEmpty(placementStr))
            {
                return placementStr;
            }

            return placement.ToString().ToTrackingName();
        }

        public SonatAdsEvents(SonatAds ads)
        {
            this.ads = ads;
            adDurationTracker = AdDurationTracker.Instance;
        }

        #region Handle

        public void OnAdLoaded(AdLoadedData data)
        {
            SonatDebugType.Ads.Log($"{data.adUnit.AdType} Loaded");

            if (SonatAds.needShowAppOpenAds && SonatAds.CheckAdPlacement(data.adUnit, AdPlacement.AppOpen))
            {
                if (UserData.FirstPlay.BoolValue)
                {
                    UserData.FirstPlay.BoolValue = false;
                    return;
                }

                SonatAds.needShowAppOpenAds = false;
                SonatAds.ShowAd(AdPlacement.AppOpen, "app_open", null, null);
            }
        }

        public void OnAdFailedToLoad(AdLoadFailedData data)
        {
            SonatDebugType.Ads.LogError($"{data.adUnit.Placement} Load Failed: {data.message}");
            var parameters = new[]
            {
                new LogParameter("ad_unit", data.adUnit.AdType.ToString()),
                new LogParameter("error_message", data.message),
                new LogParameter("error_code", data.errorCode)
            };

            SonatFirebase.analytic.LogEvent($"{data.adUnit.Mediation}_ad_load_fail".ToTrackingName(), parameters);
        }

        public void OnAdOpened(AdOpenedData data)
        {
            SonatDebugType.Ads.Log($"{data.adUnit.Placement} Opened");
        }

        public void OnAdClosed(AdClosedData data)
        {
            SonatAds.SetFullScreenAd(false);
            SonatDebugType.Ads.Log($"{data.adUnit.Placement} Closed");

            var parameter = new LogParameter("ad_source", data.adUnit.data.mediationAdapter);
            SonatFirebase.analytic.LogEvent($"{data.adUnit.Mediation}_ad_close".ToTrackingName(), parameter);
            if (data.adUnit.Placement is not AdPlacement.Banner and AdPlacement.Native)
            {
                new SonatLogLastScreenView().Post();
            }

            if (data.adUnit.Placement == AdPlacement.Rewarded) UserData.RewardedCount.Value++;
            else if (data.adUnit.Placement == AdPlacement.Interstitial) UserData.InterstitialCount.Value++;
            
            SonatAds.OnAdShowComplete?.Invoke(data.adUnit.Placement);
        }

        public void OnAdFailedToShow(AdShowFailedData data)
        {
            SonatAds.SetFullScreenAd(false);
            SonatDebugType.Ads.Log($"{data.adUnit.Placement} Show Failed");

            var parameters = new[]
            {
                new LogParameter("ad_source", data.adUnit.data.mediationAdapter),
                new LogParameter("ad_response_id", data.adUnit.data.responseId),
                new LogParameter("error_code", data.errorCode),
            };
            SonatFirebase.analytic.LogEvent($"{data.adUnit.Mediation}_ad_open_fail".ToTrackingName(), parameters);
            if (data.adUnit.Placement is not AdPlacement.Banner and AdPlacement.Native)
            {
                new SonatLogLastScreenView().Post();
            }
        }

        public void OnAdClicked(AdClickedData data)
        {
            SonatDebugType.Ads.Log($"{data.adUnit.Placement} Clicked");

            new SonatPaidAdClick()
            {
                ad_format = data.adUnit.AdType.ToAdTypeLog(),
                ad_placement = GetShowAdPlacement(data.adUnit.Placement),
                fb_instance_id = UserData.FirebaseInstanceId.Value
            }.Post();
            adDurationTracker.OnUserClickedAd();
        }

        public void OnEarnedReward(EarnedRewardData data)
        {
            SonatDebugType.Ads.Log($"{data.adUnit.Placement} Earned Reward");

            var parameter = new[]
            {
                new LogParameter("ad_source", data.adUnit.data.mediationAdapter),
                new LogParameter("ad_response_id", data.adUnit.data.responseId),
            };
            SonatFirebase.analytic.LogEvent($"{data.adUnit.Mediation}_reward_earn".ToTrackingName(), parameter);

            rewardedVideoLog?.Post();
            SonatLogRecursive.CheckLogCompleteRewardedAdsUA();

            SonatAppsFlyer.SendEvent(EventNameEnumForAf.video_rewarded.ToString());
            SonatAppsFlyer.SendEvent(EventNameEnumForAf.af_ad_view.ToString(), new Dictionary<string, string>()
            {
                { ParameterEnum.af_adrev_ad_type.ToString(), EventNameEnumForAf.video_rewarded.ToString() }
            });
        }

        public void OnAdPaid(AdPaidData data)
        {
            SonatDebugType.Ads.Log($"{data.adUnit.Placement} Paid");
            var parameters = new[]
            {
                new LogParameter("ad_source", data.adUnit.data.mediationAdapter),
                new LogParameter("ad_value", data.value.ToString()),
                new LogParameter("ad_response_id", data.adUnit.data.responseId),
            };
            //SonatFirebase.analytic.LogEvent($"{data.adUnit.Mediation}_ad_open_success".ToTrackingName(), parameters);


            SonatAnalyticTracker.LogRevenue(data.adUnit.Mediation.ToAdsPlatform(), data.adUnit.data.mediationAdapter, data.value,
                data.precision, data.adUnit.AdType.ToAdTypeLog(), UserData.FirebaseInstanceId.Value,
                GetShowAdPlacement(data.adUnit.Placement), data.currencyCode);

            if (data.adUnit.Placement != AdPlacement.Banner)
            {
                AdDurationTracker.AdMetrics adMetrics = adDurationTracker.OnAdDismissedFullScreenContent();
                new SonatLogAdDuration(data.adUnit.Mediation.ToAdsPlatform().ToString(), data.adUnit.AdType.ToAdTypeLog().ToString(),
                    data.adUnit.data.mediationAdapter,
                    data.value.ToString(), adMetrics).Post();
            }

            if (data.adUnit.Placement != AdPlacement.Banner && data.adUnit.Placement != AdPlacement.Native)
            {
                SonatLogRecursive.CheckLogPaidAdsImpressionUA();
            }
        }

        #endregion
    }
}