using System;
using Cysharp.Threading.Tasks;
using Sonat.Enums;
using SonatFramework.Scripts.UIModule;
using SonatFramework.Systems;
using SonatFramework.Systems.ObjectPooling;
using UnityEngine;

public class CollectEffectMultipleItemInGame : SonatCollectEffect
{
    public string collectEffectName = "CollectEffectItemInGame";
    public int count = 10;
    public float radius = 1.25f;
    public int itemId = 0;
    public float scale = 1f;
    public float delayMove = 0.75f;

    public override void Collect(GameResource resource, int quantity, Vector3 startPos, Vector3 endPos, Action callback)
    {
        Spawn(resource, quantity, startPos, endPos, callback).Forget();
    }

    private async UniTaskVoid Spawn(GameResource resource, int quantity, Vector3 startPos, Vector3 endPos, Action onComplete)
    {
        count = Mathf.Min(count, quantity);
        for (int i = 0; i < count; i++)
        {
            Vector3 ran = UnityEngine.Random.insideUnitCircle * radius;
            Vector3 pos = startPos + ran;
            var item = await SonatSystem.GetService<PoolingServiceAsync>().CreateAsync<UICollectEffectItemInGame>(collectEffectName, PanelManager.Instance.transform, itemId,
                pos, endPos, onComplete, scale, delayMove);
            onComplete = null;
            await UniTask.Delay(75);
        }
    }
}
