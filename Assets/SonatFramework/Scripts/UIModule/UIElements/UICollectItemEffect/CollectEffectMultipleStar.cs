using System;
using Cysharp.Threading.Tasks;
using Sonat.Enums;
using SonatFramework.Scripts.UIModule;
using SonatFramework.Systems;
using SonatFramework.Systems.ObjectPooling;
using UnityEngine;
using Random = UnityEngine.Random;

public class CollectEffectMultipleStar : SonatCollectEffect
{
    public string collectEffectName = "CollectResourceMultipleStar";
    public float radius = 0.65f;
    public bool multipleCallback;

    public override void Collect(GameResource resource, int quantity, Vector3 startPos, Vector3 endPos, Action callback)
    {
        Spawn(resource, quantity, startPos, endPos, callback).Forget();
    }

    private async UniTaskVoid Spawn(GameResource resource, int quantity, Vector3 startPos, Vector3 endPos, Action onComplete)
    {
        for (int i = 0; i < quantity; i++)
        {
            Vector3 ran = Random.insideUnitCircle * radius;
            Vector3 pos = startPos + ran;
            //Vector3 pos = startPos;
            var item = await SonatSystem.GetService<PoolingServiceAsync>().CreateAsync<UICollectEffectItemMultiple>(collectEffectName,
                PanelManager.Instance.transform, resource,
                quantity, startPos, pos, endPos, onComplete);
            if (!multipleCallback)
                onComplete = null;
        }
    }
}