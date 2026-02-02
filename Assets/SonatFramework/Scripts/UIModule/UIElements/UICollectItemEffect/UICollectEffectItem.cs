using System;
using DG.Tweening;
using Sonat.Enums;
using SonatFramework.Systems;
using SonatFramework.Systems.ObjectPooling;
using UnityEngine;
using Random = UnityEngine.Random;

public class UICollectEffectItem : MonoBehaviour, IPoolingObject
{
    protected Vector3 startPosition;
    protected Vector3 targetPosition;

    protected Action onCollect;

    [SerializeField] protected UIResourceItem uiResourceItem;
    [SerializeField] protected AnimationCurve moveXCurve, moveYCurve;
    [SerializeField] protected float duration;
    [SerializeField] protected float speed;
    [SerializeField] protected bool rotate;

    protected virtual void DOEffect()
    {
        transform.DOMoveX(targetPosition.x, duration).SetEase(moveXCurve);
        transform.DOMoveY(targetPosition.y, duration).SetEase(moveYCurve).OnComplete(() =>
        {
            try
            {
                onCollect?.Invoke();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
            
            SonatSystem.GetService<PoolingServiceAsync>().ReturnObj(this);
        });
        if (rotate)
        {
            transform.Rotate(Vector3.forward, Random.Range(-360, 360));
            transform.DORotate(Vector3.zero, duration, RotateMode.FastBeyond360).SetEase(Ease.OutBounce);
        }
    }

    public void Setup()
    {
    }

    public virtual void OnCreateObj(params object[] args)
    {
        GameResource resource = (GameResource)args[0];
        int quantity = (int)args[1];
        startPosition = (Vector3)args[2];
        targetPosition = (Vector3)args[3];
        if (args.Length > 4)
            onCollect = (Action)args[4];
        else
        {
            onCollect = null;
        }

        if (args.Length > 5)
            transform.localScale = Vector3.one * (float)args[5];
        else
        {
            transform.localScale = Vector3.one;
        }

        transform.position = startPosition;
        uiResourceItem.SetData(resource, quantity);
        DOEffect();
    }

    public void OnReturnObj()
    {
        onCollect = null;
    }

    private void OnDisable()
    {
        onCollect = null;
    }
}