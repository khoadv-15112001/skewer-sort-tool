using System.Collections;
using System.Collections.Generic;
using Gameplay.LevelData;
using TMPro;
using Tool;
using UnityEngine;

public class ToolGrillLock : ToolGrill
{
    [SerializeField] private TMP_InputField indexInputField;
    private GrillLockData grillLockData;
    
    public override void SetData(GrillData grillData)
    {
        base.SetData(grillData);
        if (grillData is GrillLockData _grillLockData)
        {
            this.grillLockData = _grillLockData;
            indexInputField.text = grillLockData.priority.ToString();
            indexInputField.onEndEdit.RemoveAllListeners();
            indexInputField.onEndEdit.AddListener(OnChangeIndex);
        }
    }

    private void OnChangeIndex(string value)
    {
        if (int.TryParse(value, out int index))
        {
            grillLockData.priority = index;
        }
    }
}
