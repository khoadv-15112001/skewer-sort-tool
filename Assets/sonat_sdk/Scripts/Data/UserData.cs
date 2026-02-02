using System.Collections.Generic;

namespace Sonat.Data
{
    public class UserData
    {
        private static Dictionary<string, PlayerPrefInt> levels = new();
        private static PlayerPrefString mode = new PlayerPrefString("userMode", "classic");
        
        public static PlayerPrefInt IsNoads = new PlayerPrefInt("IsNoAds", 0);
        public static PlayerPrefString FirebaseInstanceId = new("FirebaseInstanceId", "");
        public static PlayerPrefString AnalyticsInstanceId = new("AnalyticsInstanceId", "");
        public static PlayerPrefInt FirstPlay = new("FirstPlay", 1);
        public static PlayerPrefInt UserDay = new("userDay", 1);
        public static PlayerPrefString UserCampaignSegment = new ("SonatUserCampaignSegment", "Organic");
        public static PlayerPrefInt RewardedCount = new("RewardedCount", 0);
        public static PlayerPrefInt InterstitialCount = new("InterstitialCount", 0);

        public static int GetLevel(string mode)
        {
            if (!levels.TryGetValue(mode, out var result))
            {
                result = new PlayerPrefInt($"sonat_sdk_{mode}_level", 1);
            }

            return result.Value;
        }

        public static int GetLevel()
        {
            return GetLevel(mode.Value);
        }

        public static string GetMode()
        {
            return mode.Value;
        }

        public static void SetLevel(int level, string mode = "classic")
        {
            if (!levels.TryGetValue(mode, out var result))
            {
                result = new PlayerPrefInt($"sonat_sdk_{mode}_level", level);
                levels.Add(mode, result);
            }
            else
            {
                result.Value = level;
            }
        }

        public static void SetMode(string _mode)
        {
            mode.Value = _mode;
        }

    }
}
