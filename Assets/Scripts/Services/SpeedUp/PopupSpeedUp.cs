using Sonat.Data;
using Sonat.Enums;
using SonatFramework.Scripts.Feature.Shop.UI;
using SonatFramework.Scripts.SonatSDKAdapterModule;
using SonatFramework.Scripts.UIModule;
using SonatFramework.Systems;
using SonatFramework.Systems.InventoryManagement;
using SonatFramework.Systems.InventoryManagement.GameResources;
using GrillSort.SpeedUp;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System;

public class PopupSpeedUp : Panel
{
    [Header("UI References")]
    [SerializeField] private Button btnSpeedUpCoin;
    [SerializeField] private Button btnSpeedUpAds;
    [SerializeField] private TMP_Text txtPrice;
    
    private readonly Service<SpeedUpService> speedUpService = new();
    private Action onSpeedUpSuccess;
    
    public override void OnSetup()
    {
        base.OnSetup();
        
        // Setup button listeners
        if (btnSpeedUpCoin != null)
            btnSpeedUpCoin.onClick.AddListener(OnSpeedUpWithCoinClick);
            
        if (btnSpeedUpAds != null)
            btnSpeedUpAds.onClick.AddListener(OnSpeedUpWithAdsClick);
    }
    
    public override void Open(UIData uiData)
    {
        base.Open(uiData);
        
        // Get callback from UIData if provided
        if (uiData != null && uiData.TryGet("OnSuccess", out Action callback))
        {
            onSpeedUpSuccess = callback;
        }
        
        RefreshUI();
    }
    
    private void RefreshUI()
    {
        if (speedUpService.Instance == null)
        {
            Debug.LogError("[PopupSpeedUp] SpeedUpService not found! Please add it to Service Manager.");
            return;
        }
        
        // Get price from service
        int price = speedUpService.Instance.GetCoinPrice();
        
        if (txtPrice != null)
        {
            txtPrice.text = price.ToString();
        }
        
        // Update button states
        UpdateButtonStates();
        
        string campaignSegment = UserData.UserCampaignSegment.Value;
        Debug.Log($"[PopupSpeedUp] Campaign: {campaignSegment}, Price: {price}");
    }
    
    private void UpdateButtonStates()
    {
        // Check if reward ads is enabled
        bool adsEnabled = speedUpService.Instance.Config.enableRewardAds;
        
        if (btnSpeedUpAds != null)
        {
            btnSpeedUpAds.gameObject.SetActive(adsEnabled);
        }
    }
    
    private void OnSpeedUpWithCoinClick()
    {
        speedUpService.Instance.TrySpeedUpWithCoin((success) =>
        {
            if (success)
            {
                OnSpeedUpSuccess("coin");
            }
            else
            {
                PopupToast.Cretate("Not enough coin!");
                PanelManager.Instance.OpenPanelByName<ShopPanelBase>("ShopPanel");
            }
        });
    }
    
    private void OnSpeedUpWithAdsClick()
    {
        speedUpService.Instance.TrySpeedUpWithAds((success) =>
        {
            if (success)
            {
                OnSpeedUpSuccess("ads");
            }
        });
    }
    
    private void OnSpeedUpSuccess(string by)
    {
        Debug.Log($"[PopupSpeedUp] Speed up success by {by}");
        
        // Invoke callback if provided
        onSpeedUpSuccess?.Invoke();
        
        // Close popup
        Close();
    }
}
