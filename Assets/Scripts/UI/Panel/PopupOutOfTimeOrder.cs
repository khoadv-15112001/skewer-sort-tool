using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Gameplay.Entities.Orders;
using Sonat.Data;
using Sonat.Enums;
using SonatFramework.Scripts.Feature.Shop.UI;
using SonatFramework.Scripts.SonatSDKAdapterModule;
using SonatFramework.Scripts.UIModule;
using SonatFramework.Scripts.Utils;
using SonatFramework.Systems.InventoryManagement;
using SonatFramework.Systems.SceneManagement;
using TMPro;
using UnityEngine;

public class PopupOutOfTimeOrder : Panel
{
    private PopupContinue.Data data;
    private int coinPrice = 100;
    [SerializeField] private UIOrderListView orderListView;
    [SerializeField] private TMP_Text txtPrice;
    private OrderList.Data orderListData;
    [SerializeField] private GameObject buyWithAdsBtn;
    private CheckRemoteByDayCounter checkRwdDay;
    private CheckRemoteByLevelCounter checkRwdLevel;

    public override void OnSetup()
    {
        base.OnSetup();
        coinPrice = SonatSDKAdapter.GetValueBySegment("skip_order_price_segment", UserData.UserCampaignSegment.Value, 600);
        checkRwdDay = new CheckRemoteByDayCounter("by_day_show_rwd_skip_order", 999);
        checkRwdLevel = new CheckRemoteByLevelCounter("by_level_show_rwd_skip_order", 999);
    }

    public override void Open(UIData uiData)
    {
        base.Open(uiData);
        data = (PopupContinue.Data)uiData;
        if (uiData.TryGet("OrderListData", out orderListData))
        {
            orderListView.SetData(orderListData);
        }
        txtPrice.text = $"{coinPrice}";
        if (buyWithAdsBtn != null)
            buyWithAdsBtn.SetActive(checkRwdDay.CheckCounter() && checkRwdLevel.CheckCounter());
    }

    public void BuyWithCoinClick()
    {
        if (MySonatFramework.inventoryService.CanReduce(GameResource.Coin, coinPrice))
        {
            SpendResourceLogData logData = new SpendResourceLogData()
            {
                earnType = "booster",
                earnId = "skip_order",
                source = "non_iap"
            };
            MySonatFramework.inventoryService.ReduceResource(GameResource.Coin, coinPrice, logData);
            OnSuccess("play_on_coin", "skip_coin");
        }
        else
        {
            PanelManager.Instance.OpenPanelByName<ShopPanelBase>("ShopPanel");
            PopupToast.Cretate("Not enough coin!");
        }
    }

    public void BuyWithAdsClick()
    {
        MySonatFramework.ShowRewardAds(() =>
        {
            checkRwdDay.AddValue();
            checkRwdLevel.AddValue();
            OnSuccess("play_on_ads", "skip_rwd");
        }, "booster", "skip_order");
    }

    private void OnSuccess(string by, string skipBy)
    {
        LogEndOrder(skipBy);
        Close();
        data.onPlayOn?.Invoke(by, null);
    }

    public void OnGiveUpClick()
    {
        LogEndOrder("fail");

        if (MySonatFramework.livesService.CanPlay())
        {
            Close();
            data.onClose?.Invoke();
        }
        else
        {
            PanelManager.Instance.OpenPanel<PopupRefillLives>(new UIData().Add(UIDataKey.CallBackOnClose, (Action)(AfterRefillLive)));
            PopupToast.Cretate("No more lives left!");
        }
    }

    private void AfterRefillLive()
    {
        if (MySonatFramework.livesService.CanPlay())
        {
            Close();
            data.onClose?.Invoke();
        }
        else
        {
            GoHome();
        }
    }

    private void GoHome()
    {
        LoadingScreenInstance.Instance.Show(1.5f);
        SonatUtils.DelayCall(0.5f, () => { MySonatFramework.GetService<SceneService>().SwitchScene(GamePlacement.Home); });
    }

    private void LogEndOrder(string success)
    {
        if (orderListData == null) return;
        int timePlayOrder = (int)(orderListData.maxTime - orderListData.timeRemaining);
        int itemComplete = orderListData.orderItems.Count(e => e.isCompleted);
        float orderCompletion = itemComplete / (float)orderListData.orderItems.Count;
        MySonatFramework.customTrackingService.OnOrderEnd(success, orderListData.maxTime, timePlayOrder, orderCompletion,
            orderListData.useBoosterCount);
    }
}