#if UNITY_EDITOR
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Sonat.AdsModule;
using Sonat.Editor.PackageManager.Elements;
using UnityEditor;
using UnityEngine;

namespace Sonat.Editor.PackageManager
{
    public class SonatAdsManagerWindow
    {
        private bool usingAdmob, lastUsingAdmob, foldOutAdmob;
        private bool usingMax, lastUsingMax, foldOutMax;

        private GUIContent[] myContent;

        private SonatSDKWindow sonatSDKWindow;
        private MediationSetupDraw admobMediationSetupDraw;
        private MediationSetupDraw maxMediationSetupDraw;
        private SonatAds sonatAds;
        private SonatMediation admobMediation;
        private SonatMediation maxMediation;

        private bool usingAps;
        private bool usingNativeAdmob;

        private bool usageAdsService, lastUsageAdsService;

        private List<AdmobNetworkDraw> networksInstaller = new();

        #region Serialize

        private SerializedObject serializedObject;
        private SerializedProperty configProperty;

        #endregion

        public SonatAdsManagerWindow(SonatSDKWindow sonatSDKWindow)
        {
            this.sonatSDKWindow = sonatSDKWindow;
        }

        public void Init()
        {
            myContent = new GUIContent[]
            {
                new GUIContent("Mediation"),
                new GUIContent("Settings"),
                new GUIContent("Install")
            };


            sonatAds = SonatEditorHelper.LoadConfigSo<SonatAds>(nameof(SonatAds));
            if (sonatAds != null)
            {
                FindProperties();
                usageAdsService = SonatSDKWindow.sonatSdkServices.HasService(sonatAds);
                lastUsageAdsService = usageAdsService;
                //maxMediationSetupDraw = new MediationSetupDraw(sonatAds.GetMediation(MediationType.Max));
            }

            admobMediation = SonatEditorHelper.LoadConfigSo<SonatMediation>("AdmobMediation");
            if (admobMediation != null)
            {
                admobMediationSetupDraw = new MediationSetupDraw(admobMediation);
            }

            maxMediation = SonatEditorHelper.LoadConfigSo<SonatMediation>("MaxMediation");
            if (maxMediation != null)
            {
                maxMediationSetupDraw = new MediationSetupDraw(maxMediation);
            }

            usingAdmob = sonatAds.mediations.Contains(admobMediation);
            lastUsingAdmob = usingAdmob;
            usingMax = sonatAds.mediations.Contains(maxMediation);
            lastUsingMax = usingMax;

            networksInstaller = new();
            var applovinInstaller = new AdmobNetworkDraw(this, "Applovin", "Applovin", @"""com.google.ads.mediation:applovin:(?<ver>.+)""");

            var dtExchangeInstaller = new AdmobNetworkDraw(this, "DTExchange", "DTExchange", @"""com.google.ads.mediation:fyber:(?<ver>.+)""");


            var ironSource = new AdmobNetworkDraw(this, "IronSource", "IronSource", @"""com.google.ads.mediation:ironsource:(?<ver>.+)""");
            var Liftoff = new AdmobNetworkDraw(this, "Liftoff", "LiftoffMonetize", @"""com.google.ads.mediation:vungle:(?<ver>.+)""");
            var line = new AdmobNetworkDraw(this, "LINE", "Line", @"""com.google.ads.mediation:line:(?<ver>.+)""");
            var Meta = new AdmobNetworkDraw(this, "Meta", "MetaAudienceNetwork", @"""com.google.ads.mediation:facebook:(?<ver>.+)""");
            var mintegral = new AdmobNetworkDraw(this, "Mintegral", "Mintegral", @"""com.google.ads.mediation:mintegral:(?<ver>.+)""");
            var pangle = new AdmobNetworkDraw(this, "Pangle", "Pangle", @"""com.google.ads.mediation:pangle:(?<ver>.+)""");
            var unity = new AdmobNetworkDraw(this, "Unity Ads", "Unity", @"""com.google.ads.mediation:unity:(?<ver>.+)""");


            networksInstaller.Add(dtExchangeInstaller);
            networksInstaller.Add(applovinInstaller);
            networksInstaller.Add(ironSource);
            networksInstaller.Add(Liftoff);
            networksInstaller.Add(line);
            networksInstaller.Add(Meta);
            networksInstaller.Add(mintegral);
            networksInstaller.Add(pangle);
            networksInstaller.Add(unity);

            CheckAdmobInstalled();
            CheckAdmobNativeInstalled();
            CheckMaxInstalled();
            CheckApsInstalled();
        }

        private void FindProperties()
        {
            serializedObject = new SerializedObject(sonatAds);
            configProperty = serializedObject.FindProperty("config");
        }

        public void Show()
        {
            if (sonatAds == null) return;

            var tabStyle = EditorStyles.toolbarButton;
            tabStyle.alignment = TextAnchor.MiddleLeft;
            var selectedTabStyle = new GUIStyle(tabStyle);
            selectedTabStyle.normal.background = selectedTabStyle.active.background;

            GUILayout.BeginHorizontal();


            GUILayout.BeginVertical(GUILayout.Width(150), GUILayout.ExpandHeight(true));

            EditorGUI.BeginDisabledGroup(!usageAdsService);
            for (int i = 0; i < myContent.Length; i++)
            {
                GUIStyle style = sonatSDKWindow.adsTab == i ? selectedTabStyle : tabStyle;
                if (GUILayout.Toggle(sonatSDKWindow.adsTab == i, myContent[i], style))
                    sonatSDKWindow.adsTab = i;
            }

            GUILayout.FlexibleSpace();
            EditorGUI.EndDisabledGroup();
            GUILayout.BeginHorizontal();
            GUILayout.Space(5);
            EditorGUIUtility.labelWidth = 60;
            usageAdsService = EditorGUILayout.ToggleLeft("Using", usageAdsService);
            CheckLast();
            GUILayout.EndHorizontal();
            GUILayout.Space(5);
            GUILayout.EndVertical();

            EditorGUI.BeginDisabledGroup(!usageAdsService);
            GUILayout.Box(Texture2D.blackTexture, GUILayout.Width(1.2f), GUILayout.ExpandHeight(true));
            GUILayout.BeginHorizontal();
            GUILayout.Space(10);

            GUILayout.BeginVertical(GUI.skin.box);
            switch (sonatSDKWindow.adsTab)
            {
                case 0:
                    DrawMediationSetup();
                    break;
                case 1:
                    DrawSettingsPanel();
                    break;
                case 2:
                    DrawInstallPanel();
                    break;
            }

            GUILayout.EndVertical();
            GUILayout.EndHorizontal();
            EditorGUI.EndDisabledGroup();
            GUILayout.EndHorizontal();
        }

        private void DrawMediationSetup()
        {
            Undo.RecordObject(sonatAds, "Sonat Ads");
            GUILayout.BeginHorizontal();
            usingAdmob = EditorGUILayout.ToggleLeft("", usingAdmob, GUILayout.Width(20));
            CheckLastUsageAdmob();
            EditorGUI.BeginDisabledGroup(!usingAdmob);
            foldOutAdmob = EditorGUILayout.Foldout(foldOutAdmob, "Admob Mediation", true);
            GUILayout.EndHorizontal();
            GUILayout.BeginVertical();
            if (foldOutAdmob)
            {
                GUILayout.BeginHorizontal();
                GUILayout.Space(20);
                GUILayout.BeginVertical();
                GUILayout.Space(10);

                admobMediationSetupDraw.ShowMediationSetupTab();
                GUILayout.EndVertical();
                GUILayout.EndHorizontal();
            }

            GUILayout.EndVertical();
            EditorGUI.EndDisabledGroup();


            GUILayout.Space(10);


            GUILayout.BeginHorizontal();
            usingMax = EditorGUILayout.ToggleLeft("", usingMax, GUILayout.Width(20));
            CheckLastUsageMax();
            EditorGUI.BeginDisabledGroup(!usingMax);
            foldOutMax = EditorGUILayout.Foldout(foldOutMax, "MAX Mediation", true);
            GUILayout.EndHorizontal();
            GUILayout.BeginVertical();
            if (foldOutMax)
            {
                GUILayout.BeginHorizontal();
                GUILayout.Space(20);
                GUILayout.BeginVertical();
                GUILayout.Space(10);

                maxMediationSetupDraw.ShowMediationSetupTab();
                GUILayout.EndVertical();
                GUILayout.EndHorizontal();
            }

            GUILayout.EndVertical();
            EditorGUI.EndDisabledGroup();
            EditorUtility.SetDirty(sonatAds);
        }


        private void CheckLast()
        {
            if (usageAdsService != lastUsageAdsService)
            {
                lastUsageAdsService = usageAdsService;
                if (usageAdsService)
                {
                    SonatSDKWindow.sonatSdkServices.TryAddService(sonatAds);
                }
                else
                {
                    SonatSDKWindow.sonatSdkServices.TryRemoveService(sonatAds);
                    sonatSDKWindow.adsTab = 0;
                }
            }
        }

        private void CheckLastUsageAdmob()
        {
            if (usingAdmob != lastUsingAdmob)
            {
                lastUsingAdmob = usingAdmob;
                if (usingAdmob)
                {
                    if (!sonatAds.mediations.Contains(admobMediation))
                        sonatAds.mediations.Add(admobMediation);
                }
                else
                {
                    if (sonatAds.mediations.Contains(admobMediation))
                        sonatAds.mediations.Remove(admobMediation);
                }
            }
        }

        private void CheckLastUsageMax()
        {
            if (usingMax != lastUsingMax)
            {
                lastUsingMax = usingMax;
                if (usingMax)
                {
                    if (!sonatAds.mediations.Contains(maxMediation))
                        sonatAds.mediations.Add(maxMediation);
                }
                else
                {
                    if (sonatAds.mediations.Contains(maxMediation))
                        sonatAds.mediations.Remove(maxMediation);
                }
            }
        }

        private void DrawSettingsPanel()
        {
            sonatAds.testAds = EditorGUILayout.Toggle("Test Ads", sonatAds.testAds);
            GUILayout.Space(5);
            EditorGUIUtility.labelWidth = 200;
            EditorGUILayout.PropertyField(configProperty, new GUIContent("Configs"));
            serializedObject.ApplyModifiedProperties();
        }


        private void DrawInstallPanel()
        {
            AdmobInstalliation();
            GUILayout.Space(5);
            MaxInstallation();
            GUILayout.Space(5);
            usingAps = EditorGUILayout.Foldout(usingAps, "AMAZON", true);
            if (usingAps)
            {
                ApsInstallation();
            }
        }


        private string admobVersionInstalled = "";
        private bool admobInstalled = false, previewAfterDownload = true;
        private bool hasAdmobSymbol;
        private string[] admobVersions;
        private int admobVersionSelected;
        private bool admobUpgrade;

        private void CheckAdmobInstalled()
        {
            if (string.IsNullOrEmpty(admobVersionInstalled))
            {
                //admobVersionInstalled = SonatEditorHelper.CheckVersionInstalled("package.json", "AppsFlyer", @"""version"": ""(?<ver>.+)""");
                admobVersionInstalled =
                    SonatEditorHelper.CheckVersionInstalledByFileName("GoogleMobileAds_version-", "GoogleMobileAds", @"GoogleMobileAds_version-(?<ver>.+)_");
            }

            admobInstalled = !string.IsNullOrEmpty(admobVersionInstalled);
            hasAdmobSymbol = SonatEditorHelper.HasSymbol("using_admob", EditorUserBuildSettings.selectedBuildTargetGroup);
            admobVersions = SonatSDKWindow.packageInfo.admobUrls.Keys.ToArray();
            admobVersionSelected = admobVersions.Length - 1;
            if (!admobInstalled && hasAdmobSymbol)
            {
                hasAdmobSymbol = false;
                SonatEditorHelper.RemoveSymbolFromBuildTarget("using_admob");
            }

            if (admobInstalled)
            {
                foreach (var installer in networksInstaller)
                {
                    installer.CheckNetworkInstalled();
                }
            }
        }

        private void AdmobInstalliation()
        {
            GUILayout.BeginVertical(new GUIStyle(GUI.skin.box));
            GUILayout.Label($"Admob SDK", EditorStyles.boldLabel);
            GUILayout.Space(5);
            if (admobInstalled)
            {
                GUILayout.BeginHorizontal();

                GUILayout.Label($"Admob Installed Version: {admobVersionInstalled}", SonatSDKWindow.labelGreenStyle);
                EditorGUI.BeginDisabledGroup(!admobInstalled || hasAdmobSymbol);
                string symbolLabel = hasAdmobSymbol ? "Symbol Added" : "Add Symbol";
                if (GUILayout.Button(symbolLabel, GUILayout.Width(120)))
                {
                    if (hasAdmobSymbol)
                    {
                        sonatSDKWindow.ShowNotification(new GUIContent("Admob Symbol Has Exist!"), 0.5f);
                    }
                    else
                    {
                        SonatEditorHelper.AddSymbol(new[] { "using_admob" }, EditorUserBuildSettings.selectedBuildTargetGroup);
                    }
                }

                EditorGUI.EndDisabledGroup();
                GUILayout.EndHorizontal();
            }
            else
            {
                EditorGUILayout.HelpBox("Admob Not Installed Yet", MessageType.Error);
            }

            GUILayout.Space(3);

            if (admobInstalled)
            {
                admobUpgrade = EditorGUILayout.Foldout(admobUpgrade, "Upgrade", true);
                GUILayout.Space(3);
            }
            else
            {
                admobUpgrade = true;
            }

            if (admobUpgrade)
            {
                GUILayout.BeginHorizontal();
                //GUILayout.Label("Install");
                admobVersionSelected = EditorGUILayout.Popup("Version", admobVersionSelected, admobVersions, GUILayout.Width(200));
                previewAfterDownload = EditorGUILayout.Toggle("Preview", previewAfterDownload);
                string installLabel = "Install";
                if (admobInstalled)
                {
                    installLabel = "Upgrade";
                }

                EditorGUI.BeginDisabledGroup(admobInstalled && admobVersionInstalled == admobVersions[^1]);
                if (GUILayout.Button(installLabel, GUILayout.Width(120)))
                {
                    string verInstall = admobVersions[admobVersionSelected];
                    string url = SonatSDKWindow.packageInfo.admobUrls[admobVersions[admobVersionSelected]];
                    SonatPackageHelper.InstallPackage(url, $"GoogleMobileAds-v{verInstall}", previewAfterDownload);
                }

                EditorGUI.EndDisabledGroup();

                GUILayout.EndHorizontal();
            }

            if (admobInstalled)
            {
                GUILayout.Space(5);
                sonatSDKWindow.installAdmobNetworks = EditorGUILayout.Foldout(sonatSDKWindow.installAdmobNetworks, new GUIContent("Mediation Networks"), true,
                    new GUIStyle(EditorStyles.foldout) { fontStyle = FontStyle.Bold });
                if (sonatSDKWindow.installAdmobNetworks)
                {
                    GUILayout.BeginHorizontal();
                    GUILayout.Space(30);
                    GUILayout.BeginVertical();

                    GUILayout.BeginHorizontal();
                    GUILayout.FlexibleSpace();
                    if (GUILayout.Button("Refresh", GUILayout.Width(150)))
                    {
                        foreach (var installer in networksInstaller)
                        {
                            installer.CheckNetworkInstalled();
                        }
                    }

                    GUILayout.EndHorizontal();

                    sonatSDKWindow.installAdmobNetworksScrollPos = EditorGUILayout.BeginScrollView(sonatSDKWindow.installAdmobNetworksScrollPos,
                        new GUIStyle(GUI.skin.box), GUILayout.Height(
                            sonatSDKWindow.position.height * 0.6f), GUILayout.MinHeight(300));
                    foreach (var installer in networksInstaller)
                    {
                        installer.Draw();
                    }

                    EditorGUILayout.EndScrollView();
                    GUILayout.EndVertical();
                    GUILayout.EndHorizontal();
                }

                GUILayout.Space(5);
            }


            if (admobInstalled && hasAdmobSymbol)
            {
                GUILayout.Space(2);
                usingNativeAdmob = EditorGUILayout.ToggleLeft("Using Native Ads", usingNativeAdmob);
                if (usingNativeAdmob)
                {
                    AdmobNativeInstallation();
                }
            }

            GUILayout.Space(5);
            GUILayout.EndVertical();
        }


        private string admobNativeVersionInstalled = "";
        private bool admobNativeInstalled = false, previewAfterDownloadAdmobNative = true;
        private bool hasAdmobNativeSymbol;

        private void CheckAdmobNativeInstalled()
        {
            if (string.IsNullOrEmpty(admobNativeVersionInstalled))
            {
                admobNativeVersionInstalled = SonatEditorHelper.CheckVersionInstalled("GoogleMobileAdsNativeDependencies.xml", "GoogleMobileAdsNative",
                    @"""com.google.code.gson:gson:(?<ver>.+)""");
            }

            admobNativeInstalled = !string.IsNullOrEmpty(admobNativeVersionInstalled);
            hasAdmobNativeSymbol = SonatEditorHelper.HasSymbol("using_admob_native", EditorUserBuildSettings.selectedBuildTargetGroup);
            usingNativeAdmob = admobNativeInstalled;

            if (!admobInstalled && hasAdmobNativeSymbol)
            {
                hasAdmobNativeSymbol = false;
                SonatEditorHelper.RemoveSymbolFromBuildTarget("using_admob_native");
            }
        }


        private void AdmobNativeInstallation()
        {
            GUILayout.BeginVertical(new GUIStyle(GUI.skin.box));
            GUILayout.Label($"Admob Native Library", EditorStyles.boldLabel);
            GUILayout.Space(5);
            if (admobNativeInstalled)
            {
                GUILayout.BeginHorizontal();
                GUILayout.Label($"Admob Native Library Installed Version: {admobNativeVersionInstalled}", SonatSDKWindow.labelGreenStyle);
                EditorGUI.EndDisabledGroup();
                EditorGUI.BeginDisabledGroup(!admobNativeInstalled || hasAdmobNativeSymbol);
                string symbolLabel = hasAdmobNativeSymbol ? "Symbol Added" : "Add Symbol";
                if (GUILayout.Button(symbolLabel, GUILayout.Width(120)))
                {
                    SonatEditorHelper.AddSymbol(new[] { "using_admob_native" }, EditorUserBuildSettings.selectedBuildTargetGroup);
                }

                EditorGUI.EndDisabledGroup();
                GUILayout.EndHorizontal();
            }
            else
            {
                EditorGUILayout.HelpBox("Admob Native Library Not Installed Yet", MessageType.Error);
            }


            GUILayout.Space(3);

            GUILayout.BeginHorizontal();

            string installLabel = "Install";
            if (admobNativeInstalled)
            {
                installLabel = "Upgrade";
            }


            if (GUILayout.Button(installLabel, GUILayout.Width(120)))
            {
                SonatPackageHelper.InstallPackage(SonatSDKWindow.packageInfo.admobNativeLibraryUrl, $"GoogleMobileAds-native", previewAfterDownloadAdmobNative);
            }

            GUILayout.EndHorizontal();

            GUILayout.Space(5);
            GUILayout.EndVertical();
        }


        private string maxVersionInstalled = "";
        private bool maxInstalled = false, previewAfterDownloadMax = true;
        private bool hasMaxSymbol;
        private string[] maxVersions;
        private int maxVersionSelected;
        private bool maxUpgrade;

        private void CheckMaxInstalled()
        {
            if (string.IsNullOrEmpty(maxVersionInstalled))
            {
                //admobVersionInstalled = SonatEditorHelper.CheckVersionInstalled("package.json", "AppsFlyer", @"""version"": ""(?<ver>.+)""");
                maxVersionInstalled =
                    SonatEditorHelper.CheckVersionInstalled("Dependencies.xml", @"MaxSdk\AppLovin\Editor", @"""com.applovin:applovin-sdk:(?<ver>.+)""") ??
                    SonatEditorHelper.CheckVersionInstalledByManifest("com.applovin.mediation.ads");
            }

            maxInstalled = !string.IsNullOrEmpty(maxVersionInstalled);
            hasMaxSymbol = SonatEditorHelper.HasSymbol("using_max", EditorUserBuildSettings.selectedBuildTargetGroup);

            maxVersions = SonatSDKWindow.packageInfo.maxUrls.Keys.ToArray();
            if (maxInstalled)
            {
                foreach (var ver in SonatSDKWindow.packageInfo.maxUrls)
                {
                    if (ver.Value.Contains(maxVersionInstalled))
                    {
                        maxVersionInstalled = ver.Key;
                        break;
                    }
                }
            }

            maxVersionSelected = maxVersions.Length - 1;

            if (!maxInstalled && hasMaxSymbol)
            {
                hasMaxSymbol = false;
                SonatEditorHelper.RemoveSymbolFromBuildTarget("using_max");
            }
        }


        private void MaxInstallation()
        {
            GUILayout.BeginVertical(new GUIStyle(GUI.skin.box));
            GUILayout.Label($"MAX SDK", EditorStyles.boldLabel);
            GUILayout.Space(5);
            if (maxInstalled)
            {
                GUILayout.BeginHorizontal();
                GUILayout.Label($"MAX Installed Version: {maxVersionInstalled}", SonatSDKWindow.labelGreenStyle);
                EditorGUI.EndDisabledGroup();
                EditorGUI.BeginDisabledGroup(!maxInstalled || hasMaxSymbol);
                string symbolLabel = hasMaxSymbol ? "Symbol Added" : "Add Symbol";
                if (GUILayout.Button(symbolLabel, GUILayout.Width(120)))
                {
                    if (hasMaxSymbol)
                    {
                        sonatSDKWindow.ShowNotification(new GUIContent("MAX Symbol Has Exist!"), 0.5f);
                    }
                    else
                    {
                        SonatEditorHelper.AddSymbol(new[] { "using_max" }, EditorUserBuildSettings.selectedBuildTargetGroup);
                    }
                }

                EditorGUI.EndDisabledGroup();
                GUILayout.EndHorizontal();
            }
            else
            {
                EditorGUILayout.HelpBox("MAX Not Installed Yet", MessageType.Error);
            }


            GUILayout.Space(3);

            if (maxInstalled)
            {
                maxUpgrade = EditorGUILayout.Foldout(maxUpgrade, "Upgrade", true);
                GUILayout.Space(3);
            }
            else
            {
                maxUpgrade = true;
            }

            if (maxUpgrade)
            {
                GUILayout.BeginHorizontal();
                //GUILayout.Label("Install");
                maxVersionSelected = EditorGUILayout.Popup("Version", maxVersionSelected, maxVersions, GUILayout.Width(200));

                previewAfterDownloadMax = EditorGUILayout.Toggle("Preview", previewAfterDownloadMax);
                string installLabel = "Install";
                if (maxInstalled)
                {
                    installLabel = "Upgrade";
                }

                EditorGUI.BeginDisabledGroup(maxInstalled && maxVersionInstalled == maxVersions[^1]);
                if (GUILayout.Button(installLabel, GUILayout.Width(120)))
                {
                    string verInstall = maxVersions[maxVersionSelected];
                    string url = SonatSDKWindow.packageInfo.maxUrls[maxVersions[maxVersionSelected]];
                    SonatPackageHelper.InstallPackage(url, $"AppLovin-MAX-Unity-Plugin-{verInstall}", previewAfterDownloadMax);
                }


                GUILayout.EndHorizontal();
            }

            GUILayout.Space(5);
            GUILayout.EndVertical();
        }


        private string apsVersionInstalled = "";
        private bool apsInstalled = false, previewAfterDownloadAps = true;
        private bool hasApsSymbol;

        private void CheckApsInstalled()
        {
            if (string.IsNullOrEmpty(apsVersionInstalled))
            {
                //admobVersionInstalled = SonatEditorHelper.CheckVersionInstalled("package.json", "AppsFlyer", @"""version"": ""(?<ver>.+)""");
                //apsVersionInstalled =
                //SonatEditorHelper.CheckVersionInstalled("AmazonConstants.cs", "Amazon", @"""VERSION = ""(?<ver>.+)""");
                if (File.Exists(Path.Combine(Application.dataPath, "Amazon", "Scripts", "AmazonConstants.cs")))
                {
                    apsVersionInstalled = "1.9.2";
                }
            }

            apsInstalled = !string.IsNullOrEmpty(apsVersionInstalled);
            hasApsSymbol = SonatEditorHelper.HasSymbol("using_aps", EditorUserBuildSettings.selectedBuildTargetGroup);
            usingAps = apsInstalled;

            if (!apsInstalled && hasApsSymbol)
            {
                hasApsSymbol = false;
                SonatEditorHelper.RemoveSymbolFromBuildTarget("using_aps");
            }
        }


        private void ApsInstallation()
        {
            GUILayout.BeginVertical(new GUIStyle(GUI.skin.box));
            GUILayout.Label($"Amazon SDK", EditorStyles.boldLabel);
            GUILayout.Space(5);
            if (apsInstalled)
            {
                GUILayout.BeginHorizontal();
                GUILayout.Label($"Amazon Installed Version: {apsVersionInstalled}", SonatSDKWindow.labelGreenStyle);
                EditorGUI.EndDisabledGroup();
                EditorGUI.BeginDisabledGroup(!apsInstalled || hasApsSymbol);
                string symbolLabel = hasApsSymbol ? "Symbol Added" : "Add Symbol";
                if (GUILayout.Button(symbolLabel, GUILayout.Width(120)))
                {
                    SonatEditorHelper.AddSymbol(new[] { "using_aps" }, EditorUserBuildSettings.selectedBuildTargetGroup);
                }

                EditorGUI.EndDisabledGroup();
                GUILayout.EndHorizontal();
            }
            else
            {
                EditorGUILayout.HelpBox("Amazon SDK Not Installed Yet", MessageType.Error);
            }


            GUILayout.Space(3);

            GUILayout.BeginHorizontal();

            string installLabel = "Install";
            if (apsInstalled)
            {
                installLabel = "Upgrade";
            }


            if (GUILayout.Button(installLabel, GUILayout.Width(120)))
            {
                Application.OpenURL(SonatSDKWindow.packageInfo.apsUrl);
            }

            GUILayout.EndHorizontal();

            GUILayout.Space(5);
            GUILayout.EndVertical();
        }
    }
}
#endif