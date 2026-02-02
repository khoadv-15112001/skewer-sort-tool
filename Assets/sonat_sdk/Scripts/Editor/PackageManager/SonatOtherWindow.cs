using Sonat.Editor.PackageManager.Elements;
using UnityEditor;
using UnityEngine;

namespace Sonat.Editor.PackageManager
{
    public class SonatOtherWindow
    {
        private bool enableEditStoreItemKey;
        private Vector2 scrollPos;

        GUIContent[] myContent;

        private SonatSDKWindow sonatSDKWindow;
        private FacebookPanelDraw facebookPanel;
        private AppsFlyerPanelDraw appsFlyerPanel;

        public SonatOtherWindow(SonatSDKWindow sonatSDKWindow)
        {
            this.sonatSDKWindow = sonatSDKWindow;
        }
        
        public void Init()
        {
            myContent = new GUIContent[]
            {
                new GUIContent("AppsFlyer"),
                new GUIContent("Facebook")
            };
            
            appsFlyerPanel = new AppsFlyerPanelDraw();
            facebookPanel = new FacebookPanelDraw();
            
            appsFlyerPanel.Init();
            facebookPanel.Init();
            
        }

        public void Draw()
        {
            var tabStyle = EditorStyles.toolbarButton;
            tabStyle.alignment = TextAnchor.MiddleLeft;
            var selectedTabStyle = new GUIStyle(tabStyle);
            selectedTabStyle.normal.background = selectedTabStyle.active.background;
            
            GUILayout.BeginHorizontal();
            GUILayout.BeginVertical(GUILayout.Width(150), GUILayout.ExpandHeight(true));
            
            for (int i = 0; i < myContent.Length; i++)
            {
                GUIStyle style = sonatSDKWindow.otherTab == i ? selectedTabStyle : tabStyle;
                if (GUILayout.Toggle(sonatSDKWindow.otherTab == i, myContent[i], style))
                    sonatSDKWindow.otherTab = i;
            }
            GUILayout.Space(5);
            GUILayout.EndVertical();
            
            GUILayout.Box(Texture2D.blackTexture, GUILayout.Width(1.2f), GUILayout.ExpandHeight(true));
            
            GUILayout.BeginHorizontal();
            GUILayout.Space(10);
            GUILayout.BeginVertical();

            switch (sonatSDKWindow.otherTab)
            {
                case 0:
                    appsFlyerPanel.Draw();
                    break;
                case 1:
                    facebookPanel.Draw();
                    break;
            }

            GUILayout.EndVertical();
            GUILayout.EndHorizontal();
            GUILayout.EndHorizontal();
        }
        
    }
}
