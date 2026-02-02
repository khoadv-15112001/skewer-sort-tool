using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using Gameplay;
using Gameplay.Entities;
using Gameplay.Entities.ItemScripts;
using Gameplay.Entities.Orders;
using JetBrains.Annotations;
using SonatFramework.Scripts.Utils;
using UnityEngine;

public class CollectEffectTarget : CollectEffect
{
    public CanvasGroup canvasGroup;
    public override void SetData(Vector3[] pos, int id, PrimaryGrill grill, OrderEntity orderEntity, float duration = GameDefine.itemMergeDuration)
    {
        this.itemId = id;
        MySonatFramework.poolingContainer.CleanContainer(container);
        items.Clear();
        for (int i = 0; i < pos.Length; i++)
        {
            var item = MySonatFramework.poolingContainer.CreateObject<CollectEffectItem>(container);
            item.transform.rotation = Quaternion.identity;
            item.transform.localScale = Vector3.one;
            item.transform.localPosition = pos[i];
            item.SetData(id);
            items.Add(item);
        }

        mergeDuration = duration;
        SonatUtils.DelayCall(0.15f, () => { DOEffect(orderEntity, grill); }, this);
    }

    protected override void DOEffect(OrderEntity orderEntity, PrimaryGrill grill)
    {

        DoEffectAsync(orderEntity, grill).Forget();
    }

    private async UniTask DoEffectAsync(OrderEntity orderEntity, PrimaryGrill grill)
    {
        int center = items.Count / 2;

        for (int i = 0; i < items.Count; i++)
        {
            int offset = center - i;
            _ = items[i].transform.DOLocalMove(Vector3.left * (0.75f - Mathf.Abs(0.1f * offset)) * offset, mergeDuration);
            _ = items[i].transform.DOLocalRotate(Vector3.zero, mergeDuration);
        }

        UIItemGoal itemGoal = null;
        if (GameplayController.instance.uiTargetItemCtl.ItemGoalDict.ContainsKey(itemId))
        {
            itemGoal = GameplayController.instance.uiTargetItemCtl.ItemGoalDict[itemId];
        }

        if (itemGoal != null && !itemGoal.isCompleted)
        {
            //DO effect here
            Transform target = itemGoal.transform;
            
            Debug.LogError("Transform: " + transform.position);
            Debug.LogError("Target: " + target.position);
            float distance = Vector3.Distance(transform.position, target.position);
            float duration = distance / GameDefine.itemCollectSpeed;

            await UniTask.WaitForSeconds(mergeDuration);
            transform.SetParent(target);
            _ = transform.DOScale(transform.localScale * 0.5f, duration).SetDelay(0.15f);
            _ = transform.DOLocalMove(Vector3.zero, duration).SetEase(Ease.OutQuad).SetDelay(0.15f);
            await UniTask.WaitForSeconds(duration);
            GameplayController.instance.ProcessTarget(itemId, items.Count);
        }
        else
        {
            await UniTask.WaitForSeconds(mergeDuration);
            _ = transform.DOScale(transform.localScale * 0.5f, 0.3f).SetEase(Ease.OutQuad);
            foreach (var item in items)
            {
                Debug.LogError("Fade to zero: " + item.name, item.gameObject);
                item.FadeToZero();
            }

            await UniTask.WaitForSeconds(0.3f);
        }
        GameFactory.ReturnEntity(this);

        //Move item to dish target
        // Transform target = orderEntity.GetTarget(itemId, grill);
        // float distance = Vector3.Distance(transform.position, target.position);
        // float duration = distance / GameDefine.itemCollectSpeed;
        // duration = Mathf.Clamp(duration, 0.7f, 1f);
        // orderEntity.AddItem(itemId, grill, duration);
        // orderEntity.AddDependency(this);

        // await UniTask.WaitForSeconds(mergeDuration);

        // transform.SetParent(target);
        // float jumpPower = distance * 0.1f;
        // _ = transform.DOScale(new Vector3(0.55f, 0.45f, 0.55f), duration).SetDelay(0.15f);
        // _ = transform.DOLocalMove(Vector3.zero, duration).SetEase(Ease.OutQuad).SetDelay(0.15f).OnComplete(() =>
        // {
        //     if (!orderEntity.IsAnyItem())
        //     {
        //         orderEntity.RemoveDependency(this);
        //         GameFactory.ReturnEntity(this);
        //     }
        // });
    }
}
