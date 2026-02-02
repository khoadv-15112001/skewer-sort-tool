using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using I2.Loc;
using Sonat.Enums;
using SonatFramework.Scripts.UIModule;
using SonatFramework.Scripts.Utils;
using SonatFramework.Systems.SceneManagement;
using UnityEngine;

public class PopupAreYousure :  PopupContinueBase
{
    [SerializeField] private string DescriptionOutOfTime = "Add 30s to continue";
    [SerializeField] private string  DescriptionOutOfMove = "Merge 3 items to continue";
    public override void Open(UIData uiData)
    {
        base.Open(uiData);
        MySonatFramework.customTrackingService.OnShowPopup("safety_net_bundle", "banner", "iap", "auto");
    }
    public virtual void PlayOnWithAdsClick()
    {
        MySonatFramework.ShowRewardAds(OnReviveWithAds, "booster", "revive");
    }
    public override void OnGiveUpClick()
    {
        // if (MySonatFramework.livesService.isUnlimitedLive.BoolValue)
        // {
        //     Close();
        //     data?.onClose?.Invoke();
        // }
        // else 
        // if(MySonatFramework.livesService.CanPlay())
        // {
        //     UIData data = new UIData();
        //     data.Add("OnConfirm", (Action)OnConFirmGiveUp);
        //     PanelManager.Instance.OpenPanel<PopupLostLives>(data);
        // }
        // else
        // {
        //     //PanelManager.Instance.OpenPanel<PopupRefillLives>();
        //     Close();
        //     data?.onClose?.Invoke();
        // }
        CloseImmediately();
        LoadingScreenInstance.Instance.Show(1.5f);
        SonatUtils.DelayCall(0.5f, () => { MySonatFramework.GetService<SceneService>().SwitchScene(GamePlacement.Home); });
    }
    
    protected override void SetLayout()
    {
        switch (data.stuckType)
        {
            case StuckType.OutOfTime:
                if (txtLabel) txtLabel.GetComponent<Localize>().SetTerm("Out Of Time");
                if (txtDescription) txtDescription.GetComponent<Localize>().SetTerm(DescriptionOutOfTime);
                if (icon) icon.sprite = iconsSprites[0];
                break;
            case StuckType.OutOfMove:
                if (txtLabel) txtLabel.GetComponent<Localize>().SetTerm("Out Of Move");
                if (txtDescription) txtDescription.GetComponent<Localize>().SetTerm(DescriptionOutOfMove);
                if (icon) icon.sprite = iconsSprites[1];
                break;
        }
    }

    private void OnConFirmGiveUp()
    {
        MySonatFramework.livesService.ReduceLive(1, new() { earnType = "lose", earnId = "lose" });
        //MySonatFramework.livesService.ReduceLive(1, "lose");
        Close();
        data?.onClose?.Invoke();
    }

    public void HoldToView()
    {
        panelCanvasGroup.DOFade(0, 0.2f);
    }

    public void FinishHoldToView()
    {
        panelCanvasGroup.DOFade(1, 0.2f);
    }
}
