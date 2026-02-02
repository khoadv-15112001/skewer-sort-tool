using System.Collections.Generic;

namespace SonatFramework.Scripts.Feature.DailyReward
{
    public class DailyRewardData : IFeatureData
    {
        public int currentStreak;
        public List<int> dayClaimed = new();
        public long lastTimeLogin;
        public List<int> milestoneClaimed = new();
    }
}