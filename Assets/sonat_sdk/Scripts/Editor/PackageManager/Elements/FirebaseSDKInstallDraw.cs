using UnityEditor;
using UnityEngine;

namespace Sonat.Editor.PackageManager.Elements
{
    public class FirebaseSDKInstallDraw
    {
        private string versionInstalled = "";
        public bool installed = false;
        private bool previewAfterDownload = true;
        public bool hasSymbol;
        private int versionSelected;
        private bool upgrade;
        private bool customVersion;

        private string featureName, featureId, symbol;

        private SonatFirebaseWindow firebaseWindow;

        public FirebaseSDKInstallDraw(SonatFirebaseWindow firebaseWindow)
        {
            this.firebaseWindow = firebaseWindow;
        }

        public void Init(string featureName, string featureId, string symbol)
        {
            this.featureName = featureName;
            this.featureId = featureId;
            this.symbol = symbol;
            
            CheckAnalyticsInstalled();
        }

        private void CheckAnalyticsInstalled()
        {
            if (string.IsNullOrEmpty(versionInstalled))
            {
                //versionInstalled = SonatEditorHelper.CheckVersionInstalled("package.json", "AppsFlyer", @"""version"": ""(?<ver>.+)""");
                versionInstalled =
                    SonatEditorHelper.CheckVersionInstalledByFileName($"{featureId}_version-", @"Firebase", $@"{featureId}_version-(?<ver>.+)_");
            }

            installed = !string.IsNullOrEmpty(versionInstalled);
            hasSymbol = string.IsNullOrEmpty(symbol) || SonatEditorHelper.HasSymbol(symbol, EditorUserBuildSettings.selectedBuildTargetGroup);
            versionSelected = firebaseWindow.firebaseVersions.Length - 1;
            
            if (!installed && hasSymbol)
            {
                hasSymbol = false;
                SonatEditorHelper.RemoveSymbolFromBuildTarget(symbol);
            }
        }

        public void Draw()
        {
            GUILayout.BeginVertical(new GUIStyle(GUI.skin.box));
            GUILayout.Label($"{featureName} SDK", EditorStyles.boldLabel);
            GUILayout.Space(5);
            if (installed)
            {
                GUILayout.BeginHorizontal();
                GUILayout.Label($"{featureName} Installed Version: {versionInstalled}", SonatSDKWindow.labelGreenStyle);
                EditorGUI.BeginDisabledGroup(!installed || hasSymbol);
                string symbolLabel = hasSymbol ? "Symbol Added" : "Add Symbol";
                if (GUILayout.Button(symbolLabel, GUILayout.Width(100)))
                {
                    SonatEditorHelper.AddSymbol(new[] { symbol }, EditorUserBuildSettings.selectedBuildTargetGroup);
                }

                EditorGUI.EndDisabledGroup();
                GUILayout.EndHorizontal();
            }
            else
            {
                EditorGUILayout.HelpBox($"{featureName} Not Installed Yet", MessageType.Error);
            }

            GUILayout.Space(3);

            if (installed)
            {
                upgrade = EditorGUILayout.Foldout(upgrade, "Upgrade", true);
                GUILayout.Space(3);
            }
            else
            {
                upgrade = true;
            }

            if (upgrade)
            {
                GUILayout.BeginHorizontal();
                EditorGUIUtility.labelWidth = 80;
                if (!customVersion)
                {
                    versionSelected = firebaseWindow.selectedVersion;
                }
                versionSelected = EditorGUILayout.Popup("Version", versionSelected, firebaseWindow.firebaseVersions, GUILayout.Width(200));
                if (!customVersion)
                {
                    firebaseWindow.selectedVersion = versionSelected;
                }
                customVersion = EditorGUILayout.ToggleLeft("Fix version", customVersion);
                previewAfterDownload = EditorGUILayout.Toggle("Preview", previewAfterDownload);
                string installLabel = "Install";
                if (installed)
                {
                    installLabel = "Upgrade";
                }

                EditorGUI.BeginDisabledGroup(installed && versionInstalled == firebaseWindow.firebaseVersions[versionSelected]);
                if (GUILayout.Button(installLabel, GUILayout.Width(120)))
                {
                    string verInstall = firebaseWindow.firebaseVersions[versionSelected];
                    string url = SonatSDKWindow.packageInfo.firebase[verInstall];
                    url = url.Replace("[FEATURE]", featureId);
                    SonatPackageHelper.InstallPackage(url, $"{featureId}_{verInstall}", previewAfterDownload);
                }

                EditorGUI.EndDisabledGroup();
                GUILayout.EndHorizontal();
            }

            GUILayout.Space(5);
            GUILayout.EndVertical();
        }
    }
}
