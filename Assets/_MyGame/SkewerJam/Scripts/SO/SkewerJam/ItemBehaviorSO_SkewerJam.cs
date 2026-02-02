using Gameplay.Entities;
using MyGame.SkewerJam.Gameplay;
using UnityEngine;
using DG.Tweening;
using Sonat.Enums;
using Gameplay.Entities.Items;
using Sirenix.OdinInspector;
using SonatFramework.Systems.SettingsManagement.Vibation;
using Gameplay.LevelData;
using Cysharp.Threading.Tasks;
using SonatFramework.Scripts.Utils;

namespace MyGame.SkewerJam.Scripts.SO.Behavior
{
    [CreateAssetMenu(fileName = "ItemBehaviorSO_SkewerJam", menuName = "MyGame/SkewerJam/ItemBehaviorSO_SkewerJam")]
    public class ItemBehaviorSO_SkewerJam : ItemBehaviorSO
    {
        [Space(10)]
        [Header("Animation")]
        [SerializeField] private Vector3 scaleDown = new Vector3(0.8f, 1.1f, 1f);
        [SerializeField] private float scaleDuration = 0.05f;
        [SerializeField] private bool useSpeed = false;
        [SerializeField, ShowIf("@useSpeed == false")] private float durationMove = 0.4f;
        [SerializeField, ShowIf("useSpeed")] private float speed = 1f;
        [SerializeField] private AnimationCurve curveX = AnimationCurve.Linear(0, 0, 1, 1);
        [SerializeField] private AnimationCurve curveY = AnimationCurve.Linear(0, 0, 1, 1);

        private Item selectedItem;

        public override void OnSelected(Item item)
        {
            item.Visual.OnSelected();
            item.Slot.OnItemSelected();
        }

        public override void PlayDropSound(Item item)
        {
            // if (item.Slot.GetGrill().grillType == GrillType.Broken) return;
            // MySonatFramework.audioService.PlaySound(AudioId.Items_Pick_SMode_HLW_Grill_sort);
        }

        public override void OnMouseDown(Item item)
        {
            if (!item.isPrimary
            || item.itemState != ItemState.Idle
            || item.IsLocked
            || GameController.Instance.GameState != GameState.Playing
            || item.Slot.GetGrill().IsLock) return;

            selectedItem = item;
            item.DOKill();
            item.transform.DOScale(scaleDown, scaleDuration);
        }

        public override void OnMouseExit(Item item)
        {
            if (selectedItem == item)
            {
                selectedItem = null;
            }
            item.DOKill();
            item.transform.DOScale(Vector3.one, scaleDuration);
        }

        public override void OnMouseUp(Item item)
        {
            if (GameController.Instance.GameState != GameState.Playing) return;

            if (selectedItem == item)
            {
                var gameLogicHandler = GameController.Instance.GameLogicHandler;
                var switchSuccess = gameLogicHandler.SelectItem(item);
                MySonatFramework.GetService<VibrationService>().Vibrate(50);
                MySonatFramework.audioService.PlaySound(AudioId.Items_Pick_SMode_HLW_Grill_sort);
                if (switchSuccess == false)
                {
                    item.transform.DOKill();
                    item.transform.DOScale(Vector3.one, scaleDuration);
                }
            }
            else
            {
                if (selectedItem != null)
                {
                    selectedItem.transform.DOKill();
                    selectedItem.transform.DOScale(Vector3.one, scaleDuration);
                }
            }
            selectedItem = null;

        }

        public override void SwitchSlot(Item item, SlotBase slot)
        {
            item.itemState = ItemState.Moving;
            item.transform.DOKill();
            if (item.Slot != null)
            {
                item.Slot.ItemOut();
            }

            item.SetSlot(slot);
            slot.AddItem(item);
            item.Visual.OnDeselected();

            item.Visual.SetSortingOrder(1);

            GameController.Instance.GameLogicHandler.StartItemMoveSlot(item, slot);

            var seq = DOTween.Sequence();
            if (useSpeed)
            {
                seq.Join(item.transform.DOLocalMoveX(0, speed).SetSpeedBased(useSpeed).SetEase(curveX));
                seq.Join(item.transform.DOLocalMoveY(0, speed).SetSpeedBased(useSpeed).SetEase(curveY));
            }
            else
            {
                seq.Join(item.transform.DOLocalMoveX(0, durationMove).SetEase(curveX));
                seq.Join(item.transform.DOLocalMoveY(0, durationMove).SetEase(curveY));
            }
            seq.Append(item.transform.DOScale(scaleDown, scaleDuration));
            seq.Append(item.transform.DOScale(Vector3.one, scaleDuration));
            seq.OnComplete(() =>
            {
                item.OnDropToSlot(slot);
                item.Visual.SetSortingOrder(0);
                MySonatFramework.GetService<VibrationService>().Vibrate(50);
                GameController.Instance.GameLogicHandler.ItemMoveSlot(item, slot);

                foreach (Transform child in slot.Container)
                {
                    if (child.gameObject != item.gameObject)
                    {
                        GameFactory.Instance.ReturnEntity(child.GetComponent<Item>());
                    }
                }
            });
        }

        public override void OnExplodeBomb(ItemBombMove itemBombMove)
        {
            SonatUtils.DelayCall(1f, () =>
            {
                GameController.Instance.Lose(StuckType.SkewerJam_BombExplosion);
            }, itemBombMove);
        }

        public override async UniTask ProcessExplodeBomb(ItemBombMove itemBombMove)
        {
            GameController.Instance.ChangeGameState(GameState.Paused);
            await UniTask.WhenAny(
                UniTask.WaitUntil(() => GameController.Instance.GameLogicHandler.HasCollectItem == true),
                UniTask.Delay(1000)
            );
            if (GameController.Instance.GameLogicHandler.HasCollectItem)
            {
                // chờ thêm để xem có order nào ăn item bomb này không  
                await UniTask.Delay(4000);
            }
            GameController.Instance.ChangeGameState(GameState.Playing);
        }
    }
}