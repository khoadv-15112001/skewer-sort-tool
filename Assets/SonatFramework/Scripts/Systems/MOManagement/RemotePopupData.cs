using System.Collections.Generic;
using Sonat.Enums;
using SonatFramework.Scripts.Helper;
using SonatFramework.Systems.TimeManagement;

namespace SonatFramework.Systems.MOManagement
{
    public class RemotePopupData
    {
        private readonly Service<TimeService> timeService = new();
        public bool appOpen;
        public int daily = -1;
        public ShopItemKey iapKey;
        public int interval = 0;
        public IntDataPref lastDay;
        public int levelEnd = int.MaxValue;
        public List<int> levels;
        public List<int> levelsIgnoreAppOpen;
        public int levelStart;
        public int order;

        public string popupName;
        public IntDataPref todayShowCount;

        public void SetDailyData()
        {
            todayShowCount = new IntDataPref($"SHOW_TODAY_COUNT_{popupName}");
            lastDay = new IntDataPref($"SHOW_TODAY_LAST_DAY_{popupName}", -1);
            if (timeService.Instance.GetCurrentTime(false).DayOfYear != lastDay.Value)
            {
                todayShowCount.Value = 0;
                lastDay.Value = timeService.Instance.GetCurrentTime(false).DayOfYear;
            }
        }
    }
}