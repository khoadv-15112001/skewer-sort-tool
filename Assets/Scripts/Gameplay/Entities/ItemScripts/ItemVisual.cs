using DG.Tweening;
using Gameplay.LevelData;
using MyGame.SkewerJam.Scripts.SO.Behavior;
using UnityEngine;
using UnityEngine.Rendering;

namespace Gameplay.Entities.ItemScripts
{
    public class ItemVisual : MonoBehaviour
    {
        [SerializeField] protected Item item;
        [SerializeField] protected SpriteRenderer spriteRenderer;
        [SerializeField] protected SortingGroup sortingGroup;

        protected ItemVisualSO itemVisualSO
        {
            get
            {
                return item.ItemBehaviorSO.itemVisualSO;
            }
        }
        protected int id;
        protected bool isPrimary;

        public virtual void SetVisual(ItemData itemData)
        {
            this.id = itemData.id;
            transform.localScale = Vector3.one;
            UpdateVisual();
        }

        public virtual void SetVisual(Sprite sprite)
        {
            spriteRenderer.sprite = sprite;
        }

        public virtual Sprite GetSprite()
        {
            return spriteRenderer.sprite;
        }

        public virtual void SetPrimary(bool isPrimary)
        {
            this.isPrimary = isPrimary;
        }

        public virtual void UpdateVisual()
        {
            itemVisualSO.SetVisual(spriteRenderer, id, isPrimary);
        }

        public void OnSelected()
        {
            //transform.localScale = Vector3.one * 1.2f;
            DOTween.Kill("ItemScale");
            transform.DOScale(1.1f, 0.15f).SetId("ItemScale");
            SetMaterial(GameResourceReference.Instance.itemMaterials[1]);
            //spriteRenderer.sortingOrder = 100;
            sortingGroup.sortingOrder = 1;
            // tempVisibleState = isConveyorState;
            // SetVisibleMaskState(true);
        }

        public void OnBeginDrag()
        {
            sortingGroup.sortingOrder = 100;
            tempVisibleState = isConveyorState;
            SetVisibleMaskState(false);
        }

        // public void SetNormalLayer()
        // {
        //     sortingGroup.sortingOrder = 1;
        //     SetVisibleMaskState(tempVisibleState);
        // }

        public void OnDeselected()
        {
            DOTween.Kill("ItemScale");
            transform.DOScale(1, 0.15f).SetId("ItemScale");
            SetMaterial(GameResourceReference.Instance.itemMaterials[0]);
        }

        protected virtual void SetMaterial(Material material)
        {
            spriteRenderer.material = material;
        }

        protected bool isConveyorState = false;
        protected bool tempVisibleState = false;

        public virtual void OnIntoSlot()
        {
            sortingGroup.sortingOrder = 1;
            SetVisibleMaskState(item.Slot != null && item.Slot.isOnConveyor);
        }

        public virtual void SetVisibleMaskState(bool state)
        {
            if (state == isConveyorState) return;
            isConveyorState = state;
            switch (state)
            {
                case true:
                    spriteRenderer.maskInteraction = SpriteMaskInteraction.VisibleOutsideMask;
                    break;
                case false:
                    spriteRenderer.maskInteraction = SpriteMaskInteraction.None;
                    break;
            }
        }

        private Sequence suggestionSequence;

        public void Suggest()
        {
            transform.DOKill();
            suggestionSequence = DOTween.Sequence()
                .Append(transform.DOShakePosition(0.125f, Vector3.right * 0.065f, randomness: 0).SetEase(Ease.InOutCubic).SetLoops(4, LoopType.Yoyo))
                .AppendInterval(1f)
                .SetLoops(-1, LoopType.Restart);
        }

        public void EndSuggest()
        {
            if (suggestionSequence != null)
            {
                suggestionSequence.Kill();
                suggestionSequence = null;
            }

            transform.localPosition = Vector3.zero;
        }

        private void OnDisable()
        {
            EndSuggest();
            transform.DOKill();
            transform.localScale = Vector3.one;
            sortingGroup.sortingOrder = 1;
            SetVisibleMaskState(false);
        }

        public void Highlight()
        {
            //OnSelected();
            sortingGroup.sortingLayerName = "UI_Top";
            sortingGroup.sortingOrder = 51;
        }

        public void UnHighlight()
        {
            //OnDeselected();
            sortingGroup.sortingLayerName = "Object";
            sortingGroup.sortingOrder = 1;
        }

        public void SetSortingOrder(int sortingOrder)
        {
            sortingGroup.sortingOrder = sortingOrder;
        }
    }
}