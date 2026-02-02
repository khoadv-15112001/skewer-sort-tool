using Sonat.FirebaseModule.Analytic;
using UnityEditor;
using UnityEngine;

namespace Sonat.Editor.PackageManager.Elements
{
    public class AnalyticWindowDraw
    {
        private SonatFirebaseWindow firebaseWindow;
        private SonatFirebaseConfig firebaseConfig;
        
        private bool editEnabled;

        public AnalyticWindowDraw(SonatFirebaseWindow firebaseWindow)
        {
            this.firebaseWindow = firebaseWindow;
            this.firebaseConfig = firebaseWindow.sonatFirebaseConfig;
        }

        public void Draw()
        {
            GUILayout.BeginVertical();
            GUILayout.BeginHorizontal();
            GUILayout.Label("SDK Default Tracking");
            GUILayout.FlexibleSpace();
            EditorGUIUtility.labelWidth = 30;
            editEnabled = EditorGUILayout.Toggle("Edit", editEnabled);
            GUILayout.EndHorizontal();
            GUILayout.Space(10);
            EditorGUI.BeginDisabledGroup(!editEnabled);
            EditorGUIUtility.labelWidth = 150;
            firebaseConfig.completeLevelsLogs = EditorGUILayout.TextField("Complete Levels Logs", firebaseConfig.completeLevelsLogs);
            firebaseConfig.completeRewardAdsLogs = EditorGUILayout.TextField("Complete Rewarded Logs", firebaseConfig.completeRewardAdsLogs);
            firebaseConfig.paidAdImpressionLogs = EditorGUILayout.TextField("Paid Ad impression Logs", firebaseConfig.paidAdImpressionLogs);
            firebaseConfig.levelsLogAfIaaIap = EditorGUILayout.TextField("Levels Log Iaa Iap", firebaseConfig.levelsLogAfIaaIap);
            
            EditorGUI.EndDisabledGroup();
            GUILayout.EndVertical();
        }
        
    }
}
