using System;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using Manager;
using SonatFramework.Systems;
using SonatFramework.Systems.ObjectPooling;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class UICollectEffectItemInGame : MonoBehaviour, IPoolingObject
{
    protected Vector3 startPosition;
    protected Vector3 targetPosition;
    protected float scale = 1;
    protected Action onCollect;

    [SerializeField] protected Image icon;
    [SerializeField] protected AnimationCurve moveXCurve, moveYCurve;
    [SerializeField] protected float delayMove = 0.4f;
    [SerializeField] protected float duration;
    [SerializeField] protected bool rotate;

    protected virtual void DOEffect()
    {
        transform.DOScale(scale, delayMove).SetEase(Ease.InOutSine).From(0.1f);
        transform.DOMoveX(targetPosition.x, duration).SetEase(moveXCurve).SetDelay(delayMove);
        transform.DOMoveY(targetPosition.y, duration).SetEase(moveYCurve).SetDelay(delayMove).OnComplete(() =>
        {
            onCollect?.Invoke();
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
        int id = (int)args[0];
        startPosition = (Vector3)args[1];
        targetPosition = (Vector3)args[2];
        if (args.Length > 3)
        {
            onCollect = (Action)args[3];
        }
        if (args.Length > 4)
        {
            scale = (float)args[4];
        }

        if (args.Length > 5)
        {
            delayMove = (float)args[5];
        }

        transform.position = startPosition;
        icon.SetSpriteAsync(PathManager.ItemSprite(id)).Forget();
        icon.SetNativeSize();
        icon.transform.localScale = new Vector3(scale, scale, 1);
        DOEffect();
    }

    public void OnReturnObj()
    {
        onCollect = null;
    }
}