using System;
using MyGame.SkewerJam.Gameplay;
using Sonat.Enums;
using SonatFramework.Scripts.Feature.Shop.UI;
using SonatFramework.Scripts.SonatSDKAdapterModule;
using SonatFramework.Scripts.UIModule;
using SonatFramework.Systems.InventoryManagement;
using SonatFramework.Systems.SceneManagement;
using UnityEngine;

public class PopupSkipDynamite : Panel
{
    private PrimaryGrillBomb grillBomb;
    private Action OnSkipDynamite;
    private Action OnGiveUp;
    private int coinPrice = 100;
    [SerializeField] private GameObject buyWithAdsBtn;
    private CheckRemoteByDayCounter checkRwdDay;
    private CheckRemoteByLevelCounter checkRwdLevel;

    public override void OnSetup()
    {
        base.OnSetup();
        coinPrice = SonatSDKAdapter.GetRemoteInt("skip_grill_price", 100);
        checkRwdDay = new CheckRemoteByDayCounter("by_day_show_rwd_skip_bomb", 999);
        checkRwdLevel = new CheckRemoteByLevelCounter("by_level_show_rwd_skip_bomb", 999);
    }

    public override void Open(UIData uiData)
    {
        base.Open(uiData);
        uiData.TryGet("Dynamite", out grillBomb);
        if (!uiData.TryGet("OnSkipDynamite", out OnSkipDynamite))
        {
            OnSkipDynamite = null;
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
                earnId = "skip_dynamite",
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
        }, "booster", "skip_dynamite");
    }

    public void OnGiveUpClick()
    {
        Close();
        OnGiveUp?.Invoke();
    }

    private void OnSuccess()
    {
        grillBomb.SkipDynamite();
        Close();
        OnSkipDynamite?.Invoke();
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
