#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEngine;

namespace Sonat.Editor.PackageManager
{
    public class SonatSDKWindow : EditorWindow
    {
        public static SonatSdkSettings settings;
        public static SonatSdkServices sonatSdkServices;

        private SonatStartupDraw sonatStartupDraw;
        private SonatSDKSettingsWindow settingsWindow;
        private SonatAdsManagerWindow adsManagerWindow;
        private IAPManagerWindow iapManagerWindow;
        private SonatFirebaseWindow firebaseWindow;
        private SonatOtherWindow otherWindow;

        private int tab;
        public static SonatPackageInfo packageInfo;
        
        public static GUIStyle labelGreenStyle = new GUIStyle();
        public static Color orange;


        public int settingsTab;
        public int adsTab;
        public int iapTab;
        public int firebaseTab;
        public int otherTab;

        public bool installAdmobNetworks;
        public Vector2 installAdmobNetworksScrollPos;

        private bool checkSetup;

        // Add menu item named "My Window" to the Window menu
        [MenuItem("Sonat SDK/Management", priority = 10)]
        public static void ShowWindow()
        {
            //Draw existing window instance. If one doesn't exist, make one.
            EditorWindow.GetWindow(typeof(SonatSDKWindow), false, "Sonat SDK");
        }

        private async void LoadPackageInfo()
        {
            packageInfo = await SonatPackageHelper.LoadPackageInfo();
        }

        private void OnEnable()
        {
            Setup();
            
            Init();
        }

        public void Init()
        {
            if (!CheckSetup())
            {
                sonatStartupDraw = new SonatStartupDraw(this);
                return;
            }
            
            LoadPackageInfo();
            settings = SonatEditorHelper.LoadConfigSo<SonatSdkSettings>("SonatSdkSettings");
            sonatSdkServices = SonatEditorHelper.LoadConfigSo<SonatSdkServices>("SonatSdkServices");


            settingsWindow = new(this);
            adsManagerWindow = new SonatAdsManagerWindow(this);
            iapManagerWindow = new IAPManagerWindow(this);
            firebaseWindow = new SonatFirebaseWindow(this);
            otherWindow = new SonatOtherWindow(this);
            
            settingsWindow.Init();
            adsManagerWindow.Init();
            iapManagerWindow.Init();
            firebaseWindow.Init();
            otherWindow.Init();
        }

        private bool CheckSetup()
        {
            if (Directory.Exists(Path.Combine(Application.dataPath, "SonatSdkStorage")))
            {
                checkSetup = true;
                return true;
            }
            checkSetup = false;
            return false;
        }

        private void Setup()
        {
            ColorUtility.TryParseHtmlString("#67FF58", out var green);
            labelGreenStyle.normal.textColor = green;
            
            ColorUtility.TryParseHtmlString("#FF8000", out orange);
        }

        void OnGUI()
        {
            if (!checkSetup)
            {
                sonatStartupDraw.Draw();
                return;
            }

            tab = GUILayout.Toolbar(tab, new string[] { "Settings", "Ads", "IAP", "Firebase", "Other" });
            GUILayout.Space(2);
            switch (tab)
            {
                case 0:
                    settingsWindow.Show();
                    break;
                case 1:
                    adsManagerWindow.Show();
                    break;
                case 2:
                    iapManagerWindow.Show();
                    break;
                case 3:
                    firebaseWindow.Show();
                    break;
                case 4:
                    otherWindow.Draw();
                    break;
            }
        }
        
    }
}
#endif