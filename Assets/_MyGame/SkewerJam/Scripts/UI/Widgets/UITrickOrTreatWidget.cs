using System.Collections;
using System.Collections.Generic;
using MyGame.SkewerJam.Scripts.Service;
using SonatFramework.Scripts.UIModule;
using SonatFramework.Systems;
using UnityEngine;

public class TrickOrTreatWidget : UIHomeWidget
{
    private readonly Service<HLWEventService> hlwEventService = new();

    public override void Setup()
    {
        base.Setup();
        if (MySonatFramework.GetService<HLWEventService>().IsUnlocked() == false)
        {
            gameObject.SetActive(false);
            return;
        }

        var remainTime = hlwEventService.Instance.GetRemainTime();
        if (remainTime <= 0)
        {
            gameObject.SetActive(false);
            return;
        }

        hlwEventService.Instance.OnFinishEvent += OnFinishEvent;
    }

    private void OnDestroy()
    {
        hlwEventService.Instance.OnFinishEvent -= OnFinishEvent;
    }

    public override void OnFocus()
    {
        base.OnFocus();
        if (MySonatFramework.GetService<HLWEventService>().IsUnlocked() == false)
        {
            gameObject.SetActive(false);
        }
    }

    public override void OnLoseFocus()
    {
        base.OnLoseFocus();
        if (MySonatFramework.GetService<HLWEventService>().IsUnlocked() == false)
        {
            gameObject.SetActive(false);
        }
    }

    private void OnFinishEvent()
    {
        gameObject.SetActive(false);
    }

    public void OnClickOpenPopup()
    {
        PanelManager.Instance.OpenPanel<PopupTrickOrTreat_HLW>();
    }
}
