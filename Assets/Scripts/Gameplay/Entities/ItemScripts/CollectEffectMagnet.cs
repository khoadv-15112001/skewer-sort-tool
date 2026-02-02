using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using Gameplay;
using Gameplay.Entities.ItemScripts;
using Gameplay.Entities.Orders;
using SonatFramework.Scripts.Utils;
using UnityEngine;

public class CollectEffectMagnet : CollectEffect
{

    // protected override void DOEffect(OrderEntity orderEntity)
    // {
    //     int center = items.Count / 2;
    //
    //     for (int i = 0; i < items.Count; i++)
    //     {
    //         int offset = center - i;
    //         items[i].transform.DOLocalMove(Vector3.left * (0.75f - Mathf.Abs(0.1f * offset)) * offset, mergeDuration);
    //     }
    //     base.DOEffect(orderEntity);
    // }
}