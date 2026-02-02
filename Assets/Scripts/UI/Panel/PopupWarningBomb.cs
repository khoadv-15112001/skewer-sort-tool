using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using Gameplay.Entities.Items;
using SonatFramework.Scripts.UIModule;
using SonatFramework.Scripts.Utils;
using UnityEngine;

public class PopupWarningBomb : Panel
{
    [SerializeField] private Transform popupTransform;
    private bool clickAvailable;
    private ItemBombMove itemBomb;

    public override void Open(UIData uiData)
    {
        popupTransform.SetLocalPositionX(200);
        base.Open(uiData);
        uiData.TryGet("Bomb", out itemBomb);
    }

    public override void OnOpenCompleted()
    {
        base.OnOpenCompleted();
        popupTransform.DOLocalMoveX(0, 0.5f).OnComplete(() => clickAvailable = true);
    }

    public void SkipClick()
    {
        if(!clickAvailable) return;
        PanelManager.Instance.OpenPanel<PopupSkipBomb>(new UIData().Add("Bomb", itemBomb).Add("OnSkipBomb", (Action)OnSkipBomb));
    }

    public void FinishWarning()
    {
        clickAvailable = false;
        popupTransform.DOLocalMoveX(200, 0.5f).OnComplete(Close);
    }

    public void OnSkipBomb()
    {
        Close();
    }
    
}
