using Cysharp.Threading.Tasks;
using DG.Tweening;
using I2.Loc;
using SonatFramework.Scripts.UIModule;
using SonatFramework.Scripts.UIModule.UIElements;
using SonatFramework.Systems;
using SonatFramework.Systems.InventoryManagement;
using SonatFramework.Systems.InventoryManagement.GameResources;
using SonatFramework.Systems.ObjectPooling;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace GrillSort.Rail
{
    public class UIRailMilestoneProgress : MonoBehaviour
    {
        [SerializeField] private LocalizationParamsManager milestoneProgressLocalize;
        [SerializeField] private Slider slider;
        [SerializeField] private Button firstIcon;
        [SerializeField] private GameObject firstIconOn;
        [SerializeField] private GameObject firstIconOff;
        [SerializeField] private TMPro.TextMeshProUGUI firstIconTimeTxt;
        [SerializeField] private GameObject firstIconBubble;
        [SerializeField] private Image middleIcon;
        [SerializeField] private Image endIcon;
        [SerializeField] private UIBubbleRewardSmart middleReward;
        [SerializeField] private UIBubbleRewardSmart endReward;
        [SerializeField] private Button tapButton;
        [SerializeField] private Transform tapTxt;
        [SerializeField] private float openBoxDelay = 2f;
        [SerializeField] private float rewardOpenSoundDelay = 0.5f;
        [SerializeField] private float spawnRewardDelay = 1f;
        [SerializeField] private float slideInDelay = 2f;
        [SerializeField] private float iconMoveDuration = 0.4f;

        [Header("Box Icons")]
        [SerializeField] private Sprite blueBoxSprite;
        [SerializeField] private Sprite redBoxSprite;
        [SerializeField] private Sprite purpleBoxSprite;

        [Header("References")]
        [SerializeField] private UIRailLayoutAnim layoutAnim;
        [SerializeField] private UIRailCharacter character;
        [SerializeField] private UIRailConveyor conveyor;
        [SerializeField] private Transform rewardContainer;

        private bool isLastMilestone => RailService.Instance.IsLastMilestone();
        private Action onMilestoneReachedComplete;
        private Button closeButton;
        private RailMusicManager musicManager;
        private Action onStageComplete;
        private Tween enableButtonTween;
        private Tween playIdleTween;

        private readonly Service<PoolingContainerService> poolingService = new();

        public void Setup(Action onComplete = null)
        {
            onMilestoneReachedComplete = onComplete;

            SetupTitle();
            SetupFirstIcon();
            SetupIcons();
            SetupSlider();
        }

        private void SetupFirstIcon()
        {
            if (firstIcon != null)
            {
                // Setup button click
                firstIcon.onClick.RemoveAllListeners();
                firstIcon.onClick.AddListener(OnClickFirstIcon);

                // Update UI state
                UpdateFirstIconState();

                // Subscribe to tick event
                RailService.OnTick += OnTickFreeToken;
            }
        }

        private void OnClickFirstIcon()
        {
            if (RailService.Instance.CanClaimFreeToken())
            {
                bool success = RailService.Instance.ClaimFreeToken();
                if (success)
                {
                    Debug.Log("[UIRailMilestoneProgress] Claimed 5 free tokens!");

                    // Tạo bubble effect
                    CreateBubbleEffect();

                    // Update UI
                    UpdateFirstIconState();

                    // Trigger slide animation nếu cần
                    TrySlide();
                }
            }
        }

        private void CreateBubbleEffect()
        {
            if (firstIconBubble == null) return;

            // Instantiate bubble từ firstIconBubble
            var bubbleItem = Instantiate(firstIconBubble, PanelManager.Instance.transform);
            var rectTransform = bubbleItem.GetComponent<RectTransform>();

            if (rectTransform == null)
            {
                rectTransform = bubbleItem.AddComponent<RectTransform>();
            }

            // Set vị trí ban đầu
            rectTransform.position = firstIconBubble.transform.position;
            rectTransform.localScale = firstIconBubble.transform.localScale;

            bubbleItem.gameObject.SetActive(true);

            // Animation bay lên
            rectTransform.DOLocalMoveY(rectTransform.localPosition.y + 100f, 0.6f).SetEase(Ease.InOutSine);

            // Fade out
            var canvasGroup = bubbleItem.GetComponent<CanvasGroup>();
            if (canvasGroup == null)
            {
                canvasGroup = bubbleItem.AddComponent<CanvasGroup>();
            }

            canvasGroup.DOFade(0f, 1f).OnComplete(() =>
            {
                Destroy(bubbleItem);
            });
        }

        private void UpdateFirstIconState()
        {
            if (firstIcon == null) return;

            bool canClaim = RailService.Instance.CanClaimFreeToken();

            if (canClaim)
            {
                // Hiện On, ẩn Off
                firstIconOn?.SetActive(true);
                firstIconOff?.SetActive(false);
            }
            else
            {
                // Hiện Off, ẩn On
                firstIconOn?.SetActive(false);
                firstIconOff?.SetActive(true);

                // Update time text
                UpdateFirstIconTimeText();
            }
        }

        private void UpdateFirstIconTimeText()
        {
            if (firstIconTimeTxt == null) return;

            long remainingTime = RailService.Instance.GetRemainingCooldownTime();
            firstIconTimeTxt.text = SonatFramework.Scripts.Utils.SonatUtils.FormatTimeSmart(remainingTime);
        }

        private void OnTickFreeToken()
        {
            if (firstIcon == null) return;

            bool canClaim = RailService.Instance.CanClaimFreeToken();

            // Nếu đang trong cooldown, update thời gian
            if (!canClaim)
            {
                UpdateFirstIconTimeText();
            }
            // Nếu vừa hết cooldown, update state để hiện nút On
            else if (firstIconOff != null && firstIconOff.activeSelf)
            {
                UpdateFirstIconState();
            }
        }

        private void OnDestroy()
        {
            // Unsubscribe from tick event
            RailService.OnTick -= OnTickFreeToken;
            
            // Kill tweens để tránh memory leak
            enableButtonTween?.Kill();
            playIdleTween?.Kill();
        }

        public void SetCloseButton(Button btn)
        {
            closeButton = btn;
        }

        public void SetMusicManager(RailMusicManager manager)
        {
            musicManager = manager;
        }

        public void SetOnStageComplete(Action callback)
        {
            onStageComplete = callback;
        }

        private void SetupTitle()
        {
            milestoneProgressLocalize.SetParameterValue("value",
                $"{RailService.Instance.CurrentMilestone.Value}/{RailService.Instance.GetCurrentStage().Milestones.Count}");
        }

        private void SetupIcons()
        {
            int currentMilestone = RailService.Instance.CurrentMilestone.Value;
            int totalMilestones = RailService.Instance.GetCurrentStage().Milestones.Count;

            // Nếu đang ở milestone cuối thì ẩn middleIcon và middleReward
            if (isLastMilestone)
            {
                middleIcon.gameObject.SetActive(false);
            }
            else
            {
                middleIcon.gameObject.SetActive(true);
                // Icon giữa - tương ứng với milestone hiện tại
                SetIconBySkin(middleIcon, GetSkinForMilestone(currentMilestone, totalMilestones));

                // Setup reward cho mốc giữa (milestone hiện tại)
                if (middleReward != null)
                {
                    var currentMilestoneData = RailService.Instance.GetMilestone(currentMilestone);
                    middleReward.SetReward(currentMilestoneData.Rewards);
                }
            }

            // Icon cuối - tương ứng với milestone cuối
            SetIconBySkin(endIcon, GetSkinForMilestone(totalMilestones, totalMilestones));

            // Setup reward cho mốc cuối (milestone cuối cùng)
            if (endReward != null)
            {
                var lastMilestoneData = RailService.Instance.GetMilestone(totalMilestones);
                endReward.SetReward(lastMilestoneData.Rewards);
            }
        }

        private UIRailGiftBox.Skin GetSkinForMilestone(int milestoneIndex, int totalMilestones)
        {
            // Logic giống UIRailGiftBox
            if (milestoneIndex == totalMilestones)
                return UIRailGiftBox.Skin.Purple;
            else if (milestoneIndex % 2 == 1)
                return UIRailGiftBox.Skin.Blue;
            else
                return UIRailGiftBox.Skin.Red;
        }

        private void SetIconBySkin(Image icon, UIRailGiftBox.Skin skin)
        {
            Sprite sprite = null;

            switch (skin)
            {
                case UIRailGiftBox.Skin.Blue:
                    sprite = blueBoxSprite;
                    break;
                case UIRailGiftBox.Skin.Red:
                    sprite = redBoxSprite;
                    break;
                case UIRailGiftBox.Skin.Purple:
                    sprite = purpleBoxSprite;
                    break;
            }

            if (sprite != null && icon != null)
            {
                icon.sprite = sprite;
            }
        }

        private void SetupSlider()
        {
            var maxValue = GetMaxValue();
            slider.maxValue = maxValue;

            // Set giá trị ban đầu
            int lastToken = RailService.Instance.LastToken.Value;
            slider.value = Mathf.Min(lastToken, maxValue);

            // Ẩn tapTxt ban đầu
            if (tapTxt != null)
            {
                tapTxt.gameObject.SetActive(false);
                tapTxt.localScale = Vector3.zero;
            }

            tapButton.gameObject.SetActive(false);

            TrySlide().Forget();
        }

        private async UniTaskVoid TrySlide()
        {
            if (!RailService.Instance.IsJoinEvent()) return;

            int currentToken = RailService.Instance.GetToken();
            int itemRequire = RailService.Instance.GetCurrentMilestone().ItemRequire;

            // Luôn slide tới min(currentToken, itemRequire) - không slide quá mốc cần đạt
            int targetValue = Mathf.Min(currentToken, itemRequire);

            BlockPanel.Set(true);

            // Slide với tốc độ cố định (units per second)
            float slideSpeed = 100f; // Tốc độ cố định 100 units/giây
            await slider.DOValue(targetValue, slideSpeed).SetSpeedBased().SetEase(Ease.InOutSine);

            // Kiểm tra nếu đạt mốc
            if (currentToken >= itemRequire)
            {
                // Không set BlockPanel false ở đây vì HandleMilestoneReached sẽ xử lý
                await HandleMilestoneReached();
            }
            else
            {
                BlockPanel.Set(false);
                RailService.Instance.SyncToken();
            }
        }

        private async UniTask HandleMilestoneReached()
        {
            // BlockPanel đã được set true từ TrySlide
            await UniTask.Delay(200);

            // Slide out top & bot layout
            layoutAnim.SlideOut();

            await UniTask.Delay(500);

            BlockPanel.Set(false);

            // Set box hiện tại sang trạng thái đang đợi được mở (Idle2)
            conveyor.SetCurrentBoxWaitingToOpen();

            // Reset tap count cho box hiện tại
            var currentBox = conveyor.GetCurrentBox();
            if (currentBox != null)
            {
                currentBox.ResetTapCount();
            }

            // Hiện nút Tap và enable interaction
            tapButton.gameObject.SetActive(true);
            tapButton.interactable = true;
            tapButton.onClick.AddListener(() =>
            {
                HandleTapButton();
                // Không remove listener ở đây nữa vì cần tap 4 lần
            });

            // Active và scale tapTxt lên
            if (tapTxt != null)
            {
                tapTxt.gameObject.SetActive(true);
                tapTxt.localScale = Vector3.zero;
                tapTxt.DOScale(Vector3.one, 0.5f).SetEase(Ease.OutBack);
            }

            // Truyền callback để bên ngoài xử lý
            onMilestoneReachedComplete?.Invoke();
        }

        public async UniTask HandleTapButton()
        {
            var currentBox = conveyor.GetCurrentBox();
            if (currentBox == null) return;

            // Kill các delay cũ để tránh chạy Idle đè lên animation mới
            enableButtonTween?.Kill();
            playIdleTween?.Kill();

            // Disable button để chặn tap liên tục
            tapButton.gameObject.SetActive(false);

            // Animation scale nảy lên cho tap button/text
            if (tapTxt != null)
            {
                tapTxt.DOKill(); // Kill animation cũ nếu có
                tapTxt.DOPunchScale(Vector3.one * 0.2f, 0.3f, 5, 0.5f);
            }

            // Xử lý tap mới (4 lần)
            bool isFinalTap = currentBox.ProcessTap(out var charState, out var boxState);

            if (!isFinalTap) // Tap 1-3: Play animation rồi chuyển sang Idle
            {
                // Lấy thời gian animation character
                float charAnimDuration = character.GetTimeAnim(charState);
                float enableButtonTime = charAnimDuration * 0.2f; // Đợi 20% thời gian để enable button
                
                // Play character animation
                character.PlayAnim(charState, false);
                
                // Play box animation
                currentBox.PlayAnimation(boxState, false);

                // Delay ngắn: Enable lại button sau 0.2s để tap tiếp
                enableButtonTween = DOVirtual.DelayedCall(enableButtonTime, () =>
                {
                    tapButton.gameObject.SetActive(true);
                });

                // Delay dài: Chờ hết animation rồi chuyển sang Idle
                playIdleTween = DOVirtual.DelayedCall(charAnimDuration, () =>
                {
                    character.PlayAnim(currentBox.GetCharacterIdleState(), true);
                    currentBox.PlayAnimation(UIRailGiftBox.State.OpenBox_Idle, true);
                });
            }
            else // Tap 4: Mở hẳn box
            {
                // Remove listener sau tap cuối
                tapButton.onClick.RemoveAllListeners();

                // Scale tapTxt xuống
                if (tapTxt != null)
                {
                    await tapTxt.DOScale(Vector3.zero, 0.3f).SetEase(Ease.InBack).AsyncWaitForCompletion();
                    tapTxt.gameObject.SetActive(false);
                }

                // Ẩn nút tap
                tapButton.gameObject.SetActive(false);

                // Play animation mở hộp hoàn toàn
                await PlayOpenBoxAnimation(charState, boxState);
            }
        }

        private async UniTask PlayOpenBoxAnimation(UIRailCharacter.State characterState, UIRailGiftBox.State boxState)
        {
            int currentMilestone = RailService.Instance.CurrentMilestone.Value;
            int totalMilestones = RailService.Instance.GetCurrentStage().Milestones.Count;

            // Nhân vật và hộp quà cùng chạy animation final open (từ tap 4)
            character.PlayAnim(characterState, false);

            _ = DOVirtual.DelayedCall(character.GetTimeAnim(characterState) - 1f, () =>
            {
                character.PlayAnim(UIRailCharacter.State.AfterOpen_Forward_Idle, false);

                _ = DOVirtual.DelayedCall(character.GetTimeAnim(UIRailCharacter.State.AfterOpen_Forward_Idle), () =>
                {
                    character.PlayRandomIdle();
                });
            });

            // Gọi animation final open cho hộp quà hiện tại (boxState từ tap 4)
            conveyor.GetCurrentBox().PlayAnimation(boxState, false);

            // Play reward open sound (xé túi quà) sau một delay
            if (musicManager != null)
            {
                _ = DOVirtual.DelayedCall(rewardOpenSoundDelay, () =>
                {
                    musicManager.PlayRewardOpenSound();
                });
            }

            // Đợi tới frame mở quà (đúng thời điểm trong animation)
            await UniTask.Delay((int)(spawnRewardDelay * 1000));

            conveyor.GetCurrentBox().PlayOpenEffect();

            // Spawn và animate rewards vào grid layout
            await SpawnRewardsToGrid(currentMilestone);

            // Mark box hiện tại là đã mở (chuyển sang Idle_Open)
            conveyor.MarkCurrentBoxAsOpened();

            // Lưu rewards vào inventory và accumulated (ngay trước milestone++ để đảm bảo data sync)
            var milestoneData = RailService.Instance.GetMilestone(currentMilestone);
            if (milestoneData.Rewards != null && milestoneData.Rewards.resourceDatas != null)
            {
                // Add vào inventory ngay lập tức (persistent)
                var logData = new EarnResourceLogData
                {
                    spendType = "feature",
                    spendId = "rail"
                };
                SonatSystem.GetService<InventoryService>().AddReward(milestoneData.Rewards, logData);

                // Add vào accumulated để hiển thị popup sau
                RailService.Instance.AddAccumulatedReward(milestoneData.Rewards);
            }

            // Xử lý token sau khi mở quà
            int itemRequire = RailService.Instance.GetCurrentMilestone().ItemRequire;
            RailService.Instance.LastToken.Value = 0; // Reset LastToken về 0
            RailService.Instance.AddToken(-itemRequire); // Trừ token đã tiêu

            // Cập nhật milestone mới
            RailService.Instance.CurrentMilestone.Value++;

            // Kiểm tra xem còn milestone nào không ngay sau khi tăng
            if (RailService.Instance.CurrentMilestone.Value > totalMilestones)
            {
                // Đã hết milestone - hoàn thành stage, tăng stage và reset
                RailService.Instance.CompleteStage();

                // Slide in layout rồi đóng popup
                BlockPanel.Set(true);

                layoutAnim.SlideIn();

                await UniTask.Delay((int)(slideInDelay * 1000));

                BlockPanel.Set(false);

                // Gọi callback để xử lý đóng và mở lại popup
                onStageComplete?.Invoke();
                return;
            }

            // Còn milestone - tiếp tục animation
            // Cập nhật title
            SetupTitle();

            BlockPanel.Set(true);

            // Slide in top & bot layout
            layoutAnim.SlideIn();

            await UniTask.Delay((int)(slideInDelay * 1000));

            // Animation: Slider về 0 + middleIcon về start → SetupIcons() → endIcon về mid
            await AnimateIconsTransition();

            // Conveyor move tới hộp kế tiếp
            conveyor.SlideToBox(RailService.Instance.CurrentMilestone.Value - 1, true);

            //await UniTask.Delay(1000);

            BlockPanel.Set(false);

            SetupSlider();
        }


        private async UniTask SpawnRewardsToGrid(int milestoneIndex)
        {
            if (rewardContainer == null) return;

            // Lấy reward data từ milestone
            var milestoneData = RailService.Instance.GetMilestone(milestoneIndex);
            if (milestoneData.Rewards == null || milestoneData.Rewards.resourceDatas.Count == 0) return;

            // Lấy vị trí của gift box hiện tại (world position)
            var currentBox = conveyor.GetCurrentBox();
            if (currentBox == null) return;

            Vector3 giftBoxWorldPos = currentBox.GetOpenPosition();

            // Clear container trước
            poolingService.Instance.CleanContainer(rewardContainer);

            // Spawn các reward items
            List<UIRewardItem> rewardItems = new List<UIRewardItem>();
            foreach (var reward in milestoneData.Rewards.resourceDatas)
            {
                var rewardItem = poolingService.Instance.CreateObject<UIRewardItem>(rewardContainer);
                rewardItem.Init(reward.resource, reward.quantity);

                // Đặt vị trí ban đầu ở gift box (convert world to local)
                RectTransform rewardRect = rewardItem.GetComponent<RectTransform>();
                rewardRect.position = giftBoxWorldPos;
                rewardRect.localScale = Vector3.zero;

                rewardItems.Add(rewardItem);
            }

            // Force rebuild layout để lấy vị trí đích
            Canvas.ForceUpdateCanvases();
            UnityEngine.UI.LayoutRebuilder.ForceRebuildLayoutImmediate(rewardContainer.GetComponent<RectTransform>());

            // Đợi 1 frame để Unity hoàn thành việc layout
            await UniTask.Yield();

            // Lưu tất cả target positions trước (tránh set position nhiều lần trong loop)
            List<Vector3> targetPositions = new List<Vector3>();
            foreach (var rewardItem in rewardItems)
            {
                targetPositions.Add(rewardItem.GetComponent<RectTransform>().position);
            }

            // Reset tất cả về vị trí gift box
            foreach (var rewardItem in rewardItems)
            {
                rewardItem.GetComponent<RectTransform>().position = giftBoxWorldPos;
            }

            // Phase 1: Tất cả rewards jump cùng lúc từ hộp quà lên + scale từ 0 → 1
            List<UniTask> jumpTasks = new List<UniTask>();
            for (int i = 0; i < rewardItems.Count; i++)
            {
                var rewardRect = rewardItems[i].GetComponent<RectTransform>();
                var targetPos = targetPositions[i];

                // Jump và scale cùng lúc (không delay)
                var jumpTask = rewardRect.DOJump(targetPos, 1f, 1, 0.6f).SetEase(Ease.OutQuad).AsyncWaitForCompletion();
                var scaleTask = rewardRect.DOScale(Vector3.one, 0.6f).SetEase(Ease.OutBack).AsyncWaitForCompletion();

                jumpTasks.Add(jumpTask.AsUniTask());
                jumpTasks.Add(scaleTask.AsUniTask());
            }

            // Đợi tất cả rewards jump xong
            await UniTask.WhenAll(jumpTasks);

            // Phase 2: Đợi 1 khoảng nhỏ
            await UniTask.Delay(500);

            // Phase 3: Scale xuống về 0
            List<UniTask> scaleDownTasks = new List<UniTask>();
            foreach (var rewardItem in rewardItems)
            {
                var rewardRect = rewardItem.GetComponent<RectTransform>();
                var scaleDownTask = rewardRect.DOScale(Vector3.zero, 0.3f).SetEase(Ease.InBack).AsyncWaitForCompletion();
                scaleDownTasks.Add(scaleDownTask.AsUniTask());
            }

            // Đợi tất cả scale down xong
            await UniTask.WhenAll(scaleDownTasks);

            // Clean up
            poolingService.Instance.CleanContainer(rewardContainer);
        }

        private int GetMaxValue()
        {
            int itemRequire = RailService.Instance.GetCurrentMilestone().ItemRequire;

            // Nếu là mốc cuối: maxValue = itemRequire
            if (isLastMilestone)
                return itemRequire;

            // Nếu không phải mốc cuối: maxValue = itemRequire * 2
            return itemRequire * 2;
        }

        private async UniTask AnimateIconsTransition()
        {
            var middleRect = middleIcon.GetComponent<RectTransform>();
            var endRect = endIcon.GetComponent<RectTransform>();
            var sliderRect = slider.GetComponent<RectTransform>();

            // Lưu vị trí mid và end
            Vector2 midOriginalPosition = middleRect.anchoredPosition;
            Vector2 endOriginalPosition = endRect.anchoredPosition;

            // Tính vị trí start của slider (vị trí khi value = 0)
            // Giả sử slider fill từ trái sang phải, start position là phía trái
            float sliderWidth = sliderRect.rect.width;
            Vector2 startPosition = new Vector2(-sliderWidth / 2f, middleRect.anchoredPosition.y);

            // Step 1: Slider về 0 đồng thời middleIcon di chuyển về start (giữ sprite cũ)
            var sliderTween = slider.DOValue(0, iconMoveDuration).SetEase(Ease.InOutSine);
            var middleTween = middleRect.DOAnchorPos(startPosition, iconMoveDuration).SetEase(Ease.InOutSine);

            await sliderTween.AsyncWaitForCompletion();

            // Step 2: SetupIcons() để update sprites cho milestone mới
            SetupIcons();

            // Nếu sau khi setup là milestone cuối thì middleIcon đã bị ẩn, không cần animate nữa
            if (!isLastMilestone)
            {
                // Step 3: Reset middleIcon về vị trí end
                middleRect.anchoredPosition = endOriginalPosition;

                // Step 4: middleIcon (với sprite mới) di chuyển từ end về mid
                await middleRect.DOAnchorPos(midOriginalPosition, iconMoveDuration).SetEase(Ease.InOutSine).AsyncWaitForCompletion();
            }
        }
    }
}
