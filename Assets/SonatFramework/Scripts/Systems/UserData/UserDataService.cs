using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using Sonat;
using Sonat.Enums;
using SonatFramework.Scripts.Helper;
using SonatFramework.Systems.EventBus;
using SonatFramework.Systems.GameDataManagement;
using SonatFramework.Systems.TimeManagement;
using UnityEngine;

namespace SonatFramework.Systems.UserData
{
    [CreateAssetMenu(fileName = "UserDataService", menuName = "Sonat Services/User Data Service")]
    public class UserDataService : SonatServiceSo, IServiceInitialize
    {
        private const string UserLevelKey = "UserLevel";
        private const string UserGameModeKey = "UserLevel";
        private LongDataPref firstTimeOpen;
        private LongDataPref lastTimeOpen;
        private IntDataPref lastDay;
        private IntDataPref sessionToday;
        private IntDataPref sessionTotal;
        private int userDay;
        private int levelPlayToday;
        private Dictionary<GameMode, int> levelByGameMode = new Dictionary<GameMode, int>();


        [BoxGroup("SERVICES")] [SerializeField]
        private Service<DataService> dataService = new();

        private Service<TimeService> timeService = new();

        public void Initialize()
        {
            levelByGameMode.TryAdd(GameMode.Classic, GetLevel(GameMode.Classic));
            levelPlayToday = 0;
            userDay = 0;
            var eventBinding = new EventBinding<LevelEndedEvent>(OnLevelEnded);
            CheckTimeUser();
        }

        protected virtual void CheckTimeUser()
        {
            firstTimeOpen = new LongDataPref("FirstTimeOpen");
            lastTimeOpen = new LongDataPref("LastTimeOpen");
            lastDay = new IntDataPref("LastDay");
            sessionToday = new IntDataPref("SessionToday");
            sessionTotal = new IntDataPref("SessionTotal");

            if (firstTimeOpen.Value == 0)
            {
                firstTimeOpen.Value = timeService.Instance.GetUnixTimeSeconds(false);
            }

            DateTime now = timeService.Instance.GetCurrentTime(false);
            DateTime firstDay = DateTimeOffset.FromUnixTimeSeconds(firstTimeOpen.Value).ToLocalTime().DateTime;
            userDay = timeService.Instance.GetDaysPassed(firstDay, now);

#if sonat_sdk
            Sonat.Data.UserData.UserDay.Value = userDay;
#endif

            if (lastDay.Value != userDay)
            {
                lastDay.Value = userDay;
                levelPlayToday = 0;
                sessionToday.Value = 0;
            }
            else
            {
                sessionToday.Value++;
            }

            sessionTotal.Value++;
            lastTimeOpen.Value = timeService.Instance.GetUnixTimeSeconds(false);
        }

        private void OnLevelEnded(LevelEndedEvent eventData)
        {
            if (!eventData.success) return;
            LevelUp(eventData.gameMode);
        }

        public int GetLevel(GameMode mode = GameMode.Classic)
        {
            if (!levelByGameMode.TryGetValue(mode, out int level))
            {
                level = dataService.Instance.GetInt($"{UserLevelKey}_{mode}", 1);
            }

            return level;
        }


        public void SaveLevel(int _level, GameMode mode = GameMode.Classic)
        {
            if (levelByGameMode.ContainsKey(mode))
            {
                levelByGameMode[mode] = _level;
            }
            else
            {
                levelByGameMode.Add(mode, _level);
            }
            dataService.Instance.SetInt($"{UserLevelKey}_{mode}", _level);
        }

        public void LevelUp(GameMode mode = GameMode.Classic)
        {
            var newLevel = GetLevel(mode) + 1;
            SaveLevel(newLevel, mode);
            levelPlayToday++;
        }

        public int UserDay => userDay;

        public int LevelPlayToday => levelPlayToday;
        public int SessionToday => sessionToday.Value;
        public long FirstTimeOpen => firstTimeOpen.Value;
        public int SessionTotal => sessionTotal.Value;
    }
}