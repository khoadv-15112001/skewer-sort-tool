using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Sonat.Enums;
using GrillSort.XmasEvent;
using SonatFramework.Scripts.SonatSDKAdapterModule;
using SonatFramework.Systems.InventoryManagement.GameResources;
using SonatFramework.Systems;
using SonatFramework.Scripts.UIModule.UIElements;
using Spine.Unity;

namespace GrillSort.UI
{
    /// <summary>
    /// UI component cho mỗi pack trong PopupXmasEvent (theo design Frosty Haul)
    /// Gắn vào từng pack container trong popup
    /// </summary>
    public class XmasPackUI : MonoBehaviour
    {
        [Header("Discount Badge")]
        [SerializeField] private GameObject discountBadge;
        [SerializeField] private TMP_Text discountText; // "50% OFF", "70% OFF"

        [Header("Rewards Display")]
        [SerializeField] private UIRewardItem coinText; // "600"
        [SerializeField] private UIRewardItem infiniteLifeText; // "1h", "2h", "3h"
        [SerializeField] private UIRewardItem boosterItemCount; // "x1"

        [Header("Price & Button")]
        [SerializeField] private TMP_Text priceText; // "$3.99"
        [SerializeField] private Button buyButton;
        [SerializeField] private GameObject completedIndicator; // ✓ Checkmark khi đã mua

        [Header("Lock State")]
        [SerializeField] private SkeletonGraphic lockIconAnim; // 🔒 Spine animation cho lock
        [SerializeField] private string lockAnimState1 = "state1"; // Animation lock rung (locked)
        [SerializeField] private string lockAnimState2 = "state2"; // Animation mở khóa và rơi (unlocking)

        private int packIndex = -1;
        private Action<int> onBuyCallback;
        private bool isAnimatingUnlock = false; // Đang play animation unlock

        private void Awake()
        {
            if (buyButton != null)
            {
                buyButton.onClick.AddListener(OnBuyClick);
            }
        }

        /// <summary>
        /// Setup UI cho pack này
        /// </summary>
        /// <param name="hideNonCurrent">Nếu true: ẩn pack nếu không phải current (dùng cho shop banner)</param>
        /// <param name="playUnlockAnim">Nếu true: current pack sẽ play animation mở khóa</param>
        public void Setup(int index, XmasPackTier packTier, RewardData rewards, bool isCurrentPack, bool isPurchased, bool isLocked, Action<int> onBuy, bool hideNonCurrent = false, bool playUnlockAnim = false)
        {
            packIndex = index;
            onBuyCallback = onBuy;

            if (packTier == null)
            {
                Debug.LogWarning($"[XmasPackUI] Pack tier is null for index {index}");
                gameObject.SetActive(false);
                return;
            }

            // Option: Hide non-current packs (for shop banner)
            if (hideNonCurrent)
            {
                if (!isCurrentPack)
                {
                    gameObject.SetActive(false);
                    return;
                }
                else
                {
                    gameObject.SetActive(true);
                }
            }
            else
            {
                // Always show in popup
                gameObject.SetActive(true);
            }

            // Set discount badge
            if (discountBadge != null)
            {
                // Ẩn badge nếu discount = 0
                discountBadge.SetActive(packTier.discountPercent > 0);
            }

            if (discountText != null && packTier.discountPercent > 0)
            {
                discountText.text = $"{packTier.discountPercent}%";
            }

            // Display rewards từ ShopConfig
            DisplayRewards(rewards);

            // Set price
            if (priceText != null)
            {
                priceText.text = SonatSDKAdapter.GetProductPrice(packTier.shopItemKey);
            }

            // Set button/completed state
            if (isPurchased)
            {
                // Đã mua - show dấu tích, ẩn button
                if (buyButton != null)
                    buyButton.gameObject.SetActive(false);

                if (completedIndicator != null)
                    completedIndicator.SetActive(true);
            }
            else
            {
                // Chưa mua - show button, ẩn dấu tích
                if (buyButton != null)
                {
                    buyButton.gameObject.SetActive(true);
                    buyButton.interactable = isCurrentPack;
                }

                if (completedIndicator != null)
                    completedIndicator.SetActive(false);
            }

            // Handle lock animation (Spine)
            HandleLockAnimation(isLocked, isPurchased, isCurrentPack, playUnlockAnim);

            // Optional: Dim purchased packs
            var canvasGroup = GetComponent<CanvasGroup>();
            if (canvasGroup != null)
            {
                canvasGroup.alpha = isPurchased ? 0.7f : 1f;
            }
        }

        /// <summary>
        /// Display rewards từ ShopConfig
        /// </summary>
        private void DisplayRewards(RewardData rewards)
        {
            if (rewards == null || rewards.resourceDatas == null)
            {
                Debug.LogWarning($"[XmasPackUI] No rewards data for pack {packIndex}");
                return;
            }

            foreach (var resource in rewards.resourceDatas)
            {
                var resourceType = GameResourceHelper.ResourceType(resource.resource);
                // Coins
                if (resource.resource == GameResource.Coin && coinText != null)
                {
                    coinText.Init(resource.resource, resource.quantity);
                }
                // Infinite life
                else if (resource.resource == GameResource.Lives && infiniteLifeText != null)
                {
                    //string quantityString = "";
                    //if (resource.quantity < 3600) quantityString = $"{resource.quantity / 60}m";
                    //else
                    //{
                    //    quantityString = $"{resource.quantity * 1.0f / 3600}h";
                    //}
                    infiniteLifeText.Init(resource.resource, resource.quantity);
                }
                // Special items (boosters, etc)
                else if (resourceType == GameResourceType.Booster && boosterItemCount != null)
                {
                    boosterItemCount.Init(resource.resource, resource.quantity);
                }
            }
        }

        private void OnBuyClick()
        {
            if (packIndex >= 0 && onBuyCallback != null)
            {
                onBuyCallback.Invoke(packIndex);
            }
        }

        /// <summary>
        /// Handle lock animation states
        /// State 1: Lock tĩnh (khi đang locked) - không loop
        /// State 2: Mở khóa và rơi xuống (khi current pack + playUnlockAnim = true) -> sau đó ẩn
        /// </summary>
        private void HandleLockAnimation(bool isLocked, bool isPurchased, bool isCurrentPack, bool playUnlockAnim)
        {
            if (lockIconAnim == null) return;

            // Nếu đang play animation unlock thì không làm gì
            if (isAnimatingUnlock) return;

            // Nếu đã mua -> ẩn lock animation
            if (isPurchased)
            {
                lockIconAnim.gameObject.SetActive(false);
                return;
            }

            if (isLocked)
            {
                // Đang locked -> show và play state 1 (không loop - tĩnh)
                lockIconAnim.gameObject.SetActive(true);
                PlayLockAnimation(lockAnimState1, false); // không loop
            }
            else if (isCurrentPack && playUnlockAnim)
            {
                // Current pack + được yêu cầu play unlock animation
                isAnimatingUnlock = true;
                lockIconAnim.gameObject.SetActive(true);
                PlayLockAnimation(lockAnimState2, false, OnUnlockAnimationComplete);
                
                // Play unlock sound
                MySonatFramework.audioService?.PlaySound(AudioId.SantaHaul_Price_Unlock_Grill_sort);
            }
            else
            {
                // Current pack nhưng không play unlock anim, hoặc pack đã qua -> ẩn
                lockIconAnim.gameObject.SetActive(false);
            }
        }

        /// <summary>
        /// Play animation trên SkeletonGraphic
        /// </summary>
        private void PlayLockAnimation(string animName, bool loop, Action onComplete = null)
        {
            if (lockIconAnim == null || lockIconAnim.AnimationState == null) return;

            var trackEntry = lockIconAnim.AnimationState.SetAnimation(0, animName, loop);
            
            if (onComplete != null && trackEntry != null)
            {
                trackEntry.Complete += (entry) => onComplete?.Invoke();
            }
        }

        /// <summary>
        /// Callback khi animation unlock (state 2) hoàn thành
        /// </summary>
        private void OnUnlockAnimationComplete()
        {
            isAnimatingUnlock = false;
            if (lockIconAnim != null)
            {
                lockIconAnim.gameObject.SetActive(false);
                Debug.Log($"[XmasPackUI] Pack {packIndex} unlock animation completed - hiding lock");
            }
        }

        private void OnDestroy()
        {
            if (buyButton != null)
            {
                buyButton.onClick.RemoveAllListeners();
            }
        }
    }
}

