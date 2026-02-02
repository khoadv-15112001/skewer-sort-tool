using System.Collections;
using Sonat.Enums;
using SonatFramework.Scripts.Feature.Shop.UI;
using SonatFramework.Scripts.UIModule;
using SonatFramework.Systems.EventBus;
using UnityEngine;

public class UIShopPack : UIShopPackBase
{
    public ShopItemKey GetShopItemKey()
    {
        return key;
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
            uiData.Add("Reward", shopPack.rewardData);

            PanelManager.Instance.OpenPanel<PopupReward>(uiData);
        }
    }
    protected override void OnBuyClick()
    {
        if (!MySonatFramework.IsNetworkAvailable())
        {
            NoInternet.Instance.ForceShowPopup();
            return;
        }
        base.OnBuyClick();
    }
    // IEnumerator Collect()
    // {
    //     var collectEffect = new CollectEffectSingle();
    //
    //     foreach (var resource in shopPack.rewardData.resourceDatas)
    //     {
    //         if (resource == null || resource.quantity == 0) continue;
    //         EventBus<AddItemEvent>.Raise(new AddItemEvent()
    //             { position = transform.position, resource = resource.resource, quantity = resource.quantity, collectEffect = collectEffect });
    //         yield return new WaitForSeconds(0.2f);
    //     }
    //
    //     yield return new WaitForSeconds(0.2f);
    //     base.BuyComplete();
    // }
}