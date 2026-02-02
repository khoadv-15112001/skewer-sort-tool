using System.Collections.Generic;
using MyGame.SkewerJam.Objects.Entities;
using UnityEngine;
using MyGame.SkewerJam.Gameplay;
using Gameplay.Entities;
using Gameplay.LevelData;
using Cysharp.Threading.Tasks;
using Manager;
using DG.Tweening;
using System.Linq;
using MyGame.SkewerJam.Gameplay.Helpers;

namespace MyGame.SkewerJam.Objects
{
    public class OrderManager : MonoBehaviour
    {
        [SerializeField] private OrderManagerSO orderManagerSO;
        [SerializeField] private OrderEntityConfigSO orderEntityConfigSO;
        [SerializeField] private float delayAlignOrders = 0.5f;

        [Header("Align")] [SerializeField] private float distance = 2.6f;
        [SerializeField] private Transform rightStartPos;
        [SerializeField] private Transform leftStartPos;

        private List<(int maxNumber, int itemId, int number)> _listOrderData = new List<(int maxNumber, int itemId, int number)>();
        private List<OrderEntity> _listOrders = new List<OrderEntity>();
        private int _nextOrderIndex = 0;

        private List<Vector3> _listOrderLocalPositions = new List<Vector3>();

        public List<OrderEntity> ListOrders => _listOrders;
        public Transform LeftStartPos => leftStartPos;
        public Transform RightStartPos => rightStartPos;

        #region Init

        public async UniTask Init()
        {
            // xác định ví trí các order
            var startPos = -(orderManagerSO.MaxOrder - 1) * distance / 2;
            for (int i = 0; i < orderManagerSO.MaxOrder; i++)
            {
                var orderPos = new Vector3(startPos + distance * i, 0, 0);
                _listOrderLocalPositions.Add(orderPos);
            }

            // game events
            var gameLogicHandler = GameController.Instance.GameLogicHandler;
            gameLogicHandler.OnItemMoveSlot += GameLogicHandler_OnItemMoveSlot;

            OrderHelper.Reset();
        }

        public async UniTask SetData(List<(int maxNumber, int itemId, int number)> listOrderData)
        {
            this._listOrderData = listOrderData;

            if (listOrderData.Count == 0)
            {
                _nextOrderIndex = 0;
                for (int i = 0; i < orderManagerSO.DefaultNumberOfReadyOrder; i++)
                {
                    var (itemId, num) = OrderHelper.GetItemOrder();
                    var orderEntity = await CreateNextOrder(itemId, num);
                    orderEntity.SetOrderIndex(i);
                }

                for (int i = orderManagerSO.DefaultNumberOfReadyOrder; i < orderManagerSO.MaxOrder; i++)
                {
                    var orderEntity = await CreateNextLockedOrder();
                    orderEntity.SetOrderIndex(i);
                }

                PlayAppearOrders().Forget();
            }
            else
            {
                // backup
                for (int i = 0; i < listOrderData.Count; i++)
                {
                    var orderEntity = await CreateNextOrder((ItemId)listOrderData[i].itemId, listOrderData[i].maxNumber);
                    orderEntity.SetOrderIndex(i);
                    orderEntity.SetItems(listOrderData[i].number);
                }

                for (int i = listOrderData.Count; i < orderManagerSO.MaxOrder; i++)
                {
                    var orderEntity = await CreateNextLockedOrder();
                    orderEntity.SetOrderIndex(i);
                }

                PlayAppearOrders().Forget();
            }
        }

        public void Clear()
        {
            var gameLogicHandler = GameController.Instance.GameLogicHandler;
            gameLogicHandler.OnItemMoveSlot -= GameLogicHandler_OnItemMoveSlot;

            foreach (var order in _listOrders)
            {
                if (order != null)
                    GameFactory.Instance.ReturnEntity(order);
            }

            _listOrders.Clear();
        }

        #endregion

        public async UniTask<OrderEntity> CreateNextOrder(ItemId itemId, int num)
        {
            var orderEntity = await GameFactory.Instance.CreateEntityAsync<OrderEntity>("OrderEntity", transform);
            orderEntity.SetActive(true);
            orderEntity.Visual.SetNormalOrder(true);
            orderEntity.SetData(itemId, num);
            _listOrders.Add(orderEntity);
            _nextOrderIndex++;
            return orderEntity;
        }

        public async UniTask<OrderEntity> CreateNextLockedOrder()
        {
            var orderEntity = await GameFactory.Instance.CreateEntityAsync<OrderEntity>("OrderEntity", transform);
            orderEntity.SetActive(false);
            orderEntity.Visual.SetNormalOrder(false);
            _listOrders.Add(orderEntity);
            return orderEntity;
        }

        private async UniTask CreateNextOrder(int orderIndex)
        {
            var (itemId, num) = OrderHelper.GetItemOrder();

            var nextOrder = await CreateNextOrder(itemId, num);

            var orderPos = leftStartPos.position;
            orderPos.z = 0;
            nextOrder.transform.position = orderPos;
            nextOrder.SetOrderIndex(orderIndex);


            await UniTask.Delay((int)(orderEntityConfigSO.delayAppearNextOrder * 1000));
            nextOrder.transform.DOLocalMove(_listOrderLocalPositions[nextOrder.OrderIndex], 0.3f).SetEase(Ease.OutSine).OnComplete(() =>
            {
                GameController.Instance.GameLogicHandler.AppearNextOrder(nextOrder);
            });
        }

        private void GameLogicHandler_OnItemMoveSlot(Item item, SlotBase slot)
        {
            if (slot.GetGrill() is OrderEntity orderEntity)
            {
                if (_listOrders.Contains(orderEntity))
                {
                    if (orderEntity.CheckComplete())
                    {
                        var orderIndex = orderEntity.OrderIndex;
                        _listOrders.Remove(orderEntity);

                        var checkNextOrder = OrderHelper.CheckCreateNextOrder();
                        orderEntity.PlayComplete(() =>
                        {
                            GameController.Instance.GameLogicHandler.CompleteCollectItem(orderEntity);
                            if (checkNextOrder == false)
                            {
                                AlignObjects().Forget();
                            }
                        });

                        if (checkNextOrder)
                        {
                            GameController.Instance.GameLogicHandler.NumPendingOrder += 1;
                            CreateNextOrder(orderIndex).Forget();
                        }

                        GameController.Instance.GameLogicHandler.CollectItem(orderEntity);
                    }
                    else
                    {
                        // nếu đang không có order nào ra thì check lose game
                        GameController.Instance.GameLogicHandler.TryCheckLoseGame();
                    }
                }
            }
        }

        public async UniTask PlayAppearOrders()
        {
            await UniTask.Delay(100);
            // tất cả order xuất hiện đầu game
            for (int i = 0; i < _listOrders.Count; i++)
            {
                var order = _listOrders[i];
                order.transform.DOKill();

                var orderPos = rightStartPos.position;
                orderPos.z = 0;
                order.transform.position = orderPos;
            }

            await UniTask.Delay(2000);
            for (int i = 0; i < _listOrders.Count; i++)
            {
                var order = _listOrders[i];
                order.transform.DOLocalMove(_listOrderLocalPositions[order.OrderIndex], 0.3f).SetEase(Ease.OutSine).OnComplete(() =>
                {
                    GameController.Instance.GameLogicHandler.AppearNextOrder(order, true);
                });
                await UniTask.Delay(200);
            }
        }

        #region Functions

        public (OrderEntity order, SlotBase slot) GetDestinationSlot(Item item)
        {
            foreach (var order in _listOrders)
            {
                if (order.ItemIdTarget == (ItemId)item.id && order.Ready)
                {
                    var orderSlot = order.GetAvailableSlot();
                    if (orderSlot != null)
                    {
                        return (order, orderSlot);
                    }
                }
            }

            return (null, null);
        }

        public List<ItemId> GetTargetItemIds()
        {
            var list = new List<ItemId>();
            foreach (var order in _listOrders)
            {
                if (order.IsActive == false) continue;
                if (order.ItemIdTarget == ItemId.None) continue;
                list.Add(order.ItemIdTarget);
            }

            return list.Distinct().ToList();
        }

        public Dictionary<ItemId, (int maxItems, int num)> GetOrderItemsDict()
        {
            var dict = new Dictionary<ItemId, (int maxItems, int num)>();
            foreach (var order in _listOrders)
            {
                if (order.IsActive == false) continue;

                var targetItem = order.ItemIdTarget;
                if (dict.ContainsKey(targetItem) == false)
                {
                    dict[targetItem] = (0, 0);
                }

                var newMaxItems = dict[targetItem].maxItems + order.MaxItems;
                dict[targetItem] = (newMaxItems, dict[targetItem].num);

                foreach (var slot in order.GetSlots())
                {
                    if (slot.GetItem() != null)
                    {
                        dict[targetItem] = (newMaxItems, dict[targetItem].num + 1);
                    }
                }
            }

            return dict;
        }

        #endregion

        public async UniTask AlignObjects()
        {
            await UniTask.Delay((int)(delayAlignOrders * 1000));
            var start = -(_listOrders.Count - 1) * distance / 2;
            var sortedOrders = _listOrders.OrderBy(e => e.OrderIndex).ToList();
            var listLocalTargetPositions = new List<Vector3>();
            for (int i = 0; i < sortedOrders.Count; i++)
            {
                listLocalTargetPositions.Add(new Vector3(start + distance * i, 0, 0));

                var orderEntity = sortedOrders[i];
                orderEntity.transform.DOLocalMove(listLocalTargetPositions[i], 0.1f).SetEase(Ease.OutSine);
                await UniTask.Delay(75);
            }
        }

        public void Unlock(OrderEntity orderEntity = null)
        {
            if (orderEntity == null)
            {
                orderEntity = _listOrders.Where(e => e.IsActive == false).FirstOrDefault();
                if (orderEntity == null) return;
            }

            orderEntity.PlayUnlock();
        }

        public List<(int maxNumber, int itemId, int number)> GetOrderItemDatas()
        {
            var list = new List<(int maxNumber, int itemId, int number)>();
            foreach (var order in _listOrders)
            {
                if (order.IsActive == false) continue;
                if (order.ItemIdTarget == ItemId.None) continue;

                int number = 0;
                foreach (var slot in order.GetSlots())
                {
                    if (slot.GetItem() != null)
                    {
                        number += 1;
                    }
                }

                if (number >= order.GetSlots().Length) continue;
                list.Add((order.MaxItems, (int)order.ItemIdTarget, number));
            }

            return list;
        }
    }
}