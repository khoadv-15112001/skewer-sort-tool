using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using SonatFramework.Scripts.Utils;
using SonatFramework.Systems.ObjectPooling;
using UnityEngine;

public class UIBoosterMagicKeyEffect : MonoBehaviour, IPoolingObject
{
    [SerializeField] private Transform key;
    [SerializeField] private float moveDuration = 0.5f;
    [SerializeField] private float offsetX = 0.5f;
    [SerializeField] private float offsetY = 0f;
    [SerializeField] private AnimationCurve moveCurveX;
    [SerializeField] private AnimationCurve moveCurveY;
    [SerializeField] private float downScaleDuration = 0.3f;

    [Header("Rotate")]
    [SerializeField] private float targetRotationZ = 90f;
    [SerializeField] private float rotateDuration = 0.5f;
    [SerializeField] private Ease rotateEase;

    [Header("Shake")]
    [SerializeField] private float shakeDuration = 0.5f;
    [SerializeField] private float shakeAngle = 10f;
    [SerializeField] private int shakeVibrato = 10;
    [SerializeField] private float shakeRandomness = 90f;

    private Vector3 originPos;

    private void Awake()
    {
        originPos = key.transform.position;
    }

    public void SetData(Vector3 targePos, Action callback)
    {
        var seq = DOTween.Sequence();
        seq.Join(key.transform.DOMoveX(targePos.x - offsetX, moveDuration).SetEase(moveCurveX));
        seq.Join(key.transform.DOMoveY(targePos.y - offsetY, moveDuration).SetEase(moveCurveY));

        var targetRotation = Quaternion.Euler(0, 0, targetRotationZ);
        seq.Join(key.transform.DORotateQuaternion(targetRotation, rotateDuration).SetEase(rotateEase));

        // Shake nhẹ theo trục Z (rung góc)
        seq.Append(key.transform.DOShakeRotation(shakeDuration, new Vector3(0, 0, shakeAngle), shakeVibrato, shakeRandomness));

        seq.AppendCallback(() =>
        {
            callback?.Invoke();
            key.transform.DOScale(0f, downScaleDuration).SetEase(Ease.InBack).OnComplete(() =>
            {
                MySonatFramework.poolingService.ReturnObj(this);
            });
        });
    }

    public void OnCreateObj(params object[] args)
    {

    }

    public void OnReturnObj()
    {
        key.transform.position = originPos;
        key.transform.rotation = Quaternion.identity;
        key.transform.localScale = Vector3.one;
    }

    public void Setup()
    {
        key.GetComponent<Canvas>().sortingLayerName = "UI_Top";
        key.GetComponent<Canvas>().sortingOrder = 51;
    }
}
