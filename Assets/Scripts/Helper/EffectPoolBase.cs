using DG.Tweening;
using SonatFramework.Scripts.Utils;
using SonatFramework.Systems;
using SonatFramework.Systems.ObjectPooling;
using UnityEngine;

public class EffectPoolBase : MonoBehaviour, IPoolingObject
{
    public float timeLive = 0;

    public virtual void Setup()
    {
    }

    public virtual void OnCreateObj(params object[] args)
    {
        transform.localScale = Vector3.one;
        if (timeLive > 0)
            SonatUtils.DelayCall(timeLive, () => Destroy(), this);
    }

    public virtual void OnReturnObj()
    {
    }

    public void Destroy()
    {
        SonatSystem.GetService<PoolingService>().ReturnObj(this);
    }
}