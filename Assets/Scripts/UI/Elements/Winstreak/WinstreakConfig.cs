using GrillSort.RealTime;
using Newtonsoft.Json;
using Sonat.Data;
using SonatFramework.Scripts.SonatSDKAdapterModule;
using SonatFramework.Systems;
using SonatFramework.Systems.InventoryManagement.GameResources;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace GrillSort.Winstreak
{
    [CreateAssetMenu(fileName = "WinstreakConfig", menuName = "Sonat Configs Custom/WinstreakConfig", order = 0)]
    public class WinstreakConfig : ScriptableObject
    {
        [Header("Gameplay")]
        public int levelStart = 90;
        public int dayUnlock = 21;
        public int maxLives = 3;

        [Header("Icons")]
        public Sprite iconItem;
        public Sprite iconLives;
        public Sprite iconWidget;
        [Header("Win Streak - Board")]
        public List<Sprite> boardSprites;

        [Header("Win Streak - Weekly Window (UTC)")]
        public TimeConfig timeConfig;

        [Header("Win Streak - Chest")]
        public List<ChestSpite> chestSprites;

        [Header("Win Streak - Milestones")]
        public List<WinStreakMilestone> rewardDatas;

        private readonly Service<RealTimeService> _realTimeService = new();

        // ================= REMOTE CONFIG =================

        public void LoadRemoteConfig()
        {
            levelStart = SonatSDKAdapter.GetRemoteInt("win_streak_level_start", levelStart);
            maxLives = SonatSDKAdapter.GetRemoteInt("win_streak_max_lives", maxLives);
            dayUnlock = SonatSDKAdapter.GetRemoteInt("win_streak_user_day", dayUnlock);
            timeConfig = SonatSDKAdapter.GetRemoteConfig<TimeConfig>("win_streak_time_config", timeConfig);
            rewardDatas = SonatSDKAdapter.GetRemoteConfig<List<WinStreakMilestone>>("win_streak_milestones_config", rewardDatas);
        }

        // ================= TIME HELPERS (UTC-safe) =================

        /// <summary>Thời điểm hiện tại theo RealTimeService (UTC DateTime).</summary>
        internal DateTime TimeNow()
        {
            long nowUnix = _realTimeService.Instance.GetCurrentTimeUnix();
            return DateTimeOffset.FromUnixTimeSeconds(nowUnix).UtcDateTime;
        }

        /// <summary>Thời điểm hiện tại theo RealTimeService (UTC epoch seconds).</summary>
        internal long TimeUnix()
        {
            return _realTimeService.Instance.GetCurrentTimeUnix();
        }

        /// <summary>Convert DateTime (UTC) → epoch seconds (ép Kind=Utc nếu cần).</summary>
        internal long ToUnix(DateTime utc)
        {
            if (utc.Kind != DateTimeKind.Utc)
                utc = DateTime.SpecifyKind(utc, DateTimeKind.Utc);
            return new DateTimeOffset(utc, TimeSpan.Zero).ToUnixTimeSeconds();
        }
        // Start của cửa sổ chứa 'now':
        // - luôn là LẦN GẦN NHẤT của (startDow, startHour) mà <= now
        internal DateTime GetStartThisWeekUtc(DateTime nowUtc)
        {
            int nowDow = (int)nowUtc.DayOfWeek;
            int startDow = (int)timeConfig.eventStartDay.day;

            // số ngày kể từ lần start gần nhất (0..6)
            int backDays = (7 + nowDow - startDow) % 7;

            // ứng viên: lùi 'backDays' về đúng thứ start, đặt giờ start
            var candidate = new DateTime(
                nowUtc.Year, nowUtc.Month, nowUtc.Day,
                timeConfig.eventStartDay.hour, 0, 0, DateTimeKind.Utc
            ).AddDays(-backDays);

            // nếu ứng viên vẫn > now (cùng ngày nhưng giờ start > now) → lùi thêm 7 ngày
            if (candidate > nowUtc)
                candidate = candidate.AddDays(-7);

            return candidate;
        }

        // End > Start; nếu cùng ngày nhưng giờ end <= giờ start → +7 ngày
        internal DateTime GetEndFromStartUtc(DateTime startUtc)
        {
            int forwardDays = ((int)timeConfig.eventEndDay.day - (int)timeConfig.eventStartDay.day + 7) % 7;

            var end = new DateTime(
                startUtc.Year, startUtc.Month, startUtc.Day,
                0, 0, 0, DateTimeKind.Utc
            )
            .AddDays(forwardDays)
            .AddHours(timeConfig.eventEndDay.hour);

            if (end <= startUtc)
                end = end.AddDays(7);

            return end;
        }

        // Cửa sổ "tuần linh hoạt":
        // - Ưu tiên cửa sổ chứa now (start <= now < end)
        // - Chỉ khi now >= end mới trượt sang tuần sau
        internal (DateTime start, DateTime end) ResolveWindowFlexible(DateTime nowUtc)
        {
            DateTime start = GetStartThisWeekUtc(nowUtc);
            DateTime end = GetEndFromStartUtc(start);

            if (nowUtc >= end)
            {
                start = start.AddDays(7);
                end = GetEndFromStartUtc(start);
            }
            return (start, end);
        }


        // ================= DEBUG HELPERS =================

        [Header("Debug")]
        public bool debugWeeklyWindow = false;

        public void DebugWindowAtUtc(DateTime nowUtc, string tag = null)
        {
            var (start, end) = ResolveWindowFlexible(nowUtc);
            string prefix = string.IsNullOrEmpty(tag) ? "[WinStreak][Window]" : $"[WinStreak][Window][{tag}]";
            Debug.Log(
                $"{prefix}\n" +
                $" nowUtc  : {nowUtc:O}\n" +
                $" startUtc: {start:O}\n" +
                $" endUtc  : {end:O}\n" +
                $" nowLocal: {nowUtc.ToLocalTime():yyyy-MM-dd ddd HH:mm}\n" +
                $" startLoc: {start.ToLocalTime():yyyy-MM-dd ddd HH:mm}\n" +
                $" endLoc  : {end.ToLocalTime():yyyy-MM-dd ddd HH:mm}\n" +
                $" inWindow: {nowUtc >= start && nowUtc < end}"
            );
        }

        public void DebugWindowNow(string tag = null) => DebugWindowAtUtc(TimeNow(), tag);

        [ContextMenu("WinStreak: Self Test Weekly Window")]
        public void SelfTestWeeklyWindow()
        {
            var now = TimeNow();
            var (start, end) = ResolveWindowFlexible(now);

            var samples = new[]
            {
                ("start-2h", start.AddHours(-2)),
                ("start-1h", start.AddHours(-1)),
                ("start   ", start),
                ("start+1h", start.AddHours(1)),
                ("mid     ", start.AddDays(1)),
                ("end-1h  ", end.AddHours(-1)),
                ("end     ", end),
                ("end+1h  ", end.AddHours(1)),
            };

            Debug.Log($"[WinStreak][Window][Config]\n startUtc={start:O}\n endUtc={end:O}\n nowUtc={now:O}");
            foreach (var (label, t) in samples) DebugWindowAtUtc(t, label);
        }

        // ================= OTHER DATA =================

        public Sprite GetChestSpite(int milestone)
        {
            if (milestone <= 3) return chestSprites[0].sprite;
            if (milestone <= 6) return chestSprites[1].sprite;
            if (milestone <= 8) return chestSprites[2].sprite;
            if (milestone == 9) return chestSprites[3].sprite;

            return chestSprites[0].sprite;
        }
        internal Sprite GetSpriteBoard(int milestone)
        {
            if (milestone <= 6) return boardSprites[0];
            if (milestone <= 8) return boardSprites[1];
            if (milestone == 9) return boardSprites[2];
            return null;
        }

        internal ChestType GetChestType(int milestone)
        {
            if (milestone <= 6) return ChestType.chest_1;
            if (milestone <= 8) return ChestType.chest_2;
            if (milestone == 9) return ChestType.chest_3;
            return ChestType.none;
        }
    }

    [Serializable]
    public class WinStreakMilestone
    {
        public int levelWinRequired;
        public RewardData reward;
        public RewardData railReward;

        public RewardData GetRewardData()
        {
#if CUSTOM_ENUM
            // Nếu user join Rail event và có railReward → return railReward
            if (GrillSort.Rail.RailService.Instance != null && 
                GrillSort.Rail.RailService.Instance.IsJoinEvent() && 
                railReward != null && 
                railReward.resourceDatas != null && 
                railReward.resourceDatas.Count > 0)
            {
                return railReward;
            }
#endif
            return reward;
        }
    }

    [Serializable]
    public class ChestSpite
    {
        public ChestType chestType;
        public Sprite sprite;
    }

    public enum ChestType
    {
        none = -1, chest_1, chest_2, chest_3
    }

    [Serializable]
    public class TimeConfig
    {
        public DayConfig eventStartDay;
        public DayConfig eventEndDay;

        public TimeConfig()
        {
            eventStartDay = new DayConfig();
            eventEndDay = new DayConfig();
        }

        public TimeConfig(TimeConfig other)
        {
            if (other == null)
            {
                eventStartDay = new DayConfig();
                eventEndDay = new DayConfig();
                return;
            }
            eventStartDay = other.eventStartDay != null ? new DayConfig(other.eventStartDay) : new DayConfig();
            eventEndDay = other.eventEndDay != null ? new DayConfig(other.eventEndDay) : new DayConfig();
        }

        public TimeConfig Clone() => new TimeConfig(this);
    }

    [Serializable]
    public class DayConfig
    {
        public DayOfWeek day;
        [Range(0, 23)] public int hour;

        public DayConfig() { }

        public DayConfig(DayConfig other)
        {
            if (other == null) return;
            day = other.day;
            hour = other.hour;
        }
    }
}
