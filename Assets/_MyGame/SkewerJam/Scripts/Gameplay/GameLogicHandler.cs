using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using Gameplay.Entities;
using Gameplay.Entities.Items;
using MyGame.SkewerJam.Gameplay.Helpers;
using MyGame.SkewerJam.Objects;
using MyGame.SkewerJam.Objects.Entities;
using Sonat.Enums;
using SonatFramework.Scripts.UIModule;
using SonatFramework.Scripts.Utils;
using SonatFramework.Systems.EventBus;
using SonatFramework.Systems.InventoryManagement;
using UnityEngine;

namespace MyGame.SkewerJam.Gameplay
{
    public class GameLogicHandler : MonoBehaviour
    {
        private const string LOG_TAG = "<color=yellow>GameLogicHandler: </color>";

        [Header("Core")]
        [SerializeField] private GrillManager grillManager;
        [SerializeField] private WaitingGrillManager waitingGrillManager;
        [SerializeField] private OrderManager orderManager;
        [SerializeField] private ItemManager itemManager;

        [Header("Additional")]
        [SerializeField] private ConveyorManager conveyorManager;
        [SerializeField] private ObstacleManager obstacleManager;

        [Header("Utils")]
        [SerializeField] private SuggestManager suggestManager;

        public OrderManager OrderManager => orderManager;
        public WaitingGrillManager WaitingGrillManager => waitingGrillManager;
        public GrillManager GrillManager => grillManager;
        public ConveyorManager ConveyorManager => conveyorManager;
        public ObstacleManager ObstacleManager => obstacleManager;
        public ItemManager ItemManager => itemManager;
        public Item ItemSelected { get; set; }

        public event Action<Item, SlotBase> OnStartItemMoveSlot;
        public event Action<Item, SlotBase> OnItemMoveSlot;
        public event Action<Item, bool> OnItemStartSwitch;
        public event Action<OrderEntity> OnAppearNextOrder;
        public event Action<int> OnAppearNextOrderItem;
        public event Action<int> OnCollectItem;
        public event Action<OrderEntity> OnStartCollectItem;
        public event Action<OrderEntity> OnCompleteCollectItem;

        public int NumPendingOrder = 0;

        private int pumpkin = 0;
        public int Pumpkin => pumpkin;


        public void Init()
        {
            grillManager.Init();
            waitingGrillManager.Init();
            orderManager.Init();

            conveyorManager.Init();
            obstacleManager.Init();

            suggestManager.Init();

            pumpkin = 0;
            NumPendingOrder = 0;
        }

        public void Clear()
        {
            pumpkin = 0;
            orderManager.Clear();
            waitingGrillManager.Clear();
            grillManager.Clear();
            itemManager.Clear();

            conveyorManager.Clear();
            obstacleManager.Clear();

            suggestManager.Clear();
        }

        public bool SelectItem(Item item)
        {
            // Kiểm tra có vị trí hợp lệ ở order không
            if (CheckEnergy() == false) return false;

            ItemSelected = item;
            var (order, slot) = orderManager.GetDestinationSlot(item);
            if (slot != null)
            {
                var log = new SpendResourceLogData()
                {
                    earnType = "energy",
                    earnId = "energy",
                    source = "gameplay"
                };
                MySonatFramework.GetService<InventoryService>().ReduceResource(GameResource.Energy, 1, log);
                EventBus<ReduceItemEvent>.Raise(new ReduceItemEvent() { resource = GameResource.Energy, quantity = 1 });
                SwitchSlot(slot);
                return true;
            }

            // Kiểm tra còn vị trí ở waiting grill không
            // Khi bay tới đĩa phải kiểm tra xem có order mới không thì nhảy lên ngay
            var (waitingGrill, waitingGrillSlot) = waitingGrillManager.GetDestinationSlot();
            if (waitingGrillSlot != null)
            {
                var log = new SpendResourceLogData()
                {
                    earnType = "energy",
                    earnId = "energy",
                    source = "gameplay"
                };
                MySonatFramework.GetService<InventoryService>().ReduceResource(GameResource.Energy, 1, log);
                EventBus<ReduceItemEvent>.Raise(new ReduceItemEvent() { resource = GameResource.Energy, quantity = 1 });
                SwitchSlot(waitingGrillSlot);
                return true;
            }
            return false;
        }

        private void SwitchSlot(SlotBase slot)
        {
            if (ItemSelected == null) return;

            ItemSelected.SetLockState(true);
            ItemSelected.SwitchSlot(slot);
            OnItemStartSwitch?.Invoke(ItemSelected, true);

            // SelectItem(null, false);
            //Debug.Log("Switched Slot End");
        }

        #region Event Actions
        public void AppearNextOrder(OrderEntity orderEntity, bool startLevel = false)
        {
            orderEntity.SetReady(true);

            if (startLevel == false)
            {
                OnAppearNextOrder?.Invoke(orderEntity);
                OnAppearNextOrderItem?.Invoke((int)orderEntity.ItemIdTarget);
            }

            int count = 0;
            foreach (var order in orderManager.ListOrders)
            {
                if (order.Ready == false || order.IsActive == false) continue;
                var targetItem = order.ItemIdTarget;
                foreach (var waitingGrill in waitingGrillManager.ListWaitingGrills)
                {
                    var slot = waitingGrill.GetSlot(0);
                    var item = slot.GetItem();
                    var orderSlot = order.GetAvailableSlot();
                    if (item != null && item.id == (int)targetItem && orderSlot != null)
                    {
                        ItemSelected = item;
                        SwitchSlot(orderSlot);
                        count++;
                    }
                }
            }

            TryCheckLoseGame().Forget();
            SonatUtils.ExecuteNextFrame(() =>
            {
                NumPendingOrder = Mathf.Max(0, NumPendingOrder - 1);
            });
        }

        public void CollectItem(OrderEntity orderEntity)
        {
            OnCollectItem?.Invoke((int)orderEntity.ItemIdTarget);
            OnStartCollectItem?.Invoke(orderEntity);

            hasCollectItem = true;
            foreach (var slot in orderEntity.GetSlots())
            {
                slot.GetItem()?.OnComplete();
            }

            if (CheckWinGame())
            {
                Debug.Log("<color=green>Win Game</color>");
                GameController.Instance.Win();
            }
        }

        public void CompleteCollectItem(OrderEntity orderEntity)
        {
            OnCompleteCollectItem?.Invoke(orderEntity);

            // MySonatFramework.GetService<InventoryService>().AddResource(GameResource.Pumpkin, 1);
            pumpkin++;
            EventBus<AddItemEvent>.Raise(new AddItemEvent()
            {
                resource = GameResource.Pumpkin,
                quantity = 1,
                position = orderEntity.transform.position,
                collectEffect = new CollectEffectSingle()
                {
                    collectEffectName = "CollectResourceSingleItem_Pumpkin"
                }
            });
        }

        public void StartItemMoveSlot(Item item, SlotBase slot)
        {
            OnStartItemMoveSlot?.Invoke(item, slot);
        }

        private bool hasCollectItem = false;
        public bool HasCollectItem => hasCollectItem;
        public void ItemMoveSlot(Item item, SlotBase slot)
        {
            hasCollectItem = false;
            OnItemMoveSlot?.Invoke(item, slot);
        }

        public bool CheckEnergy()
        {
            var energy = MySonatFramework.GetService<InventoryService>().GetResource(GameResource.Energy);
            if (energy <= 0 && GameController.Instance.GameState == GameState.Playing)
            {
                GameController.Instance.ChangeGameState(GameState.Paused);
                PanelManager.Instance.OpenPanel<PopupWarningEnergy_SkewerJam>(new UIData().Add("GamePlacement", GamePlacement.Gameplay_SkewerJam));
                return false;
            }
            return true;
        }
        #endregion


        #region Check Win Lose Game
        public async UniTask TryCheckLoseGame()
        {
            await UniTask.Delay(2000);
            await UniTask.WaitUntil(() => NumPendingOrder <= 0);
            var stuckType = CheckLoseGame();
            if (stuckType != null)
            {
                Debug.Log("<color=red>Lose Game</color>");
                GameController.Instance.Stuck(stuckType.Value);
            }
        }

        public bool CheckWinGame()
        {
            // Win khi clear hết level hết order
            if (GameController.Instance.GameState != GameState.Playing) return false;
            if (grillManager.CheckClearAllItems() && waitingGrillManager.CheckClearAllItems()) return true;
            return false;
        }

        public StuckType? CheckLoseGame()
        {
            if (GameController.Instance.GameState != GameState.Playing) return null;
            // Không thể di chuyển nữa thì thua

            // waiting grill còn slot trống thì chưa thua
            var listWaitingGrill = waitingGrillManager.ListWaitingGrills;
            foreach (var waitingGrill in listWaitingGrill)
            {
                if (waitingGrill.GetSlots().Where(e => e.isEmpty() && waitingGrill.IsActive).Count() > 0) return null;
            }

            // nếu order còn có thể di chuyển item vào thì chưa thua + loại các item bị lock
            var listTargetItemIds = orderManager.GetTargetItemIds();
            var listItemIdInLayer1 = GrillHelper.GetItemIdListWithLayer(1, true);
            var dictItems = listItemIdInLayer1.GroupBy(e => e).ToDictionary(e => e.Key, e => e.Count());

            // var listLockedItems = ItemHelper.GetItemIdsInLockedGrill();
            // foreach (var item in listLockedItems)
            // {
            //     if (dictItems.ContainsKey(item) == false) continue;
            //     dictItems[item]--;
            // }

            foreach (var id in listTargetItemIds)
            {
                if (dictItems.ContainsKey(id) == true && dictItems[id] > 0) return null;
            }

            // else continue
            return StuckType.SkewerJam_OutOfSpace;
        }

        #endregion

        public List<ItemStateData> GetItemStateDatas()
        {
            var list = new List<ItemStateData>();
            foreach (var grill in grillManager.ListGrills)
            {
                for (int i = 0; i < grill.GetSlots().Length; i++)
                {
                    var slot = grill.GetSlots()[i];
                    if (slot.GetItem() == null) continue;
                    if (slot.GetItem() is ItemBombMove itemBombMove)
                    {
                        list.Add(new ItemStateData() { grillId = grill.id, slotIndex = i, id = slot.GetItem()?.id ?? 0, bombCount = itemBombMove.MoveRemaining, active = !itemBombMove.Exploded });
                    }
                }
            }

            foreach (var waitingGrill in waitingGrillManager.ListWaitingGrills)
            {
                for (int i = 0; i < waitingGrill.GetSlots().Length; i++)
                {
                    var slot = waitingGrill.GetSlots()[i];
                    if (slot.GetItem() == null) continue;
                    if (slot.GetItem() is ItemBombMove itemBombMove)
                    {
                        list.Add(new ItemStateData() { grillId = waitingGrill.id, slotIndex = i, id = slot.GetItem()?.id ?? 0, bombCount = itemBombMove.MoveRemaining, active = !itemBombMove.Exploded });
                    }
                    else
                    {
                        list.Add(new ItemStateData() { grillId = waitingGrill.id, slotIndex = i, id = slot.GetItem()?.id ?? 0, bombCount = -1, active = false });
                    }
                }
            }
            return list;
        }

        public void SetPumpkin(int currentPumpkin)
        {
            pumpkin = currentPumpkin;
        }
    }
}
