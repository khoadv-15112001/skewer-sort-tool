using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SonatFramework.Scripts.SonatSDKAdapterModule;
using SonatFramework.Scripts.Feature.Shop;
using SonatFramework.Systems;
using Sonat.Enums;
using DG.Tweening;
public class HiddenWhenNoAdsPurchased : MonoBehaviour
{
    protected readonly Service<ShopService> shopService = new();

    protected virtual void Start()
    {
        shopService.Instance.OnBuySuccess += OnBuySuccess;
        CheckNoAds(0.01f);
    }

    protected virtual void OnDestroy()
    {
        shopService.Instance.OnBuySuccess -= OnBuySuccess;
    }

    private void OnBuySuccess(ShopItemKey shopItemKey)
    {
        CheckNoAds(0.5f);
    }

    public void CheckNoAds(float delay = 0)
    {
        if (gameObject.activeSelf)
        {
            DOVirtual.DelayedCall(delay, () => {
                this.gameObject.SetActive(!SonatSDKAdapter.IsNoads());
            });
        }
    }
}
