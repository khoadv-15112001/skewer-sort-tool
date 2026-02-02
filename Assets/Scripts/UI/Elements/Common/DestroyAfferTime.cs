using System;
using UnityEngine;
using DG.Tweening;

public class DestroyAfterTime : MonoBehaviour
{
    [SerializeField] private float timeDelay = 1f;

    private Tween scaleTween;
    private Tween delayTween;

    private void OnEnable()
    {
        // Hủy các tween/delayedCall cũ nếu còn
        scaleTween?.Kill();
        delayTween?.Kill();

        // Reset scale
        transform.localScale = Vector3.zero;

        // Tween scale lên
        scaleTween = transform.DOScale(1, 0.4f).SetEase(Ease.OutBack);

        // Delay rồi scale xuống
        delayTween = DOVirtual.DelayedCall(timeDelay, () =>
        {
            scaleTween?.Kill();
            scaleTween = transform.DOScale(0, 0.4f)
                .SetEase(Ease.InBack)
                .OnComplete(() => gameObject.SetActive(false));
        });
    }

    private void OnDisable()
    {
        // Hủy tween khi object bị disable để tránh memory leak
        scaleTween?.Kill();
        delayTween?.Kill();
    }
}
