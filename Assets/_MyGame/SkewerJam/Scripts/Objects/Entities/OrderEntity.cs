using System;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using Gameplay.BoosteeManagement;
using Gameplay.Entities;
using Gameplay.LevelData;
using Manager;
using MyGame.SkewerJam.Gameplay;
using MyGame.SkewerJam.Gameplay.Helpers;
using Sonat.Enums;
using SonatFramework.Scripts.UIModule;
using UnityEngine;
using static PopupUnlock_SkewerJam;

namespace MyGame.SkewerJam.Objects.Entities
{
    public class OrderEntity : GrillBase
    {
        [Header("Order Entity Visual")]
        [SerializeField] private Transform container;
        [SerializeField] private OrderEntityVisual orderEntityVisual;
        private int orderIndex;
        private bool active = false;
        private bool ready = false;

        private ItemId itemIdTarget = ItemId.None;
        private int maxItems = 0;
        public int MaxItems => maxItems;

        public ItemId ItemIdTarget => itemIdTarget;
        public bool IsActive => active;
        public int OrderIndex => orderIndex;
        public bool Ready => ready;

        #region Implementations

        public override EntityType entityType => EntityType.PrimaryGrill;

        public OrderEntityVisual Visual => orderEntityVisual;

        public override void ChangeItem(SlotBase slot, int newId)
        {

        }

        public override bool CheckItemWithId(int id)
        {
            return false;
        }

        public override Vector3 DestroyItem(int id)
        {
            return Vector3.zero;
        }

        public override ShuffleLayerData GetMagnetLayerData()
        {
            return null;
        }

        public override ShuffleLayerData GetShuffleLayerData()
        {
            return null;
        }

        public override void SetShuffleLayerData(LayerData layerData)
        {

        }
        #endregion

        public void SetData(ItemId itemId, int num)
        {
            SetTargetItem(itemId, num);
        }

        public void SetActive(bool active)
        {
            this.active = active;
            orderEntityVisual.SetActive(active);
        }

        public void SetOrderIndex(int index)
        {
            orderIndex = index;
        }

        public void SetReady(bool ready)
        {
            this.ready = ready;
        }

        public void SetTargetItem(ItemId itemId, int num)
        {
            itemIdTarget = itemId;
            maxItems = num;
            checkCreateItemFaded = false;
            ShowTargetItem().Forget();
        }

        private bool checkCreateItemFaded = false;
        private async UniTask ShowTargetItem()
        {
            for (int i = 0; i < slots.Length; i++)
            {
                if (i >= maxItems)
                {
                    var slot = slots[i];
                    slot.ClearItem();
                    continue;
                }
                else
                {
                    var itemData = new ItemData()
                    {
                        itemType = ItemType.Normal,
                        id = (int)itemIdTarget,
                    };
                    var item = await GrillBaseBehaviorSO.gameFactorySO.CreateItem<Item>($"ItemFaded");
                    item.SetPrimary(entityType == EntityType.PrimaryGrill);
                    item.SetItemData(itemData, slots[i]);
                    item.SetIsOnConveyor(isOnConveyor);
                    item.SetLockState(true);

                    slots[i].ClearItem();
                    slots[i].SetItem(item); // căn đúng vị trí

                    slots[i].SetItem(null); // không tồn tại trong slot
                }
            }
            checkCreateItemFaded = true;

            // // căn lại ví trí slot
            // var dist = slots[1].transform.localPosition.x - slots[0].transform.localPosition.x;
            // var startPos = -(maxItems - 1) * dist / 2;
            // for (int i = 0; i < slots.Length; i++)
            // {
            //     var slot = slots[i];
            //     slot.transform.localPosition = new Vector3(startPos + dist * i, 0, 0);
            // }
        }

        public SlotBase GetAvailableSlot()
        {
            for (int i = 0; i < maxItems; i++)
            {
                var slot = slots[i];
                if (slot.GetItem() == null)
                {
                    return slot;
                }
            }
            return null;
        }

        public bool CheckComplete()
        {
            var slots = GetSlots();
            for (int i = 0; i < maxItems; i++)
            {
                var slot = slots[i];
                if (slot.GetItem() == null || slot.GetItem().itemState == ItemState.Selected)
                {
                    return false;
                }
            }
            return true; // đã bay tới vị trí slot
        }

        public void PlayComplete(Action onComplete)
        {
            foreach (var slot in slots)
            {
                slot.GetItem()?.OnComplete();
            }
            orderEntityVisual.PlayComplete(onComplete);
        }

        public override void OnReturnObj()
        {
            base.OnReturnObj();
            foreach (var slot in slots)
            {
                foreach (Transform child in slot.Container)
                {
                    if (child.TryGetComponent<Item>(out var item))
                    {
                        GrillBaseBehaviorSO.gameFactorySO.ReturnEntity(item);
                    }
                }
            }
            itemIdTarget = ItemId.None;
            orderIndex = 0;
            ready = false;

            orderEntityVisual.ResetLid();
        }

        #region Interact
        private void Update()
        {
            // Khi nhả chuột trái
            //  var popup = PanelManager.Instance.GetPanel<PopupUnlock_SkewerJam>();
            if (GameController.Instance.GameState == GameState.Playing && !active && Input.GetMouseButtonUp(0))
            {
                var hits = Physics2D.OverlapPointAll(Camera.main.ScreenToWorldPoint(Input.mousePosition));
                foreach (var hit in hits)
                {
                    if (hit.gameObject == gameObject)
                    {
                        OpenPopupUnlock();
                        break;
                    }
                }
            }
        }
        #endregion

        private void OpenPopupUnlock()
        {
            var uiData = new UIData();
            uiData.Add("SelectedObjectType", SelectedObjectType.Tray);
            uiData.Add("Price", GameController.Instance.GameConfig.unlockTrayPrice);
            uiData.Add("OnSuccess", (Action)(() =>
            {
                var orderManager = GameController.Instance.GameLogicHandler.OrderManager;
                orderManager.Unlock(this);
            }));
            PanelManager.Instance.OpenPanel<PopupUnlock_SkewerJam>(uiData);
        }

        public void PlayUnlock()
        {
            SetActive(true);
            orderEntityVisual.OpenGrill(true);


            var gameLogicHandler = GameController.Instance.GameLogicHandler;
            var orderManager = gameLogicHandler.OrderManager;

            if (OrderHelper.CheckCreateNextOrder())
            {
                var (itemId, num) = OrderHelper.GetItemOrder();
                Debug.Log("<color=red>itemId: " + itemId + ", num: " + num + "</color>");
                SetTargetItem(itemId, num);

                var orderPos = orderManager.LeftStartPos.position;
                orderPos.z = 0;
                container.position = orderPos;
                container.DOLocalMove(Vector3.zero, 0.5f).SetEase(Ease.OutSine).OnComplete(() =>
                {
                    gameLogicHandler.AppearNextOrder(this);
                });
            }
        }

        public (int itemId, int num) GetData()
        {
            return ((int)itemIdTarget, maxItems);
        }

        public async UniTask SetItems(int number)
        {
            await UniTask.WaitUntil(() => checkCreateItemFaded);
            for (int i = 0; i < number; i++)
            {
                var slot = slots[i];
                if (slot != null && slot.GetItem() == null)
                {
                    var item = await GrillBaseBehaviorSO.gameFactorySO.CreateItem<Item>($"Item{ItemType.Normal}", slot.transform);
                    var itemData = new ItemData()
                    {
                        id = (int)itemIdTarget,
                        itemType = ItemType.Normal,
                    };
                    item.SetItemData(itemData, slot);
                    item.SetIsOnConveyor(false);
                    item.SetLockState(false);

                    slot.ClearItem();
                    slot.SetItem(item);
                }
            }
        }
    }
}