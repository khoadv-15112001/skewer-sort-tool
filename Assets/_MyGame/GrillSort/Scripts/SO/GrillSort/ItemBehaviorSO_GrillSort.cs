using DG.Tweening;
using Gameplay;
using Gameplay.Entities;
using Gameplay.Entities.Items;
using UnityEngine;
using Cysharp.Threading.Tasks;
using static Gameplay.Entities.Item;
using Sonat.Enums;
using Gameplay.LevelData;

namespace MyGame.SkewerJam.Scripts.SO.Behavior
{
    [CreateAssetMenu(fileName = "ItemBehaviorSO_GrillSort", menuName = "MyGame/GrillSort/ItemBehaviorSO_GrillSort")]
    public class ItemBehaviorSO_GrillSort : ItemBehaviorSO
    {
        public override void OnMouseDown(Item item)
        {
            if (item.CanNotTouch()) return;
            item.MouseDownPos = Input.mousePosition;
            Vector3 mousePos = GameplayController.instance.mainCamera.ScreenToWorldPoint(Input.mousePosition);
            mousePos.z = item.transform.position.z;
            item.Offset = item.transform.position - mousePos;
            item.SetItemState(ItemState.Selected);
            item.OnSelected();
        }

        public override void OnMouseUp(Item item)
        {
            // Debug.Log("ItemBehaviorSO_GrillSort");
        }

        public override void OnMouseExit(Item item)
        {
            // Debug.Log("ItemBehaviorSO_GrillSort");
        }

        public override void SwitchSlot(Item item, SlotBase slot)
        {
            item.transform.DOKill();
            if (item.Slot != null)
            {
                item.Slot.ItemOut();
            }

            if (item.PlaceHolder != null)
            {
                GameFactory.ReturnEntity(item.PlaceHolder);
                item.SetPlaceHolder(null);
            }

            item.SetSlot(slot);
            slot.AddItem(item);
            item.Visual.OnDeselected();
            item.transform.DOLocalMove(Vector3.zero, GameDefine.itemMoveBackDuration).SetSpeedBased(false).OnComplete(() =>
            {
                item.OnDropToSlot(slot);
            });
        }

        public override void OnExplodeBomb(ItemBombMove itemBombMove)
        {
            GameplayController.instance.ExplosiveBomb().Forget();
        }


        public override void OnSelected(Item item)
        {
            item.Visual.OnSelected();
            item.Slot.OnItemSelected();
            GameplayController.instance.SelectItem(item);
            if (item.MatType is ItemMatType.Food)
            {
                MySonatFramework.audioService.PlaySound(AudioId.Items_Pick);
                //MySonatFramework.audioService.PlaySound((AudioId)Random.Range(20, 23));
            }
            else
            {
                MySonatFramework.audioService.PlaySound(AudioId.Items_Direct_Pick_Grill_sort);
            }
        }

        public override void PlayDropSound(Item item)
        {
            
            if (item.Slot.GetGrill().grillType == GrillType.Broken) return;
            switch (item.MatType)
            {
                case ItemMatType.Food:
                    if (LevelGenerator.dropMode)
                        MySonatFramework.audioService.PlaySound(AudioId.Items_Put_SMode_Drop_line_Grill_sort);
                    else
                        MySonatFramework.audioService.PlaySound($"Items_Put_{Random.Range(0, 3)}");
                    break;
                case ItemMatType.FruitGlass:
                    MySonatFramework.audioService.PlaySound(AudioId.Items_Glass_Put_Grill_sort);
                    break;
                case ItemMatType.FruitNormal:
                    MySonatFramework.audioService.PlaySound(AudioId.Items_Direct_Put_Grill_sort);
                    break;
                case ItemMatType.FruitPlastic:
                    MySonatFramework.audioService.PlaySound(AudioId.Items_Tray_Put_Grill_sort);
                    break;
            }
        }
    }
}