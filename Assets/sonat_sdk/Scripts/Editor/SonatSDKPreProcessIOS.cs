#if UNITY_IOS
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace Sonat
{
    public class SonatSDKPreProcessIOS : IPreprocessBuildWithReport
    {
        public void OnPreprocessBuild(BuildReport report)
        {
            CheckIosAppId();
        }

        private static void CheckIosAppId()
        {
            if (string.IsNullOrEmpty(SonatSdkManager.Settings.appID_IOS))
            {
                throw new BuildFailedException("iOS App ID is missing!!!");
            }
            else
            {
                Debug.Log("iOS App ID is " + SonatSdkManager.Settings.appID_IOS);
            }
        }

        public int callbackOrder => 1;
    }
}
#endif