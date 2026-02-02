using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using Sirenix.OdinInspector;

namespace GrillSort.Rail
{
    public class UIRailConveyor : MonoBehaviour
    {
        [SerializeField] private Transform conveyorContainer;
        [SerializeField] private UIRailGiftBox giftBoxPrefab;
        [SerializeField] private RawImage conveyorBelt;
        [SerializeField] private float spacing = 150f;
        [SerializeField] private float slideDuration = 0.5f;
        [SerializeField] private float uvScrollSpeed = 0.5f;

        private List<UIRailGiftBox> giftBoxes = new List<UIRailGiftBox>();
        private int currentBoxIndex = -1;
        private RailMusicManager musicManager;

        public void Setup(RailMusicManager railMusicManager = null)
        {
            musicManager = railMusicManager;
            ClearBoxes();
            CreateGiftBoxes();

            // Kiểm tra trạng thái join
            if (RailService.Instance.IsJoinEvent())
            {
                // Đã join - hiện hộp hiện tại ở giữa ngay lập tức (không animation)
                int currentMilestone = RailService.Instance.CurrentMilestone.Value;
                SlideToBox(currentMilestone - 1, false);
            }
            else
            {
                // Chưa join - đặt container ở ngoài màn hình bên phải
                var canvas = conveyorContainer.GetComponentInParent<Canvas>();
                var canvasWidth = canvas.GetComponent<RectTransform>().rect.width;
                var offsetToOffscreen = canvasWidth / 2f + spacing; // Đảm bảo hộp đầu tiên nằm ngoài màn hình

                conveyorContainer.localPosition = new Vector3(offsetToOffscreen, conveyorContainer.localPosition.y, conveyorContainer.localPosition.z);

                // Slide hộp 1 vào với animation
                SlideToBox(0, true);
            }
        }

        private void ClearBoxes()
        {
            foreach (var box in giftBoxes)
            {
                if (box != null)
                    Destroy(box.gameObject);
            }
            giftBoxes.Clear();
        }

        private void CreateGiftBoxes()
        {
            var stageData = UIRailController.StageData;
            if (stageData == null || stageData.Milestones == null) return;

            int milestoneCount = stageData.Milestones.Count;

            for (int i = 0; i < milestoneCount; i++)
            {
                var giftBox = Instantiate(giftBoxPrefab, conveyorContainer);
                giftBox.Setup(i + 1, milestoneCount);

                // Đặt vị trí ban đầu (tất cả ở bên phải)
                var rectTransform = giftBox.GetComponent<RectTransform>();
                rectTransform.anchoredPosition = new Vector2(spacing * i, 0);

                giftBoxes.Add(giftBox);
            }
        }

        [Button]
        public void SlideToBox(int boxIndex, bool withAnimation = true)
        {
            if (boxIndex < 0 || boxIndex >= giftBoxes.Count) return;

            currentBoxIndex = boxIndex;
            float targetX = -spacing * boxIndex;
            float currentX = conveyorContainer.localPosition.x;
            float distance = targetX - currentX;

            conveyorContainer.DOKill();

            if (withAnimation)
            {
                // Play openScene sound khi băng chuyền di chuyển
                if (musicManager != null)
                {
                    musicManager.PlayOpenSceneSound();
                }

                // Play animation Slide chỉ cho các hộp chưa mở
                foreach (var box in giftBoxes)
                {
                    if (!box.IsOpened())
                    {
                        box.PlayAnimation(UIRailGiftBox.State.Slide, true);
                    }
                }

                // Animate conveyor container
                conveyorContainer.DOLocalMoveX(targetX, slideDuration)
                    .SetEase(Ease.OutQuad)
                    .OnComplete(() =>
                    {
                        // Sau khi slide xong, chỉ set Idle cho các hộp chưa mở
                        foreach (var box in giftBoxes)
                        {
                            if (!box.IsOpened())
                            {
                                box.SetIdleNormal();
                            }
                        }
                    });

                // Animate conveyor belt UV
                if (conveyorBelt != null)
                {
                    float uvDistance = distance * uvScrollSpeed;
                    Rect currentUV = conveyorBelt.uvRect;
                    float targetUVX = currentUV.x - uvDistance / spacing;

                    DOTween.To(() => conveyorBelt.uvRect.x, x =>
                    {
                        Rect newUV = conveyorBelt.uvRect;
                        newUV.x = x;
                        conveyorBelt.uvRect = newUV;
                    }, targetUVX, slideDuration)
                    .SetEase(Ease.OutQuad);
                }
            }
            else
            {
                // Không animation, set vị trí trực tiếp
                conveyorContainer.localPosition = new Vector3(targetX, conveyorContainer.localPosition.y, conveyorContainer.localPosition.z);

                // Set Idle chỉ cho các hộp chưa mở
                foreach (var box in giftBoxes)
                {
                    if (!box.IsOpened())
                    {
                        box.SetIdleNormal();
                    }
                }
            }
        }

        public void SlideToNextBox()
        {
            SlideToBox(currentBoxIndex + 1);
        }

        public void SlideToPreviousBox()
        {
            SlideToBox(currentBoxIndex - 1);
        }

        public void SlideToCurrentMilestone()
        {
            int currentMilestone = RailService.Instance.CurrentMilestone.Value;
            SlideToBox(currentMilestone - 1);
        }

        public void PlayCurrentBoxOpenAnimation()
        {
            if (currentBoxIndex >= 0 && currentBoxIndex < giftBoxes.Count)
            {
                var currentBox = giftBoxes[currentBoxIndex];
                currentBox.PlayAnimation(UIRailGiftBox.State.OpenBox, false);
            }
        }

        public UIRailGiftBox GetCurrentBox()
        {
            if (currentBoxIndex >= 0 && currentBoxIndex < giftBoxes.Count)
            {
                return giftBoxes[currentBoxIndex];
            }
            return null;
        }

        public void MarkCurrentBoxAsOpened()
        {
            var currentBox = GetCurrentBox();
            if (currentBox != null)
            {
                currentBox.MarkAsOpened();
            }
        }

        public void SetCurrentBoxWaitingToOpen()
        {
            var currentBox = GetCurrentBox();
            if (currentBox != null)
            {
                currentBox.SetWaitingToOpen();
            }
        }

        private void OnDestroy()
        {
            conveyorContainer.DOKill();
        }
    }
}
