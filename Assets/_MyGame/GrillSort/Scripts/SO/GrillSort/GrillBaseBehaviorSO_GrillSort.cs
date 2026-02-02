using Gameplay.Entities;
using Gameplay.LevelData;
using Manager;
using SkewerJam.SO.Behavior;
using Sonat.Enums;
using UnityEngine;

namespace MyGame.SkewerJam.Scripts.SO.Behavior
{
    [CreateAssetMenu(fileName = "GrillBaseBehaviorSO_GrillSort", menuName = "MyGame/GrillSort/GrillBaseBehaviorSO_GrillSort")]
    public class GrillBaseBehaviorSO_GrillSort : GrillBaseBehaviorSO
    {
        public override void OnSlotUpdated(SlotBase slot)
        {
            GameplayController.instance.CheckOutOfMove();
        }

        public override void SetSubOffset(GrillBase grillBase, out Vector3 subOffset)
        {
            subOffset = GameplayController.instance.levelType is LevelType.Food ? Vector3.zero : Vector3.up * 0.1f;
        }

        public override ItemType ValidateItemType(ItemType itemType)
        {
            return itemType;
        }

        public override int GetIceGrillStep()
        {
            return GameRemoteConfigValue.iceGrillStep;
        }

        public override GameState GetGameState()
        {
            return GameplayController.instance.gameState;
        }

        public override void OnAddFromSub(PrimaryGrill primaryGrill, Item item, int slotIndex, int index)
        {
            if (GameplayController.instance.GetBonusTimeGrill() == this)
            {
                GameplayController.instance.SetBonusTime(null);
                item.AddBonusVisual();
            }

            item.MoveToPrimary(primaryGrill.GetSlot(slotIndex), index);
        }
    }
}