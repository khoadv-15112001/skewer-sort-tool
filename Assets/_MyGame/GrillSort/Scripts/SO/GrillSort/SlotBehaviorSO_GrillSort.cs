using Gameplay.Entities;
using Gameplay.LevelData;
using UnityEngine;

namespace MyGame.SkewerJam.Scripts.SO.Behavior
{
    [CreateAssetMenu(fileName = "SlotBehaviorSO_GrillSort", menuName = "MyGame/GrillSort/SlotBehaviorSO_GrillSort")]
    public class SlotBehaviorSO_GrillSort : SlotBaseBehaviorSO
    {
        public override void OnMouseDown(PrimarySlot slot)
        {
            if (slot.Item != null || slot.PrimaryGrill.IsLock) return;
            if (slot.PrimaryGrill.grillType == GrillType.Vending && slot.PrimaryGrill.SlotCount == 1) return;
            if (GameplayController.instance.itemSelected != null)
            {
                GameplayController.instance.SwitchSlot(slot);
            }
            else
            {
                slot.PrimaryGrill.TapOnGrill();
            }
        }
    }
}