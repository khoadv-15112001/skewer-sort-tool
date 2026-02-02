using System.Collections;
using System.Collections.Generic;
using Gameplay.LevelData;
using Tool;
using UnityEngine;

public class ToolGrillLid : ToolGrill
{
    [SerializeField] private ToolItemConditionLid toolItemConditionLid;

    public override void SetData(GrillData grillData)
    {
        base.SetData(grillData);
        toolItemConditionLid.SetData(grillData as GrillLidData);
    }

    public override void SwitchItemId(int oldId, int newId)
    {
        if(GrillData is GrillLidData grillLidData)
        {
            if (grillLidData.itemCondition == oldId)
            {
                grillLidData.itemCondition = newId;
                toolItemConditionLid.SetData(grillLidData);
            }
        };
        base.SwitchItemId(oldId, newId);
    }
}
