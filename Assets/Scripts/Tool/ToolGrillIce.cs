using System.Collections;
using System.Collections.Generic;
using Gameplay.LevelData;
using TMPro;
using Tool;
using UnityEngine;

public class ToolGrillIce : ToolGrill
{
    [SerializeField] private TMP_InputField indexInputField;
    private GrillIceData grillIceData;
    
    public override void SetData(GrillData grillData)
    {
        base.SetData(grillData);
        if (grillData is GrillIceData _grillIceData)
        {
            this.grillIceData = _grillIceData;
            indexInputField.text = grillIceData.priority.ToString();
            indexInputField.onEndEdit.RemoveAllListeners();
            indexInputField.onEndEdit.AddListener(OnChangeIndex);
        }
    }

    private void OnChangeIndex(string value)
    {
        if (int.TryParse(value, out int index))
        {
            grillIceData.priority = index;
        }
    }
}
