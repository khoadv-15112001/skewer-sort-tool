using System;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using MyGame.SkewerJam.Scripts.Service;
using Sirenix.OdinInspector;
using SkewerJam.Features.Leaderboard.Service;
using Sonat.Enums;
using Sonat.TrackingModule;
using SonatFramework.Systems;
using SonatFramework.Systems.EventBus;
using SonatFramework.Systems.TimeManagement;
using SonatFramework.Systems.UserData;
using UnityEngine;

namespace Sonat.CustomService
{
    [CreateAssetMenu(fileName = "HLWTrackingService", menuName = "MyGame/SkewerJam/HLW Tracking Service")]
    public class HLWTrackingService : SonatServiceSo
    {
        public int startCount { get => PlayerPrefs.GetInt("HLW_StartCount", 0); set => PlayerPrefs.SetInt("HLW_StartCount", value); }
        private long timeStartLevel;
        private int continueCount;

        public void Init()
        {
            new EventBinding<LevelStartedEvent_HLW>(OnLevelStarted);
            new EventBinding<LevelEndedEvent_HLW>(OnLevelEnded);

            MySonatFramework.GetService<HLWEventService>().OnStartEvent += OnStartEvent;
            MySonatFramework.GetService<HLWEventService>().OnJoinEvent += OnJoinEvent;
            MySonatFramework.GetService<HLWEventService>().OnFinishEvent += OnFinishEvent;
        }

        private void OnFinishEvent()
        {
            OnFinishEventAsync().Forget();
        }

        private void OnStartEvent()
        {
#if sonat_sdk
            var log = new SonatLogStartFeature()
            {
                level = MySonatFramework.GetService<UserDataService>().GetLevel(),
                mode = "pkr",
                start_count = 1,
            };

            log.AddExtraParameter("type", "rush");
            log.AddExtraParameter("continue_count", "0");

            log.Post();
#endif
        }

        public void OnJoinEvent()
        {
#if sonat_sdk
            var log = new SonatLogStartFeature()
            {
                level = MySonatFramework.GetService<UserDataService>().GetLevel(),
                mode = "pkr",
                start_count = 1
            };

            log.AddExtraParameter("type", "rush");
            log.AddExtraParameter("continue_count", "1");

            log.Post();
#endif
        }
        [Button("Test")]
        private async UniTask OnFinishEventAsync()
        {
            await MySonatFramework.GetService<LeaderboardHLWService>().FetchLeaderboard();
            var rank = MySonatFramework.GetService<LeaderboardHLWService>().CurrentRank;
#if sonat_sdk
            string mode = "pkr";
            string type = "rush";
            int start_count = 1;
            int continue_count = 1;
            bool success = true;
            float completion = (Mathf.Min(MySonatFramework.userDataService.GetLevel(GameMode.SkewerJam), 40) * 1f / 40f) * 100f;
            string milestone = null;
            string percentage_claim_milestone = "";
            string end_cause = "";
            int day_active = MySonatFramework.GetService<HLWEventService>().GetDayActive();

            MySonatFramework.customTrackingService.LogLevelEndFeature(mode, type, start_count, continue_count, success, completion.ToString(), percentage_claim_milestone
                , milestone, rank, end_cause, day_active);
#endif
        }

        private void OnLevelStarted(LevelStartedEvent_HLW hLW)
        {
            startCount += 1;
            continueCount += 1;
            timeStartLevel = MySonatFramework.GetService<TimeService>().GetUnixTimeSeconds();

#if sonat_sdk
            var log = new SonatLogLevelStart()
            {
                level = hLW.level,
                mode = "pkr",
                start_count = startCount
            };
            log.AddExtraParameter("continue_count", continueCount.ToString());
            log.Post();
#endif
        }

        private void OnLevelEnded(LevelEndedEvent_HLW hLW)
        {
            var playTime = MySonatFramework.GetService<TimeService>().GetUnixTimeSeconds() - timeStartLevel;
            continueCount = hLW.lose ? 0 : continueCount;
#if sonat_sdk
            var log = new SonatLogLevelEnd()
            {
                level = hLW.level,
                mode = "pkr",
                success = hLW.success,
                lose_cause = hLW.loseCause,
                play_time = (int)playTime
            };
            log.Post();
#endif
        }
    }

    public struct LevelStartedEvent_HLW : IEvent
    {
        public GameMode gameMode;
        public int level;
        public int phase;
    }

    public struct LevelEndedEvent_HLW : IEvent
    {
        public GameMode gameMode;
        public int level;
        public bool success;
        public int phase;
        public string loseCause;
        public bool lose;
    }
}