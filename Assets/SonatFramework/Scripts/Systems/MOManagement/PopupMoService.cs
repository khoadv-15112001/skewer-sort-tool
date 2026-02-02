using System.Collections.Generic;
using Newtonsoft.Json;
using Sonat.Enums;
using SonatFramework.Scripts.Feature.Shop;
using SonatFramework.Scripts.SonatSDKAdapterModule;
using SonatFramework.Systems.TimeManagement;

namespace SonatFramework.Systems.MOManagement
{
    public class PopupMoService
    {
        private readonly Dictionary<string, RemotePopupData> remotePopupDatas = new();
        private readonly Service<ShopService> shopService = new();
        private readonly Service<TimeService> timeService = new();

        public RemotePopupData GetData(string popupName, ShopItemKey iapKey = ShopItemKey.None,
            int dfLevelStart = 999999, int dfOrder = 0, bool dfAppOpen = true, int intervalDf = 0)
        {
            var json = SonatSDKAdapter.GetRemoteString($"{popupName}_remote");
            var remotePopup = new RemotePopupData();
            if (!string.IsNullOrEmpty(json))
            {
                remotePopup = JsonConvert.DeserializeObject<RemotePopupData>(json);
            }
            else
            {
                remotePopup.levelStart = dfLevelStart;
                remotePopup.appOpen = dfAppOpen;
                remotePopup.order = dfOrder;
                remotePopup.interval = intervalDf;
            }

            remotePopup.iapKey = iapKey;
            remotePopup.popupName = popupName;
            remotePopup.SetDailyData();
            return remotePopup;
        }


        public bool Pass(string popupName, bool isAppOpen, int level)
        {
            var remotePopupData = GetData(popupName);
            if (remotePopupData == null) return false;

            if (remotePopupData.levelStart > level || remotePopupData.levelEnd < level) return false;
            if (remotePopupData.appOpen != isAppOpen)
                if (remotePopupData.levelsIgnoreAppOpen == null || !remotePopupData.levelsIgnoreAppOpen.Contains(level))
                    return false;

            if (remotePopupData.levels != null && remotePopupData.levels.Contains(level)) return true;
            if (remotePopupData.levelStart != level && remotePopupData.interval <= 0) return false;

            if (remotePopupData.daily >= 0 && remotePopupData.todayShowCount.Value >= remotePopupData.daily)
                return false;
            if (remotePopupData.interval > 0 &&
                (level - remotePopupData.levelStart) % remotePopupData.interval != 0) return false;
            if (remotePopupData.iapKey != ShopItemKey.None && !shopService.Instance.VerifyPack(remotePopupData.iapKey))
                return false;

            return true;
        }
    }
}