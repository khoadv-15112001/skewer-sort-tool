using System;
using Sonat.Enums;
using SonatFramework.Scripts.UIModule;
using SonatFramework.Systems;
using SonatFramework.Systems.ObjectPooling;
using UnityEngine;

public class  CollectEffectSingle : SonatCollectEffect
{
    public string collectEffectName = "CollectResourceSingleItem";
    public float scale = 1f;
    public override void Collect(GameResource resource, int quantity, Vector3 startPos, Vector3 endPos, Action callback)
    {
        var collectEffectItem = SonatSystem.GetService<PoolingServiceAsync>().CreateAsync<UICollectEffectItem>(collectEffectName, PanelManager.Instance.transform, 
        resource, quantity, startPos, endPos, callback, scale);
    }
}
