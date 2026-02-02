using System.Linq;
using Sonat.FacebookModule;
using UnityEditor;
using UnityEngine;

namespace Sonat.Editor.PackageManager.Elements
{
    public class FacebookPanelDraw
    {
        private SonatFacebook sonatSonatFacebook;
        private SerializedObject serializedFacebookService;
        SerializedProperty loginProperty;

        public void Init()
        {
            sonatSonatFacebook = SonatEditorHelper.LoadConfigSo<SonatFacebook>(nameof(SonatFacebook));
            if (sonatSonatFacebook == null) return;

            serializedFacebookService = new SerializedObject(sonatSonatFacebook);
            loginProperty = serializedFacebookService.FindProperty("login");
            CheckFacebookInstalled();
        }

        public void Draw()
        {
            if (sonatSonatFacebook == null) return;
            GUILayout.BeginVertical(new GUIStyle(GUI.skin.box));
            GUILayout.Space(5);
            EditorGUIUtility.labelWidth = 60;
            EditorGUILayout.PropertyField(loginProperty, new GUIContent("Login"));
            serializedFacebookService.ApplyModifiedProperties();
            GUILayout.Space(5);
            GUILayout.EndVertical();

            GUILayout.Space(10);

            GUILayout.BeginVertical(new GUIStyle(GUI.skin.box));
            GUILayout.Label("INSTALL", EditorStyles.boldLabel);
            GUILayout.Box(Texture2D.whiteTexture, GUILayout.Height(1.5f), GUILayout.ExpandWidth(true));
            GUILayout.Space(5);
            
            FacebookInstallation();
            GUILayout.EndVertical();
        }


        private string facebookVersionInstalled = "";
        private bool facebookInstalled = false, previewAfterDownload = true;
        private bool hasFacebookSymbol;
        private string[] facebookVersions;
        private int facebookVersionSelected;
        private bool upgradeFacebook;

        private void CheckFacebookInstalled()
        {
            if (string.IsNullOrEmpty(facebookVersionInstalled))
            {
                //admobVersionInstalled = SonatEditorHelper.CheckVersionInstalled("package.json", "AppsFlyer", @"""version"": ""(?<ver>.+)""");
                facebookVersionInstalled =
                    SonatEditorHelper.CheckVersionInstalled("Dependencies.xml", @"FacebookSDK\Plugins\Editor", @"""FBSDKCoreKit"" version=""~> (?<ver>.+)""");
            }

            facebookInstalled = !string.IsNullOrEmpty(facebookVersionInstalled);
            hasFacebookSymbol = SonatEditorHelper.HasSymbol("using_facebook", EditorUserBuildSettings.selectedBuildTargetGroup);

            facebookVersions = SonatSDKWindow.packageInfo.facebookUrls.Keys.ToArray();
            facebookVersionSelected = facebookVersions.Length - 1;
        }

        private void FacebookInstallation()
        {
            GUILayout.BeginVertical(new GUIStyle(GUI.skin.box));
            GUILayout.Label($"Facebook SDK", EditorStyles.boldLabel);
            GUILayout.Space(5);
            if (facebookInstalled)
            {
                GUILayout.BeginHorizontal();
                GUILayout.Label($"Facebook Installed Version: {facebookVersionInstalled}", SonatSDKWindow.labelGreenStyle);
                EditorGUI.BeginDisabledGroup(!facebookInstalled || hasFacebookSymbol);
                string symbolLabel = hasFacebookSymbol ? "Symbol Added" : "Add Symbol";
                if (GUILayout.Button(symbolLabel, GUILayout.Width(100)))
                {
                    SonatEditorHelper.AddSymbol(new[] { "using_facebook" }, EditorUserBuildSettings.selectedBuildTargetGroup);
                }

                EditorGUI.EndDisabledGroup();
                GUILayout.EndHorizontal();
            }
            else
            {
                EditorGUILayout.HelpBox("Facebook Not Installed Yet", MessageType.Error);
            }

            GUILayout.Space(3);

            if (facebookInstalled)
            {
                upgradeFacebook = EditorGUILayout.Foldout(upgradeFacebook, "Upgrade", true);
                GUILayout.Space(3);
            }
            else
            {
                upgradeFacebook = true;
            }

            if (upgradeFacebook)
            {
                GUILayout.BeginHorizontal();
                //GUILayout.Label("Install");
                facebookVersionSelected = EditorGUILayout.Popup("Version", facebookVersionSelected, facebookVersions, GUILayout.Width(200));

                previewAfterDownload = EditorGUILayout.Toggle("Preview", previewAfterDownload);
                string installLabel = "Install";
                if (facebookInstalled)
                {
                    installLabel = "Upgrade";
                }

                EditorGUI.BeginDisabledGroup(facebookInstalled && facebookVersionInstalled == facebookVersions[^1]);
                if (GUILayout.Button(installLabel, GUILayout.Width(120)))
                {
                    string verInstall = facebookVersions[facebookVersionSelected];
                    string url = SonatSDKWindow.packageInfo.facebookUrls[facebookVersions[facebookVersionSelected]];
                    SonatPackageHelper.InstallPackage(url, $"facebook-unity-sdk-{verInstall}", previewAfterDownload);
                }

                EditorGUI.EndDisabledGroup();


                GUILayout.EndHorizontal();
            }

            GUILayout.Space(5);
            GUILayout.EndVertical();
        }
    }
}