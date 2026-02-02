using System;
using DG.Tweening;
using Sonat.Enums;
using SonatFramework.Scripts.UIModule.SpriteService;
using SonatFramework.Scripts.UIModule.UIElements;
using SonatFramework.Systems.ObjectPooling;
using UnityEngine;

public class UIPreBoosterEffect : MonoBehaviour, IPoolingObject
{
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private Transform icon;
    [SerializeField] private FixedImageRatio iconImage;
    [SerializeField] private Transform target;
    [SerializeField] protected AnimationCurve moveXCurve, moveYCurve;
    [SerializeField] protected float duration = 0.75f;
    [SerializeField] protected bool rotate;
    [SerializeField] protected float durationScaleUp = 0.3f;
    [SerializeField] protected float durationScaleDown = 0.2f;
    [SerializeField] protected float strength = 10;
    [SerializeField] protected int vibrato = 15;
    [SerializeField] protected float durationFadeOut = 0.7f;
    [SerializeField] protected float scaleUp = 1.2f;
    [SerializeField] protected float delay = 0.5f;

    public static float PreBoosterEffectTime = 1.5f;

    public void Setup(Vector3 startPosition = default, Action action = null, float delayNextEffect = 0)
    {
        ResetRect();
        // // effect jump
        // icon.transform.position = startPosition;
        // icon.transform.DOScale(0.1f, duration).OnComplete(() =>
        // {
        //     action?.Invoke();
        //     SonatSystem.GetService<PoolingService>().ReturnObj(this);
        // });

        // icon.transform.DOJump(target.position, 1, 1, duration);

        // if (rotate)
        // {
        //     icon.transform.DORotate(new Vector3(0, 0, 360 * 3), 1f, RotateMode.FastBeyond360)
        //          .SetEase(Ease.Linear); // Quay đều, không tăng giảm tốc độ
        // }

        // Reset trạng thái ban đầu
        icon.transform.position = startPosition;
        canvasGroup.GetComponent<CanvasGroup>().alpha = 1f;
        icon.localScale = Vector3.one;

        // Chuỗi hiệu ứng
        Sequence seq = DOTween.Sequence();


        // 1. Rung lắc (shake) nhẹ trong 0.5s
        icon.DOShakePosition(duration, strength: new Vector3(strength, strength, 0), vibrato: vibrato, randomness: 90, snapping: false, fadeOut: true).SetDelay(delay);

        // 2. Nảy lên (bounce scale)
        seq.SetDelay(delay);
        seq.Append(icon.DOScale(scaleUp, durationScaleUp).SetEase(Ease.OutBack));
        seq.Append(icon.DOScale(1f, durationScaleDown).SetEase(Ease.InOutSine));

        // 3. Giữ 1 chút rồi fade out
        if (durationFadeOut > 0)
            seq.Append(canvasGroup.DOFade(0f, durationFadeOut));

        seq.AppendInterval(delayNextEffect * 1.0f / 1000);
        // Tùy chọn: Destroy sau khi xong
        seq.OnComplete(() =>
        {
            action?.Invoke();
            MySonatFramework.GetService<PoolingService>().ReturnObj(this);
        });

    }

    public void Setup()
    {
    }

    public void OnCreateObj(params object[] args)
    {
        icon.transform.localScale = Vector3.one;
        icon.transform.localPosition = Vector3.zero;

        var resource = (GameResource)args[0];
        iconImage.SetSprite(MySonatFramework.GetService<SpriteAtlasService>().GetSprite($"ico_{resource}"));
    }

    public void OnReturnObj()
    {

    }

    public void ResetRect()
    {
        var rect = GetComponent<RectTransform>();

        // Đặt anchors full stretch (chiếm toàn bộ parent)
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;

        // Đặt offsets (khoảng cách với viền parent) về 0
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;

        // Quan trọng: reset position & sizeDelta để tránh bị lệch
        rect.anchoredPosition = Vector2.zero;
        rect.sizeDelta = Vector2.zero;

        // Reset pivot, rotation, scale
        rect.pivot = new Vector2(0.5f, 0.5f);
        target.localRotation = Quaternion.identity;
        target.localScale = Vector3.one;
    }
}
