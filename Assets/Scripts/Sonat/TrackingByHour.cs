using System.Collections.Generic;
using Sonat;
using Sonat.CustomService;
using Sonat.Enums;
using Sonat.TrackingModule;
using SonatFramework.Systems;
using SonatFramework.Systems.EventBus;
using SonatFramework.Systems.TimeManagement;
using UnityEngine;

namespace MyFramework.Sonat
{
    public static class TrackingByHour
    {
        private static long firstTimeOpen;
        private static PlayerPrefInt boosterCount;
        private static EventBinding<SwitchPlacementEvent> onSwitchPlacement;
        private static EventBinding<UseBoosterEvent> onUseBooster;

        public static void Setup()
        {
            firstTimeOpen = MySonatFramework.userDataService.FirstTimeOpen;
            var currentTime = SonatSystem.GetService<TimeService>().GetUnixTimeSeconds();
            boosterCount = new PlayerPrefInt("TrackingBoosterCount", 0);

            if (currentTime - firstTimeOpen > 3600 && boosterCount.Value == 0) return;

            onSwitchPlacement = new EventBinding<SwitchPlacementEvent>(OnSwitchPlacement);
            onUseBooster = new EventBinding<UseBoosterEvent>(OnUseBooster);
        }

        private static void OnSwitchPlacement(SwitchPlacementEvent eventData)
        {
            if (eventData.to == GamePlacement.Home)
            {
                CheckTrackingBooster24Hours();
                CheckTrackingBooster48Hours();
                CheckTrackingSessionUser24h();
            }
        }


        public static void OnUseBooster(UseBoosterEvent eventData)
        {
            if (eventData.booster.ResourceType() == GameResourceType.Booster)
            {
                boosterCount.Value++;
            }
        }

        private static void CheckTrackingBooster24Hours()
        {
            if (PlayerPrefs.HasKey("TrackingUseBooster24h")) return;
            var currentTime = SonatSystem.GetService<TimeService>().GetUnixTimeSeconds();
            if (currentTime - firstTimeOpen >= 86400)
            {
                var log = new CustomSonatLog("use_booster_first_24h", new List<LogParameter>()
                {
                    new LogParameter("booster_use", boosterCount.Value)
                });
                log.Post(true);
                PlayerPrefs.SetInt("TrackingUseBooster24h", 1);
            }
        }

        private static void CheckTrackingBooster48Hours()
        {
            if (PlayerPrefs.HasKey("TrackingUseBooster48h"))
            {
                RemoveTracking();
                return;
            }

            var currentTime = SonatSystem.GetService<TimeService>().GetUnixTimeSeconds();
            if (currentTime - firstTimeOpen >= 172800)
            {
                var log = new CustomSonatLog("use_booster_first_48h", new List<LogParameter>()
                {
                    new LogParameter("booster_use", boosterCount.Value)
                });
                log.Post(true);
                PlayerPrefs.SetInt("TrackingUseBooster48h", 1);
                RemoveTracking();
            }
        }

        private static void CheckTrackingSessionUser24h()
        {
            if (PlayerPrefs.HasKey("TrackingUseSession24h")) return;
            var currentTime = SonatSystem.GetService<TimeService>().GetUnixTimeSeconds();
            if (currentTime - firstTimeOpen >= 86400)
            {
                var log = new CustomSonatLog("session_play_first_24h", new List<LogParameter>()
                {
                    new LogParameter("session_play", MySonatFramework.userDataService.SessionTotal)
                });
                log.Post(true);
                PlayerPrefs.SetInt("TrackingUseSession24h", 1);
            }
        }

        private static void RemoveTracking()
        {
            EventBus<SwitchPlacementEvent>.Deregister(onSwitchPlacement);
            EventBus<UseBoosterEvent>.Deregister(onUseBooster);
        }
    }
}