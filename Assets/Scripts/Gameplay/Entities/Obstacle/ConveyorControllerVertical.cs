using System.Collections;
using System.Collections.Generic;
using Gameplay.Entities;
using UnityEngine;

public class ConveyorControllerVertical : ConveyorController
{
    [SerializeField] private Transform topPos;
    [SerializeField] private Transform bottomPos;

    protected override void CalculateStartPosition()
    {
        if (conveyData.speed > 0)
        {
            startPosition = bottomPos.position;
            endPosition = topPos.position;
        }
        else
        {
            startPosition = topPos.position;
            endPosition = bottomPos.position;
        }
        
        direction = conveyData.speed > 0 ? Vector3.up : Vector3.down;
    }
    
    protected override void CalculateSpacing()
    {
        spacing = 4;

        for (int i = 0; i < grills.Count; i++)
        {
            grills[i].transform.position = startPosition - direction * spacing * i;
        }
    }
    
    protected override float GetReDistance(PrimaryGrill grill)
    {
        return 4;
    }
}
