using System;
using Gameplay.Entities;
using GrillSort.PreBooster;
using Sonat.Enums;
using SonatFramework.Scripts.Feature.Shop.UI;
using SonatFramework.Scripts.SonatSDKAdapterModule;
using SonatFramework.Scripts.UIModule;
using SonatFramework.Systems.BoosterManagement;
using SonatFramework.Systems.InventoryManagement;
using SonatFramework.Systems.InventoryManagement.GameResources;
using TMPro;
using UnityEngine;


public class PopupUnlockGrill : Panel
{
    private Action callback;
    [SerializeField] private int coinPrice = 100;
    [SerializeField] private string earn = "unlock_rw_tray";
    [SerializeField] private TMP_Text txtPrice;
    [SerializeField] private GameObject magicKeyButton;
    [SerializeField] private GameObject coinButton;
    private PrimaryGrill primaryGrill;
    [SerializeField] private GameObject buyWithAdsBtn;
    [SerializeField] private string keyBuyAdsByDay;
    [SerializeField] private string keyBuyAdsByLevel;
    private CheckRemoteByDayCounter checkRwdByDay;
    private CheckRemoteByLevelCounter checkRwdByLevel;

    public override void OnSetup()
    {
        base.OnSetup();
        checkRwdByDay = new(keyBuyAdsByDay, 999);
        checkRwdByLevel = new(keyBuyAdsByLevel, 999);
    }

    public override void Open(UIData uiData)
    {
        base.Open(uiData);
        primaryGrill = null;
        callback = null;
        if (uiData.TryGet("PrimaryGrill", out primaryGrill))
        {
        }

        if (uiData.TryGet("Callback", out callback))
        {
        }

        var boosterService = MySonatFramework.GetService<BoosterService>();
        if (boosterService.CanUseBooster(GameResource.BoosterMagicKey))
        {
            magicKeyButton.SetActive(true);
            coinButton.SetActive(false);
        }
        else
        {
            magicKeyButton.SetActive(false);
            coinButton.SetActive(true);
        }
        txtPrice.text = $"{coinPrice}";

        buyWithAdsBtn?.SetActive(checkRwdByDay.CheckCounter() && checkRwdByLevel.CheckCounter());
    }

    public void BuyWithCoinClick()
    {
        if (MySonatFramework.inventoryService.CanReduce(GameResource.Coin, coinPrice))
        {
            SpendResourceLogData logData = new SpendResourceLogData()
            {
                earnType = "booster",
                earnId = earn,
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

    public void OnClickWithMagicKey()
    {
        // OnSuccess();
        GameplayController.instance.levelGenerator.BoosterMagicKeyByGrill(primaryGrill);
        Close();
    }

    public void BuyWithAdsClick()
    {
        MySonatFramework.ShowRewardAds(() =>
        {
            checkRwdByDay.AddValue();
            checkRwdByLevel.AddValue();
            OnSuccess();
        }, "booster", earn);
    }

    private void OnSuccess()
    {
        Close();
        callback?.Invoke();
    }
}