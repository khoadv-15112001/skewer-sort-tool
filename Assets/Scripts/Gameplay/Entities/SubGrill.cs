using System.Collections;
using System.Linq;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using Gameplay.BoosteeManagement;
using Gameplay.Entities.Grills;
using Gameplay.LevelData;
using Manager;
using Sonat.Enums;
using SonatFramework.Scripts.Utils;
using UnityEngine;
using UnityEngine.Rendering;

namespace Gameplay.Entities
{
    public class SubGrill : GrillBase
    {
        private PrimaryGrill primaryGrill;
        private LayerData layerData;
        [SerializeField] private SpriteRenderer visual;
        [SerializeField] private SortingGroup sortingGroup;
        [SerializeField] private string grillVisualName;

        public override EntityType entityType => EntityType.SubGrill;

        public void SetData(PrimaryGrill grill, LayerData layerData, int index)
        {
            this.layerData = layerData;
            primaryGrill = grill;
            sortingGroup.sortingOrder = index + 2;
            SetVisual();
        }

        protected virtual void SetVisual()
        {
            grillBaseBehaviorSO.grillVisualSO.SetSubGrillVisual(this, visual, grillVisualName);
        }

        public void MoveUpPrimary()
        {
            int itemIndex = 0;
            for (int i = 0; i < slots.Length; i++)
            {
                var item = slots[i].GetItem();
                if (item != null)
                {
                    primaryGrill.AddFromSub(item, i, itemIndex);
                    itemIndex++;
                }
            }

            visual.DOFade(0, 0.5f).SetDelay(0.5f).OnComplete(() => { GameFactory.ReturnEntity(this); });
            //MoveNextItem(0);
        }
        // private void MoveNextItem(int index)
        // {
        //     if (index >= slots.Length)
        //     {
        //         visual.DOFade(0, 0.5f).SetDelay(0.5f).OnComplete(() => { 
        //             GameFactory.ReturnEntity(this); 
        //         });
        //         return;
        //     }
        //
        //     var item = slots[index].GetItem();
        //     if (item != null)
        //     {
        //         primaryGrill.AddFromSub(item, index);
        //     }
        //     SonatUtils.DelayCall(0.1f, () => MoveNextItem(index + 1), this);
        // }

        public void Show()
        {
            SetLayer(layerData);
        }

        public LayerData GetCurrentData()
        {
            if (!showed)
            {
                return layerData;
            }
            else
            {
                LayerData _layerData = new LayerData(slots.Length);
                for (int i = 0; i < slots.Length; i++)
                {
                    var item = slots[i].GetItem();
                    if (item != null && item.Data != null)
                    {
                        _layerData.itemData[i] = item.Data.Clone();
                    }
                    else
                    {
                        _layerData.itemData[i] = new ItemData();
                    }
                }

                return _layerData;
            }
        }

        public override ShuffleLayerData GetShuffleLayerData()
        {
            return new ShuffleLayerData()
            {
                grill = this,
                layerData = GetCurrentData(),
                canNotShuffle = !CanShuffle()
            };
        }
        
        public virtual bool CanShuffle()
        {
            if (primaryGrill.IsLock) return false;
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
            this.layerData = layerData;
            if (showed)
            {
                SonatUtils.DelayCall(GameDefine.grillLidAnim, () =>
                {
                    RevokeLayer();
                    //SonatUtils.DelayCall(GameDefine.itemScaleIntro + 0.25f, () => SetLayer(layerData), this);
                }, this);
            }
        }

        public override ShuffleLayerData GetMagnetLayerData()
        {
            return new ShuffleLayerData()
            {
                grill = this,
                layerData = GetCurrentData()
            };
        }

        public void ForceClearGrill()
        {
            visual.DOFade(0, 0.5f).SetDelay(0.5f).OnComplete(() => { GameFactory.ReturnEntity(this); });
        }

        public override Vector3 DestroyItem(int id)
        {
            Vector3 pos = transform.position;
            for (int i = 0; i < layerData.itemData.Length; i++)
            {
                var itemData = layerData.itemData[i];
                if (itemData != null && itemData.id == id)
                {
                    if (itemData.itemType == ItemType.Key)
                    {
                        PrimaryGrillLockAndKey.grillLock.OnCollectItemKey(null);
                    }

                    layerData.itemData[i] = null;
                    break;
                }
            }

            if (showed)
            {
                foreach (var slot in slots)
                {
                    if (slot.GetItem() != null && slot.GetItem().id == id)
                    {
                        slot.GetItem()?.OnComplete();
                        slot.ClearItem();
                        pos = slot.transform.position;
                        break;
                    }
                }
            }

            return pos;
        }

        public override void ChangeItem(SlotBase selectedSlot, int newId)
        {
            int index = 0;
            foreach (var slot in slots)
            {
                if (slot == selectedSlot)
                {
                    Debug.Log($"ChangeItem: {layerData.itemData[index].id} {newId}");
                    layerData.itemData[index].id = newId;

                    var item = slot.GetItem();
                    if (item != null)
                    {
                        var itemData = item.Data.Clone();
                        itemData.id = newId;
                        item.SetItemData(itemData, slot);
                    }

                    return;
                }

                index++;
            }
        }


        public bool IsEmpty()
        {
            // if (showed)
            // {
            //     foreach (var slot in slots)
            //     {
            //         if (!slot.isEmpty()) return false;
            //     }
            //     return true;
            // }
            return !layerData.ValidateData();
        }

        public int GetEmptySlot()
        {
            if (showed)
            {
                for (int i = 0; i < slots.Length; i++)
                {
                    if (slots[i].isEmpty()) return i;
                }
            }
            else
            {
                for (int i = 0; i < layerData.itemData.Length; i++)
                {
                    if (layerData.itemData[i] == null || layerData.itemData[i].id == 0) return i;
                }
            }

            return -1;
        }

        public async UniTask CreateSpecialItem(ItemData itemData, int slot)
        {
            if (showed)
            {
                Item item = await CreateItem(itemData);
                item.SetItemData(itemData, slots[slot]);
                item.SetIsOnConveyor(isOnConveyor);
                slots[slot].SetItem(item);
            }
            else
            {
                if (layerData.itemData == null) layerData.itemData = new ItemData[slots.Length];
                layerData.itemData[slot] = itemData.Clone();
            }
        }

        public override void OnReturnObj()
        {
            base.OnReturnObj();
            visual.SetAlpha(1);
            showed = false;
            transform.localScale = Vector3.one;
        }

        public override void SetMaskVisible(bool state)
        {
            base.SetMaskVisible(state);
            visual.maskInteraction = state ? SpriteMaskInteraction.VisibleOutsideMask : SpriteMaskInteraction.None;
        }

        public override bool CheckItemWithId(int id)
        {
            return showed ? slots.Any(slot => slot.GetItem() != null && slot.GetItem().id == id) : layerData.itemData.Any(t => t == null || t.id == id);
        }
    }
}