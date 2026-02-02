using Gameplay.LevelData;
using TMPro;
using Tool;
using UnityEngine;

public class ToolGrillIceMain : ToolGrill
{
    [SerializeField] private TMP_InputField mainGrillIdInputField;
    private GrillIceMainData grillMainIceData;
    
    public override void SetData(GrillData grillData)
    {
        base.SetData(grillData);
        if (grillData is GrillIceMainData _grillIceData)
        {
            //Debug.LogError("SetData: " + _grillIceData.mainGrillId);
            this.grillMainIceData = _grillIceData;
            mainGrillIdInputField.text = grillMainIceData.mainGrillId.ToString();
            mainGrillIdInputField.onEndEdit.RemoveAllListeners();
            mainGrillIdInputField.onEndEdit.AddListener(OnChangeIndex);
        }
    }

    private void OnChangeIndex(string value)
    {
        if (byte.TryParse(value, out byte index))
        {
            grillMainIceData.mainGrillId = index;
            UIToolPanel.Instance.UpdateGrill();
        }
    }
}
