using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using Gameplay.BoosteeManagement;
using Gameplay.Entities.GrillScripts;
using Gameplay.Entities.ItemScripts;
using Gameplay.Entities.Obstacle;
using Gameplay.Entities.Orders;
using Gameplay.LevelData;
using Sonat.Enums;
using SonatFramework.Scripts.Utils;
using SonatFramework.Systems.SettingsManagement.Vibation;
using UnityEngine;

namespace Gameplay.Entities
{
    public class PrimaryGrill : GrillBase
    {
        public override EntityType entityType => EntityType.PrimaryGrill;
        [SerializeField] protected Transform container;
        [SerializeField] protected Transform subContainer;
        [SerializeField] protected GrillVisual grillVisual;
        [SerializeField] protected GrillInteractionLayer grillInteractionLayer;

        [HideInInspector] public bool available;
        public GrillVisual GrillVisual => grillVisual;
        protected GrillData grillData;
        protected List<SubGrill> subGrills;
        protected bool isProcess = false;
        [HideInInspector] public bool isInGameplaySpace = true;
        protected Vector3 subOffset;
        public Action<PrimaryGrill> onMainLayerEmpty;
        public Action<PrimaryGrill> onMainLayerComplete;
        public Transform SubContainer => subContainer;
        protected List<ObstacleBase> obstacles;
        [SerializeField] private DestroyAfterTime EffectSprinkleObj;

        //protected bool isFirstUnlock = true; // đã được unlock lần đầu

        protected virtual void ResetData()
        {
            isProcess = false;
            lockState = 0;
            onMainLayerEmpty = null;
            onMainLayerComplete = null;
            //isFirstUnlock = true;
        }

        public virtual async UniTask SetData(GrillData grillData)
        {
            grillVisual.SetDefaultGrill(grillData);
            grillInteractionLayer?.gameObject.SetActive(false);

            this.grillData = grillData;
            id = grillData.id;
            SetSlotId();

            grillBaseBehaviorSO.SetSubOffset(this, out subOffset);

            transform.position = grillData.position.ToVector3();
            if (this.grillData.layer != null && this.grillData.layer.Count > 0)
            {
                await SetLayer(grillData.layer[0]);
                if (grillData.layer.Count > 1)
                {
                    subGrills = new List<SubGrill>();
                    await SetSubGrills();
                }
            }

            isInGameplaySpace = grillData.position.y < LevelGenerator.maxYGameSpace;
            OpenGrill();
        }

        public virtual void SetSlotId()
        {
            for (int i = 0; i < slots.Length; i++)
            {
                slots[i].SetId(this.id * 1000 + i);
            }
        }

        public virtual void OpenGrill()
        {
            SonatUtils.DelayCall(0.75f, () => grillVisual.OpenGrill(), this);
        }

        protected virtual async UniTask SetSubGrills()
        {
            int layer = 0;
            for (int i = this.grillData.layer.Count - 1; i > 0; i--)
            {
                var subGrill = await CreateSubGrill(layer);
                subGrill.transform.localScale = Vector3.one;
                subGrill.grillType = this.grillType;
                subGrill.SetData(this, this.grillData.layer[i], layer);
                if (i == 1)
                {
                    subGrill.Show();
                }

                subGrills.Insert(0, subGrill);
                layer++;
            }
        }

        protected virtual async UniTask<SubGrill> CreateSubGrill(int layer)
        {
            Vector3 pos = subContainer.position + Vector3.up * layer * 0.035f + Vector3.back * layer * 0.03f + subOffset;
            string subGrillName = "SubGrillNormal";
            return await grillBaseBehaviorSO.gameFactorySO.CreateItem<SubGrill>(subGrillName, pos, subContainer);
        }

        protected virtual void UpdateSubGrills()
        {
            grillVisual.UpdateSubGrill();
            if (subGrills == null || subGrills.Count == 0)
            {
                isProcess = false;
                return;
            }

            subGrills[0].MoveUpPrimary();
            subGrills.RemoveAt(0);
            if (subGrills.Count > 0)
            {
                subGrills[0].Show();
            }

            if (IsLock)
                SetLockItems(IsLock);
            isProcess = false;
        }

        public virtual void ClearGrill()
        {
        }

        public virtual SlotBase GetNearestSlot(Vector3 position)
        {
            if (isProcess || IsLock || !available) return null;
            float minDistance = float.MaxValue;
            SlotBase nearestSlot = null;
            Item selectedItem = GameplayController.instance.itemSelected;

            if (GameplayController.swapItem)
            {
                int slotSame = 0;
                int slotEmpty = 0;
                foreach (var slot in slots)
                {
                    if (!slot.isEmpty() && slot.GetItem().id == selectedItem.id) slotSame++;
                    else if (slot.isEmpty() || !slot.GetItem().IsLocked)
                    {
                        nearestSlot = slot;
                    }
                }

                if (slotSame == slots.Length - 1 && nearestSlot != null) return nearestSlot;

                foreach (var slot in slots)
                {
                    if (slot.GetItem() != null && slot.GetItem().IsLocked) continue;
                    //if(selectedItem != null && slot.GetItem() == selectedItem) return slot;
                    float distance = Vector3.SqrMagnitude(position - slot.transform.position);
                    //if ((slot.isEmpty() || slot.GetItem() == selectedItem) && distance < minDistance)
                    if ((slot.isEmpty() || slot.GetItem() == selectedItem)) distance -= 0.3f;
                    if (distance < minDistance)
                    {
                        minDistance = distance;
                        nearestSlot = slot;
                    }
                }
            }
            else
            {
                foreach (var slot in slots)
                {
                    float distance = Vector3.SqrMagnitude(position - slot.transform.position);
                    if ((slot.isEmpty() || slot.GetItem() == selectedItem) && distance < minDistance)
                    {
                        minDistance = distance;
                        nearestSlot = slot;
                    }
                }
            }

            return nearestSlot;
        }

        public override void OnSlotUpdated(SlotBase slot)
        {
            base.OnSlotUpdated(slot);
            if (slot.isEmpty())
            {
                SonatUtils.ExecuteNextFrame(CheckEmpty);
                //CheckEmpty();
            }
            else
            {
                //SonatUtils.ExecuteNextFrame(() => CheckComplete());
                CheckComplete();
            }

            grillBaseBehaviorSO.OnSlotUpdated(slot);
        }

        public virtual void CheckEmpty()
        {
            foreach (var slot in slots)
            {
                if (!slot.isEmpty())
                {
                    isProcess = false;
                    return;
                }
            }

            UpdateSubGrills();
            onMainLayerEmpty?.Invoke(this);
        }

        // public bool IsFull()
        // {
        //     for (int i = 0; i < slots.Length; i++)
        //     {
        //         if (slots[i].isEmpty()) return false;
        //     }
        //
        //     return true;
        // }

        public virtual int NumberSlotEmpty()
        {
            if (IsLock || !available) return 0;
            int count = 0;
            for (int i = 0; i < slots.Length; i++)
            {
                if (slots[i].isEmpty() || slots[i].GetItem()?.id >= 1000) count++;
            }

            return count;
        }

        public virtual int NumberDifferentItems(int id)
        {
            if (!available) return 0;
            int count = 0;
            for (int i = 0; i < slots.Length; i++)
            {
                if (!slots[i].isEmpty() && slots[i].GetItem()?.id != id) count++;
            }

            return count;
        }

        public virtual bool CheckCompleteLogic()
        {
            if (SlotCount == 1) return false;
            int id = 0;
            foreach (var slot in slots)
            {
                if (slot.isEmpty()) return false;
                if (id == 0) id = slot.GetItem().id;
                else if (id != slot.GetItem().id) return false;
            }

            // Prevent obstacle items (ID 1500) from matching/combining
            if (id == 1500) return false;
            for (int i = 0; i < slots.Length; i++)
            {
                slots[i].GetItem().itemState = ItemState.Completed;
            }

            return true;
        }

        public virtual bool CheckComplete()
        {
            if (SlotCount == 1) return false;
            int id = 0;
            foreach (var slot in slots)
            {
                if (slot.isEmpty()) return false;
                if (id == 0) id = slot.GetItem().id;
                else if (id != slot.GetItem().id) return false;
            }

            // Prevent obstacle items (ID 1500) from matching/combining
            if (id == 1500) return false;

            OnComplete();
            return true;
        }

        protected virtual void OnComplete()
        {
            isProcess = true;
            Vector3[] pos = new Vector3[slots.Length];
            int id = slots[0].GetItem().id;
            for (int i = 0; i < slots.Length; i++)
            {
                slots[i].GetItem()?.OnComplete();
                slots[i].ClearItem();
                pos[i] = slots[i].transform.localPosition;
            }

            OrderEntity orderEntity = GameplayController.instance.levelGenerator.GetOrderQueue().PopOrderEntity(id, this);

            if (GameplayController.instance.levelGenerator.LevelMode == LevelMode.All)
            {
                var delay = 0.2f;
                if (EffectSprinkleObj != null)
                {
                    EffectSprinkleObj.gameObject.SetActive(true);

                    delay = 0.5f;
                }
                //else
                //Debug.Log("anhnt: effectObj is null");

                var effect = GameFactory.CreateEntity<CollectEffect>("CollectEffect", container.position, container);
                effect.SetData(pos, id, this, orderEntity, delay);
            }
            else if (GameplayController.instance.levelGenerator.LevelMode == LevelMode.Target)
            {
                var effect = GameFactory.CreateEntity<CollectEffectTarget>("CollectEffectTarget", container.position, container);
                effect.SetData(pos, id, this, orderEntity);
            }

            //MySonatFramework.audioService.PlaySound(AudioId.Items_Merge);
            MySonatFramework.GetService<VibrationService>().Vibrate(50);

            if (GameplayController.instance.CheckDoubleSwap())
            {
                GameplayController.instance.SetBonusTime(this);
                GameplayController.instance.ResetDoubleSwap();
            }

            GameplayController.instance.CollectItem(id, container.position, slots.Length);
            isProcess = true;
            SonatUtils.DelayCall(GameDefine.timeDelaySubGrill, UpdateSubGrills, this);
            onMainLayerComplete?.Invoke(this);
        }


        public virtual void AddFromSub(Item item, int slotIndex, int index)
        {
            grillBaseBehaviorSO.OnAddFromSub(this, item, slotIndex, index);
        }

        public virtual LayerData GetCurrentData()
        {
            LayerData _layerData = new LayerData(slots.Length);
            for (int i = 0; i < slots.Length; i++)
            {
                var item = slots[i].GetItem();
                if (item != null && item.Data != null)
                {
                    _layerData.itemData[i] = item.GetCurrentItemData();
                }
                else
                {
                    _layerData.itemData[i] = new ItemData() { id = 0 };
                }
            }

            return _layerData;
        }

        public virtual List<ShuffleLayerData> GetSubsShuffleLayerData()
        {
            List<ShuffleLayerData> data = new List<ShuffleLayerData>();
            if (subGrills != null)
            {
                foreach (var subGrill in subGrills)
                {
                    data.Add(subGrill.GetShuffleLayerData());
                }
            }

            return data;
        }

        public virtual List<ShuffleLayerData> GetSubsMagnetLayerData()
        {
            List<ShuffleLayerData> data = new List<ShuffleLayerData>();
            if (subGrills != null)
            {
                foreach (var subGrill in subGrills)
                {
                    data.Add(subGrill.GetMagnetLayerData());
                }
            }

            return data;
        }

        public virtual List<SubGrill> GetSubGrills()
        {
            return subGrills;
        }

        public virtual int SubGrillsCount()
        {
            if (subGrills == null) return 0;
            return subGrills.Count;
        }

        public override ShuffleLayerData GetShuffleLayerData()
        {
            return new ShuffleLayerData()
            {
                canNotShuffle = !CanShuffle(),
                grill = this,
                layerData = GetCurrentData()
            };
        }

        public virtual bool CanShuffle()
        {
            if (IsLock) return false;
            foreach (var slot in slots)
            {
                var item = slot.GetItem();
                if (item != null && item.Data != null)
                {
                    if (item.itemType is ItemType.Ice or ItemType.Bomb or ItemType.Key or ItemType.Key2 or ItemType.KeyArea) return false;
                }
            }

            return true;
        }

        public override void SetShuffleLayerData(LayerData layerData)
        {
            grillVisual.CloseGrill();
            SonatUtils.DelayCall(GameDefine.grillLidAnim, () =>
            {
                RevokeLayer();
                SetLayer(layerData);
                if (IsLock)
                {
                    SetLockItems(true);
                }

                SonatUtils.DelayCall(GameDefine.itemScaleIntro + 0.2f, () =>
                {
                    if (grillType != GrillType.Vending) grillVisual.OpenGrill();
                    CheckSubGrills();
                    CheckComplete();
                    SonatUtils.DelayCall(GameDefine.itemScaleIntro, CheckEmpty, this);
                }, this);
            }, this);
        }

        public override ShuffleLayerData GetMagnetLayerData()
        {
            return new ShuffleLayerData()
            {
                canNotShuffle = !CanShuffle(),
                grill = this,
                layerData = GetCurrentData()
            };
        }

        public virtual void CheckSubGrills(bool forceShowFirst = true)
        {
            if (subGrills == null || subGrills.Count == 0) return;
            int index = 0;
            bool updateFirst = false;
            while (index < subGrills.Count)
            {
                var subGrill = subGrills[index];
                if (subGrill.IsEmpty())
                {
                    if (index == 0) updateFirst = true;
                    subGrill.ForceClearGrill();
                    subGrills.Remove(subGrill);
                }
                else
                {
                    index++;
                }
            }

            if ((updateFirst || forceShowFirst) && subGrills.Count > 0) subGrills[0].Show();
        }

        public override Vector3 DestroyItem(int id)
        {
            Vector3 pos = transform.position;
            foreach (var slot in slots)
            {
                if (slot.GetItem() != null && slot.GetItem().id == id)
                {
                    slot.GetItem().OnComplete();
                    slot.ClearItem();
                    pos = slot.transform.position;
                    break;
                }
            }

            return pos;
        }


        public override void ChangeItem(SlotBase selectedSlot, int newId)
        {
            foreach (var slot in slots)
            {
                if (slot == selectedSlot)
                {
                    Debug.Log($"ChangeItem: {slot.GetItem().id} {newId}");
                    var item = slot.GetItem();
                    var itemData = item.Data.Clone();
                    itemData.id = newId;
                    item.SetItemData(itemData, slot);
                    return;
                }
            }

            if (subGrills != null)
            {
                foreach (var subGrill in subGrills)
                {
                    subGrill.ChangeItem(selectedSlot, newId);
                }
            }
        }

        #region Obstacle

        //private Dictionary<ObstacleType, ObstacleBase> obstacles = new();

        // public virtual void AddObstacle(ObstacleBase obstacle)
        // {
        //     obstacles.TryAdd(obstacle.ObstacleType, obstacle);
        // }

        // public virtual void RemoveObstacle(ObstacleType obstacleType)
        // {
        //     obstacles.Remove(obstacleType);
        // }

        // public override bool CheckObstacle(ObstacleType obstacleType)
        // {
        //     if (obstacleType == ObstacleType.Lock)
        //     {
        //         return isLock;
        //     }
        //
        //     return obstacles.ContainsKey(obstacleType);
        // }

        public virtual void AddLockState(byte stateAdd = 1)
        {
            lockState += stateAdd;
            foreach (var slot in slots)
            {
                slot.GetItem()?.SetLockState(true);
            }
        }

        public virtual void SetLockItems(bool lockState)
        {
            this.lockState = lockState ? (byte)1 : (byte)0;
            foreach (var slot in slots)
            {
                slot.GetItem()?.SetLockState(lockState);
            }
        }

        #endregion


        public virtual bool HasSubGrills()
        {
            return subGrills is { Count: > 0 };
        }

        public virtual Bounds GetSubGrillPosition()
        {
            return new Bounds(subContainer.position, Vector3.one);
            //return subContainer.position;
        }

        public override void SetMaskVisible(bool state)
        {
            base.SetMaskVisible(state);
            grillVisual.SetMaskVisible(state);
            if (subGrills != null)
            {
                foreach (var subGrill in subGrills)
                {
                    subGrill.SetMaskVisible(state);
                }
            }
        }

        public override void OnCreateObj(params object[] args)
        {
            base.OnCreateObj(args);
            available = true;
        }

        public override void OnReturnObj()
        {
            transform.DOKill();
            StopAllCoroutines();
            if (subGrills != null)
            {
                foreach (var subGrill in subGrills)
                {
                    grillBaseBehaviorSO.gameFactorySO.ReturnEntity(subGrill);
                }

                subGrills = null;
            }

            ResetData();
            SetMaskVisible(false);
            grillVisual.OnReturnGrill();
            transform.localScale = Vector3.one;
            base.OnReturnObj();
            obstacles = null;
        }

        public virtual bool CreateSpecialItem(int itemId)
        {
            if (subGrills == null || subGrills.Count == 0 || slots.Length < 3) return false;
            List<SubGrill> subGrillsTemp = new List<SubGrill>(subGrills);
            subGrillsTemp.Shuffle();
            foreach (var subGrill in subGrillsTemp)
            {
                int emptySlot = subGrill.GetEmptySlot();
                if (emptySlot >= 0)
                {
                    subGrill.CreateSpecialItem(new ItemData() { id = itemId, itemType = ItemType.Special }, emptySlot);
                    return true;
                }
            }

            return false;
        }

        public virtual void EnableClick()
        {
            grillInteractionLayer?.gameObject.SetActive(true);
            SetHighlight(true);
        }

        public void DisableClick()
        {
            grillInteractionLayer?.gameObject.SetActive(false);
            SetHighlight(false);
        }

        public virtual void SetHighlight(bool state)
        {
            if (state)
            {
                grillVisual.HighlightGrill();
            }
            else
            {
                grillVisual.UnHighlightGrill();
            }
        }

        protected void SetSlotCollider(bool state)
        {
            foreach (var slot in slots)
            {
                BoxCollider2D collider2D = slot.GetComponent<BoxCollider2D>();
                collider2D.enabled = state;
            }
        }

        public virtual void UnlockState()
        {
            if (!IsLock) return;
            lockState--;
            if (lockState <= 0)
            {
                Unlock();
            }
        }

        public virtual void Unlock()
        {
            SetLockItems(false);
            OpenGrill();
            GameplayController.OnActionUnlockGrill?.Invoke(this);
        }

        public Bounds GetBounds()
        {
            return grillVisual != null ? grillVisual.GetGrillBounds() : new Bounds(transform.position, Vector3.one);
        }

        public void TapOnGrill()
        {
            Item item = GetNearestItem();
            if (item != null && !item.CanNotTouch())
            {
                Vector3 point = GameplayController.instance.mainCamera.ScreenToWorldPoint(Input.mousePosition);
                point.z = item.transform.position.z;
                item.transform.position = point;
                item.OnMouseDown();
            }
        }

        private Item GetNearestItem()
        {
            if (isProcess || IsLock) return null;
            float minDistance = float.MaxValue;
            Item nearestItem = null;
            Vector3 point = GameplayController.instance.mainCamera.ScreenToWorldPoint(Input.mousePosition);
            point.z = slots[0].transform.position.z;
            foreach (var slot in slots)
            {
                Item item = slot.GetItem();
                if (item == null) continue;
                float distance = Vector3.Distance(point, slot.transform.position);
                if (distance < minDistance && distance < 1f)
                {
                    minDistance = distance;
                    nearestItem = item;
                }
            }

            return nearestItem;
        }

        public virtual bool CanRevokeGrill(bool complete)
        {
            return true;
        }

        public override bool CheckItemWithId(int id)
        {
            return slots.Any(slot => slot.GetItem() != null && slot.GetItem().id == id);
        }

        public virtual void OnResetConveyorCircle()
        {
        }

        public virtual GrillData GetGrillData()
        {
            return new GrillData()
            {
                id = grillData.id,
                position = grillData.position,
                layer = GetLayerData(),
                grillType = grillType,
                isLock = grillData.isLock,
                slotCount = grillData.slotCount
            };
        }

        public virtual List<LayerData> GetLayerData()
        {
            var layerData = new List<LayerData>() { GetCurrentData() };
            if (subGrills != null)
            {
                foreach (var subGrill in subGrills)
                {
                    layerData.Add(subGrill.GetCurrentData());
                }
            }

            return layerData;
        }

        public void AddObstacle(ObstacleBase obstacle)
        {
            if (obstacles == null) obstacles = new List<ObstacleBase>();
            obstacles.Add(obstacle);
        }

        public void RemoveObstacle(ObstacleBase obstacle)
        {
            if (obstacles == null) return;
            obstacles.Remove(obstacle);
        }

        public virtual bool CheckObstacles(ObstacleType obstacleType)
        {
            if (obstacles == null) return false;
            return obstacles.FindIndex(e => e.ObstacleType == obstacleType) >= 0;
        }

        public bool CheckCanOctoChef()
        {
            if (IsLock) return false;
            foreach (var slot in slots)
            {
                Item item = slot.GetItem();
                if (item != null && item.itemType is ItemType.Bomb or ItemType.Key or ItemType.Key2 or ItemType.KeyArea) return false;
            }

            return true;
        }

        public override bool ItemCanTap()
        {
            return !isProcess && base.ItemCanTap();
        }
    }
}