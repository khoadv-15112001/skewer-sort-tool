using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using Gameplay.Entities.Orders;
using Manager;
using SonatFramework.Scripts.Utils;
using SonatFramework.Systems.ObjectPooling;
using UnityEngine;
using UnityEngine.UIElements;

namespace Gameplay.Entities.ItemScripts
{
    public class CollectEffect : MonoBehaviour, IPoolingObject
    {
        [SerializeField] protected Transform container;
        protected List<CollectEffectItem> items = new();
        protected float mergeDuration;
        protected int itemId;
        public Transform Container => container;

        public virtual void SetData(Vector3[] pos, int id, PrimaryGrill grill, OrderEntity orderEntity, float duration = GameDefine.itemMergeDuration)
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

        protected virtual void DOEffect(OrderEntity orderEntity, PrimaryGrill grill)
        {
            int center = items.Count / 2;

            for (int i = 0; i < items.Count; i++)
            {
                int offset = center - i;
                items[i].transform.DOLocalMove(Vector3.left * (0.75f - Mathf.Abs(0.1f * offset)) * offset, mergeDuration);
                items[i].transform.DOLocalRotate(Vector3.zero, mergeDuration);
            }

            Transform target = orderEntity.GetTarget(itemId, grill);
            float distance = Vector3.Distance(transform.position, target.position);
            float duration = distance / GameDefine.itemCollectSpeed;
            duration = Mathf.Clamp(duration, 0.7f, 1f);
            orderEntity.AddItem(itemId, grill, duration);
            orderEntity.AddDependency(this);

            SonatUtils.DelayCall(mergeDuration, () =>
            {
                transform.SetParent(target);
                float jumpPower = distance * 0.1f;
                transform.DOScale(new Vector3(0.55f, 0.45f, 0.55f), duration).SetDelay(0.15f);
                transform.DOLocalMove(Vector3.zero, duration).SetEase(Ease.OutQuad).SetDelay(0.15f).OnComplete(() =>
                {
                    if (!orderEntity.IsAnyItem())
                    {
                        orderEntity.RemoveDependency(this);
                        GameFactory.ReturnEntity(this);
                    }
                });
            }, this);
        }

        public virtual void Setup()
        {
        }

        public virtual void OnCreateObj(params object[] args)
        {
            GameplayController.OnReplay += OnReplay;
            transform.localScale = Vector3.one;
        }

        public virtual void OnReturnObj()
        {
            GameplayController.OnReplay -= OnReplay;
            transform.localScale = Vector3.one;
            transform.DOKill();
        }

        public virtual void OnReplay()
        {
            GameFactory.ReturnEntity(this);
        }
    }
}