using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using Gameplay.Entities.ItemScripts;
using Gameplay.LevelData;
using Gameplay.SceneManager;
using Sonat.Enums;
using UnityEngine;
using Cysharp.Threading.Tasks;
using TMPro;

namespace Gameplay.Entities.Orders
{
    public class OrderQueue : MonoBehaviour
    {
        [SerializeField] private Transform container;

        //[SerializeField] private Transform spawnPoint;
        private float entitySpace = 1;
        private int numberOfOrders = 3;
        private const int numberOrdersDefault = 3;
        private List<OrderEntity> orders = new List<OrderEntity>();
        private List<Vector3> positions = new List<Vector3>();
        private Coroutine moveCoroutine;
        private int firstIndex = 0;
        private GameViewport gameViewport;
        private Vector3 spawnPosition;
        private int maxOrder;
        private List<int> orderVisuals = new List<int>();
        private int orderCompleted;

        private List<OrderData> orderData;

        //private Dictionary<int, int> itemsCount;
        private List<int> itemsInOrder;
        private float scale;

        public float outViewportPosition { get; private set; }

        private void Awake()
        {
            GameplayController.OnCollectItem += OnCollectItem;
        }

        private void OnDestroy()
        {
            GameplayController.OnCollectItem -= OnCollectItem;
        }

        public void InitData(int maxOrder, List<OrderData> orderData, float cameraOrthographicSize)
        {
            orderVisuals = OrderEntityConfig.GetOrderVisuals();
            InitDataAsync(maxOrder, orderData, cameraOrthographicSize).Forget();
        }

        public async UniTask InitDataAsync(int maxOrder, List<OrderData> orderData, float cameraOrthographicSize)
        {
            scale = cameraOrthographicSize / 13.5f;
            this.maxOrder = maxOrder;

            if (orderData != null)
            {
                this.orderData = new List<OrderData>(orderData);
                this.orderData.Sort((a, b) => a.appearCondition.CompareTo(b.appearCondition));
            }
            else
            {
                this.orderData = null;
            }

            numberOfOrders = numberOrdersDefault;
            if (numberOfOrders > maxOrder)
            {
                numberOfOrders = maxOrder;
            }

            orderCompleted = 0;
            gameViewport = GameplayController.instance.gameViewport;
            //numberOfOrders = Mathf.CeilToInt((gameViewport.maxX - gameViewport.minX - entitySpace) / entitySpace);
            entitySpace = (gameViewport.maxX - gameViewport.minX) / numberOfOrders;
            spawnPosition = new Vector3(gameViewport.minX - entitySpace / 2, container.position.y, container.position.z);
            Vector3 orderPosition = new Vector3(gameViewport.maxX - entitySpace / 2, container.position.y, container.position.z);
            outViewportPosition = gameViewport.maxX + entitySpace;


            orderPosition.x = (numberOfOrders - 1) * entitySpace / 2f;

            itemsInOrder = new();

            positions.Clear();

            for (int i = 0; i < numberOfOrders; i++)
            {
                Vector3 position = orderPosition;
                position.x -= entitySpace * i;
                positions.Add(position);
            }

            for (int i = 0; i < numberOfOrders; i++)
            {
                Vector3 spawnPos = spawnPosition - Vector3.right * entitySpace * i;
                await CreateOrderEntityAsync(spawnPos);
            }

            moveCoroutine = StartCoroutine(MoveToPos(2));
        }

        private OrderEntity CreateOrderEntity(Vector3 position)
        {
            var orderEntity = GameFactory.CreateEntity<OrderEntity>("OrderEntity", position, container);
            orderEntity.transform.localScale = Vector3.one * scale;
            orderEntity.SetData(this, GetOrderData());
            orders.Add(orderEntity);
            maxOrder--;
            orderCompleted++;
            return orderEntity;
        }

        private async UniTask<OrderEntity> CreateOrderEntityAsync(Vector3 position)
        {
            var orderEntity = await GameFactory.CreateEntityAsync<OrderEntity>("OrderEntity.prefab", position, container);
            orderEntity.transform.localScale = Vector3.one * scale;

            OrderData orderData = GetOrderData();
            await orderEntity.SetDataAsync(this, orderData);


            int count = orders.Count;
            int index = 0;

            if (orderData != null)
            {
                orders.Add(orderEntity);
            }
            else
            {
                for (int i = 0; i < count; i++)
                {
                    if (orders[i].IsAnyItem())
                    {
                        index++;
                    }
                    else
                    {
                        break;
                    }
                }

                orders.Insert(index, orderEntity);
            }

            maxOrder--;
            orderCompleted++;
            return orderEntity;
        }

        private IEnumerator MoveToPos(float speedMultiplier = 1)
        {
            int max = Mathf.Min(positions.Count, orders.Count);
            for (int i = 0; i < max; i++)
            {
                orders[i].transform.DOKill();
                orders[i].transform.DOMove(positions[i], GameDefine.orderMoveSpeed * speedMultiplier).SetSpeedBased(true);
                yield return new WaitForSeconds(0.05f);
            }

            moveCoroutine = null;
        }

        public Vector3 GetOrderPosition(int index)
        {
            return positions[index];
        }

        public OrderEntity PopOrderEntity(int id, PrimaryGrill grill)
        {
            var orderEntity = GetOrderEntryForItem(id, grill);
            if (orderEntity == null)
            {
                GameplayController.instance.Stuck(StuckType.OutOfMove);
                return null;
            }

            if (orderEntity.CheckPreComplete(id, grill))
            {
                orderEntity.IsCompleted = true;
                CheckCreateNewOrder();
            }

            return orderEntity;
        }

        private OrderEntity GetOrderEntryForItem(int itemId, PrimaryGrill grill)
        {
            foreach (var order in orders)
            {
                if (order.NeedItem(itemId, grill)) return order;
            }

            foreach (var order in orders)
            {
                if (!order.IsCompleted && order.IsAnyItem()) return order;
            }

            return null;
        }

        public async UniTask CheckCreateNewOrder()
        {
            if (maxOrder > 0)
            {
                await CreateOrderEntityAsync(spawnPosition);
            }
        }

        public void OrderNotCompleted()
        {
            maxOrder++;
        }

        public void UpdateOrders(OrderEntity orderEntity)
        {
            if (moveCoroutine != null)
            {
                StopCoroutine(moveCoroutine);
            }

            orders.Remove(orderEntity);
            moveCoroutine = StartCoroutine(MoveToPos());
        }

        public void ClearOrders()
        {
            if (moveCoroutine != null)
            {
                StopCoroutine(moveCoroutine);
            }

            foreach (var orderEntity in orders)
            {
                GameFactory.ReturnEntity(orderEntity);
            }

            orders.Clear();
            positions.Clear();
            firstIndex = 0;
        }

        public OrderEntity GetShipperOrderEntity()
        {
            foreach (var order in orders)
            {
                if (!order.IsAnyItem() && !order.IsCompleted) return order;
            }

            return null;
        }

        private int currentOrderVisual = 0;

        public int GetVisualId()
        {
            if (currentOrderVisual <= 0 || currentOrderVisual >= orderVisuals.Count)
            {
                orderVisuals.Shuffle();
                currentOrderVisual = orderVisuals.Count;
            }

            currentOrderVisual--;
            return orderVisuals[currentOrderVisual];
        }

        private OrderData GetOrderData()
        {
            if (orderData == null || orderData.Count == 0) return null;
            if (orderData[0].appearCondition <= orderCompleted - 2)
            {
                if (OrderInQueue() >= 1)
                {
                    orderData[0].appearCondition = orderCompleted - 1;
                    return null;
                }

                var res = orderData[0];
                if (GameplayController.instance.timeManager.GetTimeRemaining() < res.time) return null;
                if (!VerifyOrder(res)) return null;
                orderData.RemoveAt(0);
                return res;
            }

            return null;
        }

        private int OrderInQueue()
        {
            int count = 0;
            foreach (var order in orders)
            {
                if (!order.IsCompleted && !order.IsAnyItem()) count++;
            }

            return count;
        }

        private void OnCollectItem(int itemId)
        {
            //if (itemsCount.ContainsKey(itemId) && itemsCount[itemId] > 0)
            // {
            // itemsCount[itemId]--;
            //if (itemsCount[itemId] == 0) itemsCount.Remove(itemId);
            if (itemsInOrder.Contains(itemId))
            {
                itemsInOrder.Remove(itemId);
            }
            //}
        }

        public bool VerifyOrder(OrderData orderData)
        {
            var itemsCount = GameplayController.instance.levelGenerator.GetItemsWithLayer(false);
            if (orderData.OrderItem.Count > itemsCount.Count) return false;
            return true;
        }

        public int GetAvailableItemId(OrderItemData orderItemData)
        {
            int layer = orderItemData.layer;
            var itemsCount = GameplayController.instance.levelGenerator.GetItemsWithLayer(orderItemData.spicy);
            foreach (var itemCount in itemsCount)
            {
                if (itemCount.Key >= 999 || itemsInOrder.Contains(itemCount.Key)) continue;
                if (itemCount.Value == layer)
                {
                    itemsInOrder.Add(itemCount.Key);
                    return itemCount.Key;
                }
            }

            foreach (var itemCount in itemsCount)
            {
                if (itemCount.Key >= 999 || itemsInOrder.Contains(itemCount.Key)) continue;
                if (itemCount.Value < layer)
                {
                    itemsInOrder.Add(itemCount.Key);
                    return itemCount.Key;
                }
            }

            itemsCount = itemsCount.OrderBy(e => e.Value).ToDictionary(e => e.Key, e => e.Value);
            foreach (var itemCount in itemsCount)
            {
                if (itemCount.Key >= 999 || itemsInOrder.Contains(itemCount.Key)) continue;
                itemsInOrder.Add(itemCount.Key);
                return itemCount.Key;
            }

            return 0;
        }

        public OrderList CheckOrderRemain()
        {
            foreach (var order in orders)
            {
                if (!order.IsCompleted && !order.IsAnyItem()) return order.GetOrderNotCompleted();
            }

            return null;
        }
    }
}