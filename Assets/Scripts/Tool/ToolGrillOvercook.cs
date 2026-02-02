using Gameplay.LevelData;
using TMPro;
using Tool;
using UnityEngine;

public class ToolGrillOvercook : ToolGrill
{
    [SerializeField] private TMP_InputField timeInputTMP;
    private GrillData grillOvercookData;

    public override void SetData(GrillData grillData)
    {
        base.SetData(grillData);
        this.grillOvercookData = grillData;
        timeInputTMP.text = grillOvercookData.time.ToString();
        timeInputTMP.onEndEdit.RemoveAllListeners();
        timeInputTMP.onEndEdit.AddListener(OnChangeTime);
    }

    private void OnChangeTime(string value)
    {
        if (int.TryParse(value, out int time))
        {
            grillOvercookData.time = time;
            UIToolPanel.Instance.UpdateGrill();
        }
    }
}
