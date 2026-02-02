using I2.Loc;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using Cysharp.Threading.Tasks;
using SonatFramework.Scripts.UIModule;
using SonatFramework.Scripts.Utils;

namespace GrillSort.Rail
{
    public class UIRailController : MonoBehaviour
    {
        [SerializeField] private UIRailMilestoneTitleController milestoneTitleController;
        [SerializeField] private UIRailMilestoneProgress milestoneProgress;
        [SerializeField] private UIRailCharacter character;
        [SerializeField] private UIRailLayoutAnim layoutAnim;
        [SerializeField] private UIRailConveyor conveyor;

        [SerializeField] private Image backgroundImg;
        [SerializeField] private Button startEventButton;
        [SerializeField] private TMP_Text timeTxt;

        private Button closeButton;
        private RailMusicManager musicManager;
        private Action onStageComplete;

        public static int StageId;
        public static RailConfig.Stage StageData;

        public void Setup(Button closeBtn, RailMusicManager railMusicManager, Action onStageCompleteCallback = null)
        {
            closeButton = closeBtn;
            musicManager = railMusicManager;
            onStageComplete = onStageCompleteCallback;

            SetupData();
            SetupBackground();
            SetupTitle();
            SetupProgress();
            SetupJoinState();
            SetupConveyor(); // Gọi sau SetupJoinState để biết trạng thái

            OnTick();
            RailService.OnTick += OnTick;
        }

        private void OnDestroy()
        {
            RailService.OnTick -= OnTick;
        }

        private void SetupJoinState()
        {
            bool isJoined = RailService.Instance.IsJoinEvent();
            bool hasEverJoined = RailService.Instance.HasEverJoined.BoolValue;

            if (isJoined)
            {
                // Đã join - ẩn button start, hiện character
                startEventButton.gameObject.SetActive(false);
                character.gameObject.SetActive(true);
                character.PlayRandomIdle();
            }
            else
            {
                // Chưa join - hiện button start, ẩn botLayout và character
                startEventButton.gameObject.SetActive(true);
                character.gameObject.SetActive(false);
                layoutAnim.HideBotInstant();

                // Chỉ lần đầu tiên trong đời mới ẩn nút close
                if (!hasEverJoined)
                {
                    closeButton.gameObject.SetActive(false);
                }

                // Đăng ký sự kiện click button
                startEventButton.onClick.RemoveAllListeners();
                startEventButton.onClick.AddListener(() =>
                {
                    OnStartEventButtonClicked();
                });
            }
        }

        private async UniTaskVoid OnStartEventButtonClicked()
        {
            bool isFirstTimeJoin = !RailService.Instance.HasEverJoined.BoolValue;

            if (isFirstTimeJoin)
            {
                var popup = PanelManager.Instance.OpenPanelByName<BasePanel>("PopupRailTut");

                await UniTask.WaitUntil(() => popup == null);
            }

            startEventButton.interactable = false;
            RailService.Instance.JoinEvent();

            // Chuyển sang startConnect music
            if (musicManager != null)
            {
                musicManager.PlayStartConnect();
            }

            // Block interaction
            BlockPanel.Set(true);

            // Animation slide button start xuống
            var buttonRect = startEventButton.GetComponent<RectTransform>();
            var canvasHeight = buttonRect.GetComponentInParent<Canvas>().GetComponent<RectTransform>().rect.height;
            var slideDistance = canvasHeight / 2f + buttonRect.rect.height + Mathf.Abs(buttonRect.anchoredPosition.y);

            buttonRect.DOKill();
            _ = buttonRect.DOAnchorPosY(buttonRect.anchoredPosition.y - slideDistance, 0.5f)
                .SetEase(Ease.InBack)
                .OnComplete(() =>
                {
                    startEventButton.gameObject.SetActive(false);

                    // SlideOut cả top và bot layout
                    layoutAnim.SlideOut();

                    // Sau khi slide out xong, character walk vào
                    DOVirtual.DelayedCall(0.5f, () =>
                    {
                        // Play start scene sound khi character walk vào
                        if (musicManager != null)
                        {
                            musicManager.PlayStartSceneSound();
                        }

                        character.WalkInFromLeft(() =>
                        {
                            // Sau khi character walk in và wave xong, slide in layout
                            layoutAnim.SlideIn();

                            // Sau khi slide in xong
                            DOVirtual.DelayedCall(0.5f, () =>
                            {
                                // Unblock interaction
                                BlockPanel.Set(false);

                                // Chỉ lần đầu tiên mới scale nút close ra
                                if (isFirstTimeJoin)
                                {
                                    closeButton.gameObject.SetActive(true);
                                    closeButton.transform.localScale = Vector3.zero;
                                    closeButton.transform.DOScale(Vector3.one, 0.3f).SetEase(Ease.OutBack);
                                }
                            });
                        });
                    });
                });
        }

        private void SetupData()
        {
            StageId = RailService.Instance.CurrentStage.Value;
            StageData = RailService.Instance.GetCurrentStage();
        }

        private void SetupBackground()
        {
            backgroundImg.sprite = StageData.GetBackgroundSprite();
        }

        private void SetupTitle()
        {
            milestoneTitleController.Setup();
        }

        private void SetupConveyor()
        {
            conveyor.Setup(musicManager);
        }

        private void SetupProgress()
        {
            milestoneProgress.SetCloseButton(closeButton);
            milestoneProgress.SetOnStageComplete(onStageComplete);
            milestoneProgress.SetMusicManager(musicManager);
            milestoneProgress.Setup(() =>
            {
                // Callback khi milestone reached hoàn thành
                // Có thể làm gì đó tiếp theo ở đây
            });
        }

        private void OnTick()
        {
            timeTxt.text = SonatUtils.FormatTimeSmart(RailService.Instance.GetRemainingEventTime());
        }

    }
}