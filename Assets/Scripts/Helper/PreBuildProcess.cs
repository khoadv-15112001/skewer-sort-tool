#if UNITY_EDITOR
using System.IO;
using GrillSort.OnlineService;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Content;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace Helper
{
    public class PreBuildProcess : IPreprocessBuildWithReport
    {
        public int callbackOrder
        {
            get { return 0; }
        }

        public void OnPreprocessBuild(BuildReport report)
        {
            if (report.summary.platform == BuildTarget.iOS || EditorUserBuildSettings.buildAppBundle)
            {
                string filePath = FindFilePath("OnlineServiceConfig.asset", "Online", false);
                if (string.IsNullOrEmpty(filePath)) return;

                var onlineServiceConfig = AssetDatabase.LoadAssetAtPath(filePath, typeof(OnlineServiceConfig)) as OnlineServiceConfig;
                if (onlineServiceConfig == null) return;
                Debug.Log("Online service has been loaded");
                onlineServiceConfig.testService = false;
                onlineServiceConfig.enableLog = false;
            }
        }
        
        public string FindFilePath(string fileName, string folder, bool full = true)
        {
            var paths = Directory.GetFiles(Application.dataPath, fileName, SearchOption.AllDirectories);
            for (int i = 0; i < paths.Length; i++)
            {
                if (paths[i].Contains(folder) || string.IsNullOrEmpty(folder))
                {
                    if (full)
                        return paths[i];
                    else
                    {
                        return paths[i].Replace(Application.dataPath, "Assets");
                    }
                }
            }

            return null;
        }
    }
    
    
}
#endif