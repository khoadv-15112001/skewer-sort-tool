using DG.Tweening;
using GrillSort.BlackFriday;
using Sonat.Enums;
using Sonat.IapModule;
using SonatFramework.Scripts.Feature.Shop;
using SonatFramework.Scripts.Helper;
using SonatFramework.Scripts.SonatSDKAdapterModule;
using SonatFramework.Scripts.UIModule;
using SonatFramework.Scripts.UIModule.UIElements;
using SonatFramework.Systems;
using SonatFramework.Systems.AudioManagement;
using SonatFramework.Systems.InventoryManagement;
using SonatFramework.Systems.InventoryManagement.GameResources;
using SonatFramework.Systems.TimeManagement;
using System;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

namespace GrillSort.UI
{
    public class PopupBlackFriday : Panel
    {
        [Header("UI References")]
        //[SerializeField] private FixedImageRatio iconReward;
        [SerializeField] private ShopItemKey shopItemKey;
        [SerializeField] private TMP_Text coinRewardText;
        [SerializeField] private TMP_Text originalPriceText;
        [SerializeField] private TMP_Text finalPriceText;
        [SerializeField] private Button buyButton;
        [SerializeField] private TMP_Text availableText;

        [SerializeField] private TMP_Text pricePackText;

        [SerializeField] private float scrollDuration = 1.2f;
        [SerializeField] private float scrollHeightPerDiscount = 80f;
        [SerializeField] private Ease scrollEase = Ease.OutQuart;
        [SerializeField] private BlackFridaySpinController spinController;
        [SerializeField] private UITimeCounter timeCounter;

        [Header("Audio Settings")]
        [SerializeField][Range(0f, 1f)] private float bgmVolumeOnPopup = 0.3f;
        [SerializeField] private float fadeDuration = 0.5f;
        [SerializeField] private bool playMusicOnFirstOpen = true;

        [Header("Animation Settings")]
        [SerializeField] private float introAnimDelay = 1f;
        [SerializeField] private float introAnimDuration = 2f;
        [Header("Effects")]
        [SerializeField] private GameObject discountEffect40;
        [SerializeField] private GameObject discountEffect60;
        [SerializeField] private GameObject completedEffect;
        [Header("Cheat Settings")]
        [SerializeField] private GameObject cheatContainer;
        [SerializeField] private TMP_Dropdown userGroupDropdown;
        [SerializeField] private TMP_InputField ltvInputField;
        [SerializeField] private TMP_Dropdown discountDropdown;


        private readonly Service<BlackFridayService> blackFridayService = new();
        private readonly Service<ShopService> shopService = new();
        private readonly Service<UserGroupService> userGroupService = new();
        private float originalBgmVolume;
        private static bool hasPlayedMusicBefore = false;

        private void OnEnable()
        {
            // Set shopItemKey based on saved discount
            int currentDiscount = blackFridayService.Instance.GetCurrentDiscount();
            shopItemKey = blackFridayService.Instance.GetBlackFridayPackKey(currentDiscount);

            int spinPrice = blackFridayService.Instance.GetSpinPrice();
            spinController.Setup(spinPrice);

            // Update available text on enable
            UpdateAvailableText();

            var currentTime = SonatSystem.GetService<TimeService>().GetUnixTimeSeconds();

            var exp = blackFridayService.Instance.GetTimeExp();

            timeCounter.SetData(exp - currentTime, () =>
            {
                Close();
            });
        }

        public void OnSpining(ShopItemKey key)
        {
            UpdatePackPriceUI(key);
        }

        public void OnSpinCompleted(int discount, ShopItemKey shopItemKey)
        {
            Debug.Log($"anhnt: discount: {discount}% key={shopItemKey}");
            this.shopItemKey = shopItemKey;
            UpdatePackPriceUI(shopItemKey);

            // Phát sound theo discount
            PlayDiscountSound(discount);

            PlayDiscountEffect(discount);
        }

        private void PlayDiscountEffect(int discount)
        {
            if(completedEffect != null)
                completedEffect.SetActive(true);
            if (discountEffect40 != null)
                discountEffect40.SetActive(discount == 40);   
            if(discountEffect60 != null)
                discountEffect60.SetActive(discount == 60);
        }

        public override void OnSetup()
        {
            base.OnSetup();

            shopItemKey = blackFridayService.Instance.GetBlackFridayPackKey(0);

            if (buyButton != null)
                buyButton.onClick.AddListener(OnBuyClick);

            // Subscribe to shop purchase success event
            shopService.Instance.OnBuySuccess += OnBuySuccess;

            // Subscribe to purchase count changed event
            blackFridayService.Instance.OnPurchaseCountChanged += UpdateAvailableText;

            // Setup cheat dropdown
            SetupCheatDropdown();
            
            // Setup LTV input field
            SetupLTVInputField();
            
            // Setup discount dropdown
            SetupDiscountDropdown();
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

            // Show/hide cheat container
            UpdateCheatVisibility();

            // Update available purchases text
            UpdateAvailableText();

            // Set shopItemKey based on saved discount
            int currentDiscount = blackFridayService.Instance.GetCurrentDiscount();
            shopItemKey = blackFridayService.Instance.GetBlackFridayPackKey(currentDiscount);
            Debug.Log($"[BlackFriday] Open popup with saved discount: {currentDiscount}%, shopItemKey: {shopItemKey}");

            RefreshUI();

            // Play intro animation for discount container (scroll to current discount)
            if (spinController != null)
            {
                spinController.PlayIntroAnimation(introAnimDelay, introAnimDuration);
            }
        }
        protected void OnBuyClick()
        {
            // Check if user can still purchase
            if (!blackFridayService.Instance.CanPurchase())
            {
                Debug.LogWarning("[BlackFriday] Cannot purchase - lifetime limit reached");
                PopupToast.Cretate("You have reached the purchase limit!");
                return;
            }

            shopService.Instance.BuyPack(shopItemKey);
        }

        protected void OnBuySuccess(ShopItemKey shopItemKey)
        {
            if (this.shopItemKey != shopItemKey)
            {
                return;
            }

            Debug.Log($"[BlackFriday] OnBuySuccess - shopItemKey: {shopItemKey}");

            // Lấy reward từ pack (ShopService đã tự động add rồi)
            var packData = shopService.Instance.GetPackData(shopItemKey);
            RewardData totalReward = new RewardData();

            if (packData?.rewardData != null)
            {
                // Add reward từ pack
                foreach (var resource in packData.rewardData.resourceDatas)
                {
                    totalReward.AddReward(resource);
                }
            }

            // Add thêm bonus reward từ Black Friday
            var bonusReward = blackFridayService.Instance.GetCoinReward();
            totalReward.AddReward(bonusReward);

            // Add bonus reward vào inventory
            var bonusRewardData = new RewardData();
            bonusRewardData.AddReward(bonusReward);
            var logData = new EarnResourceLogData
            {
                spendType = "black_friday_bonus",
                spendId = "black_friday",
                source = "iap"
            };
            MySonatFramework.inventoryService.AddReward(bonusRewardData, logData);

            // Show popup reward với tổng reward
            var uiData = new UIData();
            uiData.Add("IAP", true);
            uiData.Add("Reward", totalReward);

            Debug.Log($"[BlackFriday] Opening PopupReward with total reward count: {totalReward.resourceDatas.Count}");
            PanelManager.Instance.OpenPanel<PopupReward>(uiData);

            // Increment lifetime purchase count
            blackFridayService.Instance.IncrementPurchaseCount();

            // Reset lại về pack mặc định sau khi mua xong
            ResetToDefaultPack();
        }

        private void RefreshUI()
        {
            var service = blackFridayService.Instance;
            var resource = service.GetCoinReward();

            //iconReward.SetSprite(MySonatFramework.GetService<SpriteAtlasService>().GetSprite($"ico_{resource}"));
            coinRewardText.text = resource.quantity.ToString();

            SetOriginPackPrice();
            UpdatePackPriceUI(shopItemKey);
            UpdatePricePackText();
        }

        private void UpdatePackPriceUI(ShopItemKey shopItemKey)
        {
            if (finalPriceText != null && shopItemKey != ShopItemKey.None)
                finalPriceText.text = $"{SonatSDKAdapter.GetProductPrice(shopItemKey)}";
        }

        private void SetOriginPackPrice()
        {
            if (originalPriceText != null)
            {
                originalPriceText.text = $"{SonatSDKAdapter.GetProductPrice(blackFridayService.Instance.GetBlackFridayPackKey(0))}";
            }
        }

        private void UpdatePricePackText()
        {
            if (pricePackText != null && shopItemKey != ShopItemKey.None)
            {
                pricePackText.text = $"{SonatSDKAdapter.GetProductPrice(shopItemKey)}";
            }
        }

        private void UpdateAvailableText()
        {
            if (availableText == null) return;

            int purchaseCount = blackFridayService.Instance.GetLifetimePurchaseCount();
            int remaining = blackFridayService.Instance.GetRemainingPurchases();

            // Set localization parameter VALUE = "x/2"
            availableText.SetLocalizeParam("VALUE", $"{remaining}/2");

            // Disable buy button if reached limit
            bool canPurchase = blackFridayService.Instance.CanPurchase();
            if (buyButton != null)
            {
                buyButton.interactable = canPurchase;
            }

            Debug.Log($"[BlackFriday] Available text updated: {purchaseCount}/2 purchased, {remaining} remaining, button enabled: {canPurchase}");

            // Close popup if no more purchases available
            if (!canPurchase && remaining == 0)
            {
                Debug.Log("[BlackFriday] No more purchases available - closing popup in 2 seconds");
                DOVirtual.DelayedCall(2f, () =>
                {
                    if (this != null && gameObject.activeInHierarchy)
                    {
                        Close();
                    }
                });
            }
        }

        private void ResetToDefaultPack()
        {
            // Reset currentDiscount về 0 trong service
            blackFridayService.Instance.SetCurrentDiscount(0);

            // Reset về pack mặc định (0%)
            shopItemKey = blackFridayService.Instance.GetBlackFridayPackKey(0);

            // Refresh UI với pack mới
            RefreshUI();

            Debug.Log($"[BlackFriday] Reset to default pack: {shopItemKey} (discount: 0%)");
        }

        private int GetDiscountIndex(int discount)
        {
            return discount switch
            {
                60 => 0,
                40 => 1,
                30 => 2,
                20 => 3,
                _ => 3
            };
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

        private void OnDestroy()
        {
            // Unsubscribe to prevent memory leak
            if (shopService.Instance != null)
            {
                shopService.Instance.OnBuySuccess -= OnBuySuccess;
            }

            if (blackFridayService.Instance != null)
            {
                blackFridayService.Instance.OnPurchaseCountChanged -= UpdateAvailableText;
            }
        }

        internal void SetInteractableButton(bool interactable)
        {
            buyButton.interactable = interactable;
        }

        public void OpenTutorial()
        {
            PanelManager.Instance.OpenPanelByName<BasePanel>("PopupTutBlackFriday");
        }

        private void PlayOpenSound()
        {
            // Phát sound box appear
            MySonatFramework.audioService?.PlaySound(AudioId.Friday_sale_Box_Appear_Grill_sort);

            // Phát music chỉ lần đầu tiên mở popup
            if (playMusicOnFirstOpen && !hasPlayedMusicBefore)
            {
                MySonatFramework.audioService?.PlaySound(AudioId.Friday_sale_Music_Grill_sort);
                hasPlayedMusicBefore = true;
            }
        }

        private void PlayDiscountSound(int discount)
        {
            AudioId soundId = discount switch
            {
                60 => AudioId.Friday_sale_Discount_60_Grill_sort,
                40 => AudioId.Friday_sale_Discount_40_Grill_sort,
                30 => AudioId.Friday_sale_Discount_30_Grill_sort,
                20 => AudioId.Friday_sale_Discount_20_Grill_sort,
                _ => AudioId.Friday_sale_Discount_20_Grill_sort
            };

            MySonatFramework.audioService?.PlaySound(soundId);
        }

        #region Cheat Methods

        private void SetupCheatDropdown()
        {
            if (userGroupDropdown == null) return;

            // Clear existing options
            userGroupDropdown.ClearOptions();

            // Add all UserGroup enum values (chỉ có 5 types)
            var options = new System.Collections.Generic.List<string>();
            foreach (UserGroup userGroup in System.Enum.GetValues(typeof(UserGroup)))
            {
                options.Add(userGroup.ToString());
            }

            userGroupDropdown.AddOptions(options);

            // Auto-select dropdown theo current user group
            UserGroup currentUserGroup = blackFridayService.Instance.GetUserGroup();
            userGroupDropdown.value = (int)currentUserGroup;

            // Add listener
            userGroupDropdown.onValueChanged.RemoveAllListeners();
            userGroupDropdown.onValueChanged.AddListener(OnCheatUserGroupChanged);
            
            Debug.Log($"[BlackFriday] Cheat dropdown setup - Current user group: {currentUserGroup}");
        }

        private void SetupLTVInputField()
        {
            if (ltvInputField == null) return;

            // Set placeholder text
            ltvInputField.contentType = TMP_InputField.ContentType.DecimalNumber;
            
            // Set current value (CheatLTV nếu > 0, không thì real LTV)
            if (UserGroupService.CheatLTV > 0)
            {
                ltvInputField.text = UserGroupService.CheatLTV.ToString("F2");
            }
            else
            {
                ltvInputField.text = SonatIap.sn_ltv_iap.ToString("F2");
            }

            // Add listener
            ltvInputField.onEndEdit.RemoveAllListeners();
            ltvInputField.onEndEdit.AddListener(OnLTVInputChanged);
            
            Debug.Log($"[BlackFriday] LTV input field setup - Current LTV: {ltvInputField.text}");
        }

        private void SetupDiscountDropdown()
        {
            if (discountDropdown == null) return;

            // Clear existing options
            discountDropdown.ClearOptions();

            // Add discount options: None, 20, 30, 40, 60
            var options = new System.Collections.Generic.List<string>
            {
                "None (0%)",
                "20%",
                "30%",
                "40%",
                "60%"
            };

            discountDropdown.AddOptions(options);

            // Auto-select dropdown theo current discount
            int currentDiscount = blackFridayService.Instance.GetCurrentDiscount();
            int dropdownIndex = GetDropdownIndexFromDiscount(currentDiscount);
            discountDropdown.value = dropdownIndex;

            // Add listener
            discountDropdown.onValueChanged.RemoveAllListeners();
            discountDropdown.onValueChanged.AddListener(OnCheatDiscountChanged);
            
            Debug.Log($"[BlackFriday] Discount dropdown setup - Current discount: {currentDiscount}%");
        }

        private int GetDropdownIndexFromDiscount(int discount)
        {
            return discount switch
            {
                0 => 0,  // None
                20 => 1, // 20%
                30 => 2, // 30%
                40 => 3, // 40%
                60 => 4, // 60%
                _ => 0   // Default to None
            };
        }

        private int GetDiscountFromDropdownIndex(int index)
        {
            return index switch
            {
                0 => 0,  // None
                1 => 20, // 20%
                2 => 30, // 30%
                3 => 40, // 40%
                4 => 60, // 60%
                _ => 0   // Default to None
            };
        }

        private void OnLTVInputChanged(string value)
        {
            if (!CheatPanel.IsCheating) return;

            if (string.IsNullOrEmpty(value))
            {
                // Reset về real LTV (set CheatLTV = 0)
                userGroupService.Instance.SetCheatLTV(0f);
                ltvInputField.text = SonatIap.sn_ltv_iap.ToString("F2");
            }
            else if (float.TryParse(value, out float ltv))
            {
                // Set cheat LTV
                userGroupService.Instance.SetCheatLTV(ltv);
                Debug.Log($"[BlackFriday] Cheat: LTV set to {ltv}");
            }
            else
            {
                // Invalid input - reset về giá trị hiện tại
                if (UserGroupService.CheatLTV > 0)
                    ltvInputField.text = UserGroupService.CheatLTV.ToString("F2");
                else
                    ltvInputField.text = SonatIap.sn_ltv_iap.ToString("F2");
                
                Debug.LogWarning("[BlackFriday] Invalid LTV input");
            }

            // Recalculate user group cho event này sau khi set cheat LTV
            blackFridayService.Instance.RecalculateUserGroup();

            // Update dropdown về user group mới
            if (userGroupDropdown != null)
            {
                UserGroup newUserGroup = blackFridayService.Instance.GetUserGroup();
                userGroupDropdown.value = (int)newUserGroup;
            }

            // Refresh UI
            RefreshUI();
            int spinPrice = blackFridayService.Instance.GetSpinPrice();
            spinController.Setup(spinPrice);
        }

        private void UpdateCheatVisibility()
        {
            if (cheatContainer == null) return;

            // Show cheat container only if CheatPanel is active
            bool showCheat = CheatPanel.IsCheating;
            cheatContainer.SetActive(showCheat);

            if (showCheat)
            {
                UserGroup currentUserGroup = blackFridayService.Instance.GetUserGroup();
                Debug.Log($"[BlackFriday] Cheat mode active. Current UserGroup: {currentUserGroup}");
            }
        }

        private void OnCheatUserGroupChanged(int index)
        {
            if (!CheatPanel.IsCheating) return;
            
            // Set user group trong BlackFridayService
            UserGroup selectedUserGroup = (UserGroup)index;
            blackFridayService.Instance.SetUserGroup(selectedUserGroup);
            
            Debug.Log($"[BlackFriday] Cheat: Changed user group to {selectedUserGroup}");

            // Reset về pack mặc định của user group mới
            shopItemKey = blackFridayService.Instance.GetBlackFridayPackKey(0);

            // Refresh UI with new user group
            RefreshUI();

            // Update spin price
            int spinPrice = blackFridayService.Instance.GetSpinPrice();
            spinController.Setup(spinPrice);
        }

        private void OnCheatDiscountChanged(int index)
        {
            if (!CheatPanel.IsCheating) return;

            int selectedDiscount = GetDiscountFromDropdownIndex(index);
            
            Debug.Log($"[BlackFriday] Cheat: Changed discount to {selectedDiscount}%");

            // Set current discount
            blackFridayService.Instance.SetCurrentDiscount(selectedDiscount);
            
            // Update shopItemKey theo discount mới
            shopItemKey = blackFridayService.Instance.GetBlackFridayPackKey(selectedDiscount);

            // Refresh UI with new discount
            RefreshUI();
            
            // Update spin controller position to show the new discount
            if (spinController != null)
            {
                spinController.PlayIntroAnimation(0f, 0.5f);
            }
            
            // Play discount effect if not None
            if (selectedDiscount > 0)
            {
                PlayDiscountEffect(selectedDiscount);
            }
            else
            {
                // Hide all effects for None
                if (completedEffect != null) completedEffect.SetActive(false);
                if (discountEffect40 != null) discountEffect40.SetActive(false);
                if (discountEffect60 != null) discountEffect60.SetActive(false);
            }
        }

        /// <summary>
        /// Cheat method để reset toàn bộ event data (chỉ hiện trong cheat mode)
        /// NOTE: CheatLTV sẽ KHÔNG bị reset để có thể test với LTV cao trước khi event init
        /// </summary>
        public void CheatResetAllEventData()
        {
            if (!CheatPanel.IsCheating) return;

            // KHÔNG reset cheat LTV - giữ nguyên để test với LTV cao trước khi event init
            
            // Reset event data
            blackFridayService.Instance.CheatResetAllEventData();
            
            // Refresh UI sau khi reset
            RefreshUI();
            UpdateAvailableText();
            
            // Update dropdown về user group mới (đã được recalculate với CheatLTV nếu có)
            if (userGroupDropdown != null)
            {
                UserGroup newUserGroup = blackFridayService.Instance.GetUserGroup();
                userGroupDropdown.value = (int)newUserGroup;
            }
            
            // Update discount dropdown về 0 (None)
            if (discountDropdown != null)
            {
                discountDropdown.value = 0; // None
            }
            
            // Update LTV input field về giá trị hiện tại (CheatLTV nếu > 0, không thì real LTV)
            if (ltvInputField != null)
            {
                if (UserGroupService.CheatLTV > 0)
                    ltvInputField.text = UserGroupService.CheatLTV.ToString("F2");
                else
                    ltvInputField.text = SonatIap.sn_ltv_iap.ToString("F2");
            }
            
            // Reset spin controller
            int spinPrice = blackFridayService.Instance.GetSpinPrice();
            spinController.Setup(spinPrice);
            
            Debug.Log("[BlackFriday] Cheat: All event data reset. User group recalculated (CheatLTV preserved).");
        }

        /// <summary>
        /// Cheat method để clear CheatLTV về real LTV (chỉ hiện trong cheat mode)
        /// </summary>
        public void CheatClearLTV()
        {
            if (!CheatPanel.IsCheating) return;

            // Clear CheatLTV trong UserGroupService
            userGroupService.Instance.CheatClearLTV();
            
            // Recalculate user group cho event này
            blackFridayService.Instance.RecalculateUserGroup();
            
            // Update UI
            if (ltvInputField != null)
            {
                ltvInputField.text = SonatIap.sn_ltv_iap.ToString("F2");
            }
            
            // Update dropdown về user group mới
            if (userGroupDropdown != null)
            {
                UserGroup newUserGroup = blackFridayService.Instance.GetUserGroup();
                userGroupDropdown.value = (int)newUserGroup;
            }
            
            // Refresh UI
            RefreshUI();
            int spinPrice = blackFridayService.Instance.GetSpinPrice();
            spinController.Setup(spinPrice);
            
            Debug.Log("[BlackFriday] Cheat: CheatLTV cleared.");
        }

        /// <summary>
        /// Cheat method để simulate sang năm sau (chỉ hiện trong cheat mode)
        /// </summary>
        public void CheatSimulateNextYear()
        {
            if (!CheatPanel.IsCheating) return;

            blackFridayService.Instance.CheatSimulateNextYear();
            Debug.Log("[BlackFriday] Cheat: Next year simulated. Please restart the game to see the reset.");
        }

        #endregion
    }
}
