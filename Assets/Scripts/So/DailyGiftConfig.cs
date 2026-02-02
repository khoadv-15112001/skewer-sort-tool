using System;
using System.Linq;
using Sirenix.OdinInspector;
using SonatFramework.Systems.InventoryManagement.GameResources;
using UnityEngine;

namespace GrillSort.DailyGift
{
    [CreateAssetMenu(fileName = "DailyGiftConfig", menuName = "Sonat Configs Custom/DailyGiftConfig", order = 0)]
    public class DailyGiftConfig : ScriptableObject
    {
        public int maxLoginStreakGapDays = 7;
        public DailyGiftEntry[] dailyGifts;
        public LiveOpsPackData liveOpsPackData;

        public DailyGiftEntry GetGiftForDay(int currentStreakDay)
        {
            return dailyGifts.FirstOrDefault(gift => gift.day == currentStreakDay);
        }

        public int GetFinishDay()
        {
            //return dailyGifts.Max(gift => gift.day);
            return dailyGifts[^1].day;
        }

        public bool CheckGift(int currentStreakDay)
        {
            return dailyGifts.Any(gift => gift.day == currentStreakDay);
        }
    }

    [Serializable]
    public class DailyGiftEntry
    {
        public int day;
        public RewardData reward;
        public bool isSpecial;
        [ShowIf("isSpecial")] public Sprite sprite;

    }
}
