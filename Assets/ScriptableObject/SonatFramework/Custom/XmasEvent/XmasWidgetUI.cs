using System;
using Cysharp.Threading.Tasks;
using GrillSort.XmasEvent;
using GrillSort.UI;
using SonatFramework.Scripts.UIModule;
using SonatFramework.Scripts.UIModule.UIElements;
using SonatFramework.Systems;
using UnityEngine;
using UnityEngine.UI;
using Sonat.Enums;
using SonatFramework.Scripts.SonatSDKAdapterModule;

/// <summary>
/// Simple Xmas Event Widget - chỉ có 1 icon để click mở popup
/// </summary>
public class XmasWidgetUI : UIHomeWidget
{
    [Header("Widget - Simple Icon")]
    [SerializeField] private Button openButton;
    [SerializeField] private GameObject notificationDot; // Chấm đỏ thông báo
    [SerializeField] private Image iconImage; // Icon event (optional - có thể dùng sprite trong button)
    [SerializeField] private UITimeCounter timeCounter; // Countdown timer
    
    private readonly Service<XmasEventService> xmasEventService = new();
    
    private bool shouldShowPopup = false;
    private ShopItemKey shopItemKey;

    public override void Setup()
    {
        base.Setup();
        
        // Setup button
        if (openButton != null)
        {
            openButton.onClick.AddListener(OnClickWidget);
        }
        
        // Check if should show widget
        active = CheckActive() && xmasEventService.Instance.ShouldShowEvent();
        gameObject.SetActive(active);
        
        if (active)
        {
            RefreshUI();
            SetupTimeCounter();
            
            // Subscribe to events
            xmasEventService.Instance.OnStateChanged += OnEventStateChanged;
            xmasEventService.Instance.OnPackPurchased += RefreshUI;
            
            // Check if should show popup on first time
            shouldShowPopup = CheckShouldShowPopup();
        }
    }

    private void SetupTimeCounter()
    {
        if (timeCounter == null) return;

        long timeRemaining = xmasEventService.Instance.GetTimeRemaining();
        timeCounter.SetData(timeRemaining, () =>
        {
            // Event ended - hide widget
            Debug.Log("[XmasWidget] Event time expired");
            gameObject.SetActive(false);
            active = false;
        });
    }

    public override void OnFocus()
    {
        base.OnFocus();
        
        // Refresh when coming back to home
        if (active)
        {
            bool shouldShow = xmasEventService.Instance.ShouldShowEvent();
            
            if (!shouldShow)
            {
                // Event ended or completed - hide widget
                gameObject.SetActive(false);
                active = false;
            }
            else
            {
                RefreshUI();
            }
        }
    }

    public override void OnLoseFocus()
    {
        base.OnLoseFocus();
    }

    public override async UniTask<bool> ProcessTask()
    {
        // Auto open popup if needed (first time showing)
        if (shouldShowPopup && active)
        {
            shouldShowPopup = false;
            OpenPopup();
            xmasEventService.Instance.SetHasParticipated(true);
            return true; // Popup was opened
        }
        
        return false;
    }

    private bool CheckShouldShowPopup()
    {
        // Show popup lần đầu khi event mới bắt đầu
        return !xmasEventService.Instance.data.hasParticipated;
    }

    private void RefreshUI()
    {
        var service = xmasEventService.Instance;
        
        // Check if should still show
        if (!service.ShouldShowEvent())
        {
            gameObject.SetActive(false);
            active = false;
            return;
        }
        
        // Get current pack for tracking
        var currentPack = service.GetCurrentPack();
        if (currentPack != null)
        {
            shopItemKey = currentPack.shopItemKey;
        }
        
        // Show notification dot if pack is available to buy
        if (notificationDot != null)
        {
            notificationDot.SetActive(service.CanBuyCurrentPack());
        }
    }

    private void OnEventStateChanged()
    {
        // Event state changed - check if should hide widget
        if (!xmasEventService.Instance.ShouldShowEvent())
        {
            gameObject.SetActive(false);
            active = false;
        }
    }

    private void OnClickWidget()
    {
        OpenPopup();
    }

    private void OpenPopup()
    {
        PanelManager.Instance.OpenPanel<PopupXmasEvent>();
        
        // Tracking với shopItemKey của current pack
        if (shopItemKey != ShopItemKey.None)
        {
            MySonatFramework.customTrackingService?.OnShowPopup(
                SonatSDKAdapter.FindProductId(shopItemKey), 
                "widget", 
                "iap", 
                "user"
            );
        }
    }

    private void OnDestroy()
    {
        // Unsubscribe events
        if (xmasEventService.Instance != null)
        {
            xmasEventService.Instance.OnStateChanged -= OnEventStateChanged;
            xmasEventService.Instance.OnPackPurchased -= RefreshUI;
        }
        
        // Remove button listener
        if (openButton != null)
        {
            openButton.onClick.RemoveListener(OnClickWidget);
        }
    }
}
