using Sonat.Enums;
using SonatFramework.Scripts.Feature.Shop;
using SonatFramework.Systems.TimeManagement;
using SonatFramework.Systems;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GrillSort.BlackFriday;
using SonatFramework.Scripts.UIModule.UIElements;
using SonatFramework.Scripts.UIModule;
using SonatFramework.Scripts.Utils;
using GrillSort.UI;
using SonatFramework.Scripts.SonatSDKAdapterModule;

public class UIBlackFridayWidget : PackIapWidget
{
    private readonly Service<BlackFridayService> service = new();

    public UITimeCounter timeCounter;
    private long currentTime;
    private long exp;

    public override void Setup()
    {
        base.Setup();

        popupName = "PopupBlackFriday";

        exp = service.Instance.GetTimeExp();

        gameObject.SetActive(CheckActive());

        if (gameObject.activeSelf)
        {
            timeCounter.SetData(exp - currentTime, () =>
            {
                gameObject.SetActive(false);
            });
        }
    }

    public override void OnFocus()
    {
        base.OnFocus();
        gameObject.SetActive(CheckActive());
    }
    public override void OnLoseFocus()
    {
        base.OnLoseFocus();
        gameObject.SetActive(CheckActive());
    }
    protected override bool CheckActive()
    {
        currentTime = SonatSystem.GetService<TimeService>().GetUnixTimeSeconds();

        if(!service.Instance.IsInEvent()) return false;
        return true;
    }

    public override void OpenPopup()
    {
        PanelManager.Instance.OpenForget<PopupBlackFriday>();
        MySonatFramework.customTrackingService.OnShowPopup(SonatSDKAdapter.FindProductId(shopItemKey), "widget", "iap", "user");
    }
}
