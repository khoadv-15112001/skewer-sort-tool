using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using SonatFramework.Scripts.Utils;
using SonatFramework.Systems.EventBus;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public class BoosterMagicKeyAnim : BoosterAnim
{
    [SerializeField] private float idleDelay = 0.5f;
    [SerializeField] private float moveDuration = 0.5f;
    [SerializeField] private Transform idlePostion;

    [SerializeField] private Image keyImage;
    [SerializeField] private float upScale = 1.5f;
    [SerializeField] private float upDuration = 0.5f;
    [SerializeField] private float downScale = 0.5f;
    [SerializeField] private AnimationCurve upScaleCurve;
    [SerializeField] private AnimationCurve downScaleCurve;

    public override void SetData(Vector3 position)
    {
        base.SetData(position);

        SonatUtils.DelayCall(idleDelay, () =>
        {
            keyImage.transform.DOScale(upScale, upDuration).SetEase(upScaleCurve);
            transform.DOMoveX(idlePostion.position.x, moveDuration).SetEase(moveXCurve).SetDelay(upDuration);
            transform.DOMoveY(idlePostion.position.y, moveDuration).SetEase(moveYCurve).SetDelay(upDuration);
            keyImage.transform.DOScale(downScale, moveDuration).SetEase(downScaleCurve).SetDelay(upDuration);
        }, this);

        //PopupGuideMagicKey.onClose += OnClose;
    }

    private void OnClose()
    {
        Destroy();
    }

    public override void OnReturnObj()
    {
        base.OnReturnObj();
        //PopupGuideMagicKey.onClose -= OnClose;
        transform.DOKill();
        keyImage.transform.DOKill();
        keyImage.transform.localScale = Vector3.one;
        transform.position = Vector3.zero;
    }
}
