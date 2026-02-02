using DG.Tweening;
using UnityEngine;

namespace GrillSort.QuestEvent
{
    public class UIElementProgress : MonoBehaviour
    {
        [SerializeField] private RectTransform imgProgressBefore;
        [SerializeField] private RectTransform imgProgressAfter;
        [SerializeField] private GameObject objProgressBefore;
        [SerializeField] private GameObject objProgressAfter;

        [SerializeField] private Vector2 defaultSize;

        public void SetValue(float value)
        {
            imgProgressBefore.sizeDelta = new Vector2(defaultSize.x, defaultSize.y * Mathf.Min(0.5f, value));
            imgProgressAfter.sizeDelta = new Vector2(defaultSize.x, defaultSize.y * Mathf.Max(0, value - 0.5f));
        }

        public void PlayTween(float beginValue, float endValue, float delay)
        {
            imgProgressBefore.DOKill();
            imgProgressAfter.DOKill();

            imgProgressBefore.sizeDelta = new Vector2(defaultSize.x, defaultSize.y * Mathf.Min(0.5f, beginValue));
            imgProgressAfter.sizeDelta = new Vector2(defaultSize.x, defaultSize.y * Mathf.Max(0, beginValue - 0.5f));

            var newSizeBefore = new Vector2(defaultSize.x, defaultSize.y * Mathf.Min(0.5f, endValue));
            var newSizeAfter = new Vector2(defaultSize.x, defaultSize.y * Mathf.Max(0, endValue - 0.5f));

            var duration = 0.25f;
            if (beginValue >= 0.5f)
            {
                imgProgressAfter.DOSizeDelta(newSizeAfter, duration).SetEase(Ease.Linear).SetDelay(delay);
            }
            else
            {
                imgProgressBefore.DOSizeDelta(newSizeBefore, duration).SetEase(Ease.Linear).SetDelay(delay).OnComplete(() =>
            {
                imgProgressAfter.DOSizeDelta(newSizeAfter, duration).SetEase(Ease.Linear);
            });
            }

        }

        public void HideProgressBefore()
        {
            objProgressBefore.gameObject.SetActive(false);
        }

        public void HideProgressAfter()
        {
            objProgressAfter.gameObject.SetActive(false);
        }
    }
}

