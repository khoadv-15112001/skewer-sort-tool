using System;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using Sonat.Enums;
using SonatFramework.Scripts.UIModule;
using SonatFramework.Systems;
using SonatFramework.Systems.ObjectPooling;
using UnityEngine;
using Random = UnityEngine.Random;

public class CollectEffectMultiple : SonatCollectEffect
{
    public string collectEffectName = "CollectResourceMultipleItem";
    public int count = 10;
    public float radius = 1.25f;
    public override void Collect(GameResource resource, int quantity, Vector3 startPos, Vector3 endPos, Action callback)
    {
        Spawn(resource, quantity, startPos, endPos, callback).Forget();
    }

    private async UniTaskVoid Spawn(GameResource resource, int quantity, Vector3 startPos, Vector3 endPos, Action callback)
    {
        for (int i = 0; i < count; i++)
        {
            Vector3 ran = Random.insideUnitCircle * radius;
            Vector3 pos = startPos + ran;
            var item = await SonatSystem.GetService<PoolingServiceAsync>().CreateAsync<UICollectEffectItemMultiple>(collectEffectName, PanelManager.Instance.transform, resource,
                quantity, startPos, pos, endPos, callback);
            callback = null;
        }
    }
}
