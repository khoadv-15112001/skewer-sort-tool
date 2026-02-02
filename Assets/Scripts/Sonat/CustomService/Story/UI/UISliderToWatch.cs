using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;
using DG.Tweening;
using Spine.Unity;
using UnityEngine.UI;
using Cysharp.Threading.Tasks;
using SonatFramework.Scripts.UIModule;

namespace GrillSort.Story
{
    public class UISlideToWatch : MonoBehaviour, IDragHandler, IPointerDownHandler, IPointerUpHandler
    {
        [SerializeField] private CanvasGroup canvasGroup;
        [SerializeField] private RectTransform handle;
        [SerializeField] private RectTransform slideArea;
        [SerializeField] private TMP_Text label;
        [SerializeField] private CanvasGroup title;
        [SerializeField] private float triggerPercent = 0.8f; // % độ dài cần trượt để kích hoạt
        [SerializeField] private float resetDuration = 0.25f;

        [SerializeField] private AudioClip slideAudioClip;
        [SerializeField] private AudioClip spinAudioClip;

        [SerializeField] private SkeletonGraphic cardAnim;
        [SerializeField] private Image darkPanel;
        public float timePlayCardAnim;

        private PopupStoryCard popupStoryCard;

        private float slideWidth;
        private Vector2 startPos;
        private bool isTriggered;
        private bool isPlayingSlideAudio;

        void Start()
        {
            startPos = handle.anchoredPosition;
            slideWidth = slideArea.rect.width - handle.rect.width;

            popupStoryCard = PanelManager.Instance.GetPanel<PopupStoryCard>();
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            if (isTriggered) return;

            label.DOKill();
            label.DOFade(0f, resetDuration);

            cardAnim.AnimationState.SetAnimation(0, "Slide", true);
        }

        [SerializeField] private float speedThreshold = 500f; // tốc độ px/s cần đạt để phát âm thanh
        [SerializeField] private float slideAudioCooldown = 0.3f; // tránh spam âm thanh

        public void OnDrag(PointerEventData eventData)
        {
            if (isTriggered) return;

            Vector2 pos = handle.anchoredPosition;
            pos.x += eventData.delta.x;
            pos.x = Mathf.Clamp(pos.x, 0, slideWidth);
            handle.anchoredPosition = pos;

            float progress = pos.x / slideWidth;

            float dragSpeed = Mathf.Abs(eventData.delta.x / Time.deltaTime);

            if (dragSpeed >= speedThreshold && !isPlayingSlideAudio)
            {
                isPlayingSlideAudio = true;
                MySonatFramework.audioService.PlayAudio("", slideAudioClip);

                DOVirtual.DelayedCall(slideAudioCooldown, () =>
                {
                    isPlayingSlideAudio = false;
                });
            }

            if (progress >= triggerPercent)
            {
                isTriggered = true;
                OnSlideComplete();
            }
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            if (isTriggered) return;

            float progress = handle.anchoredPosition.x / slideWidth;
            if (progress >= triggerPercent)
            {
                isTriggered = true;
                OnSlideComplete();
            }
            else
            {
                ResetHandle();
            }
        }

        private void ResetHandle()
        {
            handle.DOAnchorPos(startPos, resetDuration).SetEase(Ease.OutBack);

            label.DOKill();
            label.DOFade(1f, resetDuration);

            cardAnim.Skeleton.SetToSetupPose();
            cardAnim.AnimationState.ClearTracks();
            cardAnim.AnimationState.Update(0);
        }

        private void OnSlideComplete()
        {
            handle.DOAnchorPos(new Vector2(slideWidth, startPos.y), 0.2f).SetEase(Ease.OutQuad);
            canvasGroup.DOFade(0f, 0.4f);
            title.DOFade(0f, 0.4f);

            PlayCardAnim().Forget();
        }

        private async UniTask PlayCardAnim()
        {
            MySonatFramework.audioService.PlayAudio("", spinAudioClip);

            cardAnim.AnimationState.SetAnimation(0, "Spin", false);

            await UniTask.WaitForSeconds(timePlayCardAnim);

            await darkPanel.DOFade(1f, 0.8f);

            //StoryService.Instance.MoveToStory(popupStoryCard.Story);

            //MySonatFramework.inventoryService.AddReward(popupStoryCard.Story.GetData().RewardData, new() { spendType = "feature", spendId = "story" });

            PopupVideoStory.OnFinish += OnFinish;

            TabHome.OnSlideOut?.Invoke();

            StoryService.Instance.PlayVideoByID(popupStoryCard.Story.GetData().Id, OnFailedLoadVideo);
        }

        private void OnFinish()
        {
            popupStoryCard.CloseImmediately();
        }

        private void OnFailedLoadVideo()
        {
            OnFinish();
        }
    }
}
