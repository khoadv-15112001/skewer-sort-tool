using Gameplay.LevelData;
using TMPro;
using Tool;
using UnityEngine;

public class ToolGrillBomb : ToolGrill
{
    [SerializeField] private TMP_InputField moveInputTMP;
    private GrillData grillOvercookData;

    public override void SetData(GrillData grillData)
    {
        base.SetData(grillData);

        Debug.LogError($"SetData: {grillData.move}");
        this.grillOvercookData = grillData;
        moveInputTMP.text = grillOvercookData.move.ToString();
        moveInputTMP.onEndEdit.RemoveAllListeners();
        moveInputTMP.onEndEdit.AddListener(OnChangeMove);
    }

    private void OnChangeMove(string value)
    {
        if (int.TryParse(value, out int move))
        {
            grillOvercookData.move = move;
            Debug.LogError($"Updated move to: {move}");
            Debug.LogError($"GrillData type: {grillOvercookData.GetType().Name}");
            UIToolPanel.Instance.UpdateGrill();
        }
    }
}
