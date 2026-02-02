using System;
using DG.Tweening;
using SonatFramework.Scripts.UIModule;
using SonatFramework.Scripts.Utils;
using UnityEngine;

public class PopupWarningDynamite : Panel
{
    [SerializeField] private Transform popupTransform;
    private bool clickAvailable;
    private PrimaryGrillBomb primaryGrillBomb;

    public override void Open(UIData uiData)
    {
        popupTransform.SetLocalPositionX(200);
        base.Open(uiData);
        uiData.TryGet("Dynamite", out primaryGrillBomb);
    }

    public override void OnOpenCompleted()
    {
        base.OnOpenCompleted();
        popupTransform.DOLocalMoveX(0, 0.5f).OnComplete(() => clickAvailable = true);
    }

    public void SkipClick()
    {
        if(!clickAvailable) return;
        PanelManager.Instance.OpenPanel<PopupSkipDynamite>(new UIData().Add("Dynamite", primaryGrillBomb).Add("OnSkipDynamite", (Action)OnSkipDynamite));
    }

    public void FinishWarning()
    {
        clickAvailable = false;
        popupTransform.DOLocalMoveX(200, 0.5f).OnComplete(Close);
    }

    public void OnSkipDynamite()
    {
        Close();
    }
}
