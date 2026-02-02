using Sonat.AdsModule;
using UnityEditor;
using UnityEngine;

namespace Sonat.Editor.PackageManager.Elements
{
    public class MediationSetupDraw
    {
        private SonatMediation mediation;
        private bool fold;
        private AdsConfigDraw androidConfigDraw;
        private AdsConfigDraw iosConfigDraw;
        private AdsConfigDraw testConfigDraw;

#if using_aps
        private AdsConfigDraw apsConfigDraw;
#endif

        public MediationSetupDraw(SonatMediation mediation)
        {
            this.mediation = mediation;
            androidConfigDraw = new AdsConfigDraw(this.mediation.androidConfig, " ANDROID");
            iosConfigDraw = new AdsConfigDraw(this.mediation.iosConfig, " IOS");
            testConfigDraw = new AdsConfigDraw(this.mediation.testConfig, " TEST");
#if using_aps
            apsConfigDraw = new AdsConfigDraw(this.mediation.amazonConfig, "AMAZON");
#endif
        }


        public void ShowMediationSetupTab()
        {
            GUILayout.BeginHorizontal();
            GUILayout.Space(10);
            GUILayout.BeginVertical();
            fold = EditorGUILayout.Foldout(fold, "Mediation Setup", true);

            if (fold)
            {
                androidConfigDraw.Draw();
                iosConfigDraw.Draw();
                testConfigDraw.Draw();
#if using_aps
                mediation.usingAps = EditorGUILayout.ToggleLeft("Using Amazon", mediation.usingAps);
                if (mediation.usingAps)
                {
                    apsConfigDraw.Draw();
                }
#endif
            }

            EditorGUILayout.EndFoldoutHeaderGroup();
            GUILayout.EndVertical();
            GUILayout.EndHorizontal();
            EditorUtility.SetDirty(mediation);
        }


        void ShowHeaderContextMenu(Rect position)
        {
            // var menu = new GenericMenu();
            // menu.AddItem(new GUIContent("Move to (0,0,0)"), false, OnItemClicked);
            // menu.DropDown(position);
        }

        void OnItemClicked()
        {
            // Undo.RecordObject(Selection.activeTransform, "Moving to center of world");
            // Selection.activeTransform.position = Vector3.zero;
        }
    }
}