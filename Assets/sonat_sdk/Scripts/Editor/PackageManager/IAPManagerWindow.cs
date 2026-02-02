using System.Collections.Generic;
using System.Linq;
using Sonat.Attributes;
using Sonat.Editor.PackageManager.Elements;
using Sonat.IapModule;
using UnityEditor;
using UnityEditor.PackageManager;
using UnityEditor.PackageManager.Requests;
using UnityEngine;

namespace Sonat.Editor.PackageManager
{
    public class IAPManagerWindow
    {
        private bool enableEditStoreItemKey;
        private Vector2 scrollPos;

        GUIContent[] myContent;

        private SonatSDKWindow sonatSDKWindow;
        private SonatIap sonatIap;
        private SerializedObject serializedIapService;
        private List<StoreProductDraw> storeProductDraws;

        public EnumList enumList;
        private string lastEnumName;
        public List<string> enumNames;
        private bool usageAdsService, lastUsageAdsService;


        SerializedProperty autoRestoreProperty, timeOutProperty, verifyUrlProperty;

        public IAPManagerWindow(SonatSDKWindow sonatSDKWindow)
        {
            this.sonatSDKWindow = sonatSDKWindow;
        }

        public void Init()
        {
            myContent = new GUIContent[]
            {
                new GUIContent("Products"),
                new GUIContent("Settings"),
                new GUIContent("Install")
            };


            sonatIap = SonatEditorHelper.LoadConfigSo<SonatIap>(nameof(SonatIap));
            if (sonatIap == null) return;

            usageAdsService = SonatSDKWindow.sonatSdkServices.HasService(sonatIap);
            lastUsageAdsService = usageAdsService;

            serializedIapService = new SerializedObject(sonatIap);
            autoRestoreProperty = serializedIapService.FindProperty("autoRestore");
            timeOutProperty = serializedIapService.FindProperty("timeWaitStore");
            verifyUrlProperty = serializedIapService.FindProperty("verifyUrl");

            UpdateItemKeyEnum();

            storeProductDraws = new List<StoreProductDraw>();
            if (sonatIap.StoreProductDescriptors != null)
                foreach (StoreProductDescriptor product in sonatIap.StoreProductDescriptors)
                {
                    var drawItem = new StoreProductDraw(product, this);
                    storeProductDraws.Add(drawItem);
                }
            
            CheckIapInstalled();
        }

        private void UpdateItemKeyEnum()
        {
            if (lastEnumName == SonatSDKWindow.settings.iapKeyEnum) return;
            lastEnumName = SonatSDKWindow.settings.iapKeyEnum;
            if (!string.IsNullOrEmpty(lastEnumName))
            {
                enumList = new EnumList(lastEnumName);
                if (enumList is { Names: not null })
                {
                    enumNames = enumList.Names.Select(x => x.text).ToList();
                }
                else
                {
                    enumList = null;
                    enumNames = null;
                }
            }
            else
            {
                enumList = null;
                enumNames = null;
            }
        }

        public void Show()
        {
            if (sonatIap == null) return;

            var tabStyle = EditorStyles.toolbarButton;
            tabStyle.alignment = TextAnchor.MiddleLeft;
            var selectedTabStyle = new GUIStyle(tabStyle);
            selectedTabStyle.normal.background = selectedTabStyle.active.background;

            GUILayout.BeginHorizontal();
            GUILayout.BeginVertical(GUILayout.Width(150), GUILayout.ExpandHeight(true));

            EditorGUI.BeginDisabledGroup(!usageAdsService);
            for (int i = 0; i < myContent.Length; i++)
            {
                GUIStyle style = sonatSDKWindow.iapTab == i ? selectedTabStyle : tabStyle;
                if (GUILayout.Toggle(sonatSDKWindow.iapTab == i, myContent[i], style))
                    sonatSDKWindow.iapTab = i;
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

            GUILayout.Box(Texture2D.blackTexture, GUILayout.Width(1.2f), GUILayout.ExpandHeight(true));

            EditorGUI.BeginDisabledGroup(!usageAdsService);
            GUILayout.BeginHorizontal();
            GUILayout.Space(10);
            GUILayout.BeginVertical();

            switch (sonatSDKWindow.iapTab)
            {
                case 0:
                    DrawProductPanel();
                    break;
                case 1:
                    DrawSettingsPanel();
                    break;
                case 2:
                    IapInstallation();
                    break;
            }

            GUILayout.EndVertical();
            GUILayout.EndHorizontal();
            EditorGUI.EndDisabledGroup();
            GUILayout.EndHorizontal();
        }

        private void DrawProductPanel()
        {
            GUILayout.BeginHorizontal();


            EditorGUI.BeginDisabledGroup(!enableEditStoreItemKey);
            SonatSDKWindow.settings.iapKeyEnum = SonatEditorHelper.TextField("Item Key Enum", SonatSDKWindow.settings.iapKeyEnum, GUILayout.ExpandWidth(true),
                GUILayout.MaxWidth(500));
            EditorGUI.EndDisabledGroup();
            GUILayout.Space(10);
            GUILayout.FlexibleSpace();
            enableEditStoreItemKey = EditorGUILayout.ToggleLeft("Edit", enableEditStoreItemKey, GUILayout.Width(50));
            if (enableEditStoreItemKey)
            {
                UpdateItemKeyEnum();
            }


            GUILayout.EndHorizontal();

            GUILayout.Space(10);


            if (sonatIap.StoreProductDescriptors != null && sonatIap.StoreProductDescriptors.Count > 0)
            {
                scrollPos = EditorGUILayout.BeginScrollView(scrollPos, new GUIStyle(GUI.skin.box), GUILayout.ExpandHeight(true));
                for (int i = 0; i < storeProductDraws.Count; i++)
                {
                    storeProductDraws[i].Draw();
                    GUILayout.Space(2);
                }

                EditorGUILayout.EndScrollView();
            }
            else
            {
                GUILayout.Label("Product list is empty", EditorStyles.centeredGreyMiniLabel);
            }

            GUILayout.Space(5);

            if (GUILayout.Button("+ Product", EditorStyles.miniButton, GUILayout.ExpandWidth(false)))
            {
                AddItem();
            }
            EditorUtility.SetDirty(sonatIap);
        }

        private void CheckLast()
        {
            if (usageAdsService != lastUsageAdsService)
            {
                lastUsageAdsService = usageAdsService;
                if (usageAdsService)
                {
                    SonatSDKWindow.sonatSdkServices.TryAddService(sonatIap);
                }
                else
                {
                    SonatSDKWindow.sonatSdkServices.TryRemoveService(sonatIap);
                }
                EditorUtility.SetDirty(SonatSDKWindow.sonatSdkServices);
            }
        }

        private bool edit;
        private void DrawSettingsPanel()
        {
            Undo.RecordObject(sonatIap, "Edit IAP Settings");
            GUILayout.BeginVertical(new GUIStyle(GUI.skin.box));
            GUILayout.Space(5);
            EditorGUIUtility.labelWidth = 100;
            EditorGUILayout.PropertyField(autoRestoreProperty, new GUIContent("Auto Restore"));
            
            GUILayout.BeginVertical(new GUIStyle(GUI.skin.box));
            edit = EditorGUILayout.ToggleLeft("Edit", edit);
            
            EditorGUI.BeginDisabledGroup(!edit);
            EditorGUILayout.PropertyField(timeOutProperty, new GUIContent("Time Out"), GUILayout.MaxWidth(750));
            EditorGUILayout.PropertyField(verifyUrlProperty, new GUIContent("Verify URL"), GUILayout.MaxWidth(750));
            EditorGUI.EndDisabledGroup();
            GUILayout.EndVertical();
            
            serializedIapService.ApplyModifiedProperties();
            GUILayout.Space(5);
            GUILayout.EndVertical();
            EditorUtility.SetDirty(sonatIap);
        }

        private void AddItem()
        {
            var newProduct = new StoreProductDescriptor();
            sonatIap.StoreProductDescriptors.Add(newProduct);

            var drawItem = new StoreProductDraw(newProduct, this);
            storeProductDraws.Add(drawItem);
        }

        public void RemoveProduct(StoreProductDescriptor product)
        {
            int index = sonatIap.StoreProductDescriptors.IndexOf(product);
            if (index < 0) return;
            sonatIap.StoreProductDescriptors.Remove(product);
            storeProductDraws.RemoveAt(index);
        }


        private string iapVersionInstalled = "";
        private string iapVersionToInstall = "";
        private bool iapInstalled = false;
        private bool hasIapSymbol;
        static AddRequest Request;

        private void CheckIapInstalled()
        {
            if (string.IsNullOrEmpty(iapVersionInstalled))
            {
                //admobVersionInstalled = SonatEditorHelper.CheckVersionInstalled("package.json", "AppsFlyer", @"""version"": ""(?<ver>.+)""");
                iapVersionInstalled =
                    SonatEditorHelper.CheckVersionInstalledByManifest("com.unity.purchasing");
            }

            iapInstalled = !string.IsNullOrEmpty(iapVersionInstalled);
            hasIapSymbol = SonatEditorHelper.HasSymbol("using_iap", EditorUserBuildSettings.selectedBuildTargetGroup);
            if (!iapInstalled && hasIapSymbol)
            {
                hasIapSymbol = false;
                SonatEditorHelper.RemoveSymbolFromBuildTarget("using_iap");
            }
        }

        private void IapInstallation()
        {
            GUILayout.BeginVertical(new GUIStyle(GUI.skin.box));
            GUILayout.Label($"Unity Purchasing", EditorStyles.boldLabel);
            GUILayout.Space(5);
            if (iapInstalled)
                GUILayout.Label($"Unity Purchasing Installed Version: {iapVersionInstalled}", SonatSDKWindow.labelGreenStyle);
            else
            {
                EditorGUILayout.HelpBox("Unity Purchasing Not Installed Yet", MessageType.Error);
            }

            GUILayout.Space(5);
            GUILayout.BeginHorizontal();
            //GUILayout.Label("Install");
            //iapVersionToInstall = EditorGUILayout.TextField("Version", iapVersionToInstall, GUILayout.ExpandWidth(true));
            // previewAfterDownloadMax = EditorGUILayout.Toggle("Preview", previewAfterDownloadMax);
            string installLabel = "Install";
            if (iapInstalled && iapVersionToInstall != iapVersionInstalled)
            {
                installLabel = "Upgrade";
            }
            else if (string.IsNullOrEmpty(iapVersionToInstall))
            {
                installLabel = "Install (Latest)";
            }

            if (GUILayout.Button(installLabel, GUILayout.Width(120)))
            {
                Request = Client.Add("com.unity.purchasing");
                EditorApplication.update += Progress;
            }

            EditorGUI.BeginDisabledGroup(!iapInstalled || hasIapSymbol);
            string symbolLabel = hasIapSymbol ? "Symbol Added" : "Add Symbol";
            if (GUILayout.Button(symbolLabel, GUILayout.Width(100)))
            {
                if (hasIapSymbol)
                {
                    sonatSDKWindow.ShowNotification(new GUIContent("Iap Symbol Has Exist!"), 0.5f);
                }
                else
                {
                    SonatEditorHelper.AddSymbol(new[] { "using_iap" }, EditorUserBuildSettings.selectedBuildTargetGroup);
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