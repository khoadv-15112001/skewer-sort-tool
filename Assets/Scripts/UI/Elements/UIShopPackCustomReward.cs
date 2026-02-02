using Sonat.Enums;
using SonatFramework.Scripts.Feature.Shop.UI;
using SonatFramework.Scripts.UIModule;
using SonatFramework.Systems.InventoryManagement.GameResources;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIShopPackCustomReward : UIShopPackBase
{
    public RewardData rewardData;
    protected override void OnBuyClick()
    {
        if (!MySonatFramework.IsNetworkAvailable())
        {
            NoInternet.Instance.ForceShowPopup();
            return;
        }
        base.OnBuyClick();
    }
    protected override void BuyComplete()
    {
        //StartCoroutine(Collect());
        base.BuyComplete();
        //bool noAdsOnly = shopPack.noAds && (shopPack.rewardData == null || shopPack.rewardData.resourceDatas == null || shopPack.rewardData.resourceDatas.Count == 0);
        if (key != ShopItemKey.no_ads && key != ShopItemKey.BattlePass_ActivatePack)
        {
            var uiData = new UIData();
            uiData.Add("IAP", true);
            uiData.Add("Reward", rewardData);
            PanelManager.Instance.OpenPanel<PopupReward>(uiData);
        }
    }
}
