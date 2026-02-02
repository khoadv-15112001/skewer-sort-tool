using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Gameplay.Entities;
using MyGame.SkewerJam.Gameplay;
using MyGame.SkewerJam.Gameplay.Utils;
using MyGame.SkewerJam.Objects.Entities;
using Sirenix.OdinInspector;
using UnityEngine;
using DG.Tweening;
using System.Linq;
using Gameplay.LevelData;
using Gameplay.Entities.Items;

namespace MyGame.SkewerJam.Objects
{
    public class WaitingGrillManager : MonoBehaviour
    {
        [SerializeField] private int maxWaitingGrills = 10;
        [SerializeField] private Transform centerRefPoint;
        [SerializeField] private float distance = 1.2f;

        private List<WaitingGrill> listWaitingGrills = new List<WaitingGrill>();

        public List<WaitingGrill> ListWaitingGrills => listWaitingGrills;

        void Start()
        {
            foreach (Transform child in transform)
            {
                child.gameObject.SetActive(false);
            }
        }

        public void Init()
        {
            transform.position = centerRefPoint.position;

            // game events
            var gameLogicHandler = GameController.Instance.GameLogicHandler;
            gameLogicHandler.OnItemMoveSlot += GameLogicHandler_OnItemMoveSlot;
        }

        private void GameLogicHandler_OnItemMoveSlot(Item item, SlotBase slot)
        {
            if (slot.GetGrill() is WaitingGrill waitingGrill)
            {
                GameController.Instance.GameLogicHandler.TryCheckLoseGame();
            }
        }

        public void Clear()
        {
            foreach (var waitingGrill in listWaitingGrills)
            {
                GameFactory.Instance.ReturnEntity(waitingGrill);
            }
            listWaitingGrills.Clear();
        }

        public (WaitingGrill waitingGrill, SlotBase slot) GetDestinationSlot()
        {
            foreach (var waitingGrill in listWaitingGrills)
            {
                if (waitingGrill.IsActive == false) continue;
                var slot = waitingGrill.GetSlot(0);
                if (slot.GetItem() == null)
                {
                    return (waitingGrill, slot);
                }
            }

            return (null, null);
        }

        public async UniTask SetData(List<int> listWaitingGrillIds, List<ItemStateData> itemStateDataInWaitingGrill)
        {
            for (int i = 0; i < listWaitingGrillIds.Count; i++)
            {
                var waitingGrill = await GameFactory.Instance.CreateEntityAsync<WaitingGrill>("WaitingGrill", transform);
                waitingGrill.SetId(1000 + listWaitingGrills.Count());
                waitingGrill.SetActive(true);
                listWaitingGrills.Add(waitingGrill);
            }

            if (itemStateDataInWaitingGrill != null)
            {
                foreach (var itemStateData in itemStateDataInWaitingGrill)
                {
                    var waitingGrill = listWaitingGrills.FirstOrDefault(e => e.id == itemStateData.grillId);
                    if (waitingGrill != null)
                    {
                        if (itemStateData.slotIndex >= waitingGrill.GetSlots().Length) continue;

                        var slot = waitingGrill.GetSlot(itemStateData.slotIndex);

                        Item item;
                        if (itemStateData.bombCount >= 0 && itemStateData.active)
                        {
                            item = await GameFactory.Instance.CreateEntityAsync<Item>($"Item{ItemType.Bomb}", waitingGrill.transform);
                            item.SetItemData(
                                new ItemBombData() { id = itemStateData.id, itemType = ItemType.Bomb, moveLimit = itemStateData.bombCount },
                                waitingGrill.GetSlot(itemStateData.slotIndex)
                            );
                            (item as ItemBombMove).StartBomb();
                        }
                        else
                        {
                            item = await GameFactory.Instance.CreateEntityAsync<Item>($"Item{ItemType.Normal}", waitingGrill.transform);
                            item.SetItemData(new ItemData() { id = itemStateData.id, itemType = ItemType.Normal }, waitingGrill.GetSlot(itemStateData.slotIndex));
                        }

                        item.SetIsOnConveyor(false);
                        slot.SetItem(item);
                    }
                }
            }

            await AddLockedWaitingGrill();
            GameplayUtils.AlignObjects(transform, distance);
        }

        public async UniTask<WaitingGrill> AddLockedWaitingGrill()
        {
            var waitingGrill = await GameFactory.Instance.CreateEntityAsync<WaitingGrill>("WaitingGrill", transform);
            waitingGrill.SetActive(false);
            listWaitingGrills.Add(waitingGrill);
            return waitingGrill;
        }

        public async UniTask AlignObjects(Action callback)
        {
            var start = -(transform.childCount - 1) * distance / 2;
            var listLocalTargetPositions = new List<Vector3>();
            for (int i = 0; i < transform.childCount; i++)
            {
                listLocalTargetPositions.Add(new Vector3(start + distance * i, 0, 0));

                var waitingGrill = transform.GetChild(i).GetComponent<WaitingGrill>();
                waitingGrill.transform.DOLocalMove(listLocalTargetPositions[i], 0.1f).SetEase(Ease.OutSine);
                await UniTask.Delay(75);
            }

            callback?.Invoke();
        }

        public void Unlock()
        {
            var lockedWaitingGrill = listWaitingGrills.Where(e => e.IsActive == false).FirstOrDefault();
            if (lockedWaitingGrill != null)
            {
                lockedWaitingGrill.PlayUnlock(listWaitingGrills.Count < maxWaitingGrills);
            }
        }

        public bool CheckClearAllItems()
        {
            foreach (var waitingGrill in listWaitingGrills)
            {
                if (waitingGrill.IsActive && waitingGrill.GetSlots().Where(e => e.GetItem() != null).Count() != 0)
                {
                    return false;
                }
            }
            return true;
        }

        public List<int> GetWaitingGrillIds()
        {
            var list = new List<int>();
            foreach (var waitingGrill in listWaitingGrills)
            {
                if (waitingGrill.IsActive)
                {
                    var item = waitingGrill.GetSlot(0).GetItem();
                    if (item != null)
                    {
                        list.Add(item.id);
                    }
                    else
                    {
                        list.Add(0);
                    }
                }
            }
            return list;
        }

        public List<ItemStateData> GetItemStateDataInWaitingGrill()
        {
            var list = new List<ItemStateData>();
            foreach (var waitingGrill in listWaitingGrills)
            {
                var item = waitingGrill.GetSlot(0).GetItem();
                if (item != null && item is ItemBombMove)
                {
                    list.Add(new ItemStateData() { id = item.id, bombCount = (item as ItemBombMove).MoveRemaining });
                }
                else
                {
                    list.Add(new ItemStateData() { id = 0, bombCount = -1 });
                }
            }
            return list;
        }
    }
}