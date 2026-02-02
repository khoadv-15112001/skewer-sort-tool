using Cysharp.Threading.Tasks;
using Helper;
using Manager;
using Sonat.Data;
using Sonat.Enums;
using Sonat.TrackingModule;
using SonatFramework.Scripts.Helper;
using SonatFramework.Scripts.SonatSDKAdapterModule;
using SonatFramework.Scripts.Utils;
using SonatFramework.Systems;
using SonatFramework.Systems.EventBus;
using SonatFramework.Systems.InventoryManagement;
using SonatFramework.Systems.InventoryManagement.GameResources;
using SonatFramework.Systems.TimeManagement;
using SonatFramework.Systems.TrackingModule;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Services.Core;
using UnityEngine;

namespace GrillSort.LavaQuest
{
    [CreateAssetMenu(fileName = "LavaQuestService", menuName = "My Services/LavaQuestService")]
    public class LavaQuestService : SonatServiceSo, IServiceInitializeAsync
    {
        public static LavaQuestService Instance => Service<LavaQuestService>.Get();

        [SerializeField] private LavaQuestConfig _config;
        public LavaQuestConfig Config => _config;

        public IntDataPref isUnlocked;
        public IntDataPref isShownPopup;
        public IntDataPref isJoined;
        public IntDataPref notiTut;
        public IntDataPref step;
        public IntDataPref saveCoin;
        public LongDataPref timeEnd;
        public LongDataPref nextTimeEvent;
        public IntDataPref state;

        public ClassDataPref<LavaQuestConfig.Players> players;
        public IntDataPref isExpired;

        public IntDataPref start_count;

        public static Action OnTick;
        public static Action OnJoinAction;
        public static Action OnReset;
        public static Action OnWinEvent;

        public static int CacheCountCurPlayer;
        public static int CacheCountLastPlayer;

        protected EventBinding<ProfileService.ProfileChangeEvent> profileChangeEvent;

        private TimeService timeService => MySonatFramework.GetService<TimeService>();

        public const string NamePopupTut = "PopupLavaQuestTut";

        public enum EState
        {
            Normal,
            Win,
            Lose
        }

        public async UniTaskVoid InitializeAsync()
        {
            isUnlocked = new IntDataPref("lava_quest_is_unlocked");
            isShownPopup = new IntDataPref("lava_quest_is_shown_popup");
            isJoined = new IntDataPref("lava_quest_is_joined");
            notiTut = new IntDataPref("lava_quest_noti_tut");
            step = new IntDataPref("lava_quest_step");
            saveCoin = new IntDataPref("lava_quest_save_coin");
            players = new ClassDataPref<LavaQuestConfig.Players>("lava_quest_players");
            timeEnd = new LongDataPref("lava_quest_time_end");
            nextTimeEvent = new LongDataPref("lava_quest_next_time_event");
            isExpired = new IntDataPref("lava_quest_is_expired");
            state = new IntDataPref("lava_quest_state");

            start_count = new IntDataPref("lava_quest_start_count", 1);

            await UniTask.Yield();

            TryActive();
            CheckResetEvent();

            TickService.Register(Tick, TickService.TickPhase.TickRealtime);

            profileChangeEvent = new EventBinding<ProfileService.ProfileChangeEvent>(UpdateProfile);

            new EventBinding<LevelEndedEvent>(OnLevelEnd);
        }

        private void Tick()
        {
            if (!isUnlocked.BoolValue) return;

            long now = timeService.GetUnixTimeSeconds();

            if (nextTimeEvent.Value != 0)
            {
                if (now >= nextTimeEvent.Value)
                {
                    start_count.Value++;
                    LogStartFeature(0);
                    nextTimeEvent.Value = 0;
                }
            }

            if (now >= timeEnd.Value && isJoined.BoolValue)
            {
                isExpired.BoolValue = true;
            }

            OnTick?.Invoke();
        }

        public int GetLevelStart()
        {
            return SonatSDKAdapter.GetRemoteInt("lava_quest_level_start", Config.levelStart);
        }

        public void TryActive()
        {
            if (isUnlocked.BoolValue || isExpired.BoolValue) return;

            if (MySonatFramework.userDataService.GetLevel() >= GetLevelStart())
            {
                LogStartFeature(0);

                isUnlocked.BoolValue = true;
            }
        }

        public void LogStartFeature(int continue_count)
        {
            //Debug.LogError($"[LavaQuestService]: LogStartFeature: start_count={start_count.Value}, continue_count={continue_count}");

            MySonatFramework.customTrackingService.LogLevelStartFeature("tq", "win_streak", start_count.Value, continue_count);
        }

        public void LogProgressFeature()
        {
            bool isComplete = step.Value == Config.maxStep;
            float completion = (step.Value * 1f / Config.maxStep) * 100f;
            string percentage_claim_resource = isComplete ? "coin:100" : "coin:0";
            string milestone_count = step.Value.ToString();
            int rank = GetListPlayersAtState(LavaQuestConfig.Players.Player.EState.Live).Count;

            MySonatFramework.customTrackingService.LogLevelProgressFeature("tq", "win_streak", start_count.Value, 1,
                completion.ToString(), percentage_claim_resource, milestone_count, rank
                );
        }

        public void LogEndFeature(bool success, string end_cause = null)
        {
            if (end_cause == null)
                end_cause = success ? "user_win" : "user_failed";

            //Debug.LogError($"[LavaQuestService]: LogEndFeature: start_count={start_count.Value}, continue_count={1}, end_cause={end_cause}");

            float completion = (step.Value * 1f / Config.maxStep) * 100f;
            string percentage_claim_resource = success ? "coin:100" : "coin:0";
            int milestone_count = step.Value;
            int rank = GetListPlayersAtState(LavaQuestConfig.Players.Player.EState.Live).Count;

            MySonatFramework.customTrackingService.LogLevelEndFeature("tq", "win_streak", start_count.Value, 1, success,
                completion.ToString(), percentage_claim_resource, milestone_count.ToString(), rank, end_cause);

            if (success)
                OnWinEvent?.Invoke();
        }

        private void CheckResetEvent()
        {
            if (step.Value >= Config.maxStep)
            {
                if (saveCoin.Value > 0)
                {
                    var log = new EarnResourceLogData()
                    {
                        spendId = "tq",
                        spendType = "feature"
                    };

                    Service<InventoryService>.Get().AddResource(GameResource.Coin, saveCoin.Value, log, true);
                }

                ResetEvent(true);
            }
        }

        public static bool IsClassicMode()
        {
            return SonatSystem.GetService<GameplayAnalyticsService>().levelPlayData.gameMode == GameMode.Classic;
        }

        public void OnLevelEnd(LevelEndedEvent levelEndedEvent)
        {
            if (levelEndedEvent.gameMode != GameMode.Classic) return;

            //Debug.LogError("[LavaQuestService]: OnLevelEnd success: " + levelEndedEvent.success);

            if (levelEndedEvent.success)
            {
                OnWin();
                return;
            }

            if (!levelEndedEvent.success)
            {
                OnLose();
                return;
            }
        }

        public void OnWin()
        {
            if (!isUnlocked.BoolValue)
            {
                TryActive();
                return;
            }

            if (!isJoined.BoolValue) return;

            step.Value++;

            TryKillPlayer();

            if (step.Value == Config.maxStep)
            {
                saveCoin.Value = GetCoinAfterSeparate();
                nextTimeEvent.Value = timeService.GetCurrentTime().Date.AddDays(1).ToUnixTimeSeconds();
                state.Value = (int)EState.Win;
            }

            LogProgressFeature();
        }

        public void OnLose()
        {
            if (!isJoined.BoolValue) return;

            ResetEvent(false);
        }

        public void ResetEvent(bool success, string end_cause = null)
        {
            LogEndFeature(success, end_cause);

            isJoined.BoolValue = false;
            isExpired.BoolValue = false;
            step.Value = 0;
            saveCoin.Value = 0;

            if (!success)
                nextTimeEvent.Value = timeService.GetCurrentTime().AddMinutes(30).ToUnixTimeSeconds();

            state.Value = success ? (int)EState.Win : (int)EState.Lose;

            OnReset?.Invoke();
        }
        public bool CanShowByTimeGap()
        {
            return timeService.GetUnixTimeSeconds() >= nextTimeEvent.Value;
        }

        public int GetCoinAfterSeparate()
        {
            return Config.coin / GetListPlayersAtState(LavaQuestConfig.Players.Player.EState.Live).Count;
        }

        public void OnJoin()
        {
            LogStartFeature(1);

            isJoined.BoolValue = true;
            step.Value = 0;
            state.Value = (int)EState.Normal;

            GeneratePlayers();

            timeEnd.Value = timeService.GetCurrentTime().AddDays(1).ToUnixTimeSeconds();

            OnJoinAction?.Invoke();
        }

        private void GeneratePlayers()
        {
            var profile = SonatSystem.GetService<ProfileService>();

            players.Clear();

            var myPlayer = new LavaQuestConfig.Players.Player(profile.FrameId, profile.AvatarId);

            players.Value.datas.Add(myPlayer);

            for (int i = 1; i < 100; i++)
            {
                var randomPlayer = new LavaQuestConfig.Players.Player(Config.GetRandomFrameID(), Config.GetRandomAvatarID());

                players.Value.datas.Add(randomPlayer);
            }

            players.Save();
        }

        public List<LavaQuestConfig.Players.Player> GetXPlayersLive(int x)
        {
            List<LavaQuestConfig.Players.Player> list = new();

            foreach (var player in players.Value.datas)
            {
                if (list.Count == x) break;

                if (player.state == LavaQuestConfig.Players.Player.EState.Die) continue;

                list.Add(player);
            }

            return list;
        }

        public List<LavaQuestConfig.Players.Player> GetListPlayersAtState(LavaQuestConfig.Players.Player.EState eState)
        {
            List<LavaQuestConfig.Players.Player> list = new();

            foreach (var player in players.Value.datas)
            {
                if (player.state != eState) continue;

                list.Add(player);
            }

            return list;
        }

        public List<int> GetIndexesOfPlayersAtState(LavaQuestConfig.Players.Player.EState eState)
        {
            List<int> indexes = new();

            for (int i = 0; i < players.Value.datas.Count; i++)
            {
                if (players.Value.datas[i].state == eState)
                {
                    indexes.Add(i);
                }
            }

            return indexes;
        }

        private void TryKillPlayer()
        {
            int countPlayerKilled = Config.GetCountPlayerKilled(step.Value);

            var aliveIndexes = GetIndexesOfPlayersAtState(LavaQuestConfig.Players.Player.EState.Live);

            CacheCountLastPlayer = aliveIndexes.Count;

            aliveIndexes.Remove(0);

            countPlayerKilled = Mathf.Min(countPlayerKilled, aliveIndexes.Count);

            Debug.LogError("Kill: " + countPlayerKilled);

            // Shuffle indexes
            for (int i = 0; i < aliveIndexes.Count; i++)
            {
                int randIndex = UnityEngine.Random.Range(i, aliveIndexes.Count);
                (aliveIndexes[i], aliveIndexes[randIndex]) = (aliveIndexes[randIndex], aliveIndexes[i]);
            }

            // Kill theo index trong players.Value.datas
            for (int i = 0; i < countPlayerKilled; i++)
            {
                int idx = aliveIndexes[i];

                var p = players.Value.datas[idx];
                p.state = LavaQuestConfig.Players.Player.EState.Die;
                players.Value.datas[idx] = p;
            }

            players.Save();

            CacheCountCurPlayer = CacheCountLastPlayer - countPlayerKilled;
        }

        public void UpdateProfile(ProfileService.ProfileChangeEvent profileChangeEvent)
        {
            if (!isJoined.BoolValue) return;

            if (!players.Exist) return;

            players.Value.datas[0].UpdateProfile(profileChangeEvent.frameID, profileChangeEvent.avatarID);

            players.Save();
        }

        public string GetStringTimeCountdown()
        {
            if (isExpired.BoolValue)
                return FormatTimeClock(TimeSpan.Zero);

            DateTime now = timeService.GetCurrentTime();
            DateTime endTime = timeEnd.Value.ToLocalDateTime();

            TimeSpan remaining = endTime - now;

            if (remaining <= TimeSpan.Zero)
                return FormatTimeClock(TimeSpan.Zero);

            return FormatTimeClock(remaining);
        }

        public string GetStringTimeGapCountdown()
        {
            DateTime now = timeService.GetCurrentTime();
            DateTime endTime = nextTimeEvent.Value.ToLocalDateTime();

            TimeSpan remaining = endTime - now;

            if (remaining <= TimeSpan.Zero)
                return FormatTimeClock(TimeSpan.Zero);

            return FormatTimeClock(remaining);
        }

        private string FormatTimeClock(TimeSpan timeSpan)
        {
            return $"{(int)timeSpan.TotalHours:00}:{timeSpan.Minutes:00}:{timeSpan.Seconds:00}";
        }
    }
}