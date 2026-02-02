using System.Collections.Generic;
using Gameplay.LevelData;
using SonatFramework.Scripts.Utils;
using UnityEngine;

namespace Gameplay.Entities.Obstacle
{
    public class ConveyorHorizontalSimple : ConveyorController
    {
        [SerializeField] private float spaceSlot = 1.675f;
        public override void GetGrills()
        {
            hasSubGrill = false;
            grills = new List<PrimaryGrill>();
            foreach (var grill in GameplayController.instance.levelGenerator.GetPrimaryGrills())
            {
                if (grill.grillType == GrillType.Simple)
                {
                    grills.Add(grill);
                    grill.SetMaskVisible(false);
                }
            }
            grills.Sort((a, b) => Vector3.SqrMagnitude(a.transform.position - endPosition).CompareTo(Vector3.SqrMagnitude(b.transform.position - endPosition)));
        }
        
        protected override void CalculateSpacing()
        {
            if (grills.Count == 0) return;

            spacing = spaceSlot;
            startPosition.x = (Mathf.RoundToInt(startPosition.x / spaceSlot) + 0.5f) * spaceSlot;
            grills[0].transform.position = startPosition;

            for (int i = 1; i < grills.Count; i++)
            {
                grills[i].transform.position = grills[i - 1].transform.position - direction * spacing;
            }
        }
        
        protected override float GetReDistance(PrimaryGrill grill)
        {
            return spaceSlot;
        }
    }
    
}