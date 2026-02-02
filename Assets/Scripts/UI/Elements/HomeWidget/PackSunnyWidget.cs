using System;
using System.Collections;
using System.Collections.Generic;
using SonatFramework.Systems;
using SonatFramework.Systems.TimeManagement;
using UnityEngine;

public class PackSunnyWidget : PackIapWidget
{
    public override void Setup()
    {
        active = active && CheckDayOfWeek();
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
        return currentTime.DayOfWeek > DayOfWeek.Sunday && currentTime.DayOfWeek < DayOfWeek.Friday;
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
}