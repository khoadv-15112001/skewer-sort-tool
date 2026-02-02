using Cysharp.Threading.Tasks;
using DG.Tweening;
using Gameplay.BoosteeManagement;
using Gameplay.LevelData;
using SkewerJam.SO.Behavior;
using SonatFramework.Systems.ObjectPooling;
using UnityEngine;

namespace Gameplay.Entities
{
    public abstract class GrillBase : EntityBase, IPoolingObject
    {
        public GrillType grillType;
        [SerializeField] protected SlotBase[] slots;
        protected bool showed = false;
        public int SlotCount => slots.Length;
        protected bool isOnConveyor;
        protected byte lockState;
        public bool IsLock => lockState > 0;
        public int LockState => lockState;

        [Space(10)] [Header("Behavior SO")] [SerializeField]
        protected GrillBaseBehaviorSO grillBaseBehaviorSO;

        public GrillBaseBehaviorSO GrillBaseBehaviorSO => grillBaseBehaviorSO;

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (slots == null || slots.Length == 0) slots = GetComponentsInChildren<SlotBase>();
        }
#endif
        public virtual async UniTask SetLayer(LayerData layerData)
        {
            ClearGrill();
            int count = 0;
            for (int i = 0; i < layerData.itemData.Length; i++)
            {
                if (layerData.itemData[i] != null && layerData.itemData[i].id > 0)
                {
                    if (count > slots.Length - 1)
                    {
                        break;
                    }

                    var item = await CreateItem(layerData.itemData[i]);
                    item.SetItemData(layerData.itemData[i], slots[i]);
                    item.SetIsOnConveyor(isOnConveyor);
                    slots[i].SetItem(item);
                    count++;
                }
            }

            showed = true;
        }


        public virtual void ClearGrill()
        {
            for (int i = 0; i < slots.Length; i++)
            {
                slots[i].ClearItem();
            }
        }

        protected async UniTask<Item> CreateItem(ItemData itemData)
        {
            var itemType = grillBaseBehaviorSO.ValidateItemType(itemData.itemType);
            Item item = await grillBaseBehaviorSO.gameFactorySO.CreateItem<Item>($"Item{itemData.itemType}");
            item.SetPrimary(entityType == EntityType.PrimaryGrill);
            return item;
        }

        public virtual void OnSlotUpdated(SlotBase slot)
        {
        }


        public abstract ShuffleLayerData GetShuffleLayerData();
        public abstract void SetShuffleLayerData(LayerData layerData);

        public abstract ShuffleLayerData GetMagnetLayerData();

        public virtual void RevokeLayer()
        {
            for (int i = 0; i < slots.Length; i++)
            {
                var slot = slots[i];
                Item it = slot.GetItem();
                slot.SetItem(null);
                if (it != null)
                {
                    it.transform.DOScale(0, GameDefine.itemScaleIntro).SetEase(Ease.InBack).OnComplete(() => { GameFactory.ReturnEntity(it); });
                    //slot.ClearItem();
                }
            }
        }

        public abstract Vector3 DestroyItem(int id);

        public abstract void ChangeItem(SlotBase slot, int newId);

        public SlotBase[] GetSlots()
        {
            return slots;
        }

        public SlotBase GetSlot(int index)
        {
            return slots[index];
        }

        public virtual void SetMaskVisible(bool state)
        {
            this.isOnConveyor = state;
            for (int i = 0; i < slots.Length; i++)
            {
                slots[i].SetConveyor(state);
            }
        }

        public virtual void Setup()
        {
        }

        public virtual void OnCreateObj(params object[] args)
        {
        }

        public virtual void OnReturnObj()
        {
            for (int i = 0; i < slots.Length; i++)
            {
                slots[i].ClearItem();
            }

            showed = false;
            isOnConveyor = false;
        }

        public virtual bool ItemCanTap()
        {
            return !IsLock;
        }

        public abstract bool CheckItemWithId(int id);
    }
}