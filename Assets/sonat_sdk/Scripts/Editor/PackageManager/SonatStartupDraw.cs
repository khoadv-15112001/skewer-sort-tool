using UnityEditor;
using UnityEditor.PackageManager;
using UnityEditor.PackageManager.Requests;
using UnityEngine;

namespace Sonat.Editor.PackageManager
{
    public class SonatStartupDraw
    {
        private SonatSDKWindow sonatSDKWindow;

        private bool setup;

        public SonatStartupDraw(SonatSDKWindow sonatWindow)
        {
            this.sonatSDKWindow = sonatWindow;
            CheckNewtonsoftInstalled();
        }

        public void Draw()
        {
            GUIStyle style = new GUIStyle(GUI.skin.label)
            {
                alignment = TextAnchor.MiddleCenter,
                fontStyle = FontStyle.Bold,
                fontSize = 30,
                normal = { textColor = SonatSDKWindow.orange },
            };
            GUILayout.BeginVertical();

            Rect r = new Rect(0, 0, 500, 200); // size
            r.center = new Vector2(sonatSDKWindow.position.width / 2, sonatSDKWindow.position.height / 2);

            GUILayout.BeginArea(r);

            GUILayout.Label("Welcome to Sonat SDK!", style);
            GUILayout.Space(20);
            GUIStyle style2 = new GUIStyle();
            style2.fontSize = 13;
            style2.alignment = TextAnchor.MiddleCenter;
            style2.normal.textColor = Color.white;
            GUILayout.Label("Click the buttons below to set up for the first time", style2);

            GUILayout.Space(10);

            EditorGUI.BeginDisabledGroup(newtonInstalled);
            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            EditorGUI.BeginDisabledGroup(setup);
            string label = newtonInstalled ? "Newtonsoft-Json Installed" : "Install Newtonsoft-Json";
            if (GUILayout.Button(label, new GUIStyle(GUI.skin.button) { fontSize = 13 }, GUILayout.Width(200), GUILayout.Height(25)))
            {
                Request = Client.Add("com.unity.nuget.newtonsoft-json");
                EditorApplication.update += Progress;
            }

            EditorGUI.EndDisabledGroup();
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
            EditorGUI.EndDisabledGroup();

            GUILayout.Space(10);
        
            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            EditorGUI.BeginDisabledGroup(setup || !newtonInstalled);
            if (GUILayout.Button("Setup", new GUIStyle(GUI.skin.button) { fontSize = 13 }, GUILayout.Width(200), GUILayout.Height(25)))
            {
                string path = SonatEditorHelper.FindFilePath("SonatSdkSo.unitypackage", "");
                if (!string.IsNullOrEmpty(path))
                {
                    AssetDatabase.ImportPackage(path, false);
                    setup = true;
                }
            }

            EditorGUI.EndDisabledGroup();
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();

            GUILayout.Space(10);

            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            EditorGUI.BeginDisabledGroup(!setup);
            if (GUILayout.Button("Start", new GUIStyle(GUI.skin.button) { fontSize = 13 }, GUILayout.Width(200), GUILayout.Height(25)))
            {
                string path = SonatEditorHelper.FindFilePath("SonatSdkSo.unitypackage", "");
                if (!string.IsNullOrEmpty(path))
                {
                    sonatSDKWindow.Init();
                }
            }

            EditorGUI.EndDisabledGroup();
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();

            GUILayout.EndArea();

            GUILayout.EndVertical();
        }

        private string newtonVersionInstalled = "";
        private string newtonVersionToInstall = "";
        private bool newtonInstalled = false;
        private bool hasNewtonSymbol;
        static AddRequest Request;

        private void CheckNewtonsoftInstalled()
        {
            if (string.IsNullOrEmpty(newtonVersionInstalled))
            {
                newtonVersionInstalled = SonatEditorHelper.CheckVersionInstalledByManifest("com.unity.nuget.newtonsoft-json");
            }

            newtonInstalled = !string.IsNullOrEmpty(newtonVersionInstalled);
            hasNewtonSymbol = SonatEditorHelper.HasSymbol("using_newtonsoft", EditorUserBuildSettings.selectedBuildTargetGroup);
            if (!newtonInstalled && hasNewtonSymbol)
            {
                hasNewtonSymbol = false;
                SonatEditorHelper.RemoveSymbolFromBuildTarget("using_newtonsoft");
            }
            else if(newtonInstalled && !hasNewtonSymbol)
            {
                SonatEditorHelper.AddSymbol(new[] { "using_newtonsoft" }, BuildTargetGroup.Unknown);
            }
        }


        private void Progress()
        {
            if (Request.IsCompleted)
            {
                if (Request.Status == StatusCode.Success)
                    Debug.Log("Installed: " + Request.Result.packageId);
                else if (Request.Status >= StatusCode.Failure)
                    Debug.Log(Request.Error.message);

                EditorApplication.update -= Progress;
                EditorUtility.ClearProgressBar();
                //SonatEditorHelper.AddSymbol(new[] { "using_newtonsoft" }, BuildTargetGroup.Unknown);
            }
            else if (Request.Status == StatusCode.InProgress)
            {
                EditorUtility.DisplayProgressBar($"Download Newtonsoft-Json Package", $"Downloading", 0);
            }
        }
    }
}