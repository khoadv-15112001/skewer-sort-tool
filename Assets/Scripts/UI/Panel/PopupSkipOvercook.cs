using MyGame.SkewerJam.Gameplay;
using Sonat.Data;
using Sonat.Enums;
using SonatFramework.Scripts.Feature.Shop.UI;
using SonatFramework.Scripts.SonatSDKAdapterModule;
using SonatFramework.Scripts.UIModule;
using SonatFramework.Systems.InventoryManagement;
using SonatFramework.Systems.SceneManagement;
using System;
using TMPro;
using UnityEngine;

public class PopupSkipOvercook : Panel
{
    //private ItemBombMove itemBomb;
    private PrimaryGrillOvercooked grillOvercooked;
    private Action OnSkipOvercook;
    private Action OnGiveUp;
    private int coinPrice = 100;
    [SerializeField] private TMP_Text txtPrice;
    [SerializeField] private GameObject buyWithAdsBtn;
    private CheckRemoteByDayCounter checkRwdDay;
    private CheckRemoteByLevelCounter checkRwdLevel;


    public override void OnSetup()
    {
        base.OnSetup();
        coinPrice = SonatSDKAdapter.GetValueBySegment("skip_grill_price_over_cooked_segment",UserData.UserCampaignSegment.Value,600);
        checkRwdDay = new CheckRemoteByDayCounter("by_day_show_rwd_over_cooked", 999);
        checkRwdLevel = new CheckRemoteByLevelCounter("by_level_show_rwd_over_cooked", 999);
    }

    public override void Open(UIData uiData)
    {
        base.Open(uiData);

        txtPrice.text = coinPrice.ToString();

        uiData.TryGet("Overcook", out grillOvercooked);
        if (!uiData.TryGet("OnSkipOvercook", out OnSkipOvercook))
        {
            OnSkipOvercook = null;
        }

        if (!uiData.TryGet("OnGiveUp", out OnGiveUp))
        {
            OnGiveUp = null;
        }

        buyWithAdsBtn.SetActive(checkRwdDay.CheckCounter() && checkRwdLevel.CheckCounter());
    }

    public void BuyWithCoinClick()
    {
        if (MySonatFramework.inventoryService.CanReduce(GameResource.Coin, coinPrice))
        {
            SpendResourceLogData logData = new SpendResourceLogData()
            {
                earnType = "booster",
                earnId = "skip_overcooked",
                source = "non_iap"
            };
            MySonatFramework.inventoryService.ReduceResource(GameResource.Coin, coinPrice, logData);
            OnSuccess();
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
            OnSuccess();
        }, "booster", "skip_overcooked");
    }


    public void OnGiveUpClick()
    {
        Close();
        OnGiveUp?.Invoke();
    }

    private void OnSuccess()
    {
        grillOvercooked.SkipOvercook();
        Close();
        OnSkipOvercook?.Invoke();
    }

    public override void Close()
    {
        base.Close();
        if (MySonatFramework.GetService<SceneService>().GetCurrentGamePlacement() == GamePlacement.Gameplay_SkewerJam)
        {
            if (GameController.Instance.GameState != GameState.GameOver)
            {
                GameController.Instance.ChangeGameState(GameState.Playing);
            }
        }
    }
}
