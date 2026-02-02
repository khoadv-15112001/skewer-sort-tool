using System.Collections.Generic;
using Newtonsoft.Json;
using Sonat.AdsModule;
using Sonat.Data;
using Sonat.FirebaseModule.RemoteConfig;
using SonatFramework.Scripts.SonatSDKAdapterModule;
using UnityEngine;

namespace Manager
{
    public static class GameSetup
    {
        private static bool setupDone = false;

        public static void Setup()
        {
            if (setupDone) return;
            SonatSDKAdapter.SetExternalConditionShowAds(CheckByLevelTimeGap);
            GameRemoteConfigValue.LoadData();
            setupDone = true;

            // Dictionary<string, RemoteDataByLevel> data = new Dictionary<string, RemoteDataByLevel>();
            // data.Add("IAP", new RemoteDataByLevel(){data = new Dictionary<int, object>() {{0, 0}}});
            // data.Add("IAA", new RemoteDataByLevel(){data = new Dictionary<int, object>() {{0, 9999}}});
            // Debug.Log($"REMOTE: {JsonConvert.SerializeObject(data)}");
            // int timeGap = SonatSDKAdapter.GetValueBySegment("level_start_show_interstitial_segment", "IAP", 180);
            // Debug.Log($"timeGap: {timeGap}");
        }

        public static bool CheckByLevelTimeGap(AdPlacement adPlacement)
        {
#if sonat_sdk
            switch (adPlacement)
            {
                case AdPlacement.Interstitial:
                    int timeGapInter = SonatSDKAdapter.GetValueByLevelSegment("by_level_time_gap_interstitial_segment", UserData.UserCampaignSegment.Value,
                        UserData.GetLevel(), 30);
                    float currentTime = Time.time;

                    if (currentTime - SonatAds.LastTimeShowAds(AdPlacement.Interstitial) < timeGapInter) return false;
                    int timeGapInterRwd = SonatSDKAdapter.GetValueByLevelSegment("by_level_time_gap_interstitial_rewarded_segment",
                        UserData.UserCampaignSegment.Value, UserData.GetLevel(), 30);
                    if (currentTime - SonatAds.LastTimeShowAds(AdPlacement.Rewarded) < timeGapInterRwd) return false;
                    return true;
            }
#endif
            return true;
        }
    }
}