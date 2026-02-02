using Cysharp.Threading.Tasks;
using GrillSort.LavaQuest;
using Helper;
using Sonat.Enums;
using SonatFramework.Scripts.Helper;
using SonatFramework.Scripts.SonatSDKAdapterModule;
using SonatFramework.Scripts.Utils;
using SonatFramework.Systems;
using SonatFramework.Systems.EventBus;
using SonatFramework.Systems.TimeManagement;
using Spine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace GrillSort.KitchenMission
{
    [CreateAssetMenu(fileName = "KitchenMissionService", menuName = "My Services/KitchenMissionService")]
    public class KitchenMissionService : SonatServiceSo, IServiceInitializeAsync
    {
        public static KitchenMissionService Instance => SonatSystem.GetService<KitchenMissionService>();

        [SerializeField] private KitchenMissionConfig _config;
        public KitchenMissionConfig Config => _config;

        public IntDataPref isUnlocked;
        public IntDataPref isShownPopup;
        public IntDataPref isShownTut;
        public IntDataPref isEventActive;
        public IntDataPref isJoined;

        public LongDataPref timeStartEvent;
        public LongDataPref timeEndEvent;
        public IntDataPref stage;
        public ClassDataPref<KitchenMissionConfig.Players> players;
        public LongDataPref lastTimeCycle;
        public IntDataPref isOver;
        public IntDataPref isCompleteEvent;

        public IntDataPref numUseBooster;
        public IntDataPref numUseRevive;
        public IntDataPref isExpiredModifyBot;
        public IntDataPref isBoughtPack;

        public IntDataPref start_count;
        public IntDataPref continue_count;
        public IntDataPref isLogStart;

        public static int NumKey;

        private const string prefix = "kitchen_mission";

        public static Action OnChangePlayerData;
        public static Action OnCompleteEvent;
        public static Action OnNextStage;
        public static Action OnTick;
        public static Action OnResetEvent;
        public static Action OnBoughtPack;

        private TimeService timeService => MySonatFramework.GetService<TimeService>();

        public async UniTaskVoid InitializeAsync()
        {
            isUnlocked = new IntDataPref($"{prefix}_is_unlocked");
            isShownPopup = new IntDataPref($"{prefix}_is_shown_popup");
            isShownTut = new IntDataPref($"{prefix}_is_shown_tut");
            isEventActive = new IntDataPref($"{prefix}_is_event_active");
            isJoined = new IntDataPref($"{prefix}_is_joined");

            timeStartEvent = new LongDataPref($"{prefix}_time_start_event");
            timeEndEvent = new LongDataPref($"{prefix}_time_end_event");

            stage = new IntDataPref($"{prefix}_stage");
            players = new ClassDataPref<KitchenMissionConfig.Players>($"{prefix}_players");
            lastTimeCycle = new LongDataPref($"{prefix}_last_time_cycle");
            isOver = new IntDataPref($"{prefix}_is_over");
            isCompleteEvent = new IntDataPref($"{prefix}_is_complete_event");

            numUseBooster = new IntDataPref($"{prefix}_num_use_booster");
            numUseRevive = new IntDataPref($"{prefix}_num_use_revive");
            isExpiredModifyBot = new IntDataPref($"{prefix}_is_expired_modify_bot");
            isBoughtPack = new IntDataPref($"{prefix}_is_bought_pack");

            start_count = new IntDataPref($"{prefix}_start_count");
            continue_count = new IntDataPref($"{prefix}_continue_count");
            isLogStart = new IntDataPref($"{prefix}_is_log_start");

            await UniTask.Yield();

            TryActive();

            new EventBinding<ProfileService.ProfileChangeEvent>(UpdateProfile);
            new EventBinding<LevelEndedEvent>(OnLevelEnd);
            new EventBinding<UseBoosterEvent>(OnUseBooster);
            new EventBinding<LevelContinueEvent>(OnUseRevive);

            // For Cheat test
            CheckOver();

            TickService.Register(Tick, TickService.TickPhase.TickRealtime);
        }

        public bool CanActive()
        {
            return isUnlocked.BoolValue && IsInTimeEvent() && !isCompleteEvent.BoolValue;
        }

        private void CheckOver()
        {
            if (!isJoined.BoolValue) return;
            if (isOver.BoolValue) return;

            foreach (var player in players.Value.datas)
            {
                if (player.step == GetCurrentStageData().maxStep)
                {
                    isOver.BoolValue = true;
                    return;
                }
            }
        }

        private void Tick()
        {
            if (!isUnlocked.BoolValue) return;

            OnTick?.Invoke();

            if (isEventActive.BoolValue && CheckPassTime())
            {
                if (isJoined.BoolValue)
                {
                    CheckModifyBot();
                    CheckTimeCycle();
                    isOver.BoolValue = true;
                }
                else
                {
                    ResetEvent();
                }

                return;
            }

            if (!isJoined.BoolValue) return;

            CheckModifyBot();
            CheckTimeCycle();
        }

        public bool IsInTimeEvent()
        {
            DateTime now = GetTimeNow();
            int dayOfWeek = (int)now.DayOfWeek;

            DateTime startEvent = now.Date.AddDays(-(dayOfWeek - 5));
            if (dayOfWeek < 5)
                startEvent = startEvent.AddDays(-7);

            DateTime endEvent = startEvent.AddDays(3);

            bool isInEvent = now >= startEvent && now < endEvent;

            return isInEvent;
        }

        public bool CheckPassTime()
        {
            return GetTimeUnixNow() >= timeEndEvent.Value;
        }

        private void CheckTimeCycle()
        {
            if (isOver.BoolValue) return;

            long now = GetTimeUnixNow();
            long lastTime = lastTimeCycle.Value;
            long timePerCycle = Config.botConfig.TimeCycle;

            if (lastTime <= 0)
            {
                lastTimeCycle.Value = now;
                return;
            }

            long elapsed = now - lastTime;

            if (elapsed >= timePerCycle)
            {
                long cycles = elapsed / timePerCycle;

                cycles = (long)Mathf.Clamp(cycles, 0, 25);

                for (int i = 0; i < cycles; i++)
                {
                    OnCycleElapsed();
                }

                long remainder = elapsed % timePerCycle;
                lastTimeCycle.Value = now - remainder;
            }
        }

        private void OnCycleElapsed()
        {
            //Debug.LogError("[KitchenMissionService] OnCycleElapsed");

            var listPlayer = players.Value.datas;

            for (int i = 0; i < listPlayer.Count; i++)
            {
                var player = listPlayer[i];

                if (player.isYourself) continue;

                float randomPercentage = UnityEngine.Random.Range(0f, 1f);

                if (randomPercentage <= Config.botConfig.GetLoseRate())
                {
                    player.LoseBot();
                }
                else if (randomPercentage <= Config.botConfig.GetSpeedAfterCalculateVariance())
                {
                    player.WinBot();
                }
            }

            players.Save();

            OnChangePlayerData?.Invoke();
        }

        private void CheckModifyBot()
        {
            if (isExpiredModifyBot.BoolValue) return;

            var timeSafe = timeStartEvent.Value + GetCurrentStageData().modifyBotConfig.timeRange;
            var now = GetTimeUnixNow();

            if (now >= timeSafe) isExpiredModifyBot.BoolValue = true;
        }

        public bool IsPassTimeSafe()
        {
            return isExpiredModifyBot.BoolValue;
        }

        private void TryActive()
        {
            if (isUnlocked.BoolValue) return;

            if (MySonatFramework.userDataService.GetLevel() >= GetLevelStart())
            {
                isUnlocked.BoolValue = true;
                //LogStartFeature(0);
            }
        }

        public void UpdateTimeEnd()
        {
            timeEndEvent.Value = GetTimeEnd();
        }

        public int GetLevelStart()
        {
            return SonatSDKAdapter.GetRemoteInt($"{prefix}_level_start", Config.levelStart);
        }

        public void OnJoin()
        {
            if (stage.Value == 0)
            {
                isEventActive.BoolValue = true;
                timeStartEvent.Value = GetTimeUnixNow();
            }

            continue_count.Value++;
            LogStartFeature(continue_count.Value);

            isJoined.BoolValue = true;
            lastTimeCycle.Value = GetTimeUnixNow();
            isOver.BoolValue = false;

            numUseBooster.Value = 0;
            numUseRevive.Value = 0;
            isExpiredModifyBot.Value = 0;

            GeneratePlayers();

            OnChangePlayerData?.Invoke();
        }

        private void ResetEvent()
        {
            if (isJoined.BoolValue)
            {
                LogEndFeature(false, "feature_ended");
            }

            isEventActive.BoolValue = false;
            isJoined.BoolValue = false;
            isOver.BoolValue = false;
            isCompleteEvent.BoolValue = false;
            isBoughtPack.BoolValue = false;

            players.Clear();
            stage.Value = 0;
            continue_count.Value = 0;

            isLogStart.BoolValue = false;

            OnResetEvent?.Invoke();
        }

        private long GetTimeEnd()
        {
            DateTime now = GetTimeNow();

            int dayOfWeek = (int)now.DayOfWeek;
            DateTime startEvent = now.Date.AddDays(-(dayOfWeek - 5));
            if (dayOfWeek < 5)
                startEvent = startEvent.AddDays(-7);

            DateTime endEvent = startEvent.AddDays(3);

            return new DateTimeOffset(endEvent).ToUnixTimeSeconds();
        }

        private void GeneratePlayers()
        {
            var profile = SonatSystem.GetService<ProfileService>();

            players.Clear();

            List<string> randomNames = Config.GetRandomBotNames(4);

            for (int i = 0; i < 4; i++)
            {
                var randomPlayer = new KitchenMissionConfig.Players.Player(randomNames[i], profile.GetRandomFrameSpriteID(), profile.GetRandomAvatarSpriteID());

                players.Value.datas.Add(randomPlayer);
            }

            var myPlayer = new KitchenMissionConfig.Players.Player(profile.Name, profile.FrameId, profile.AvatarId, true);

            players.Value.datas.Insert(2, myPlayer);

            players.Save();
        }

        public void UpdateProfile(ProfileService.ProfileChangeEvent profileChangeEvent)
        {
            if (!isJoined.BoolValue) return;

            if (!players.Exist) return;

            players.Value.datas[2].UpdateProfile(profileChangeEvent.name, profileChangeEvent.frameID, profileChangeEvent.avatarID);

            players.Save();
        }

        public KitchenMissionConfig.Stages.Stage GetCurrentStageData()
        {
            return Config.stages.datas[Mathf.Clamp(stage.Value, 0, Config.stages.datas.Count - 1)];
        }

        public void UpdateStepAllPlayers()
        {
            foreach (var player in players.Value.datas)
                player.lastStep = player.step;

            players.Save();
        }

        private void OnLevelEnd(LevelEndedEvent levelEndedEvent)
        {
            if (levelEndedEvent.gameMode != GameMode.Classic) return;

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

        private void OnWin()
        {
            if (!isUnlocked.BoolValue)
            {
                TryActive();
                return;
            }

            if (!isJoined.BoolValue) return;

            NumKey = 1;

            bool canUp = players.Value.datas[2].WinUser();
            players.Save();

            if (canUp)
                LogProgressFeature();

            OnChangePlayerData?.Invoke();
        }

        private void OnLose()
        {
            if (!isJoined.BoolValue) return;

            //isJoined.BoolValue = false;
            players.Value.datas[2].LoseUser();
            players.Save();

            if (!isOver.BoolValue)
                LogProgressFeature();

            OnChangePlayerData?.Invoke();
        }

        private void OnUseBooster(UseBoosterEvent @event)
        {
            if (isJoined.BoolValue == false) return;

            numUseBooster.Value++;
        }

        private void OnUseRevive(LevelContinueEvent @event)
        {
            if (isJoined.BoolValue == false) return;

            numUseRevive.Value++;
        }

        public int GetMyRank()
        {
            var list = players.Value.datas;
            if (list == null || list.Count == 0)
                return -1;

            var myPlayer = list.FirstOrDefault(p => p.isYourself);
            if (myPlayer == null)
                return -1;

            int rank = 1;
            foreach (var p in list)
            {
                if (p.step > myPlayer.step)
                    rank++;
            }

            return rank;
        }

        public bool IsBotWin()
        {
            foreach (var player in players.Value.datas)
            {
                if (!player.isYourself && player.IsCompleted()) return true;
            }

            return false;
        }

        public bool IsUserWin()
        {
            foreach (var player in players.Value.datas)
            {
                if (player.isYourself && player.IsCompleted()) return true;
            }

            return false;
        }

        public bool IsUserFailed()
        {
            if (IsUserWin()) return false;

            return IsBotWin() || CheckPassTime();
        }

        public void NextStage()
        {
            LogEndFeature(true);

            stage.Value++;

            if (stage.Value >= Config.stages.datas.Count)
            {
                stage.Value = Config.stages.datas.Count - 1;
                CompleteEvent(true);
            }

            players.Clear();
            isJoined.BoolValue = false;
            isOver.BoolValue = false;

            OnNextStage?.Invoke();
        }

        public void CompleteEvent(bool isWin)
        {
            isCompleteEvent.BoolValue = true;
            OnCompleteEvent?.Invoke();

            if (CheckPassTime())
            {
                ResetEvent();
                return;
            }

            if (!isWin)
                LogEndFeature(false, "user_failed");

            players.Clear();
            isJoined.BoolValue = false;
            isOver.BoolValue = false;
        }

        public void BoughtPack()
        {
            isBoughtPack.BoolValue = true;

            OnBoughtPack?.Invoke();
        }

        public void LogStartFeature(int continue_count)
        {
            //Debug.LogError($"[KitchenMissionService]: LogStartFeature: start_count={start_count.Value},stage={stage.Value + 1} ,continue_count={continue_count}");

            Dictionary<string, string> extraParams = new()
            {
                { "stage",(stage.Value + 1).ToString()}
            };

            MySonatFramework.customTrackingService.LogLevelStartFeature("km", "race", start_count.Value, continue_count, extraParams);
        }

        public void LogProgressFeature()
        {
            float completion = players.Exist ? (players.Value.datas[2].step * 1.0f / GetCurrentStageData().maxStep) : 0f;

            string percentage_claim_resource = "";

            string _stage = (stage.Value + 1).ToString();
            string step = players.Exist ? players.Value.datas[2].step.ToString() : "0";
            string milestone = $"{_stage}_{step}";
            int rank = players.Exist ? GetMyRank() : 0;

            Dictionary<string, string> extraParams = new()
            {
                { "stage",_stage}
            };

            MySonatFramework.customTrackingService.LogLevelProgressFeature("km", "race", start_count.Value, 1,
                completion.ToString(), percentage_claim_resource, milestone, rank, extraParams: extraParams);
        }

        public void LogEndFeature(bool success, string end_cause = null)
        {
            if (end_cause == null)
                end_cause = success ? "user_win" : "user_failed";

            float completion = players.Exist ? (players.Value.datas[2].step * 1.0f / GetCurrentStageData().maxStep) : 0f;

            string percentage_claim_resource = "";

            string _stage = (stage.Value + 1).ToString();
            string step = players.Exist ? players.Value.datas[2].step.ToString() : "0";
            string milestone = success ? "" : $"{_stage}_{step}";
            int rank = players.Exist ? GetMyRank() : 0;

            Dictionary<string, string> extraParams = new()
            {
                { "stage",_stage}
            };

            MySonatFramework.customTrackingService.LogLevelEndFeature("km", "race", start_count.Value, 1, success,
                completion.ToString(), percentage_claim_resource, milestone, rank, end_cause, extraParams: extraParams);
        }

        public string GetStringTimeCountdown()
        {
            DateTime now = GetTimeNow();
            DateTime endTime = timeEndEvent.Value.ToLocalDateTime();

            TimeSpan remaining = endTime - now;

            if (remaining <= TimeSpan.Zero)
                return FormatTimeClock(TimeSpan.Zero);

            return FormatTimeClock(remaining);
        }

        private string FormatTimeClock(TimeSpan timeSpan)
        {
            return SonatUtils.FormatTimeSmart((long)timeSpan.TotalSeconds);
        }

        private long GetTimeUnixNow()
        {
            return GetTimeNow().ToUnixTimeSeconds();
        }

        private DateTime GetTimeNow()
        {
#if UNITY_EDITOR
            return DateTime.Now;
#else
            return timeService.GetCurrentTime();
#endif
        }
    }
}