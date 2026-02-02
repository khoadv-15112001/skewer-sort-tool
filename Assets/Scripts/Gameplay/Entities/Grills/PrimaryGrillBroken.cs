using System.Collections;
using System.Collections.Generic;
using Gameplay.Entities;
using UnityEngine;

public class PrimaryGrillBroken : PrimaryGrill
{
    public override bool CheckComplete()
    {
        return false;
    }
}
