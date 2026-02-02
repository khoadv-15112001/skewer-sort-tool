using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using Sonat.Enums;
using SonatFramework.Systems;
using SonatFramework.Systems.ObjectPooling;
using TMPro;
using UnityEngine;

public class UIDoubleStarEffect : MonoBehaviour, IPoolingObject
{
    [SerializeField] private Transform icon;
    [SerializeField] private Transform target;
    [SerializeField] protected AnimationCurve moveXCurve, moveYCurve;
    [SerializeField] protected float duration = 0.75f;
    [SerializeField] protected float scaleDown = 0.25f;
    [SerializeField] protected bool rotate;

    public void Setup(Vector3 startPosition = default, Action action = null, float delay = 0)
    {
        ResetRect();
        icon.gameObject.SetActive(false);
        DOVirtual.DelayedCall(delay, () =>
        {
            icon.gameObject.SetActive(true);
            // effect jump
            icon.transform.position = startPosition;
            icon.transform.DOScale(scaleDown, duration).OnComplete(() =>
            {
                action?.Invoke();
                MySonatFramework.GetService<PoolingService>().ReturnObj(this);
            });

            icon.transform.DOJump(target.position, 1, 1, duration);

            if (rotate)
            {
                icon.transform.DORotate(new Vector3(0, 0, 360 * 3), 1f, RotateMode.FastBeyond360)
                     .SetEase(Ease.Linear); // Quay đều, không tăng giảm tốc độ
            }
        });

    }

    public void Setup()
    {
    }

    public void OnCreateObj(params object[] args)
    {
        transform.localScale = Vector3.one;
        icon.transform.localScale = Vector3.one;
        icon.transform.localPosition = Vector3.zero;
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
