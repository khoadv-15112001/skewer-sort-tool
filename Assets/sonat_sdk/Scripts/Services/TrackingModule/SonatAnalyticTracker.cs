#if using_appsflyer
#endif

using System;
using System.Collections.Generic;
using System.Globalization;
#if using_appsflyer
using AppsFlyerSDK;
#endif
#if using_firebase_analytics
using Firebase.Analytics;
#endif
using Sonat.Debugger;
using Sonat.FirebaseModule;
using UnityEngine;

// ReSharper disable InconsistentNaming

namespace Sonat.TrackingModule
{
    public static class SonatAnalyticTracker
    {
        public static bool FirebaseReady { get; set; }

        public static float sn_ltv_iaa
        {
            get => PlayerPrefs.GetFloat(nameof(sn_ltv_iaa), 0);
            set
            {
                var last = PlayerPrefs.GetFloat(nameof(sn_ltv_iaa));
                if (Math.Abs(last - value) > 0.0001f)
                {
                    SonatFirebase.analytic.SetUserProperty(nameof(sn_ltv_iaa), value.ToString(CultureInfo.InvariantCulture));
                    PlayerPrefs.SetFloat(nameof(sn_ltv_iaa), value);
                }
            }
        }

        public static void LogRevenue(AdsPlatform platform, string adapter, double revenue, string precision,
            AdTypeLog adType, string fb_instance_id, string placement, string currencyCode = "USD")
        {
            LogFirebaseRevenue(platform, adapter, revenue, precision, adType.ToString(), fb_instance_id, placement, currencyCode);
            LogAppsFlyerAdRevenue(platform, adapter, revenue, adType.ToString(), fb_instance_id, placement, currencyCode);
            sn_ltv_iaa += (float)revenue;
        }


        public static void LogFirebaseRevenue(AdsPlatform platform, string adapter, double revenue, string precision,
            string adType, string fb_instance_id, string placement, string currencyCode = "USD")
        {
            if (!FirebaseReady) return;

#if using_firebase || using_firebase_analytics
            Parameter[] parameters =
            {
                new Parameter("valuemicros", revenue * 1000000f),
                new Parameter("value", (float)revenue),
                // These values below won’t be used in ROAS recipe.
                // But buyLog for purposes of debugging and future reference.
                new Parameter("currency", currencyCode),
                new Parameter("precision", precision),
                new Parameter(ParameterEnum.ad_format.ToString(), adType),
                new Parameter(ParameterEnum.fb_instance_id.ToString(), fb_instance_id),
                new Parameter(ParameterEnum.ad_placement.ToString(), placement),
                new Parameter(ParameterEnum.ad_source.ToString(), SonatTrackingHelper.GetNetworkName(adapter, platform)),
                new Parameter(ParameterEnum.ad_platform.ToString(), platform.ToString()),
            };
            FirebaseAnalytics.LogEvent(EventNameEnum.paid_ad_impression.ToString(), parameters);
            if (platform != AdsPlatform.googleadmob)
            {
                FirebaseAnalytics.LogEvent(EventNameEnum.ad_impression.ToString(), parameters);
            }
#endif
        }

        public static void LogAppsFlyerAdRevenue(AdsPlatform platform, string adapter, double revenue, string adType, string firebase_instance_id,
            string placement,
            string currencyCode = "USD")
        {
            Dictionary<string, string> dic = new Dictionary<string, string>();
            dic.Add("af_quantity", "1");
            dic.Add("ad_type", adType);
            dic.Add("ad_unit", adType);
            dic.Add("placement", placement);
            dic.Add("segment", placement);

            SonatDebugType.Tracking.Log($"logAdRevenue adapter:{adapter},platform:{platform},revenue{revenue}");
#if using_appsflyer
#if appsflyer_6_15_or_older
            AppsFlyerAdRevenue.logAdRevenue(SonatTrackingHelper.GetNetworkName(adapter, platform),
                GetNetworkType(platform), revenue, currencyCode, dic);
#else
            var logRevenue = new AFAdRevenueData(SonatTrackingHelper.GetNetworkName(adapter, platform), GetNetworkType(platform), currencyCode, revenue);
            AppsFlyer.logAdRevenue(logRevenue, dic);
#endif
#endif
        }

#if using_appsflyer
#if appsflyer_6_15_or_older
        private static AppsFlyerAdRevenueMediationNetworkType GetNetworkType(AdsPlatform platform)
        {
            switch (platform)
            {
                case AdsPlatform.applovinmax:
                    return AppsFlyerAdRevenueMediationNetworkType.AppsFlyerAdRevenueMediationNetworkTypeApplovinMax;
                case AdsPlatform.googleadmob:
                    return AppsFlyerAdRevenueMediationNetworkType.AppsFlyerAdRevenueMediationNetworkTypeGoogleAdMob;
                case AdsPlatform.ironsource:
                    return AppsFlyerAdRevenueMediationNetworkType.AppsFlyerAdRevenueMediationNetworkTypeIronSource;
                default:
                    throw new ArgumentOutOfRangeException(nameof(platform), platform, null);
            }
        }
#else
        private static MediationNetwork GetNetworkType(AdsPlatform platform)
        {
            switch (platform)
            {
                case AdsPlatform.applovinmax:
                    return MediationNetwork.ApplovinMax;
                case AdsPlatform.googleadmob:
                    return MediationNetwork.GoogleAdMob;
                case AdsPlatform.ironsource:
                    return MediationNetwork.IronSource;
                default:
                    throw new ArgumentOutOfRangeException(nameof(platform), platform, null);
            }
        }
#endif
#endif
    }
}