using System;
using System.Collections;
using System.Collections.Generic;
using SonatFramework.Systems;
using SonatFramework.Systems.TimeManagement;
using UnityEngine;

public class BackToSchoolWidget : PackIapWidget
{
    [SerializeField] private long timeFinish;
    public override void Setup()
    {
        active = active && CheckActive();
        base.Setup();
    }
    
    public override void OnFocus()
    {
        if (!CheckActive())
        {
            gameObject.SetActive(false);
            return;
        }
        base.OnFocus();
    }

    protected override bool CheckActive()
    {
        if(!base.CheckActive()) return false;
        long currentTime = SonatSystem.GetService<TimeService>().GetUnixTimeSeconds();
        if (currentTime >= timeFinish)
        {
            return false;
        }
        return true;
    }
    
}