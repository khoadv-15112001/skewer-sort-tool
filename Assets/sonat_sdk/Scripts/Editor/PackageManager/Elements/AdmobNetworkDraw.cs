using UnityEditor;
using UnityEngine;

namespace Sonat.Editor.PackageManager.Elements
{
    public class AdmobNetworkDraw
    {
        private string versionInstalled = "";
        private bool installed = false, previewAfterDownload = true;
        private int versionSelected;
        private bool upgrade;
        private string[] versions;
        private string networkName, networkIdId, versionPattern;
        private string url;

        private SonatAdsManagerWindow sonatAdsWindow;

        public AdmobNetworkDraw(SonatAdsManagerWindow sonatAdsWindow, string netWorkName, string networkIdId, string versionPattern)
        {
            this.sonatAdsWindow = sonatAdsWindow;
            this.networkName = netWorkName;
            this.networkIdId = networkIdId;
            this.versionPattern = versionPattern;
            this.url = SonatSDKWindow.packageInfo.admobNetworkUrls[networkIdId]["1.0"];
        }


        public void CheckNetworkInstalled()
        {
            if (string.IsNullOrEmpty(versionInstalled))
            {
                versionInstalled = SonatEditorHelper.CheckVersionInstalled($"{networkIdId}MediationDependencies.xml", @"GoogleMobileAds\Mediation", versionPattern);
                
            }

            installed = !string.IsNullOrEmpty(versionInstalled);
            //versionSelected = versions.Length - 1;
           
        }

        public void Draw()
        {
            GUILayout.BeginVertical(new GUIStyle(GUI.skin.box));
            GUILayout.Label($"{networkName} Mediation Network", EditorStyles.boldLabel);
            GUILayout.Space(5);
            if (installed)
            {
                GUILayout.BeginHorizontal();
                GUILayout.Label($"{networkName} Installed Version: {versionInstalled}", SonatSDKWindow.labelGreenStyle);;
                GUILayout.EndHorizontal();
            }
            else
            {
                EditorGUILayout.HelpBox($"{networkName} Not Installed Yet", MessageType.Error);
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

                //versionSelected = EditorGUILayout.Popup("Version", versionSelected, versions, GUILayout.Width(200));
 
               // previewAfterDownload = EditorGUILayout.Toggle("Preview", previewAfterDownload);
                string installLabel = "Install";
                if (installed)
                {
                    installLabel = "Upgrade";
                }

                //EditorGUI.BeginDisabledGroup(installed && versionInstalled == versions[versionSelected]);
                if (GUILayout.Button(installLabel, GUILayout.Width(120)))
                {
                    // string verInstall = versions[versionSelected];
                    // string url = SonatSDKWindow.packageInfo.admobNetworkUrls[networkIdId][versions[versionSelected]];
                    // url = url.Replace("[FEATURE]", networkIdId);
                    // SonatPackageHelper.InstallPackage(url, $"{networkIdId}_{verInstall}", previewAfterDownload);
                    Application.OpenURL(url);
                }

                //EditorGUI.EndDisabledGroup();
                GUILayout.EndHorizontal();
            }

            GUILayout.Space(5);
            GUILayout.EndVertical();
        }
    }
}
