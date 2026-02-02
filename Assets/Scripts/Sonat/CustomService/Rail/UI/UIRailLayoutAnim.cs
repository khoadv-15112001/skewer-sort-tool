using UnityEngine;
using DG.Tweening;
using Sirenix.OdinInspector;

namespace GrillSort.Rail
{
    public class UIRailLayoutAnim : MonoBehaviour
    {
        [SerializeField] private RectTransform topLayout;
        [SerializeField] private RectTransform botLayout;

        [SerializeField] private float duration = 0.45f;
        [SerializeField] private Ease ease = Ease.InOutQuad;

        private Vector2 topOriginPos;
        private Vector2 botOriginPos;
        private float halfScreenHeight;

        private void Awake()
        {
            topOriginPos = topLayout.anchoredPosition;
            botOriginPos = botLayout.anchoredPosition;

            var parentRect = topLayout.GetComponentInParent<Canvas>().GetComponent<RectTransform>();
            halfScreenHeight = parentRect.rect.height / 2f;
        }

        [Button] 
        public void SlideIn()
        {
            SlideInTop();
            SlideInBot();
        }

        [Button]
        public void SlideOut()
        {
            SlideOutTop();
            SlideOutBot();
        }

        public void SlideInTop()
        {
            topLayout.DOKill();
            topLayout.anchoredPosition = topOriginPos + new Vector2(0, halfScreenHeight);
            topLayout.DOAnchorPos(topOriginPos, duration).SetEase(ease);
        }

        public void SlideOutTop()
        {
            topLayout.DOKill();
            topLayout.DOAnchorPos(topOriginPos + new Vector2(0, halfScreenHeight), duration).SetEase(ease);
        }

        public void SlideInBot()
        {
            botLayout.DOKill();
            botLayout.anchoredPosition = botOriginPos - new Vector2(0, halfScreenHeight);
            botLayout.DOAnchorPos(botOriginPos, duration).SetEase(ease);
        }

        public void SlideOutBot()
        {
            botLayout.DOKill();
            botLayout.DOAnchorPos(botOriginPos - new Vector2(0, halfScreenHeight), duration).SetEase(ease);
        }

        public void ShowBotInstant()
        {
            botLayout.DOKill();
            botLayout.anchoredPosition = botOriginPos;
        }

        public void HideBotInstant()
        {
            botLayout.DOKill();
            botLayout.anchoredPosition = botOriginPos - new Vector2(0, halfScreenHeight);
        }
    }
}
