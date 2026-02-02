using Sonat;
using System;
using UnityEngine;
using SonatFramework.Systems;
using Cysharp.Threading.Tasks;
using System.Linq;
using GrillSort.RealTime;
using SonatFramework.Systems.EventBus;
using SonatFramework.Scripts.Helper;
using Newtonsoft.Json;

namespace GrillSort.Winstreak
{
    [CreateAssetMenu(fileName = "WinstreakService", menuName = "My Services/WinstreakService")]
    public class WinStreakManager : SonatServiceSo, IServiceInitializeAsync
    {
        public WinstreakConfig config;

        // Persistent states - save progress
        public IntDataPref lives;
        public IntDataPref winningInARow;
        private ListDataPref<int> milestoneClaimed;

        // khong save progress
        public PlayerPrefLong timeFinishEvent;
        public PlayerPrefInt EventActive;

        public Action OnEventActive;
        public Action OnEventFinish;

        public int lastWinningInARow;

        public int ItemCollect = 0;
        public int TotalMilestone => config.rewardDatas != null ? config.rewardDatas.Count : 0;
        public int Level => MySonatFramework.userDataService.GetLevel();

        public Action OnChangeData { get; internal set; }

        private readonly Service<RealTimeService> realTimeService = new();

        // Intro popup 1 lần
        public bool ShowIntroOnOpen
        {
            get => PlayerPrefs.GetInt("WIN_STREAK_INTRO_SHOWN", 0) == 0;
            set => PlayerPrefs.SetInt("WIN_STREAK_INTRO_SHOWN", value ? 0 : 1);
        }

        // ================= INIT =================

        public async UniTaskVoid InitializeAsync()
        {
            await LoadData();
        }

        public async UniTask LoadData()
        {
            lives = new IntDataPref("WIN_STREAK_LIVES");
            winningInARow = new IntDataPref("WIN_STREAK_IN_AROW");
            milestoneClaimed = new ListDataPref<int>("WIN_STREAK_MILESTONE_CLAIMEDS");

            EventActive = new PlayerPrefInt("WIN_STREAK_ACTIVE");
            timeFinishEvent = new PlayerPrefLong("WIN_STREAK_TIME_FINISH");

            config.LoadRemoteConfig();

            lastWinningInARow = winningInARow.Value;

            ItemCollect = 0;

            await UniTask.Yield();

            CheckActiveEvent();

            new EventBinding<LevelEndedEvent>(OnEndLevel);

            Debug.Log("[Winstreak] init done.");
        }

        // ================= PUBLIC =================
        internal bool HasNoti()
        {
            return ItemCollect > 0;
        }
        public bool IsMax()
        {
            if (config.rewardDatas == null || config.rewardDatas.Count == 0) return false;
            return winningInARow.Value >= config.rewardDatas.Last().levelWinRequired;
        }

        public bool CanUnlock()
        {
            Debug.Log($"[Winstreak] userDay={MySonatFramework.userDataService.UserDay} -- config={config.dayUnlock}");
            return Level >= config.levelStart && MySonatFramework.userDataService.UserDay >= config.dayUnlock;
        }
        public bool CanReduceLives() => lives.Value > 1 && winningInARow.Value > 0;
        public void ReduceLives() => lives.Value = Mathf.Max(0, lives.Value - 1);

        public WinStreakMilestone GetMilestoneData(int index)
        {
            if (config.rewardDatas == null) return null;
            if (index < 0 || index >= config.rewardDatas.Count) return null;
            return config.rewardDatas[index];
        }

        public int GetCurrentMilestone()
        {
            if (config.rewardDatas == null || config.rewardDatas.Count == 0) return 0;
            for (int i = 0; i < config.rewardDatas.Count; i++)
                if (winningInARow.Value < config.rewardDatas[i].levelWinRequired) return i;
            return config.rewardDatas.Count - 1;
        }

        public void ClaimReward(int milestone)
        {
            if (!milestoneClaimed.Contains(milestone))
            {
                milestoneClaimed.Add(milestone);

                if (IsMax()) CheckFinishEvent();

                OnChangeData?.Invoke();
            }
        }

        public bool CheckMilestoneClaimed(int milestone) => milestoneClaimed.Contains(milestone);

        public static string FomatTimeFromSecDSort(long sec)
        {
            TimeSpan time = TimeSpan.FromSeconds(sec);
            if (sec > 3600) return $"{time.Days}d:{time.Hours}h";
            else return $"{time.Minutes}m:{time.Seconds}s";
        }

        public long GetTimeToReset() => GetRemainingTimeUnix();

        internal bool CheckHasMilestoneClaimed()
        {
            for (var i = 0; i < GetCurrentMilestone(); i++)
                if (!milestoneClaimed.Contains(i)) return true;
            return false;
        }

        // ================= EVENT FLOW =================
        private void ResetWinstreak()
        {
            milestoneClaimed.Clear();

            //Debug.Log($"[Winstreak] milestoneClaimed={JsonConvert.SerializeObject(milestoneClaimed)}");

            winningInARow.Value = 0;
            lives.Value = config.maxLives;
            ItemCollect = 0;

            Debug.Log($"[Winstreak] reset winstreak");
        }
        public void ActiveEvent()
        {
            if (EventActive.BoolValue) return;

            EventActive.BoolValue = true;

            ResetWinstreak();

            var (start, end) = config.ResolveWindowFlexible(config.TimeNow());
            timeFinishEvent.Value = config.ToUnix(end);
            Debug.Log($"[Winstreak] ActiveEvent: start={start:O} end={end:O} now={config.TimeNow():O}");

            OnEventActive?.Invoke();
        }

        public void FinishEvent()
        {
            if (!EventActive.BoolValue) return;
            Debug.Log($"[Winstreak] FinishEvent");

            EventActive.BoolValue = false;

            ResetWinstreak();

            DateTime now = config.TimeNow();
            var (start, end) = config.ResolveWindowFlexible(now);

            OnEventFinish?.Invoke();
        }

        public void CheckActiveEvent()
        {
            if (EventActive.BoolValue)
            {
                CheckFinishEvent();
            }

            if (!CanUnlock()) return;

            DateTime now = config.TimeNow();
            var (start, end) = config.ResolveWindowFlexible(now);

            bool inWindow = now >= start && now < end;

            if (inWindow)
            {
                timeFinishEvent.Value = config.ToUnix(end);
                if (!EventActive.BoolValue) ActiveEvent();
            }

        }

        public void CheckFinishEvent()
        {
            if (!EventActive.BoolValue) return;

            if (realTimeService.Instance.GetCurrentTimeUnix() > timeFinishEvent.Value)
                FinishEvent();
        }

        private void OnEndLevel(LevelEndedEvent e)
        {
            if (e.success)
            {
                if (!EventActive.BoolValue)
                {
                    CheckActiveEvent();
                    return;
                }

                if (IsMax()) return;

                lastWinningInARow = winningInARow.Value;

                winningInARow.Value++;
                ItemCollect = 1;
            }
            else
            {
                if (!EventActive.BoolValue) return;

                if (CanReduceLives()) ReduceLives();
                else
                {
                    lastWinningInARow = winningInARow.Value;
                    winningInARow.Value = 0;
                    lives.Value = config.maxLives;
                }
                ItemCollect = 0;

                CheckFinishEvent();
            }
        }

        // ================= PRIVATE =================

        private long GetRemainingTimeUnix()
        {
            long now = config.TimeUnix();
            long end = timeFinishEvent.Value;
            return (end > now) ? (end - now) : 0L;
        }

        internal bool CheckWarning()
        {
            return EventActive.BoolValue && !CheckCompletedEvent();
        }

        internal bool CheckCompletedEvent()
        {
            return IsMax() && !CheckHasMilestoneClaimed();
        }
    }
}
