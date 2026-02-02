using System;
using System.Linq;
using Sonat.AppsFlyerModule;
using UnityEditor;
using UnityEngine;

namespace Sonat.Editor.PackageManager.Elements
{
    public class AppsFlyerPanelDraw
    {
        private SonatAppsFlyer sonatAppsFlyer;

        //private SerializedObject serializedAppFlyer;
        SerializedProperty loginProperty;
        private bool edit;
        private string afVersion;

        public void Init()
        {
            sonatAppsFlyer = SonatEditorHelper.LoadConfigSo<SonatAppsFlyer>(nameof(SonatAppsFlyer));
            if (sonatAppsFlyer == null) return;

            hasAppsFlyerSymbol = SonatEditorHelper.HasSymbol("using_appsflyer", EditorUserBuildSettings.selectedBuildTargetGroup);
            CheckAppsFlyerInstalled();
            CheckAppsFlyerConnectorInstalled();
        }

        public void Draw()
        {
            if (sonatAppsFlyer == null) return;
            GUILayout.BeginVertical(new GUIStyle(GUI.skin.box));
            GUILayout.Space(5);
            //EditorGUILayout.PropertyField(loginProperty, new GUIContent("Login"));
            //serializedAppFlyer.ApplyModifiedProperties();
            GUILayout.BeginHorizontal();
            EditorGUIUtility.labelWidth = 60;
            EditorGUI.BeginDisabledGroup(!edit);
            sonatAppsFlyer.devKey = EditorGUILayout.TextField("Dev Key", sonatAppsFlyer.devKey, GUILayout.Width(300));
            EditorGUI.EndDisabledGroup();
            GUILayout.FlexibleSpace();
            EditorGUIUtility.labelWidth = 30;
            edit = EditorGUILayout.Toggle("Edit", edit);
            EditorGUIUtility.labelWidth = 50;
            GUILayout.EndHorizontal();

            GUILayout.Space(5);
            GUILayout.EndVertical();


            GUILayout.Space(10);

            GUILayout.BeginVertical(new GUIStyle(GUI.skin.box));
            GUILayout.Label("INSTALL", EditorStyles.boldLabel);

            GUILayout.Box(Texture2D.whiteTexture, GUILayout.Height(1.5f), GUILayout.ExpandWidth(true));

            GUILayout.Space(5);
            AppsFlyerInstallation();
            GUILayout.Space(5);
            AppsFlyerConnectorInstallation();
            GUILayout.Space(5);

            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            EditorGUI.BeginDisabledGroup(!appsFlyerConnectorInstalled || !appsFlyerInstalled || hasAppsFlyerSymbol);
            string symbolLabel = hasAppsFlyerSymbol ? "Symbol Added" : "Add Symbol";
            if (GUILayout.Button(symbolLabel, GUILayout.Width(120)))
            {
                SonatEditorHelper.AddSymbol(new[] { "using_appsflyer" }, EditorUserBuildSettings.selectedBuildTargetGroup);
            }

            EditorGUI.EndDisabledGroup();
            GUILayout.EndHorizontal();

            GUILayout.EndVertical();

            EditorUtility.SetDirty(sonatAppsFlyer);
        }

        private string appsFlyerVersionInstalled = "";
        private bool appsFlyerInstalled;

        private bool hasAppsFlyerSymbol;
        private string[] appsFlyerVersions;
        private int appsFlyerVersionSelected;
        private bool appsFlyerUpgrade;
        private bool previewAppsFlyerAfterDownload = true;

        private void CheckAppsFlyerInstalled()
        {
            if (string.IsNullOrEmpty(appsFlyerVersionInstalled))
            {
                appsFlyerVersionInstalled =
                    SonatEditorHelper.CheckVersionInstalled("AppsFlyerDependencies.xml", "AppsFlyer", @"""com.appsflyer:unity-wrapper:(?<ver>.+)""");
                if (!string.IsNullOrEmpty(appsFlyerVersionInstalled))
                {
                    if (new Version(appsFlyerVersionInstalled) < new Version("6.15.0"))
                    {
                        if (!SonatEditorHelper.HasSymbol("appsflyer_6_15_or_older", EditorUserBuildSettings.selectedBuildTargetGroup))
                            SonatEditorHelper.AddSymbol(new string[] { "appsflyer_6_15_or_older" }, EditorUserBuildSettings.selectedBuildTargetGroup);
                    }
                    else
                    {
                        if (SonatEditorHelper.HasSymbol("appsflyer_6_15_or_older", EditorUserBuildSettings.selectedBuildTargetGroup))
                            SonatEditorHelper.RemoveSymbolFromBuildTarget("appsflyer_6_15_or_older");
                    }
                }
            }

            appsFlyerInstalled = !string.IsNullOrEmpty(appsFlyerVersionInstalled);
            appsFlyerVersions = SonatSDKWindow.packageInfo.appsFlyerUrls.Keys.ToArray();
            appsFlyerVersionSelected = appsFlyerVersions.Length - 1;

            if (!appsFlyerInstalled && hasAppsFlyerSymbol)
            {
                hasAppsFlyerSymbol = false;
                SonatEditorHelper.RemoveSymbolFromBuildTarget("using_appsflyer");
            }
        }

        private void AppsFlyerInstallation()
        {
            GUILayout.BeginVertical(new GUIStyle(GUI.skin.box));
            GUILayout.Label($"AppsFlyer SDK", EditorStyles.boldLabel);
            GUILayout.Space(5);
            if (appsFlyerInstalled)
                GUILayout.Label($"AppsFlyer Installed Version: {appsFlyerVersionInstalled}", SonatSDKWindow.labelGreenStyle);
            else
            {
                EditorGUILayout.HelpBox("AppsFlyer Not Installed Yet", MessageType.Error);
            }

            GUILayout.Space(5);

            if (appsFlyerInstalled)
            {
                appsFlyerUpgrade = EditorGUILayout.Foldout(appsFlyerUpgrade, "Upgrade", true);
                GUILayout.Space(3);
            }
            else
            {
                appsFlyerUpgrade = true;
            }

            if (appsFlyerUpgrade)
            {
                GUILayout.BeginHorizontal();
                //GUILayout.Label("Install");
                appsFlyerVersionSelected = EditorGUILayout.Popup("Version", appsFlyerVersionSelected, appsFlyerVersions, GUILayout.Width(200));

                previewAppsFlyerAfterDownload = EditorGUILayout.Toggle("Preview", previewAppsFlyerAfterDownload);
                string installLabel = "Install";
                if (appsFlyerInstalled)
                {
                    installLabel = "Upgrade";
                }

                EditorGUI.BeginDisabledGroup(appsFlyerInstalled && appsFlyerVersionInstalled == appsFlyerVersions[^1]);
                if (GUILayout.Button(installLabel, GUILayout.Width(120)))
                {
                    string verInstall = appsFlyerVersions[appsFlyerVersionSelected];
                    string url = SonatSDKWindow.packageInfo.appsFlyerUrls[appsFlyerVersions[appsFlyerVersionSelected]];
                    SonatPackageHelper.InstallPackage(url, $"appsflyer-unity-purchase-connector-{verInstall}", previewAppsFlyerAfterDownload);
                }

                EditorGUI.EndDisabledGroup();


                GUILayout.EndHorizontal();
            }

            GUILayout.Space(5);
            GUILayout.EndVertical();
        }


        private string appsFlyerConnectorVersionInstalled = "";
        private bool appsFlyerConnectorInstalled;
        private bool appsFlyerConnectorUpgrade;
        private bool previewConnectorAfterDownload = true;
        private string[] appsFlyerConnectorVersions;
        private int appsFlyerConnectorVersionSelected;

        private void CheckAppsFlyerConnectorInstalled()
        {
            appsFlyerConnectorVersions = SonatSDKWindow.packageInfo.appsFlyerPurchaseUrls.Keys.ToArray();
            
            if (string.IsNullOrEmpty(appsFlyerConnectorVersionInstalled))
            {
                string verAFPurchase = SonatEditorHelper.CheckVersionInstalled("AppsFlyerPurchaseConnectorDependencies.xml", "AppsFlyer",
                    @"""com.appsflyer:af-purchaseconnector-unity:(?<ver>[^""]+)""");
                if (!string.IsNullOrEmpty(verAFPurchase))
                    appsFlyerConnectorVersionInstalled = appsFlyerConnectorVersions.FirstOrDefault(e => e.StartsWith(verAFPurchase));
            }

            appsFlyerConnectorInstalled = !string.IsNullOrEmpty(appsFlyerConnectorVersionInstalled);
           
            appsFlyerConnectorVersionSelected = appsFlyerConnectorVersions.Length - 1;
        }

        private void AppsFlyerConnectorInstallation()
        {
            GUILayout.BeginVertical(new GUIStyle(GUI.skin.box));
            GUILayout.Label($"AppsFlyer Purchase Connector SDK", EditorStyles.boldLabel);
            GUILayout.Space(5);
            if (appsFlyerConnectorInstalled)
                GUILayout.Label($"AppsFlyer Purchase Connector Installed Version: {appsFlyerConnectorVersionInstalled}", SonatSDKWindow.labelGreenStyle);
            else
            {
                EditorGUILayout.HelpBox("AppsFlyer Purchase Connector Not Installed Yet", MessageType.Error);
            }

            GUILayout.Space(5);

            if (appsFlyerConnectorInstalled)
            {
                appsFlyerConnectorUpgrade = EditorGUILayout.Foldout(appsFlyerConnectorUpgrade, "Upgrade", true);
                GUILayout.Space(3);
            }
            else
            {
                appsFlyerConnectorUpgrade = true;
            }

            if (appsFlyerConnectorUpgrade)
            {
                GUILayout.BeginHorizontal();
                //GUILayout.Label("Install");
                appsFlyerConnectorVersionSelected =
                    EditorGUILayout.Popup("Version", appsFlyerConnectorVersionSelected, appsFlyerConnectorVersions, GUILayout.Width(200));

                previewConnectorAfterDownload = EditorGUILayout.Toggle("Preview", previewConnectorAfterDownload);
                string installLabel = "Install";
                if (appsFlyerConnectorInstalled)
                {
                    installLabel = "Upgrade";
                }

                EditorGUI.BeginDisabledGroup(appsFlyerConnectorInstalled && appsFlyerConnectorVersionInstalled == appsFlyerConnectorVersions[^1]);
                if (GUILayout.Button(installLabel, GUILayout.Width(120)))
                {
                    string verInstall = appsFlyerConnectorVersions[appsFlyerConnectorVersionSelected];
                    string url = SonatSDKWindow.packageInfo.appsFlyerPurchaseUrls[appsFlyerConnectorVersions[appsFlyerConnectorVersionSelected]];
                    SonatPackageHelper.InstallPackage(url, $"appsflyer-unity-purchase-connector-{verInstall}", previewConnectorAfterDownload);
                }

                EditorGUI.EndDisabledGroup();


                GUILayout.EndHorizontal();
            }

            GUILayout.Space(5);
            GUILayout.EndVertical();
        }
    }
}