using System.Collections;
using System.Collections.Generic;
using Gameplay.Entities.GrillScripts;
using Manager;
using UnityEngine;

public class GrillAdsVisual : GrillVisual
{
    [SerializeField] private SpriteRenderer lid_lock;

    protected override void Awake()
    {
        if (GameRemoteConfigValue.popupUnlockTray)
        {
            lid.gameObject.SetActive(false);
            lid_lock.gameObject.SetActive(true);
            lid = lid_lock;
        }
        else
        {
            lid.gameObject.SetActive(true);
            lid_lock.gameObject.SetActive(false);
        }
        base.Awake();
    }
}
