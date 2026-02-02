using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEditor;
using UnityEngine;

public class BoosterAnim : EffectPoolBase
{
    [SerializeField] protected AnimationCurve moveXCurve;
    [SerializeField] protected AnimationCurve moveYCurve;
    [SerializeField] protected float duration = 0.5f;

    public virtual void SetData(Vector3 position)
    {
        transform.position = position;
        transform.DOLocalMoveX(0, duration).SetEase(moveXCurve);
        transform.DOLocalMoveY(0, duration).SetEase(moveYCurve);
    }
}
