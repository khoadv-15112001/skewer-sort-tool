using GrillSort.RealTime;
using GrillSort.Schedule;
using Helper;
using Sonat.Enums;
using SonatFramework.Systems;
using SonatFramework.Systems.InventoryManagement.GameResources;
using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "TeamOfferConfig", menuName = "Sonat Configs Custom/Team Offer Config")]
public class TeamOfferConfig : ScriptableObject
{
    public int maxBuyCount = 1;

    public TimeConfig timeConfig;

    public List<TeamOfferData> offerData;

    private readonly Service<RealTimeService> _realTimeService = new();

    #region Time

    // Start của cửa sổ chứa 'now'
    // - luôn là LẦN GẦN NHẤT của (startDow, startHour) mà <= now
    internal DateTime GetStartThisWeek(DateTime now)
    {
        int nowDow = (int)now.DayOfWeek;
        int startDow = (int)timeConfig.eventStartDay.day;

        // số ngày kể từ lần start gần nhất (0..6)
        int backDays = (7 + nowDow - startDow) % 7;

        // ứng viên: lùi 'backDays' về đúng thứ start, đặt giờ start
        var candidate = new DateTime(
            now.Year, now.Month, now.Day,
            timeConfig.eventStartDay.hour, 0, 0, GetDateTimeKind()
        ).AddDays(-backDays);

        // nếu ứng viên vẫn > now (cùng ngày nhưng giờ start > now) → lùi thêm 7 ngày
        if (candidate > now)
            candidate = candidate.AddDays(-7);

        return candidate;
    }

    // End > Start; nếu cùng ngày nhưng giờ end <= giờ start → +7 ngày
    internal DateTime GetEndFromStart(DateTime start)
    {
        int forwardDays = ((int)timeConfig.eventEndDay.day - (int)timeConfig.eventStartDay.day + 7) % 7;

        var end = new DateTime(
            start.Year, start.Month, start.Day,
            0, 0, 0, GetDateTimeKind()
        )
        .AddDays(forwardDays)
        .AddHours(timeConfig.eventEndDay.hour);

        if (end <= start)
            end = end.AddDays(7);

        return end;
    }

    // Cửa sổ "tuần linh hoạt":
    // - Ưu tiên cửa sổ chứa now (start <= now < end)
    // - Chỉ khi now >= end mới trượt sang tuần sau
    internal (DateTime start, DateTime end) ResolveWindowFlexible(DateTime now)
    {
        DateTime start = GetStartThisWeek(now);
        DateTime end = GetEndFromStart(start);

        if (now >= end)
        {
            start = start.AddDays(7);
            end = GetEndFromStart(start);
        }
        return (start, end);
    }

    /// <summary>Current time in Local DateTime</summary>
    internal DateTime TimeNow()
    {
        if (timeConfig.useTimeLocal)
            return _realTimeService.Instance.GetCurrentTime();
        else return _realTimeService.Instance.GetCurrentTime().ToUniversalTime();
    }

    /// <summary>Current time in Unix timestamp</summary>
    internal long TimeUnix()
    {
        return TimeNow().ToUnixTimeSeconds();
    }

    internal DateTimeKind GetDateTimeKind()
    {
        return timeConfig.useTimeLocal ? DateTimeKind.Local : DateTimeKind.Utc;
    }
    #endregion

    [Serializable]
    public class TeamOfferData
    {
        public ShopItemKey shopItemKey;
        public string name;
        public RewardData selfRewards;
        public RewardData teamRewards;
        public float price;

        public Color contentColor;
        public Sprite giftIcon;
        public Sprite bgChat;
        public Sprite innerBgChat;
    }
}