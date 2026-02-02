using System;
using SonatFramework.Scripts.UIModule;
using SonatFramework.Systems;
using SonatFramework.Systems.InventoryManagement;
using SonatFramework.Systems.InventoryManagement.GameResources;
using SonatFramework.Systems.AudioManagement;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Sonat.Enums;
using GrillSort.XmasEvent;
using SonatFramework.Scripts.Feature.Shop;
using SonatFramework.Scripts.SonatSDKAdapterModule;
using SonatFramework.Scripts.UIModule.UIElements;
using DG.Tweening;

namespace GrillSort.UI
{
    public class PopupXmasEvent : Panel
    {
        [SerializeField] private Button btnClose;
        [Header("Header")]
        [SerializeField] private UITimeCounter timeCounter;

        [Header("Pack UI Elements - 3 Packs")]
        [SerializeField] private XmasPackUI[] packUIs = new XmasPackUI[3]; // 3 pack buttons

        [Header("Audio Settings")]
        [SerializeField][Range(0f, 1f)] private float bgmVolumeOnPopup = 0.3f;
        [SerializeField] private float fadeDuration = 0.5f;
        [SerializeField] private float delayPlaySoundRing = 1.5f;
        [SerializeField] private bool playMusicOnFirstOpen = true;

        [Header("Cheat Settings")]
        [SerializeField] private GameObject cheatContainer;
        [SerializeField] private TMP_Dropdown userGroupDropdown;
        [SerializeField] private TMP_Text currentSegmentText; // Display current segment

        private readonly Service<XmasEventService> xmasEventService = new();
        private readonly Service<ShopService> shopService = new();
        
        private bool shouldPlayUnlockAnim = false; // Flag để play unlock animation cho current pack
        private float originalBgmVolume;
        private static bool hasPlayedMusicBefore = false;

        public override void OnSetup()
        {
            base.OnSetup();

            btnClose.onClick.RemoveAllListeners();
            btnClose.onClick.AddListener(Close);

            // Setup cheat dropdown
            SetupCheatDropdown();

            // Subscribe events
            shopService.Instance.OnBuySuccess += OnBuySuccess;
            // Note: Không subscribe OnPackPurchased vì ta sẽ refresh thủ công sau khi PopupReward đóng
        }

        public override void Open(UIData uiData)
        {
            base.Open(uiData);

            // Giảm âm lượng BGM khi mở popup
            if (MySonatFramework.audioService is SonatAudioService audioService)
            {
                originalBgmVolume = audioService.GetVolume(AudioTracks.Music);
                audioService.FadeVolume(bgmVolumeOnPopup, fadeDuration);
            }

            // Phát sound khi mở popup
            PlayOpenSound();

            // Update cheat visibility
            UpdateCheatVisibility();

            // Mở popup lần đầu -> play unlock animation cho current pack
            shouldPlayUnlockAnim = true;
            RefreshUI();
            shouldPlayUnlockAnim = false;
            
            SetupTimeCounter();
        }

        public override void Close()
        {
            // Tăng âm lượng BGM về ban đầu khi đóng popup
            if (MySonatFramework.audioService is SonatAudioService audioService)
            {
                audioService.FadeVolume(originalBgmVolume, fadeDuration);
            }

            base.Close();
        }

        private void PlayOpenSound()
        {
    
            // Phát music chỉ lần đầu tiên mở popup
            if (playMusicOnFirstOpen && !hasPlayedMusicBefore)
            {
                MySonatFramework.audioService?.PlaySound(AudioId.SantaHaul_Board_Music_Grill_sort);
                hasPlayedMusicBefore = true;
            }
            else
            {
                // Phát sound board open
                DOVirtual.DelayedCall(delayPlaySoundRing, () =>
                {
                    MySonatFramework.audioService?.PlaySound(AudioId.SantaHaul_Board_open_Grill_sort);
                });
            }
        }

        private void SetupTimeCounter()
        {
            if (timeCounter == null) return;

            long timeRemaining = xmasEventService.Instance.GetTimeRemaining();
            timeCounter.SetData(timeRemaining, () =>
            {
                // Event ended
                Debug.Log("[XmasEvent] Event time expired");
                Close();
            });
        }

        private void RefreshUI()
        {
            var service = xmasEventService.Instance;

            // Update current segment display
            UpdateCurrentSegmentDisplay();

            // Get all pack tiers
            var packTiers = service.GetAllPackTiers();
            int currentIndex = service.GetCurrentPackIndex();

            if (packTiers == null || packTiers.Length == 0)
            {
                Debug.LogWarning("[XmasEvent] No pack tiers found");
                return;
            }

            // Update each pack UI
            for (int i = 0; i < packUIs.Length && i < packTiers.Length; i++)
            {
                if (packUIs[i] == null) continue;

                var packTier = packTiers[i];
                if (packTier == null)
                {
                    Debug.LogWarning($"[XmasEvent] Pack tier {i} is null");
                    continue;
                }

                // Get rewards từ ShopConfig
                var rewards = service.GetPackRewards(i);
                if (rewards == null)
                {
                    Debug.LogWarning($"[XmasEvent] No rewards found for pack {i}: {packTier.shopItemKey}");
                }

                // Determine pack state
                bool isCurrentPack = (i == currentIndex);
                bool isPurchased = (i < currentIndex);
                bool isLocked = (i > currentIndex);

                // Setup pack UI
                packUIs[i].Setup(
                    index: i,
                    packTier: packTier,
                    rewards: rewards,
                    isCurrentPack: isCurrentPack && service.CanBuyCurrentPack(),
                    isPurchased: isPurchased,
                    isLocked: isLocked,
                    onBuy: OnBuyPack,
                    hideNonCurrent: false,
                    playUnlockAnim: shouldPlayUnlockAnim && isCurrentPack
                );
            }
        }

        private void OnBuyPack(int tierIndex)
        {
            var service = xmasEventService.Instance;

            // Verify this is current pack
            if (tierIndex != service.GetCurrentPackIndex())
            {
                Debug.LogWarning($"[XmasEvent] Cannot buy pack {tierIndex} - current pack is {service.GetCurrentPackIndex()}");
                PopupToast.Cretate("Please buy packs in order!");
                return;
            }

            // Get pack tier
            var packTiers = service.GetAllPackTiers();
            if (tierIndex >= packTiers.Length)
            {
                Debug.LogWarning($"[XmasEvent] Pack tier {tierIndex} out of range");
                return;
            }

            var packTier = packTiers[tierIndex];
            if (packTier == null)
            {
                Debug.LogWarning($"[XmasEvent] Pack tier {tierIndex} is null");
                return;
            }

            // Buy pack
            Debug.Log($"[XmasEvent] Buying pack {tierIndex}: {packTier.shopItemKey}");
            shopService.Instance.BuyPack(packTier.shopItemKey);
        }

        private void OnBuySuccess(ShopItemKey shopItemKey)
        {
            var service = xmasEventService.Instance;
            var packTiers = service.GetAllPackTiers();

            // Find which pack was purchased
            for (int i = 0; i < packTiers.Length; i++)
            {
                if (packTiers[i].shopItemKey == shopItemKey)
                {
                    Debug.Log($"[XmasEvent] Pack {i} purchased successfully: {shopItemKey}");
                    
                    // Get rewards từ ShopConfig (ShopService đã tự add rewards rồi)
                    var packData = shopService.Instance.GetPackData(shopItemKey);
                    RewardData totalReward = new RewardData();

                    if (packData?.rewardData != null)
                    {
                        foreach (var resource in packData.rewardData.resourceDatas)
                        {
                            totalReward.AddReward(resource);
                        }
                    }
                   
                    // Update event data
                    service.OnPackBought(i);

                    // Show PopupReward với callback khi đóng
                    var uiData = new UIData();
                    uiData.Add("IAP", true);
                    uiData.Add("Reward", totalReward);
                    uiData.Add("Title", "REWARD!");
                    
                    // Callback khi PopupReward đóng -> refresh UI với unlock animation cho pack tiếp theo
                    uiData.Add(UIDataKey.CallBackOnClose, (Action)OnPopupRewardClosed);

                    Debug.Log($"[XmasEvent] Opening PopupReward with {totalReward.resourceDatas?.Count ?? 0} rewards");
                    PanelManager.Instance.OpenPanel<PopupReward>(uiData);

                    // Check if completed
                    if (service.IsCompleted())
                    {
                        Debug.Log("[XmasEvent] All packs purchased - event completed!");
                    }

                    return;
                }
            }
        }

        private void OnPopupRewardClosed()
        {
            Debug.Log("[XmasEvent] PopupReward closed - refreshing UI with unlock animation");
            
            // Refresh UI với unlock animation cho pack tiếp theo
            shouldPlayUnlockAnim = true;
            RefreshUI();
            shouldPlayUnlockAnim = false;
        }

        private void OnDestroy()
        {
            // Unsubscribe events
            if (shopService.Instance != null)
            {
                shopService.Instance.OnBuySuccess -= OnBuySuccess;
            }
        }

        #region Cheat Methods

        private void SetupCheatDropdown()
        {
            if (userGroupDropdown == null) return;

            // Clear existing options
            userGroupDropdown.ClearOptions();

            // Add all UserGroup enum values
            var options = new System.Collections.Generic.List<string>();
            foreach (UserGroup userGroup in System.Enum.GetValues(typeof(UserGroup)))
            {
                options.Add(userGroup.ToString());
            }

            userGroupDropdown.AddOptions(options);

            // Auto-select dropdown theo current user group
            UserGroup currentUserGroup = xmasEventService.Instance.GetUserGroup();
            userGroupDropdown.value = (int)currentUserGroup;

            // Add listener
            userGroupDropdown.onValueChanged.RemoveAllListeners();
            userGroupDropdown.onValueChanged.AddListener(OnCheatUserGroupChanged);

            Debug.Log($"[XmasEvent] Cheat dropdown setup - Current user group: {currentUserGroup}");
        }

        private void UpdateCheatVisibility()
        {
            if (cheatContainer == null) return;

            // Show cheat container only if CheatPanel is active
            bool showCheat = CheatPanel.IsCheating;
            cheatContainer.SetActive(showCheat);

            if (showCheat)
            {
                UserGroup currentUserGroup = xmasEventService.Instance.GetUserGroup();
                Debug.Log($"[XmasEvent] Cheat mode active. Current UserGroup: {currentUserGroup}");
            }
        }

        private void UpdateCurrentSegmentDisplay()
        {
            if (currentSegmentText == null) return;

            UserGroup currentUserGroup = xmasEventService.Instance.GetUserGroup();
            currentSegmentText.text = $"Segment: {currentUserGroup}";
        }

        private void OnCheatUserGroupChanged(int index)
        {
            if (!CheatPanel.IsCheating) return;

            // Set user group trong XmasEventService
            UserGroup selectedUserGroup = (UserGroup)index;
            xmasEventService.Instance.SetUserGroup(selectedUserGroup);

            Debug.Log($"[XmasEvent] Cheat: Changed user group to {selectedUserGroup}");

            // Refresh UI with new user group (new packs)
            RefreshUI();
        }

        /// <summary>
        /// Cheat method để reset toàn bộ event data (chỉ hiện trong cheat mode)
        /// </summary>
        public void CheatResetEventData()
        {
            if (!CheatPanel.IsCheating) return;

            // Reset event data
            xmasEventService.Instance.CheatResetEventData();

            // Refresh UI sau khi reset
            RefreshUI();

            // Update dropdown về user group mới (đã được recalculated)
            if (userGroupDropdown != null)
            {
                UserGroup newUserGroup = xmasEventService.Instance.GetUserGroup();
                userGroupDropdown.value = (int)newUserGroup;
            }

            Debug.Log("[XmasEvent] Cheat: Event data reset");
        }

        #endregion
    }
}
