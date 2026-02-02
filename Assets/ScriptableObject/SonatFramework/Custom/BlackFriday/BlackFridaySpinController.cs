using System;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Threading;
using SonatFramework.Systems;
using Sonat.Enums;
using SonatFramework.Systems.SettingsManagement.Vibation;
using SonatFramework.Scripts.UIModule;
using SonatFramework.Scripts.Feature.Shop.UI;
using SonatFramework.Systems.InventoryManagement;
using Spine.Unity;
using System.Reflection;
using GrillSort.UI;

namespace GrillSort.BlackFriday
{
    public class BlackFridaySpinController : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private RectTransform discountContainer;
        [SerializeField] private TMP_Text[] discountItems; // 9 text: 60,40,30,20 (bộ1), 60,40,30,20 (bộ2), 0
        [SerializeField] private Button buttonSpin;
        [SerializeField] private TMP_Text priceText;
        [SerializeField] private PopupBlackFriday parentPopup;
        [Header("Spin Settings")]
        [SerializeField] private float itemHeight = 100f;
        [SerializeField] private float spinDuration = 2.8f;
        [SerializeField] private float tickInterval = 0.07f;
        [SerializeField] private int minLoopCount = 3;
        [SerializeField] private int maxLoopCount = 5;
        [SerializeField] private float bounceDistance = 20f;
        [SerializeField] SkeletonGraphic arrownAnim;

        // 2 bộ discount giống nhau + 1 discount 0% ở cuối
        // Layout: [60,40,30,20, 60,40,30,20, 0] = 9 items
        private readonly int[] discountValues = { 60, 40, 30, 20, 60, 40, 30, 20, 0 };
        private bool isSpinning;
        private CancellationTokenSource cts;
        private BlackFridayService service;

        private float fullCycle => itemHeight * discountItems.Length;
        private int currentIndex = 0;
        private int currentDiscount;
        private Action<int, ShopItemKey> onSpinCompleted;
        private Action<ShopItemKey> onSpining;

        private Tween activeTween;
        public ShopItemKey shopItemKey;
        private int spinCost;

        public Action OnInsufficientFunds;

        public void Setup(int spinPrice)
        {
            service = MySonatFramework.GetService<BlackFridayService>();
            if (service == null)
            {
                Debug.LogWarning("[BlackFriday] BlackFridayService not found via MySonatFramework.GetService, trying Resources.Load");
                service = Resources.Load<BlackFridayService>("BlackFridayService");
            }
            if(parentPopup == null)
            {
                parentPopup = GetComponentInParent<PopupBlackFriday>();
                Debug.LogWarning("[BlackFriday] parentPopup reference is missing!");
            }
            // show clear UI for free spin and make button visible
            priceText.text = spinPrice == 0 ? "FREE" : spinPrice.ToString();
            priceText.color = spinPrice == 0 ? Color.yellow : Color.white;
            this.onSpining = parentPopup.OnSpining;
            this.onSpinCompleted = parentPopup.OnSpinCompleted;
            spinCost = spinPrice;

            buttonSpin.onClick.RemoveAllListeners();
            buttonSpin.onClick.AddListener(OnSpinClicked);
            buttonSpin.interactable = true;

            Debug.Log($"[BlackFriday] Setup called - spinPrice={spinPrice}, priceText={priceText.text}");
        }

        /// <summary>
        /// Animate discount container to show current discount on popup open
        /// Scroll to second set of discounts for smooth long animation
        /// </summary>
        public void PlayIntroAnimation(float delayBefore = 1f, float duration = 2f)
        {
            if (discountContainer == null || service == null) return;

            // Kill any existing tweens
            discountContainer.DOKill();

            // Get current discount from service
            int savedDiscount = service.GetCurrentDiscount();
            
            // Find target index - always use second set for non-zero discounts
            int targetIndex = GetTargetIndexForDiscount(savedDiscount);
            
            // Calculate target position
            float targetY = targetIndex * itemHeight;
            
            // Set start position to beginning (0)
            discountContainer.anchoredPosition = new Vector2(0, 0);

            // Animate to target position with delay
            discountContainer.DOAnchorPosY(targetY, duration)
                .SetEase(Ease.OutQuart)
                .SetDelay(delayBefore);

            // Update current index
            currentIndex = targetIndex;
            currentDiscount = savedDiscount;

            Debug.Log($"[BlackFriday] Intro animation - scrolling to discount: {savedDiscount}% (index: {targetIndex}, position: {targetY}px, delay: {delayBefore}s, duration: {duration}s)");
        }

        /// <summary>
        /// Get target index for discount (use second set for smooth animation)
        /// Layout: [60,40,30,20, 60,40,30,20, 0]
        /// Indices: [0, 1, 2, 3,  4, 5, 6, 7,  8]
        /// </summary>
        private int GetTargetIndexForDiscount(int discount)
        {
            // Discount 0% → index 8 (cuối cùng)
            if (discount == 0) return 8;
            
            // Use second set (indices 4-7) for smooth animation
            return discount switch
            {
                60 => 4, // 60% bộ 2
                40 => 5, // 40% bộ 2
                30 => 6, // 30% bộ 2
                20 => 7, // 20% bộ 2
                _ => 8   // Default to 0% if unknown
            };
        }

        private void OnDisable()
        {
            Debug.Log("[BlackFriday] OnDisable called");
            StopAllSounds();
            activeTween?.Kill();
            cts?.Cancel();
            isSpinning = false;
            if (buttonSpin != null) buttonSpin.interactable = true;
        }

        private void OnSpinClicked()
        {
            Debug.Log($"[BlackFriday] OnSpinClicked - isSpinning={isSpinning}, spinCost={spinCost}, currentIndex={currentIndex}");
            if (isSpinning) return;

            // Free spin -> start immediately
            if (spinCost == 0)
            {
                Debug.Log("[BlackFriday] Free spin detected -> starting without cost checks");
                StartSpinSequence();
                return;
            }

            // Check coin via inventoryService
            var inv = MySonatFramework.inventoryService;
            if (inv == null)
            {
                Debug.LogWarning("[BlackFriday] inventoryService missing - cannot check coins");
                PopupToast.Cretate("Not enough coin!");
                PanelManager.Instance.OpenPanelByName<ShopPanelBase>("ShopPanel");
                return;
            }

            if (!inv.CanReduce(GameResource.Coin, spinCost))
            {
                Debug.Log("[BlackFriday] Not enough coins (inventoryService.CanReduce == false)");
                StopAllSounds();
                PopupToast.Cretate("Not enough coin!");
                PanelManager.Instance.OpenPanelByName<ShopPanelBase>("ShopPanel");
                return;
            }

            // Reserve/consume coins now
            inv.ReduceResource(GameResource.Coin, spinCost, new SpendResourceLogData
            {
                earnType = "BF",
                earnId = "black_friday_spin",
                source = "non_iap"
            });

            StartSpinSequence();
        }

        // helper kiểm tra UI còn sống
        private bool IsUiAlive()
        {
            // UnityEngine.Object implicit null check handles destroyed objects
            return discountContainer != null && discountItems != null && priceText != null && buttonSpin != null;
        }

        private void StartSpinSequence()
        {
            Debug.Log("[BlackFriday] Starting spin sequence");

            if (!IsUiAlive())
            {
                Debug.LogWarning("[BlackFriday] StartSpinSequence aborted - UI not alive");
                StopAllSounds();
                isSpinning = false;
                return;
            }

            buttonSpin.interactable = false;
            isSpinning = true;
            cts = new CancellationTokenSource();

            if (service == null) service = MySonatFramework.GetService<BlackFridayService>();
            var result = service?.SpinAndGetPack() ?? (discountPercent: discountValues[0], packKey: default(ShopItemKey));
            Debug.Log($"[BlackFriday] Service returned discountPercent={result.discountPercent}, shopItemKey={result.packKey}");
            this.shopItemKey = result.packKey;

            // notify spinning start (optional)
            //onSpining?.Invoke(result.packKey);

            PlaySpin(result.discountPercent, cts.Token).Forget();
        }

        private async UniTask PlaySpin(int targetDiscount, CancellationToken token)
        {
            Debug.Log($"[BlackFriday] PlaySpin started targetDiscount={targetDiscount}");
            var audio = MySonatFramework.audioService;
            var vib = MySonatFramework.GetService<VibrationService>();

            if (!IsUiAlive())
            {
                Debug.LogWarning("[BlackFriday] PlaySpin aborted - UI not alive");
                StopAllSounds();
                isSpinning = false;
                cts?.Cancel();
                return;
            }
            parentPopup.SetInteractableButton(false);
            
            // Get target index - use second set for landing position
            int targetIndex = GetTargetIndexForDiscount(targetDiscount);
            float startOffset = currentIndex * itemHeight;

            if (discountContainer == null)
            {
                Debug.LogWarning("[BlackFriday] discountContainer null - aborting");
                StopAllSounds();
                isSpinning = false;
                return;
            }

            discountContainer.anchoredPosition = new Vector2(0, startOffset);
            int loopCount = UnityEngine.Random.Range(minLoopCount, maxLoopCount + 1);
            float totalDistance = loopCount * fullCycle + (targetIndex * itemHeight - startOffset);
            float endOffset = startOffset + totalDistance;

            float tickTimer = 0f;

            audio?.PlaySound(AudioId.Friday_sale_Spin_Rolling_Grill_sort);
            if (arrownAnim != null) arrownAnim.AnimationState.SetAnimation(0, "spin_loop", true);

            activeTween = DOTween.To(() => startOffset, y =>
            {
                // kill tween early if UI destroyed
                if (!IsUiAlive() || discountContainer == null)
                {
                    activeTween?.Kill();
                    return;
                }

                float loopedY = y % fullCycle;
                discountContainer.anchoredPosition = new Vector2(0, loopedY);

                // Tính discount hiện tại dựa trên vị trí spin
                int currentVisibleIndex = Mathf.RoundToInt(loopedY / itemHeight) % discountItems.Length;
                int currentVisibleDiscount = discountValues[currentVisibleIndex];

                tickTimer += Time.deltaTime;
                if (tickTimer >= tickInterval)
                {            
                    onSpining?.Invoke(service.GetBlackFridayPackKey(currentVisibleDiscount));

                    tickTimer = 0f;
                    audio?.PlaySound(AudioId.SpinTick);
                    vib?.Vibrate(8);
                }
            }, endOffset, spinDuration).SetEase(Ease.OutCubic);

            try
            {
                await activeTween.ToUniTask(cancellationToken: token);
            }
            catch (OperationCanceledException)
            {
                Debug.Log("[BlackFriday] PlaySpin canceled");
                StopAllSounds();
                isSpinning = false;
                return;
            }

            if (!IsUiAlive())
            {
                Debug.LogWarning("[BlackFriday] UI destroyed after spin - aborting finish steps");
                StopAllSounds();
                isSpinning = false;
                return;
            }

            if (arrownAnim != null) arrownAnim.AnimationState.SetAnimation(0, "spin_end", false);
            audio?.PlaySound(AudioId.Spin_Win_prize_Grill_sort);
            vib?.Vibrate(100);

            await Bounce(itemHeight * targetIndex);
            FlashHighlight(targetIndex).Forget();

            currentIndex = targetIndex;
            currentDiscount = targetDiscount;
            isSpinning = false;
            if (buttonSpin != null) buttonSpin.interactable = true;

            Debug.Log($"[BlackFriday] Spin completed: currentDiscount={currentDiscount}, currentIndex={currentIndex}");
            
            // Save current discount to service
            if (service != null)
            {
                service.SetCurrentDiscount(currentDiscount);
            }
            
            onSpinCompleted?.Invoke(currentDiscount, shopItemKey);
            parentPopup.SetInteractableButton(true);
            
            // Cập nhật lại giá spin sau khi quay (vì lần đầu free, lần sau phải trả tiền)
            UpdateSpinPrice();
        }

        private async UniTask Bounce(float targetY)
        {
            if (!IsUiAlive() || discountContainer == null) return;
            await discountContainer.DOAnchorPosY(targetY - bounceDistance, 0.12f).SetEase(Ease.OutQuad);
            if (!IsUiAlive()) return;
            await discountContainer.DOAnchorPosY(targetY, 0.18f).SetEase(Ease.OutBack);
        }

        private async UniTask FlashHighlight(int index)
        {
            if (!IsUiAlive() || discountItems == null || index < 0 || index >= discountItems.Length) return;
            var txt = discountItems[index];
            if (txt == null) return;

            var rt = txt.rectTransform;
            if (rt == null) return;

            var originalScale = rt.localScale;
            float scaleUp = 1.2f;
            float singleDuration = 0.12f;

            for (int i = 0; i < 2; i++)
            {
                await rt.DOScale(scaleUp, singleDuration).SetEase(Ease.OutQuad).ToUniTask();
                if (!IsUiAlive()) return;
                await rt.DOScale(originalScale, singleDuration).SetEase(Ease.InQuad).ToUniTask();
                if (!IsUiAlive()) return;
            }
        }

        private void UpdateSpinPrice()
        {
            if (service == null || priceText == null) return;
            
            int newSpinPrice = service.GetSpinPrice();
            spinCost = newSpinPrice;
            priceText.text = newSpinPrice == 0 ? "FREE" : newSpinPrice.ToString();
            priceText.color = newSpinPrice == 0 ? Color.yellow : Color.white;
            
            Debug.Log($"[BlackFriday] Price updated to: {priceText.text}");
        }

        private void StopAllSounds()
        {
            var audio = MySonatFramework.audioService;
            if (audio == null) return;

            var t = audio.GetType();
            try
            {
                var stopAll = t.GetMethod("StopAll") ?? t.GetMethod("StopAllSounds") ?? t.GetMethod("StopAllSfx") ?? t.GetMethod("StopAllSound") ?? t.GetMethod("Stop");
                stopAll?.Invoke(audio, null);

                var stopSound = t.GetMethod("StopSound", new[] { typeof(AudioId) });
                if (stopSound != null)
                {
                    stopSound.Invoke(audio, new object[] { AudioId.Spin_Rolling_Grill_sort });
                    stopSound.Invoke(audio, new object[] { AudioId.Spin_Win_prize_Grill_sort });
                    stopSound.Invoke(audio, new object[] { AudioId.Friday_sale_Spin_Rolling_Grill_sort });
                }
            }
            catch (Exception ex)
            {
                Debug.LogWarning("[BlackFriday] StopAllSounds failed: " + ex);
            }
        }
    }
}
