using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace GrillSort.OnlineService.Editor
{
    /// <summary>
    /// Validates OnlineService configuration before building to prevent publishing with test service enabled
    /// </summary>
    public class OnlineServiceBuildValidator : IPreprocessBuildWithReport
    {
        public int callbackOrder => 0;

        public void OnPreprocessBuild(BuildReport report)
        {
            // Find all OnlineServiceConfig assets
            string[] guids = AssetDatabase.FindAssets("t:OnlineServiceConfig");
            
            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                OnlineServiceConfig config = AssetDatabase.LoadAssetAtPath<OnlineServiceConfig>(path);
                
                if (config != null && config.testService)
                {
                    string errorMessage = $"[BUILD ERROR] OnlineServiceConfig '{config.name}' has testService enabled!\n" +
                                        $"Location: {path}\n" +
                                        $"Please set testService = false before publishing.\n" +
                                        $"This prevents accidentally using test/dev server in production builds.";
                    
                    Debug.LogError(errorMessage, config);
                    
                    // Show dialog to user
                    bool continueAnyway = EditorUtility.DisplayDialog(
                        "Build Validation Failed",
                        $"OnlineServiceConfig '{config.name}' has testService = true!\n\n" +
                        "Building with test service enabled is not recommended for production.\n\n" +
                        "Do you want to continue anyway? (NOT RECOMMENDED)",
                        "Cancel Build",
                        "Continue Anyway"
                    );
                    
                    if (continueAnyway)
                    {
                        // User chose to continue anyway (clicked "Continue Anyway")
                        Debug.LogWarning($"[BUILD WARNING] User chose to continue building with testService enabled in '{config.name}'");
                    }
                    else
                    {
                        // User chose to cancel (clicked "Cancel Build")
                        throw new BuildFailedException($"Build cancelled: testService is enabled in OnlineServiceConfig '{config.name}' at {path}");
                    }
                }
            }
        }
    }
}

