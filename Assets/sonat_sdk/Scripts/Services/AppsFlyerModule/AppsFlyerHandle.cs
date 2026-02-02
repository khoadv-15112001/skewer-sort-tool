#if using_appsflyer
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
#if appsflyer_6_16_or_older
using AppsFlyerConnector;
#endif
using AppsFlyerSDK;
using Sonat.Data;
using Sonat.Debugger;
using Sonat.FirebaseModule;
using Sonat.TrackingModule;
using UnityEngine;

namespace Sonat.AppsFlyerModule
{
    public class AppsFlyerHandle : MonoBehaviour, IAppsFlyerConversionData
    {
        private SonatAppsFlyer sonatAppsFlyer;

        public void Initialize(SonatAppsFlyer sonatAppsFlyer)
        {
            this.sonatAppsFlyer = sonatAppsFlyer;

            AppsFlyer.setCustomerUserId(SonatAppsFlyer.CustomerUserId);
            // These fields are set from the editor so do not modify!
            //******************************//
            AppsFlyer.setIsDebug(sonatAppsFlyer.isDebug);

            AppsFlyer.initSDK(sonatAppsFlyer.devKey.RemoveWhiteSpace(), sonatAppsFlyer.PlatformID.RemoveWhiteSpace(),
                sonatAppsFlyer.getConversionData ? this : null);

            //******************************/
#if appflyer_6_16_or_older
            AppsFlyerPurchaseConnector.init(this, AppsFlyerConnector.Store.GOOGLE);
#else
            AppsFlyerPurchaseConnector.init(this, Store.GOOGLE);
#endif
            AppsFlyerPurchaseConnector.setAutoLogPurchaseRevenue(
                AppsFlyerAutoLogPurchaseRevenueOptions.AppsFlyerAutoLogPurchaseRevenueOptionsAutoRenewableSubscriptions
                , AppsFlyerAutoLogPurchaseRevenueOptions.AppsFlyerAutoLogPurchaseRevenueOptionsInAppPurchases);
            AppsFlyerPurchaseConnector.setPurchaseRevenueValidationListeners(true);
            AppsFlyerPurchaseConnector.build();
            AppsFlyerPurchaseConnector.startObservingTransactions();


            var gdprUserConsent = AppsFlyerConsent.ForGDPRUser(true, true);
            AppsFlyer.setConsentData(gdprUserConsent);
            AppsFlyer.startSDK();


            SonatSdkUtils.DoActionDelay(() => this.sonatAppsFlyer.OnInitializeCompleted(), 2);
            SonatSdkUtils.WaitActionUntil(() => SonatAnalyticTracker.FirebaseReady, SetupUserId);
        }

        private void SetupUserId()
        {
            SonatSdkUtils.DoActionDelay(() => { SonatFirebase.SetCustomUserId(SonatAppsFlyer.CustomerUserId); }, 3);
        }

        public void onConversionDataSuccess(string conversionData)
        {
            AppsFlyer.AFLog("didReceiveConversionData", conversionData);
            Dictionary<string, object> conversionDataDictionary = AppsFlyer.CallbackStringToDictionary(conversionData);


            SonatDebugType.Tracking.Log("first conversionData: " + conversionData);
            StartCoroutine(Wait(conversionData));
        }


        IEnumerator Wait(string conversionData)
        {
            while (!SonatAnalyticTracker.FirebaseReady)
                yield return null;

            yield return new WaitForSeconds(3);
//        if (!ConversionLogged)
            {
//            ConversionLogged = true;
                SonatDebugType.Tracking.Log("ConversionDataSuccess" + conversionData);
                var objDict = Json.Deserialize(conversionData) as Dictionary<string, object>;

                if (objDict != null)
                {
                    List<Parameter> parameters = new List<Parameter>();
                    string campaignType = null;
                    string mediaSource = null;
                    string campaignName = null;
                    foreach (var keyValuePair in objDict)
                    {
                        if (!(keyValuePair.Value == null || keyValuePair.Value.ToString().Length > 100))
                        {
                            var strValue = keyValuePair.Value.ToString();
                            parameters.Add(new Parameter(keyValuePair.Key, strValue, GetPrior(strValue)));
                        }

                        if (keyValuePair is { Value: not null, Key: "campaign" })
                        {
                            campaignName = keyValuePair.Value.ToString();
                            string[] regex = campaignName.Split(new[] { "_", "-", " - " }, StringSplitOptions.None);
                            if (regex.Length > 0)
                            {
                                parameters.Add(new Parameter("ad_network", regex[0], GetPrior("ad_network")));
                            }
                        }
                        else if (keyValuePair is { Value: not null, Key: "media_source" })
                        {
                            mediaSource = keyValuePair.Value.ToString();
                        }
                    }

                    campaignType = GetCampaignType(campaignName, mediaSource);
                    parameters.Add(new Parameter("campaign_type", campaignType, GetPrior("campaign_type")));
                    UserData.UserCampaignSegment.Value = campaignType;


                    var logs = parameters.OrderBy(pr => pr.order).Take(25).ToArray();
                    SonatDebugType.Tracking.Log("AF: " + string.Join(",", logs.Select(x => x.log)));

                    if (!SonatAppsFlyer.ConversionLogged)
                    {
#if using_firebase || using_firebase_analytics
                        SonatFirebase.analytic.LogEvent("af_conversion_data", logs.Select(x => x.Param).ToArray());
#endif
                        SonatAppsFlyer.ConversionLogged = true;
                    }

                    var last = JsonUtility.FromJson<DictStringString2>(
                        PlayerPrefs.GetString("last_conversation_set_user_property"));
                    var setDict = GetListPropertySet(objDict);
                    if (!string.IsNullOrEmpty(campaignType))
                    {
                        setDict.Set("campaign_type", campaignType);
                    }

                    SonatDebugType.Tracking.Log("Dict " + setDict.GetString());
                    if (last != null)
                        SonatDebugType.Tracking.Log("Last Dict = " + last.GetString());

                    bool update = setDict.Count > 0;
                    if (update && (last == null || last.Count == 0 || last.GetString() != setDict.GetString()))
                    {
                        SonatDebugType.Tracking.Log("Conversion dat success set user property");
#if using_firebase || using_firebase_analytics
                        SonatFirebase.analytic.LogEvent("sdk_tracking", new Firebase.Analytics.Parameter("data", "af_conversion_data"),
                            new Firebase.Analytics.Parameter("type", "update"));
#endif
                        PlayerPrefs.SetString("last_conversation_set_user_property", JsonUtility.ToJson(setDict));
                        for (int i = 0; i < setDict.Count; i++)
                        {
                            SonatFirebase.analytic.SetUserProperty(setDict.keys[i], setDict.values[i]);
                        }
                    }
                    else
                    {
                        SonatDebugType.Tracking.Log("No conversation data changed");
                    }
                }
                else
                {
                    SonatDebugType.Tracking.LogError(this.name + " obj2 == null");
                }
            }

            DictStringString2 GetListPropertySet(Dictionary<string, object> dict)
            {
                DictStringString2 get = new DictStringString2();

                bool campaign_id = dict.ContainsKey("campaign_id");
                foreach (var keyValuePair in dict)
                    if (_updateProperty.Contains(keyValuePair.Key))
                        get.Set(keyValuePair.Key, keyValuePair.Value == null ? "null" : keyValuePair.Value.ToString());
                if (!campaign_id)
                    foreach (var keyValuePair in dict)
                        if (keyValuePair.Key == "af_c_id")
                            get.Set(keyValuePair.Key, keyValuePair.Value == null ? "null" : keyValuePair.Value.ToString());

                return get;
            }
        }


        private readonly string[] _updateProperty =
        {
            "media_source",
            "campaign_id",
            //  "af_c_id",
            "campaign",
            "af_adset_id",
            "af_adset",
            "af_ad_id",
            "af_ad",
            "af_channel",
            "af_cpi",
        };


        public class Parameter
        {
#if using_firebase || using_firebase_analytics
            public Firebase.Analytics.Parameter Param;
#endif
            public int order { get; private set; }

            public string log { get; private set; }

            public Parameter(string name, string value, int order = 0)
            {
                this.order = order;
#if using_firebase || using_firebase_analytics
                Param = new Firebase.Analytics.Parameter(name, value);
#endif
                log = name + "=" + value;
            }
        }

        private int GetPrior(string compare)
        {
            if (compare == "ad_network"
                || compare == "af_ad"
                || compare == "af_ad_format"
                || compare == "af_ad_id"
                || compare == "af_ad_type"
                || compare == "af_adset_id"
                || compare == "af_adset"
                || compare == "af_cpi"
                || compare == "af_status"
                || compare == "af_user_id"
                || compare == "campaign"
                || compare == "campaign_id"
                || compare == "campaign_type"
                || compare == "click_time"
                || compare == "install_time"
                || compare == "media_source"
                || compare == "dma")
                return 0;

            if (compare.StartsWith("af_sub")
                || compare.StartsWith("iscache")
                || compare.StartsWith("af_r")
                || compare.StartsWith("is_universal_link")
                || compare.StartsWith("af_click_lookback")
                || compare.StartsWith("is_incentivized")
               )
                return 1;

            if (compare == "clickid"
                || compare == "match_type"
                || compare == "is_branded_link"
                || compare == "af_r"
                || compare == "http_referrer "
               )
                return 2;
            return 3;
        }


        public void onConversionDataFail(string error)
        {
#if using_appsflyer
            AppsFlyerSDK.AppsFlyer.AFLog("didReceiveConversionDataWithError", error);
#endif
        }

        public void onAppOpenAttribution(string attributionData)
        {
#if using_appsflyer
            AppsFlyerSDK.AppsFlyer.AFLog("onAppOpenAttribution", attributionData);
            Dictionary<string, object> attributionDataDictionary = AppsFlyerSDK.AppsFlyer.CallbackStringToDictionary(attributionData);
#endif
            // add direct deeplink logic here
        }

        public void onAppOpenAttributionFailure(string error)
        {
#if using_appsflyer
            AppsFlyerSDK.AppsFlyer.AFLog("onAppOpenAttributionFailure", error);
#endif
        }

        public void didReceivePurchaseRevenueValidationInfo(string validationInfo)
        {
            SonatDebugType.Ads.Log($"ReceivePurchaseRevenueValidationInfo {validationInfo}");
#if using_appsflyer
            AppsFlyerSDK.AppsFlyer.AFLog("didReceivePurchaseRevenueValidationInfo", validationInfo);
            // deserialize the string as a dictionnary, easy to manipulate
            Dictionary<string, object> dictionary =
                AFMiniJSON.Json.Deserialize(validationInfo) as Dictionary<string, object>;

            // if the platform is Android, you can create an object from the dictionnary 
            // deserialize the string as a dictionnary, easy to manipulate
            if (dictionary == null)
                return;
            // if the platform is Android, you can create an object from the dictionnary 
#if UNITY_ANDROID
            if (dictionary.ContainsKey("productPurchase") && dictionary["productPurchase"] != null)
            {
                // Create an object from the JSON string.
//            InAppPurchaseValidationResult iapObject = JsonUtility.FromJson<InAppPurchaseValidationResult>(validationInfo);
                if (dictionary.ContainsKey("success"))
                    SonatDebugType.Tracking.Log($"IapObject.success:{dictionary["success"]}");
                if (dictionary.ContainsKey("token"))
                    SonatDebugType.Tracking.Log($"IapObject.token:{dictionary["token"]}");
            }
            else if (dictionary.ContainsKey("subscriptionPurchase") && dictionary["subscriptionPurchase"] != null)
            {
                //  SubscriptionValidationResult iapObject = JsonUtility.FromJson<SubscriptionValidationResult>(validationInfo);
            }
#endif
#endif
        }

        public static string GetCampaignType(string paramsCampaignType, string paramsMediaSource)
        {
            string pType = (paramsCampaignType ?? "").ToLower();
            string media = (paramsMediaSource ?? "").ToLower();

            // IAP
            if (ContainsAny(pType, "iaproas", "iap roas", "iap cpe") &&
                IsMediaIn(media, "applovin_int", "googleadwords_int"))
            {
                return "IAP";
            }

            // Hybrid_High
            if (ContainsAny(pType, "hybrid", "bldroasd", "bldd", "totalroasd", "totalroas d", "hybridroas", "exact") &&
                IsMediaIn(media, "applovin_int", "googleadwords_int", "apple search ads"))
            {
                return "Hybrid_High";
            }

            // Hybrid
            if (ContainsAny(pType, "hybrid", "bldroasd", "bldd", "totalroasd", "totalroas d", "hybridroas", "discovery") &&
                IsMediaIn(media, "unityads_int", "mintegral_int", "apple search ads"))
            {
                return "Hybrid";
            }

            // IAA
            if (ContainsAny(pType, "-roas", "adroas", "_roas", "roas", "-troas", "_troas", "ad roas", "adroas",
                    "roas0", "roas7", "aeo", "cpe", "cpa") &&
                IsMediaIn(media, "applovin_int", "googleadwords_int", "unityads_int"))
            {
                return "IAA";
            }

            // FO-1
            if (ContainsAny(pType, "fo", "fo_maxcvs", "maxcvs"))
            {
                return "FO";
            }

            // FO-2
            if (ContainsAny(pType,
                    "-roas", "adroas", "_roas", "roas", "-troas", "_troas", "ad roas", "adroas",
                    "roas0", "roas7", "aeo", "cpe", "cpa") &&
                IsMediaIn(media, "mintegral_int", "facebook ads", "tiktokglobal_int"))
            {
                return "FO";
            }

            // Organic
            return "Organic";
        }

        private static bool ContainsAny(string source, params string[] keywords)
        {
            return keywords.Any(k => source.Contains(k));
        }

        private static bool IsMediaIn(string media, params string[] validMedias)
        {
            if (string.IsNullOrEmpty(media))
                return false;
            return validMedias.Any(m => media.Equals(m, StringComparison.OrdinalIgnoreCase));
        }
    }
}
#endif