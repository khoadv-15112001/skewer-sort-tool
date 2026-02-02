using MyGame.SkewerJam.Scripts.Service;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PackIapWidget_NoAdsJustFun : PackIapWidget
{
    public override void Setup()
    {
        active = !MySonatFramework.GetService<HLWEventService>().IsUnlocked();

        base.Setup();
    }
}
