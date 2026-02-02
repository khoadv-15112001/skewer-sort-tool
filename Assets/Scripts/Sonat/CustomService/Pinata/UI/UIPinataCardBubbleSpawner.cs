using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

namespace GrillSort.Pinata
{
    public class UIPinataCardBubbleSpawner : MonoBehaviour
    {
        [SerializeField] private UIPinataCardBubble bubblePrefab;

        [Header("Spawn Radius")]
        [SerializeField] private float radius = 150f;
        [SerializeField] private bool spawnInsideCircle = true; // nếu true: trong vùng tròn, false: trên biên

        [Header("Animation Settings")]
        [SerializeField] private float appearScaleFrom = 0.7f;
        [SerializeField] private float appearDuration = 0.3f;
        [SerializeField] private float moveUpDistance = 200f;
        [SerializeField] private float moveDuration = 1.5f;
        [SerializeField] private Ease appearEase = Ease.OutBack;
        [SerializeField] private Ease moveEase = Ease.OutQuad;

        public void Spawn(int star)
        {
            // --- Tạo prefab ---
            var bubble = Instantiate(bubblePrefab, transform);
            bubble.BindData(star);

            // --- Random vị trí trong vòng tròn ---
            Vector2 randomPos = GetRandomPositionInCircle(radius, spawnInsideCircle);

            var rect = bubble.GetComponent<RectTransform>();
            rect.anchoredPosition = randomPos;

            // --- Reset trạng thái ban đầu ---
            rect.localScale = Vector3.one * appearScaleFrom;

            var canvasGroup = bubble.GetComponent<CanvasGroup>();
            if (canvasGroup == null)
                canvasGroup = bubble.gameObject.AddComponent<CanvasGroup>();
            canvasGroup.alpha = 1f;

            // --- Hiệu ứng xuất hiện + bay lên ---
            Sequence seq = DOTween.Sequence();

            // scale 0.7 → 1
            seq.Append(rect.DOScale(1f, appearDuration).SetEase(appearEase));

            // move lên và fade dần
            float randomY = moveUpDistance * Random.Range(0.9f, 1.1f);
            float randomDuration = moveDuration * Random.Range(0.9f, 1.1f);

            seq.Append(
                DOTween.Sequence()
                    .Append(rect.DOLocalMoveY(rect.localPosition.y + randomY, randomDuration).SetEase(moveEase))
                    .Join(canvasGroup.DOFade(0f, randomDuration))
            );

            seq.OnComplete(() => Destroy(bubble.gameObject));
        }

        private Vector2 GetRandomPositionInCircle(float r, bool inside)
        {
            float angle = Random.Range(0f, Mathf.PI * 2f);
            float distance = inside ? Random.Range(0f, r) : r;
            return new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * distance;
        }
    }
}
