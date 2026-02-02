using System.Collections.Generic;
using Sonat.FirebaseModule.RemoteConfig;
using UnityEngine;

namespace Sonat.FirebaseModule.Analytic
{
    [CreateAssetMenu(menuName = "SonatSDK/Configs/Firebase Config", fileName = nameof(SonatFirebaseConfig))]
    public class SonatFirebaseConfig : ScriptableObject
    {
        public string completeLevelsLogs;
        public string completeRewardAdsLogs;
        public string paidAdImpressionLogs;
        public string levelsLogAfIaaIap;
        
        public List<RemoteConfigDefaultByString> defaultConfigs = new List<RemoteConfigDefaultByString>();

        public List<int> CompleteLevelsLog()
        {
            List<int> result = new List<int>();
            string logs = RemoteConfigKey.complete_level_logs.GetString(completeLevelsLogs);
            if (!string.IsNullOrEmpty(logs))
            {
                result = SonatSdkHelper.GetIntListFromRegex(logs);
            }

            return result;
        }
        
        public List<int> CompleteRewardAdsLog()
        {
            List<int> result = new List<int>();
            string logs = RemoteConfigKey.complete_rw_ads_logs.GetString(completeRewardAdsLogs);
            if (!string.IsNullOrEmpty(logs))
            {
                result = SonatSdkHelper.GetIntListFromRegex(logs);
            }

            return result;
        }
        
        public List<int> PaidAdImpressionLog()
        {
            List<int> result = new List<int>();
            string logs = RemoteConfigKey.paid_ad_impression_logs.GetString(paidAdImpressionLogs);
            if (!string.IsNullOrEmpty(logs))
            {
                result = SonatSdkHelper.GetIntListFromRegex(logs);
            }

            return result;
        }
        
        
        public List<int> LevelLogAfIaaIaa()
        {
            List<int> result = new List<int>();
            string logs = RemoteConfigKey.levels_log_iaa_iap.GetString(levelsLogAfIaaIap);
            if (!string.IsNullOrEmpty(logs))
            {
                result = SonatSdkHelper.GetIntListFromRegex(logs);
            }

            return result;
        }

    }
}
