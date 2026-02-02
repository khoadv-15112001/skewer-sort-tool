using System.Collections;
using System.Collections.Generic;
using Gameplay.LevelData;
using TMPro;
using Tool;
using UnityEngine;

public class ToolGrillIceNeighbor : ToolGrill
{
    [SerializeField] private TMP_InputField mainGrillIdInputField;
    private GrillIceNeighborData grillIceData;

    public override void SetData(GrillData grillData)
    {
        base.SetData(grillData);
        if (grillData is GrillIceNeighborData _grillIceData)
        {
            Debug.LogError("SetData: " + _grillIceData.mainGrillId);
            this.grillIceData = _grillIceData;
            mainGrillIdInputField.text = grillIceData.mainGrillId.ToString();
            mainGrillIdInputField.onEndEdit.RemoveAllListeners();
            mainGrillIdInputField.onEndEdit.AddListener(OnChangeIndex);
        }
    }

    private void OnChangeIndex(string value)
    {
        if (byte.TryParse(value, out byte index))
        {
            grillIceData.mainGrillId = index;
            UIToolPanel.Instance.UpdateGrill();
        }
    }
}
