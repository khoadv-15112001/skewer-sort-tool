using UnityEngine;
using SonatFramework.Systems;
using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using System;
using Microsoft.Win32;
using GrillSort.RealTime;
using Sonat;

namespace GrillSort.PackServiceRegistry
{
    [CreateAssetMenu(fileName = "PackServiceRegistry", menuName = "My Services/PackServiceRegistry")]
    public class PackServiceRegistry : SonatServiceSo, IServiceInitializeAsync
    {
        [Header("Line Configs")]
        [SerializeField] private List<LineConfig> lineConfigs;

        private Dictionary<BannerName, PackLevelManager> managers = new Dictionary<BannerName, PackLevelManager>();

        public List<LineConfig> LineConfigs { get => lineConfigs; }

        private const string PREFS_LAST_HANDLED_DAY = "PackServiceRegistry_LastHandledDay";

        private DateTime lastHandledDay;
        public PlayerPrefInt lastPackBought = new PlayerPrefInt("PackServiceRegistry_LastPackBought");
        private readonly Service<RealTimeService> _realTimeService = new();

        public async UniTaskVoid InitializeAsync()
        {
            LoadLastHandledDay();

            await LoadData();
        }

        private async UniTask LoadData()
        {
            foreach (var config in lineConfigs)
            {
                if (config != null)
                {
                    var mgr = new PackLevelManager(config);
                    managers[config.bannerName] = mgr;
                }
                config.LoadRemoteConfig();
            }
        }

        public PackLevelManager GetManager(BannerName bannerName)
        {
            if (managers.TryGetValue(bannerName, out var m))
                return m;
            return null;
        }
        private void LoadLastHandledDay()
        {
            string s = PlayerPrefs.GetString(PREFS_LAST_HANDLED_DAY, "");
            if (!string.IsNullOrEmpty(s) && DateTime.TryParse(s, out var dt))
            {
                lastHandledDay = dt.Date;
            }
            else
            {
                // lần đầu chạy, cho lastHandledDay là ngày *hôm trước* để logic next-day trigger
                DateTime today = _realTimeService.Instance.GetCurrentTime().Date;
                SaveLastHandledDay(today.AddDays(-1));
            }
        }

        private void SaveLastHandledDay(DateTime day)
        {
            lastHandledDay = day.Date;
            PlayerPrefs.SetString(PREFS_LAST_HANDLED_DAY, lastHandledDay.ToString("yyyy-MM-dd"));
            PlayerPrefs.Save();
        }

        public void HandleNextDayInternalIfNeeded()
        {
            DateTime currentDay = _realTimeService.Instance.GetCurrentTime().Date;

            //Debug.Log($"anhnt last={lastHandledDay.Date}, cur={currentDay}");
            if (lastHandledDay.Date != currentDay)
            {
                foreach (var kv in managers)
                {
                    kv.Value.OnNewDay();
                }

                lastHandledDay = currentDay;
                SaveLastHandledDay(currentDay);
            }
        }

        internal BannerName GetLinePageToFront()
        {
            return (BannerName)lastPackBought.Value;
        }

        internal long RemainingTimeCooldown(BannerName bannerName)
        {
            return lineConfigs.Find(x => x.bannerName == bannerName).cooldown;
        }
    }
}
