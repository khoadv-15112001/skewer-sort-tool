using System;
using Gameplay.LevelData;
using Tool;
using UnityEngine;
using UnityEngine.UI;

public class ToolGrillShutter : ToolGrill
{
    [SerializeField] private Toggle stateToggle;
    private GrillData grillShutterData;
    public override void SetData(GrillData grillData)
    {
        base.SetData(grillData);
        this.grillShutterData = grillData;
        stateToggle.isOn = grillShutterData.isLock;
        stateToggle.onValueChanged.RemoveAllListeners();
        stateToggle.onValueChanged.AddListener(OnChangeState);
    }

    private void OnChangeState(bool isOn)
    {
        grillShutterData.isLock = isOn;
        UIToolPanel.Instance.UpdateGrill();
    }
}
