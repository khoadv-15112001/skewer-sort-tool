// using System;
// using System.IO;
// using Sirenix.OdinInspector;
// using Sirenix.OdinInspector.Editor;
// using SonatFramework.Scripts.UIModule;
// using UnityEditor;
// using UnityEngine;
// using Object = UnityEngine.Object;
//
// namespace SonatFramework.Scripts.Editor
// {
//     public enum PanelType
//     {
//         None,
//         WinPanel,
//         LosePanel,
//     }
//     
//     public class CreateTemplatePanel
//     {
//         private readonly SonatUIEditorCreatePanelData data;
//         private GameObject newPanelObj;
//         public PanelType panelType;
//         
//         
//         private NewScriptState newScriptState;
//         public string uiScripPath = "Scripts/UI/Panel";
//         
//         private GameObject panelPrefab;
//         private string panelName => panelType.ToString();
//         public CreateTemplatePanel()
//         {
//             data = (SonatUIEditorCreatePanelData)AssetDatabase.LoadAssetAtPath(SonatWiz.createPanelDataPath2,
//                 typeof(SonatUIEditorCreatePanelData));
//             if (data == null)
//             {
//                 data = ScriptableObject.CreateInstance<SonatUIEditorCreatePanelData>();
//                 AssetDatabase.CreateAsset(data, SonatWiz.createPanelDataPath2);
//             }
//         
//             panelType = Enum.Parse<PanelType>(panelName);
//             newScriptState = data.newScriptState;
//         }
//         
//         public void OnUpdateIm()
//         {
//             if (string.IsNullOrEmpty(panelName)) return;
//             switch (newScriptState)
//             {
//                 case NewScriptState.Compile:
//                     var t = SonatFrameworkEditorHelper.GetTypeFromAllAssemblies(panelName);
//                     if (t == null) return;
//                     CreateObject(t);
//                     break;
//                 case NewScriptState.AddScript:
//                     OpenScript();
//                     break;
//             }
//         }
//         
//         
//         [Button("Create New Panel", ButtonSizes.Medium)]
//         private void CreateNewData()
//         {
//             if (panelType == PanelType.None || panelName.Contains(" "))
//             {
//                 SonatFrameworkEditorHelper.ShowNotification("Invalid script name!'" + "'", true, false);
//                 return;
//             }
//         
//             // var alreadyExistingTypeFullName = GetFullNameIfScriptExists(panelName);
//             // if (alreadyExistingTypeFullName != null)
//             // {
//             //     SonatFrameworkEditorHelper.ShowNotification(
//             //         "Invalid script name. A script already exists as '" + alreadyExistingTypeFullName + "'", true,
//             //         false);
//             //     return;
//             // }
//         
//             data.panelName = panelName;
//             panelPrefab = (GameObject)AssetDatabase.LoadAssetAtPath(Path.Combine(SonatWiz.templateUIPath, "PanelPrefab", $"{panelType}.prefab"), typeof(GameObject));
//             data.panelPrefab = panelPrefab;
//
//             string scriptPath = Path.Combine(Application.dataPath, SonatWiz.RootDirectory, $"Templates/UI/ScriptBase/{panelType}.cs");
//             if (!File.Exists(scriptPath))
//             {
//                 scriptPath += ".disable";
//             }
//             else
//             {
//                 //AssetDatabase.RenameAsset(scriptPath, $"{panelType}.cs.disable");
//                 var newPath = Path.Combine(Application.dataPath, uiScripPath, $"{panelType}.cs");
//                 AssetDatabase.MoveAsset(scriptPath, )
//             }
//
//             var text = File.ReadAllText(scriptPath);
//             SaveScript(text);
//         }
//         
//         private void CreateObject(Type t)
//         {
//             var panelContainer = Object.FindObjectOfType<PanelManager>()?.transform;
//             if (panelContainer == null) panelContainer = Selection.activeTransform;
//
//             newPanelObj = Object.Instantiate(panelPrefab, panelContainer);
//         
//             data.newScriptState = NewScriptState.AddScript;
//             newScriptState = NewScriptState.AddScript;
//         }
//         
//         private void OpenScript()
//         {
//             data.newScriptState = NewScriptState.None;
//             newScriptState = NewScriptState.None;
//             OpenScript(newPanelObj.GetComponent<Panel>());
//         }
//         
//         private void SaveScript(string newText)
//         {
//             var direcPath = Path.Combine(Application.dataPath, uiScripPath);
//             if (!Directory.Exists(direcPath))
//                 try
//                 {
//                     Directory.CreateDirectory(direcPath);
//                 }
//                 catch
//                 {
//                     Debug.LogError("Could not create directory: " + direcPath);
//                     return;
//                 }
//         
//             var scriptPath = Path.Combine(direcPath, $"{panelName}.cs");
//             File.WriteAllTextAsync(scriptPath, newText);
//             AssetDatabase.ImportAsset(FileUtil.GetProjectRelativePath(scriptPath));
//             data.newScriptState = NewScriptState.Compile;
//             newScriptState = NewScriptState.Compile;
//         }
//         
//         private void OpenScript(Panel panel)
//         {
//             var monoScript = MonoScript.FromMonoBehaviour(panel);
//             var success = AssetDatabase.OpenAsset(monoScript);
//             if (!success) Debug.Log("Could not open '" + panel.GetType().Name + "' in external code editor");
//         }
//     }
// }