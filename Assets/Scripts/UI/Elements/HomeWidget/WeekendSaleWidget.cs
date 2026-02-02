using System.Collections;
using System.Collections.Generic;
using Sonat;
using SonatFramework.Systems;
using SonatFramework.Systems.TimeManagement;
using UnityEngine;
using System;

public class WeekendSaleWidget : PackIapWidget
{
    private PlayerPrefLong lastTimeBuy;

    public override void Setup()
    {
        lastTimeBuy = new PlayerPrefLong("LastTimeBuyWeekendSale");

        active = active && CheckDayOfWeek() && CheckLastTime();

        base.Setup();
    }

    public override void OnFocus()
    {
        if (!CheckDayOfWeek())
        {
            gameObject.SetActive(false);
            return;
        }
        base.OnFocus();
    }

    private bool CheckDayOfWeek()
    {
        var timeService = SonatSystem.GetService<TimeService>();
        var currentTime = timeService.GetCurrentTime();

        return currentTime.DayOfWeek == DayOfWeek.Sunday || currentTime.DayOfWeek >= DayOfWeek.Friday;
    }

    private bool CheckLastTime()
    {
        if (lastTimeBuy.Value == 0) return true;
        
        var timeService = SonatSystem.GetService<TimeService>();
        var currentTime = timeService.GetCurrentTime();
        var lastBuyTime = DateTimeOffset.FromUnixTimeSeconds(lastTimeBuy.Value).ToLocalTime().DateTime;
        
        // Check if the last purchase was in the same week as current time
        var currentWeekStart = GetWeekStart(currentTime);
        var lastBuyWeekStart = GetWeekStart(lastBuyTime);
        
        // If both dates are in the same week, return false (widget should be inactive)
        return currentWeekStart != lastBuyWeekStart;
    }
    
    private DateTime GetWeekStart(DateTime date)
    {
        // Get the start of the week (Monday)
        int daysSinceMonday = (int)date.DayOfWeek - (int)DayOfWeek.Monday;
        if (daysSinceMonday < 0) daysSinceMonday += 7; // Sunday becomes 6
        return date.Date.AddDays(-daysSinceMonday);
    }

    public override bool CheckShowPopup()
    {
        return base.CheckShowPopup();
    }

    public override void OpenPopup()
    {
        if (!CheckDayOfWeek())
        {
            gameObject.SetActive(false);
            return;
        }
        base.OpenPopup();
    }

    protected override void BuyComplete()
    {
        base.BuyComplete();
        lastTimeBuy.Value = SonatSystem.GetService<TimeService>().GetUnixTimeSeconds();
        active = false;
        gameObject.SetActive(false);
    }
}
