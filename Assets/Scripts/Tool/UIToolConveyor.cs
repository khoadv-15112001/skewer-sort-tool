using System;
using System.Collections;
using System.Collections.Generic;
using Gameplay.LevelData;
using TMPro;
using Tool;
using UnityEngine;
using UnityEngine.UI;

public class UIToolConveyor : MonoBehaviour
{
    [SerializeField] private TMP_InputField txtSpeed;
    [SerializeField] private TMP_Text txtCount;
    [SerializeField] private Button addSimpleGrill;

    //private ConveyorData conveyorData;
    private ToolConveyor toolConveyor;

    private void Awake()
    {
        txtSpeed.onValueChanged.AddListener(OnSpeedChanged);
    }

    public void SetData(ToolConveyor conveyor)
    {
        this.toolConveyor = conveyor;
        txtSpeed.text = conveyor.conveyData.speed.ToString();
        UpdateGrillCount();
        addSimpleGrill.gameObject.SetActive(conveyor.conveyData.conveyorType is ConveyorType.HorizontalMin or ConveyorType.HorizontalSimple);
    }

    private void OnSpeedChanged(string newSpeed)
    {
        if (float.TryParse(newSpeed, out float newSpeedFloat))
        {
            toolConveyor.conveyData.speed = newSpeedFloat;
        }
    }

    public void UpdateGrillCount()
    {
        txtCount.text = $"Grill: {toolConveyor.conveyData.grillIds.Count}";
    }

    public void AddGrill()
    {
        switch (toolConveyor.conveyData.conveyorType)
        {
            case ConveyorType.HorizontalSimple:
            {
                ToolGrill toolGrill = UIToolPanel.Instance.CreateGrill(GrillType.Simple, 1);
                toolGrill.transform.position = toolConveyor.transform.position + (toolConveyor.conveyData.grillIds.Count - 8) * Vector3.right * 1.45f;
                toolGrill.UpdatePosition();
                toolConveyor.CheckGrill();
                UIToolPanel.Instance.OnSelectToolConveyor(toolConveyor);
                break;
            }
            case ConveyorType.HorizontalMin:
            {
                ToolGrill toolGrill = UIToolPanel.Instance.CreateGrill(GrillType.SingleMin, 1);
                toolGrill.transform.position = toolConveyor.transform.position + (toolConveyor.conveyData.grillIds.Count - 8) * Vector3.right * 1.5f;
                toolGrill.UpdatePosition();
                toolConveyor.CheckGrill();
                UIToolPanel.Instance.OnSelectToolConveyor(toolConveyor);
                break;
            }
        }
    }
}