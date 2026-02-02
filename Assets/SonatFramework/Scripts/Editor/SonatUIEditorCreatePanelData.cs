using UnityEngine;
using UnityEngine.Serialization;
using static UICreatePanelEditor;

[CreateAssetMenu(menuName = "SonatUI/CreatePanelData")]
public class SonatUIEditorCreatePanelData : ScriptableObject
{
    public string panelName;
    public NewScriptState newScriptState;
    public GameObject panelPrefab;
}