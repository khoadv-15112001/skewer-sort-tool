using System;
using System.Collections.Generic;
#if using_appsflyer
using AppsFlyerSDK;
#endif
using UnityEngine;

namespace Sonat.TrackingModule
{
    public static class SonatTrackingHelper
    {
        public static readonly List<SonatLogBase> NotReadyQueues = new List<SonatLogBase>();

        public static bool TryToPostQueues()
        {
            if (SonatAnalyticTracker.FirebaseReady)
            {
                foreach (var log in NotReadyQueues)
                    log.Post(log.PostAf);
                NotReadyQueues.Clear();
                return true;
            }

            return false;
        }

        public static void CrossAppPromotionClick(string appId, string campaign, Dictionary<string, string> parameters, MonoBehaviour go)
        {
#if using_appsflyer
            AppsFlyer.attributeAndOpenStore(appId, campaign, parameters, go);
#endif
        }

        public static void CrossAppPromotionImpression(string promotedAppID, string campaign, Dictionary<string, string> parameters)
        {
#if using_appsflyer
            AppsFlyer.recordCrossPromoteImpression(promotedAppID, campaign, parameters);
#endif
        }

        private static string GetDefault(AdsPlatform platform)
        {
            switch (platform)
            {
                case AdsPlatform.applovinmax:
                    return platform.ToString();
                case AdsPlatform.googleadmob:
                    return "admob";
                case AdsPlatform.ironsource:
                    return platform.ToString();
                default:
                    throw new ArgumentOutOfRangeException(nameof(platform), platform, null);
            }
        }

        private static readonly Dictionary<string, string> mediationDict = new Dictionary<string, string>()
        {
            { "googleadmanager", "googleadmanager" },
            { "admob", "admob" },
            { "applovin", "applovinmax" },
            { "max", "applovinmax" },
            { "fyber", "fyber" },
            { "appodeal", "appodeal" },
            { "inmobi", "inmobi" },
            { "vungle", "vungle" },
            { "admost", "admost" },
            { "topon", "topon" },
            { "tradplus", "tradplus" },
            { "chartboost", "chartboost" },
            { "facebook", "facebook" },
            { "meta", "facebook" },
            { "mintegral", "mintegral" },
            { "mtg", "mintegral" },
            { "ironsource", "ironsource" },
            { "unity", "unity" },
            { "pangle", "pangle" },
            { "bytedance", "bytedance" },
            { "bidmachine", "bidmachine" },
            { "liftoff", "liftoff" },
            { "mytarget", "mytarget" },
            { "smaato", "smaato" },
            { "tapjoy", "tapjoy" },
            { "verve", "verve" },
            { "yahoo", "yahoo" },
            { "yandex", "yandex" },
            { "google", "admob" },
        };

        public static string FindNetworkName(string splitLower)
        {
            foreach (var keyValuePair in mediationDict)
                if (splitLower.Contains(keyValuePair.Key))
                    return keyValuePair.Value;

            return null;
        }


        public static string GetNetworkName(string fullNetworkName, AdsPlatform platform)
        {
            if (string.IsNullOrEmpty(fullNetworkName))
                return GetDefault(platform);

            var split = fullNetworkName.Split('.');
            var lower = split[split.Length - 1].ToLower();
            return FindNetworkName(lower) ?? GetDefault(platform);
        }
    }
}