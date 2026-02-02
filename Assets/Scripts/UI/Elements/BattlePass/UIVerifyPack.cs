using GrillSort.BattlePass;
using GrillSort.XmasEvent;
using Sonat.Enums;
using SonatFramework.Scripts.Feature.Shop;
using SonatFramework.Systems;
using SonatFramework.Systems.TimeManagement;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;

public class UIPackVerifyPack : MonoBehaviour
{
    [SerializeField] private ShopItemKey key;

    [SerializeField] private GameObject activeObj;
    [SerializeField] private GameObject inactiveObj;
    [SerializeField] private UnityEvent OnDisablePack;
    protected readonly Service<ShopService> shopService = new();
    private void OnEnable()
    {
        UpdateUI();
    }
    private void OnApplicationFocus(bool focus)
    {
        if (focus)
            UpdateUI();
    }
    private void UpdateUI()
    {
        Debug.Log(VerifyPack(key)
            ? $"Pack {key} is active. Enabling related UI element."
            : $"Pack {key} is not active. Disabling related UI element.");
        if (VerifyPack(key))
        {
            if (activeObj != null)
                activeObj.SetActive(true);
            if (inactiveObj != null)
                inactiveObj.SetActive(false);
        }
        else
        {
            if (activeObj != null)
                activeObj.SetActive(false);
            if (inactiveObj != null)
                inactiveObj.SetActive(true);
            OnDisablePack?.Invoke();
        }
    }

    private bool VerifyPack(ShopItemKey key)
    {
        if(key == ShopItemKey.Santa_Key_Check)
        {
            var xmasService = SonatSystem.GetService<XmasEventService>();

            return xmasService.CanShow();
        }
        if (key == ShopItemKey.StarterBundle || key == ShopItemKey.StarterBundleSale)
        {
            return CheckActiveStarterPack(key);
        }
        if (key == ShopItemKey.BattlePass_ActivatePack)
        {
            var battlePassService = SonatSystem.GetService<BattlePassService>();
            if (battlePassService == null || battlePassService.IsUnlocked() == false)
                return false;
        }
        return shopService.Instance.VerifyPack(key);
    }

    protected bool CheckActiveStarterPack(ShopItemKey key)
    {
        var currentTime = SonatSystem.GetService<TimeService>().GetUnixTimeSeconds();
        var exp = MySonatFramework.userDataService.FirstTimeOpen + 48 * 3600;

        if (key == ShopItemKey.StarterBundleSale)
            if (exp < currentTime) return false;
        if (key == ShopItemKey.StarterBundle)
            if (exp >= currentTime) return false;

        var active = shopService.Instance.VerifyPack(ShopItemKey.StarterBundleSale) &&
            shopService.Instance.VerifyPack(ShopItemKey.StarterBundle);

        if (!active)
            return false;

        return true;
    }
}
