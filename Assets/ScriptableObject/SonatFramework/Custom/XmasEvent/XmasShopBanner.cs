using System;
using UnityEngine;
using UnityEngine.Events;
using GrillSort.XmasEvent;
using SonatFramework.Systems;
using SonatFramework.Scripts.UIModule;

namespace GrillSort.UI
{
    /// <summary>
    /// Component để hiển thị Xmas pack trong Shop Banner
    /// Tự động show pack current, ẩn pack khác
    /// </summary>
    public class XmasShopBanner : MonoBehaviour
    {
        [Header("Pack UI References - 3 Packs")]
        [SerializeField] private XmasPackUI[] packUIs = new XmasPackUI[3];
        
        [Header("Options")]
        [SerializeField] private bool autoHideWhenCompleted = true; // Ẩn banner khi đã mua hết 3 packs
        
        [Header("Events")]
        [SerializeField] private UnityEvent OnDisablePack; // Event khi banner bị ẩn (dùng để ẩn page slide)
        
        private readonly Service<XmasEventService> xmasEventService = new();

        private void Start()
        {
            RefreshUI();
            
            // Subscribe to events
            if (xmasEventService.Instance != null)
            {
                xmasEventService.Instance.OnPackPurchased += RefreshUI;
                xmasEventService.Instance.OnStateChanged += RefreshUI;
            }
        }

        private void OnEnable()
        {
            RefreshUI();
        }

        public void RefreshUI()
        {
            var service = xmasEventService.Instance;

            // Check CanShow() - nếu false thì disable và invoke event
            if (!service.CanShow())
            {
                DisableBanner();
                return;
            }

            // Check if should show banner
            if (!service.ShouldShowEvent())
            {
                DisableBanner();
                return;
            }

            // Hide banner if completed (optional)
            if (autoHideWhenCompleted && service.IsCompleted())
            {
                DisableBanner();
                return;
            }

            gameObject.SetActive(true);

            // Get all pack tiers
            var packTiers = service.GetAllPackTiers();
            int currentIndex = service.GetCurrentPackIndex();

            if (packTiers == null || packTiers.Length == 0)
            {
                Debug.LogWarning("[XmasShopBanner] No pack tiers found");
                gameObject.SetActive(false);
                return;
            }

            // Setup all pack UIs
            for (int i = 0; i < packUIs.Length && i < packTiers.Length; i++)
            {
                if (packUIs[i] == null) continue;

                var packTier = packTiers[i];
                if (packTier == null) continue;

                // Get rewards
                var rewards = service.GetPackRewards(i);

                // Determine pack state
                bool isCurrentPack = (i == currentIndex);
                bool isPurchased = (i < currentIndex);
                bool isLocked = (i > currentIndex);

                // Setup với hideNonCurrent = true (chỉ show current pack)
                packUIs[i].Setup(
                    index: i,
                    packTier: packTier,
                    rewards: rewards,
                    isCurrentPack: isCurrentPack,
                    isPurchased: isPurchased,
                    isLocked: isLocked,
                    onBuy: OnBuyPack,
                    hideNonCurrent: true // ⭐ Chỉ show pack current
                );
            }
        }

        private void OnBuyPack(int tierIndex)
        {
            // Open popup thay vì buy trực tiếp (để user xem full info)
            OpenPopup();
        }

        public void OpenPopup()
        {
            PanelManager.Instance.OpenPanel<PopupXmasEvent>();
        }

        /// <summary>
        /// Disable banner và invoke OnDisablePack event
        /// </summary>
        private void DisableBanner()
        {
            gameObject.SetActive(false);
            OnDisablePack?.Invoke();
        }

        private void OnDestroy()
        {
            if (xmasEventService.Instance != null)
            {
                xmasEventService.Instance.OnPackPurchased -= RefreshUI;
                xmasEventService.Instance.OnStateChanged -= RefreshUI;
            }
        }
    }
}

