using System;
using Gameplay.Entities.Items;
using MyGame.SkewerJam.Gameplay;
using Sonat.Enums;
using SonatFramework.Scripts.Feature.Shop.UI;
using SonatFramework.Scripts.SonatSDKAdapterModule;
using SonatFramework.Scripts.UIModule;
using SonatFramework.Systems.InventoryManagement;
using SonatFramework.Systems.SceneManagement;
using TMPro;
using UnityEngine;

public class PopupSkipBomb : Panel
{
    private ItemBombMove itemBomb;
    private Action OnSkipBomb;
    private Action OnGiveUp;
    [SerializeField] private TMP_Text txtBomb;
    [SerializeField] private TMP_Text txtPrice;
    private int coinPrice = 100;
    [SerializeField] private GameObject buyWithAdsBtn;
    private CheckRemoteByDayCounter checkRwdDay;
    private CheckRemoteByLevelCounter checkRwdLevel;


    public override void OnSetup()
    {
        base.OnSetup();
        coinPrice = SonatSDKAdapter.GetRemoteInt("skip_bomb_price_new", 100);
        checkRwdDay = new CheckRemoteByDayCounter("by_day_show_rwd_skip_bomb", 999);
        checkRwdLevel = new CheckRemoteByLevelCounter("by_level_show_rwd_skip_bomb", 999);
    }

    public override void Open(UIData uiData)
    {
        base.Open(uiData);

        txtPrice.text = $"{coinPrice}";

        uiData.TryGet("Bomb", out itemBomb);
        txtBomb.text = itemBomb.MoveRemaining.ToString();
        if (!uiData.TryGet("OnSkipBomb", out OnSkipBomb))
        {
            OnSkipBomb = null;
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
                earnId = "skip_bomb",
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
        }, "booster", "skip_bomb");
    }


    public void OnGiveUpClick()
    {
        Close();
        OnGiveUp?.Invoke();
    }

    private void OnSuccess()
    {
        itemBomb.SkipBomb();
        Close();
        OnSkipBomb?.Invoke();
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