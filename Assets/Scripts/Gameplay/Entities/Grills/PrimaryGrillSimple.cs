using System.Linq;
using Gameplay.LevelData;
using UnityEngine;

namespace Gameplay.Entities.Grills
{
    public class PrimaryGrillSimple : PrimaryGrillSingle
    {
        public override SlotBase GetNearestSlot(Vector3 position)
        {
            return null;
        }
        
        public override void CheckEmpty()
        {
            return;
        }

        public override void OnResetConveyorCircle()
        {
            foreach (var slot in slots)
            {
                if (!slot.isEmpty()) return;
            }

            UpdateSubGrills();
        }
    }
}