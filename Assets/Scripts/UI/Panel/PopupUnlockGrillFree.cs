using System;
using System.Collections;
using System.Collections.Generic;
using Gameplay.Entities;
using GrillSort.PreBooster;
using Sonat.Enums;
using SonatFramework.Scripts.Feature.Shop.UI;
using SonatFramework.Scripts.SonatSDKAdapterModule;
using SonatFramework.Scripts.UIModule;
using SonatFramework.Systems.InventoryManagement;
using UnityEngine;

public class PopupUnlockGrillFree : Panel
{
    private Action callback;
    private PrimaryGrill primaryGrill;

    public override void Open(UIData uiData)
    {
        base.Open(uiData);
        primaryGrill = null;
        callback = null;
        if (uiData.TryGet("PrimaryGrill", out primaryGrill))
        {

        }
        if (uiData.TryGet("Callback", out callback))
        {

        }
    }

    public void OnClickUnlockFree()
    {
        Close();
        callback?.Invoke();
    }
}
