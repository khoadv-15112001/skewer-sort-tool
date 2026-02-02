using System;
using System.Collections.Generic;
using System.Linq;
using Sonat.Debugger;
using UnityEditor;
using UnityEditor.PackageManager;
using UnityEditor.PackageManager.Requests;
using UnityEngine;

namespace Sonat.Editor.PackageManager
{
    public class SonatSDKSettingsWindow
    {
        private SonatSDKWindow sonatSDKWindow;
        private SonatSdkSettings settings;

        private List<SonatDebugType> logEnumTypes = new List<SonatDebugType>();
        private List<int> Selected = new List<int>();

        private GUIContent[] myContent;
        private bool usingTransport;

        public SonatSDKSettingsWindow(SonatSDKWindow sonatSDKWindow)
        {
            this.sonatSDKWindow = sonatSDKWindow;
        }

        public void Init()
        {
            myContent = new GUIContent[]
            {
                new GUIContent("Settings"),
                new GUIContent("Development")
            };
            settings = SonatSDKWindow.settings;
            logEnumTypes = Enum.GetValues(typeof(SonatDebugType)).Cast<SonatDebugType>().ToList();
            for (int i = 0; i < logEnumTypes.Count; i++)
            {
                if (settings.logTypes.Contains(logEnumTypes[i]) || settings.logTypes.Contains(SonatDebugType.All))
                {
                    Selected.Add(i);
                }
            }

            CheckTransportInstalled();
        }

        public void Show()
        {
            var tabStyle = EditorStyles.toolbarButton;
            tabStyle.alignment = TextAnchor.MiddleLeft;
            var selectedTabStyle = new GUIStyle(tabStyle);
            selectedTabStyle.normal.background = selectedTabStyle.active.background;

            GUILayout.BeginHorizontal();
            GUILayout.BeginVertical(GUILayout.Width(150), GUILayout.ExpandHeight(true));
            for (int i = 0; i < myContent.Length; i++)
            {
                GUIStyle style = sonatSDKWindow.settingsTab == i ? selectedTabStyle : tabStyle;
                if (GUILayout.Toggle(sonatSDKWindow.settingsTab == i, myContent[i], style))
                    sonatSDKWindow.settingsTab = i;
            }

            GUILayout.EndVertical();

            GUILayout.Box(Texture2D.blackTexture, GUILayout.Width(1.2f), GUILayout.ExpandHeight(true));

            GUILayout.BeginVertical(new GUIStyle(GUI.skin.box));
            switch (sonatSDKWindow.settingsTab)
            {
                case 0:
                    ShowSettingsPanel();
                    break;
                case 1:
                    ShowDevelopGroup();
                    break;
            }

            GUILayout.EndVertical();
            GUILayout.EndHorizontal();
        }

        private void ShowSettingsPanel()
        {
            Undo.RecordObject(settings, "SonatSDK Settings");
            GUILayout.BeginVertical(new GUIStyle(GUI.skin.box));
            settings.appID_Android = EditorGUILayout.TextField("App ID Android", settings.appID_Android);
            settings.appID_IOS = EditorGUILayout.TextField("App ID IOS", settings.appID_IOS);
            GUILayout.EndVertical();
            GUILayout.Space(10);

            GUILayout.BeginVertical(new GUIStyle(GUI.skin.box));
            settings.timeout = EditorGUILayout.IntField("Timeout", settings.timeout);
            GUILayout.EndVertical();

            GUILayout.Space(10);
            EditorUtility.SetDirty(settings);
        }

        private void ShowDevelopGroup()
        {
            Undo.RecordObject(settings, "SonatSDK Develop");
            GUILayout.BeginVertical(new GUIStyle(GUI.skin.box));

            settings.internetConnection = EditorGUILayout.Toggle("Internet Connection", settings.internetConnection);
            GUILayout.Space(5);
            DrawDebugSelector();
            GUILayout.Space(5);
            GUILayout.EndVertical();
            usingTransport = EditorGUILayout.ToggleLeft("Using Transport (For Debug View)", usingTransport);
            if (usingTransport)
            {
                
                TransportInstallation();
                if (GUILayout.Button("Download Debug View PC"))
                {
                    Application.OpenURL("https://drive.google.com/file/d/1aIBPmNC4plU1As89aEyGW8fR7hAaUcjA/view?usp=sharing");
                }
            }
            EditorUtility.SetDirty(settings);
        }

        void OnPointSelected(object index)
        {
            var intIndex = (int)index;

            if (Selected.Contains(intIndex))
            {
                if (intIndex == logEnumTypes.Count - 1)
                {
                    Selected.Clear();
                    settings.logTypes.Clear();
                }
                else
                {
                    Selected.Remove(intIndex);
                    if (settings.logTypes.Contains(logEnumTypes[intIndex]))
                        settings.logTypes.RemoveAt(intIndex);
                    if (settings.logTypes.Contains(SonatDebugType.All))
                    {
                        settings.logTypes = new List<SonatDebugType>(logEnumTypes);
                        settings.logTypes.Remove(logEnumTypes[intIndex]);
                        settings.logTypes.Remove(SonatDebugType.All);
                        Selected.Remove(logEnumTypes.Count - 1);
                    }
                }
            }
            else
            {
                if (intIndex == logEnumTypes.Count - 1)
                {
                    Selected = Enumerable.Range(0, logEnumTypes.Count).ToList();
                    settings.logTypes = new List<SonatDebugType>() { SonatDebugType.All };
                }
                else
                {
                    Selected.Add(intIndex);
                    settings.logTypes.Add(logEnumTypes[intIndex]);
                    if (settings.logTypes.Count == logEnumTypes.Count - 1)
                    {
                        settings.logTypes = new List<SonatDebugType>() { SonatDebugType.All };
                    }
                }
            }
        }

        private void DrawDebugSelector()
        {
            var selectedPointButtonSb = "";
            //var selectedPointButtonSb = new System.Text.StringBuilder();

            if (Selected.Count == 0)
            {
                //selectedPointButtonSb.Append("No points selected.");
                selectedPointButtonSb = "None";
            }
            else
            {
                if (Selected.Count == 1)
                {
                    selectedPointButtonSb = logEnumTypes[Selected[0]].ToString();
                }
                else if (Selected.Count == logEnumTypes.Count)
                {
                    selectedPointButtonSb = "All";
                }
                else
                {
                    for (int i = 0; i < Selected.Count; i++)
                    {
                        selectedPointButtonSb += logEnumTypes[Selected[i]].ToString();
                        if (i != Selected.Count - 1)
                            selectedPointButtonSb += ", ";
                    }
                }
            }

            GUILayout.BeginHorizontal();

            GUILayout.Label("Debug Types:", GUILayout.Width(100));
            GUILayout.Space(EditorGUIUtility.labelWidth - 100);
            if (GUILayout.Button(selectedPointButtonSb, new GUIStyle(EditorStyles.miniButtonMid),
                    GUILayout.ExpandWidth(true)))
            {
                var selectedMenu = new GenericMenu();

                for (var i = 0; i < logEnumTypes.Count; ++i)
                {
                    var menuString = $"{logEnumTypes[i]}";
                    var selected = Selected.Contains(i);
                    selectedMenu.AddItem(new GUIContent(menuString), selected, OnPointSelected, i);
                    selectedMenu.AddSeparator("");
                }

                selectedMenu.ShowAsContext();
            }

            //GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
        }


        private string transportVersionInstalled = "";
        private string transportVersionToInstall = "";
        private bool transportInstalled = false;
        private bool hasTransportSymbol;
        static AddRequest Request;

        private void CheckTransportInstalled()
        {
            if (string.IsNullOrEmpty(transportVersionInstalled))
            {
                //admobVersionInstalled = SonatEditorHelper.CheckVersionInstalled("package.json", "AppsFlyer", @"""version"": ""(?<ver>.+)""");
                transportVersionInstalled =
                    SonatEditorHelper.CheckVersionInstalledByManifest("com.unity.transport");
            }

            transportInstalled = !string.IsNullOrEmpty(transportVersionInstalled);
            hasTransportSymbol = SonatEditorHelper.HasSymbol("using_networking_transport",
                EditorUserBuildSettings.selectedBuildTargetGroup);
            if (!transportInstalled && hasTransportSymbol)
            {
                hasTransportSymbol = false;
                SonatEditorHelper.RemoveSymbolFromBuildTarget("using_networking_transport");
            }

            usingTransport = transportInstalled;
        }

        private void TransportInstallation()
        {
            GUILayout.BeginVertical(new GUIStyle(GUI.skin.box));
            GUILayout.Label($"Unity Network Transport", EditorStyles.boldLabel);
            GUILayout.Space(5);
            if (transportInstalled)
                GUILayout.Label($"Unity Network Transport Installed Version: {transportVersionInstalled}",
                    SonatSDKWindow.labelGreenStyle);
            else
            {
                EditorGUILayout.HelpBox("Unity Network Transport Not Installed Yet", MessageType.Error);
            }

            GUILayout.Space(5);
            GUILayout.BeginHorizontal();
            //GUILayout.Label("Install");
            //iapVersionToInstall = EditorGUILayout.TextField("Version", iapVersionToInstall, GUILayout.ExpandWidth(true));
            // previewAfterDownloadMax = EditorGUILayout.Toggle("Preview", previewAfterDownloadMax);
            string installLabel = "Install";
            if (transportInstalled && transportVersionToInstall != transportVersionInstalled)
            {
                installLabel = "Upgrade";
            }
            else if (string.IsNullOrEmpty(transportVersionToInstall))
            {
                installLabel = "Install (Latest)";
            }

            if (GUILayout.Button(installLabel, GUILayout.Width(120)))
            {
                Request = Client.Add("com.unity.transport");
                EditorApplication.update += Progress;
            }

            EditorGUI.BeginDisabledGroup(!transportInstalled || hasTransportSymbol);
            string symbolLabel = hasTransportSymbol ? "Symbol Added" : "Add Symbol";
            if (GUILayout.Button(symbolLabel, GUILayout.Width(100)))
            {
                if (hasTransportSymbol)
                {
                    sonatSDKWindow.ShowNotification(new GUIContent("Unity Network Transport Symbol Has Exist!"), 0.5f);
                }
                else
                {
                    SonatEditorHelper.AddSymbol(new[] { "using_networking_transport" },
                        EditorUserBuildSettings.selectedBuildTargetGroup);
                }
            }

            EditorGUI.EndDisabledGroup();
            GUILayout.EndHorizontal();
            GUILayout.Space(5);
            GUILayout.EndVertical();
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
            }
            else if (Request.Status == StatusCode.InProgress)
            {
                EditorUtility.DisplayProgressBar($"Download Unity Purchasing Package", $"Downloading", 0);
            }
        }
    }
}