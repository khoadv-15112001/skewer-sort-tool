#if using_appsflyer
#endif

using System.Collections.Generic;
using Sonat.AppsFlyerModule;
using Sonat.FirebaseModule;
using Sonat.FirebaseModule.RemoteConfig;

namespace Sonat.TrackingModule
{
    public static class SonatLogRecursive
    {
        private static List<int> completeLevelsNeedLog;
        private static PlayerPrefListInt completeLevelLogged;
        
        
        public static void CheckLogCompleteLevelUA(int level)
        {
            if (completeLevelsNeedLog == null)
            {
                completeLevelsNeedLog = SonatFirebase.GetConfig().CompleteLevelsLog();
            }
        
            if (completeLevelLogged == null)
                completeLevelLogged = new PlayerPrefListInt("complete_level_logged", new List<int>());
        
        
            if (completeLevelsNeedLog.Contains(level) && !completeLevelLogged.Contains(level))
            {
                completeLevelLogged.AddDistinct(level);
                var eventName = $"complete_level_{level:D3}";
                SonatFirebase.analytic.LogEvent(eventName);
                SonatAppsFlyer.SendEvent(eventName);
            }
        }

        private static List<int> completeRewardedNeedLog;
        private static PlayerPrefInt showRewardedCount;
        
        public static void CheckLogCompleteRewardedAdsUA()
        {
            if (completeRewardedNeedLog == null)
            {
                completeRewardedNeedLog = SonatFirebase.GetConfig().CompleteRewardAdsLog();
            }
        
        
            if(showRewardedCount == null)
                showRewardedCount = new PlayerPrefInt("show_rewarded_count", 0);
        
            showRewardedCount.Value++;
        
            if (completeRewardedNeedLog.Contains(showRewardedCount.Value))
            {
                string eventName = "";
                if (showRewardedCount.Value == 1)
                {
                    eventName = $"complete_rwd";
                }
                else
                {
                    eventName = $"complete_rwd_{showRewardedCount.Value:D2}";
                }
                
                SonatFirebase.analytic.LogEvent(eventName);
                SonatAppsFlyer.SendEvent(eventName);
            }
        }

        
        private static List<int> paidAdImpressionNeedLog;
        private static PlayerPrefInt paidAdImpressionCount;

        public static void CheckLogPaidAdsImpressionUA()
        {
            if (paidAdImpressionNeedLog == null)
            {
                paidAdImpressionNeedLog = SonatFirebase.GetConfig().PaidAdImpressionLog();
            }


            if(paidAdImpressionCount == null)
                paidAdImpressionCount = new PlayerPrefInt("paid_ad_impression_count", 0);

            paidAdImpressionCount.Value++;

            if (paidAdImpressionNeedLog.Contains(paidAdImpressionCount.Value))
            {
                string eventName = "";
                // if (paidAdImpressionCount.Value == 1)
                // {
                //     eventName = $"paid_ad_impression";
                // }
                // else
                // {
                    eventName = $"paid_ad_impression_{paidAdImpressionCount.Value:D2}";
                //}
                
                SonatFirebase.analytic.LogEvent(eventName);
                SonatAppsFlyer.SendEvent(eventName);
            }
        }
        
        private static List<int> levelsNeedLogIaaIap;
        private static PlayerPrefListInt levelsLogIaaIap;
        public static bool NeedLogIaaIap(int level)
        {
            if (levelsNeedLogIaaIap == null)
                levelsNeedLogIaaIap = SonatFirebase.GetConfig().LevelLogAfIaaIaa();
            
            if (levelsLogIaaIap == null)
                levelsLogIaaIap = new PlayerPrefListInt("log_iaa_iap_level", new List<int>());

            if (levelsNeedLogIaaIap.Contains(level) && !levelsLogIaaIap.Contains(level))
            {
                levelsLogIaaIap.Add(level);
                return true;
            }
        
            return false;
        }
    }
}