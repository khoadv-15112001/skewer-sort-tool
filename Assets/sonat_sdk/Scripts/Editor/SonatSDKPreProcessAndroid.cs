#if UNITY_ANDROID
using UnityEditor;
using UnityEditor.Build;
using UnityEngine;
using UnityEditor.Build.Reporting;
public class SonatSDKPreProcessAndroid : IPreprocessBuildWithReport
{
    public int callbackOrder => 1;

    public void OnPreprocessBuild(BuildReport report)
    {
        ValidateAutomaticScreenReporting();
    }

    private void ValidateAutomaticScreenReporting()
    {
        var asset = AssetDatabase.LoadAssetAtPath<TextAsset>("Assets/Plugins/Android/AndroidManifest.xml");
        if (asset != null)
        {
            var disable_automatic_reporting =
                asset.text.Contains("google_analytics_automatic_screen_reporting_enabled");

            if (!disable_automatic_reporting)
            {
                Debug.LogError("SONAT ERROR : add google_analytics_automatic_screen_reporting_enabled to AndroidManifest");
                Debug.LogError("<meta-data android:name=\"google_analytics_automatic_screen_reporting_enabled\" android:value=\"false\" />");
                throw new BuildFailedException("SONAT : Build was canceled because not disable_automatic_reporting.");
            }
            else
            {
                Debug.Log("good");
            }
        }
    }
}
#endif