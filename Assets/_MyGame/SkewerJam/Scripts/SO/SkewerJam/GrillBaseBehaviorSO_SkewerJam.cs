using UnityEngine;
using SkewerJam.SO.Behavior;
using Gameplay.Entities;
using System;
using Gameplay.LevelData;
using Manager;
using Sonat.Enums;
using MyGame.SkewerJam.Gameplay;

namespace MyGame.SkewerJam.Scripts.SO.Behavior
{
    [CreateAssetMenu(fileName = "GrillBaseBehaviorSO_SkewerJam", menuName = "MyGame/SkewerJam/GrillBaseBehaviorSO_SkewerJam")]
    public class GrillBaseBehaviorSO_SkewerJam : GrillBaseBehaviorSO
    {
        public override void OnSlotUpdated(SlotBase slot)
        {

        }

        public override void SetSubOffset(GrillBase grillBase, out Vector3 subOffset)
        {
            subOffset = Vector3.zero;
        }

        public override ItemType ValidateItemType(ItemType itemType)
        {
            if (itemType == ItemType.Ice) return ItemType.Normal;
            return itemType;
        }

        public override int GetIceGrillStep()
        {
            return GameRemoteConfigValue.iceGrillStep_SkewerJam;
        }

        public override GameState GetGameState()
        {
            return GameController.Instance.GameState;
        }

        public override void OnAddFromSub(PrimaryGrill primaryGrill, Item item, int slotIndex, int index)
        {
            item.MoveToPrimary(primaryGrill.GetSlot(slotIndex), index);
        }
    }
}