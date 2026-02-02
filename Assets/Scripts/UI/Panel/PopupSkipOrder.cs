using System;
using System.Collections;
using System.Collections.Generic;
using Gameplay.Entities.Orders;
using Sonat.Enums;
using SonatFramework.Scripts.Feature.Shop.UI;
using SonatFramework.Scripts.SonatSDKAdapterModule;
using SonatFramework.Scripts.UIModule;
using SonatFramework.Systems.InventoryManagement;
using UnityEngine;
using System.Linq;
using TMPro;

public class PopupSkipOrder : Panel
{
    private Action OnSkipOrder;
    private int coinPrice = 100;
    [SerializeField] private UIOrderListView orderListView;
    private OrderList.Data orderListData;
    [SerializeField] private GameObject buyWithAdsBtn;
    private CheckRemoteByDayCounter checkRwdDay;
    private CheckRemoteByLevelCounter checkRwdLevel;
    [SerializeField] private TMP_Text txtPrice;
    public override void OnSetup()
    {
        base.OnSetup();
        coinPrice = SonatSDKAdapter.GetRemoteInt("skip_order_price_new", 600);
        checkRwdDay = new CheckRemoteByDayCounter("by_day_show_rwd_skip_order", 999);
        checkRwdLevel = new CheckRemoteByLevelCounter("by_level_show_rwd_skip_order", 999);
    }

    public override void Open(UIData uiData)
    {
        base.Open(uiData);
        if (uiData.TryGet("OnSkipOrder", out OnSkipOrder))
        {
        }

        if (uiData.TryGet("OrderListData", out orderListData))
        {
            orderListView.SetData(orderListData);
        }
        txtPrice.text = $"{coinPrice}";

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
            OnSuccess("skip_coin");
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
            checkRwdDay.AddValue(1);
            checkRwdLevel.AddValue(1);
            OnSuccess("skip_rwd");
        }, "booster", "skip_order");
    }

    private void OnSuccess(string skipBy)
    {
        LogEndOrder(skipBy);
        Close();
        OnSkipOrder?.Invoke();
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