using System.Linq;
using Sonat.Editor.PackageManager.Elements;
using Sonat.FirebaseModule.Analytic;
using UnityEditor;
using UnityEngine;

#if UNITY_EDITOR
namespace Sonat.Editor.PackageManager
{
    public class SonatFirebaseWindow
    {
        private SonatSDKWindow sonatSDKWindow;
        private GUIContent[] myContent;

        public SonatFirebaseConfig sonatFirebaseConfig;

        private AnalyticWindowDraw analyticWindowDraw;
        private RemoteConfigWindowDraw remoteConfigWindowDraw;

        public string[] firebaseVersions;
        public int selectedVersion;
        private FirebaseSDKInstallDraw analyticsInstall;
        private FirebaseSDKInstallDraw installationInstall;
        private FirebaseSDKInstallDraw crashlyticsInstall;
        private FirebaseSDKInstallDraw remoteConfigInstall;
        private FirebaseSDKInstallDraw messagingInstall;

        public SonatFirebaseWindow(SonatSDKWindow sonatSDKWindow)
        {
            this.sonatSDKWindow = sonatSDKWindow;
        }

        public void Init()
        {
            myContent = new GUIContent[]
            {
                new GUIContent("RemoteConfig"),
                new GUIContent("Analytics"),
                new GUIContent("Install")
            };

            sonatFirebaseConfig = SonatEditorHelper.LoadConfigSo<SonatFirebaseConfig>(nameof(SonatFirebaseConfig));

            analyticWindowDraw = new AnalyticWindowDraw(this);
            remoteConfigWindowDraw = new RemoteConfigWindowDraw();
            remoteConfigWindowDraw.Init(this);

            firebaseVersions = SonatSDKWindow.packageInfo.firebase.Keys.ToArray();
            analyticsInstall = new(this);
            installationInstall = new(this);
            crashlyticsInstall = new(this);
            remoteConfigInstall = new(this);
            messagingInstall = new(this);

            analyticsInstall.Init("Firebase Analytics", "FirebaseAnalytics", "using_firebase_analytics");
            installationInstall.Init("Firebase Installations", "FirebaseInstallations", "using_firebase_installation");
            crashlyticsInstall.Init("Firebase Crashlytics", "FirebaseCrashlytics", "using_firebase_crashlytics");
            remoteConfigInstall.Init("Firebase Remote Config", "FirebaseRemoteConfig", "using_firebase_remote");
            messagingInstall.Init("Firebase Messaging", "FirebaseMessaging", "using_firebase_message");
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
                GUIStyle style = sonatSDKWindow.firebaseTab == i ? selectedTabStyle : tabStyle;
                if (GUILayout.Toggle(sonatSDKWindow.firebaseTab == i, myContent[i], style))
                    sonatSDKWindow.firebaseTab = i;
            }

            GUILayout.EndVertical();

            GUILayout.Box(Texture2D.blackTexture, GUILayout.Width(1.2f), GUILayout.ExpandHeight(true));

            GUILayout.BeginVertical(new GUIStyle(GUI.skin.box));
            switch (sonatSDKWindow.firebaseTab)
            {
                case 0:
                    ShowRemoteConfigPanel();
                    break;
                case 1:
                    ShowAnalyticPanel();
                    break;
                case 2:
                    ShowInstallPanel();
                    break;
            }

            GUILayout.EndVertical();
            GUILayout.EndHorizontal();
        }

        private void ShowAnalyticPanel()
        {
            Undo.RecordObject(sonatFirebaseConfig, "SonatFirebaseConfig");
            analyticWindowDraw.Draw();
            EditorUtility.SetDirty(sonatFirebaseConfig);
        }

        private void ShowRemoteConfigPanel()
        {
            Undo.RecordObject(sonatFirebaseConfig, "SonatFirebaseConfig");
            remoteConfigWindowDraw.Draw();
            EditorUtility.SetDirty(sonatFirebaseConfig);
        }

        private Vector2 scrollPosition = Vector2.zero;

        private void ShowInstallPanel()
        {
            // GUILayout.BeginHorizontal();
            // GUILayout.Label("Firebase Version Common", EditorStyles.boldLabel);
            // selectedVersion = EditorGUILayout.Popup("", selectedVersion, firebaseVersions, GUILayout.Width(200));
            // GUILayout.EndHorizontal();
            //
            // GUILayout.Box(Texture2D.whiteTexture, GUILayout.Height(1.5f), GUILayout.ExpandWidth(true));
            //
            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
            analyticsInstall.Draw();
            installationInstall.Draw();
            crashlyticsInstall.Draw();
            remoteConfigInstall.Draw();
            messagingInstall.Draw();
            
            EditorGUILayout.EndScrollView();

            GUILayout.BeginHorizontal();
            
            GUILayout.FlexibleSpace();
            
            bool allInstalled = analyticsInstall.installed && installationInstall.installed && crashlyticsInstall.installed && remoteConfigInstall.installed &&
                                messagingInstall.installed;
            bool hasSymbolAll = analyticsInstall.hasSymbol && installationInstall.hasSymbol && crashlyticsInstall.hasSymbol && remoteConfigInstall.hasSymbol &&
                                messagingInstall.hasSymbol;

            EditorGUI.BeginDisabledGroup(!allInstalled || hasSymbolAll);
            if (GUILayout.Button("Add All Symbols"))
            {
                SonatEditorHelper.AddSymbol(
                    new[]
                    {
                        "using_firebase_analytics", "using_firebase_installation", "using_firebase_crashlytics", "using_firebase_remote",
                        "using_firebase_message"
                    }, EditorUserBuildSettings.selectedBuildTargetGroup);
            }

            EditorGUI.EndDisabledGroup();
            GUILayout.EndHorizontal();

        }
    }
}
#endif