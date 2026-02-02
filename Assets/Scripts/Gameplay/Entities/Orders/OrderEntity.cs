using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using Gameplay.Entities.ItemScripts;
using Gameplay.LevelData;
using I2.Loc;
using Manager;
using SonatFramework.Scripts.UIModule;
using SonatFramework.Scripts.Utils;
using SonatFramework.Systems;
using SonatFramework.Systems.ObjectPooling;
using UnityEngine;

namespace Gameplay.Entities.Orders
{
    public class OrderEntity : MonoBehaviour, IPoolingObject
    {
        [SerializeField] private Transform target;

        //[SerializeField] private Transform targetOrder;
        [SerializeField] private Transform visualContainer;
        [SerializeField] private OrderList orderList;
        [SerializeField] private GameObject targetVisual;
        private OrderQueue orderQueue;
        private OrderVisual orderVisual;
        private List<IPoolingObject> dependencies = new List<IPoolingObject>();
        private bool isShipper;

        //public Transform Target => isShipper ? targetOrder : target;
        public bool IsCompleted { get; set; }

        public void SetData(OrderQueue orderQueue, OrderData orderData)
        {
            this.orderQueue = orderQueue;
            isShipper = orderData != null;
            if (!GameRemoteConfigValue.noCharacter)
            {
                string path = GetOrderVisualName(isShipper);
                orderVisual = GameFactory.CreateEntity<OrderVisual>(path, visualContainer.position, visualContainer);
                orderVisual.transform.localScale = Vector3.one;
            }

            SetOrderList(orderData);
            IsCompleted = false;
            dependencies.Clear();
            targetVisual.SetActive(false);
        }


        public async UniTask SetDataAsync(OrderQueue orderQueue, OrderData orderData)
        {
            this.orderQueue = orderQueue;
            isShipper = orderData != null;
            targetVisual.SetActive(false);

            if (isShipper || !GameRemoteConfigValue.noCharacter)
            {
                string path = GetOrderVisualName(!GameRemoteConfigValue.noCharacter && isShipper);
                await CreateOrderVisual($"{path}.prefab");
                orderVisual.transform.localScale = Vector3.one;
                if (isShipper)
                {
                    transform.position += Vector3.left * 6.5f;
                    transform.DOMove(orderQueue.GetOrderPosition(2), GameDefine.orderMoveSpeed).SetSpeedBased(true);
                }
            }

            SetOrderList(orderData);
            IsCompleted = false;
            dependencies.Clear();
        }

        private string GetOrderVisualName(bool isShiper)
        {
            if (isShiper)
            {
                return $"OrderVisual_{OrderEntityConfig.GetRandomShipper()}";
            }
            else
            {
                return $"OrderVisual_{orderQueue.GetVisualId()}";
            }
        }

        private async UniTask CreateOrderVisual(string path)
        {
            orderVisual = await GameFactory.CreateEntityAsync<OrderVisual>(path, visualContainer.position, visualContainer);
        }

        public void AddItem(int itemId, PrimaryGrill grill, float duration)
        {
            orderList.OnAddItem(itemId, grill, duration);
            if (isShipper)
            {
            }
            else
            {
                targetVisual.transform.localScale = Vector3.zero;
                targetVisual.SetActive(true);
                targetVisual.transform.DOScale(Vector3.one, 0.175f);
            }

            SonatUtils.DelayCall(duration + 0.25f, () =>
            {
                SonatSystem.GetService<PoolingService>().Create<EffectPoolBase>("ReceiveFood", orderList.transform.position, target);
                CheckComplete();
            }, this);
        }

        public bool CheckPreComplete(int itemId, PrimaryGrill grill)
        {
            return orderList.CheckPreComplete(itemId, grill);
        }

        public void PlayCompleteAnimation()
        {
            orderVisual?.OnComplete();
        }

        public void CheckComplete()
        {
            if (orderList.IsComplete())
            {
                PlayCompleteAnimation();
                OnComplete();
            }
        }

        public void OnComplete()
        {
            SonatUtils.DelayCall(0.65f, () => { MoveOut(true); }, this);
            //target.parent.DOScale(Vector3.zero, 0.4f).SetEase(Ease.InBack).SetDelay(0.4f);
        }

        public async UniTask MoveOut(bool complete)
        {
            if (!complete)
            {
                orderQueue.OrderNotCompleted();
                await orderQueue.CheckCreateNewOrder();
            }

            orderQueue.UpdateOrders(this);
            Vector3 outPos = transform.position;
            outPos.x = orderQueue.outViewportPosition;
            transform.DOMove(outPos, GameDefine.orderMoveSpeed).SetSpeedBased(true).OnComplete(
                () => { GameFactory.ReturnEntity(this); });
            //target.parent.DOScale(Vector3.zero, 0.3f).SetEase(Ease.InBack);
        }

        public void SetOrderList(OrderData orderData)
        {
            if (orderData == null)
            {
                orderList.gameObject.SetActive(false);
            }
            else
            {
                orderList.gameObject.SetActive(true);
                orderList.SetData(orderData);
            }
        }

        public bool NeedItem(int id, PrimaryGrill grill)
        {
            if (orderList.gameObject.activeInHierarchy && !IsCompleted)
            {
                return orderList.NeedItem(id, grill);
            }

            return false;
        }

        public bool IsAnyItem()
        {
            return !orderList.gameObject.activeInHierarchy;
        }

        public int GetItemOrder(OrderItemData orderItemData)
        {
            return orderQueue.GetAvailableItemId(orderItemData);
        }

        public Transform GetTarget(int itemId, PrimaryGrill grill)
        {
            if (!isShipper) return target;
            return orderList.GetTarget(itemId, grill);
        }

        public void PlayVoice(string voice)
        {
            if (orderVisual != null)
            {
                orderVisual.PlayVoice(voice);
            }
        }

        public void Setup()
        {
        }

        public void OnCreateObj(params object[] args)
        {
        }

        public void OnReturnObj()
        {
            transform.DOKill();
            StopAllCoroutines();
            if (orderVisual != null)
            {
                GameFactory.ReturnEntity(orderVisual);
                orderVisual = null;
            }

            ClearDependencies();
            IsCompleted = false;
            target.parent.localScale = Vector3.one;
        }

        public void RemoveDependency(IPoolingObject dependency)
        {
            dependencies.Remove(dependency);
        }

        public void AddDependency(IPoolingObject dependency)
        {
            dependencies.Add(dependency);
        }

        private void ClearDependencies()
        {
            foreach (var dependency in dependencies)
            {
                if (dependency != null && dependency.transform.gameObject.activeSelf)
                {
                    dependency.transform.DOKill();
                    GameFactory.ReturnEntity(dependency);
                }
            }

            dependencies.Clear();
        }

        public OrderList GetOrderNotCompleted()
        {
            return orderList;
        }
    }
}