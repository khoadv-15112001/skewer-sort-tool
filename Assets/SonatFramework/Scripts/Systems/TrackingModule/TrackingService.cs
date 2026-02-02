using SonatFramework.Systems.EventBus;
using UnityEngine;

namespace SonatFramework.Systems.TrackingModule
{
    public abstract class TrackingService : SonatServiceSo
    {
        [HideInInspector] public string placement;
        [HideInInspector] public string location;
        [HideInInspector] public string screen;

        public abstract void LogShowAds();


        public abstract void LogShowRewardAds();

        public abstract void LogShowInterAds();

        public abstract void LogLevelComplete(int level);

        public abstract void LogLevelStart();
        public abstract void LogLevelStartTurn();

        public abstract void LogLevelEnd();
        public abstract void LogLevelEndTurn();

        public abstract void LogLevelUp();

        public abstract void LogUseBooster(string boosterName);

        public abstract void LogShortcut(string shortcutName);

        public abstract void LogEarnCurrency(EarnResourceEvent eventData);

        public abstract void LogSpendCurrency(SpendResourceEvent eventData);

        public abstract void OnShowPopup(string uiName, string uiType, string uiClass, string openBy, string action = "open");

        public abstract void TrackingScreenView();
    }
}