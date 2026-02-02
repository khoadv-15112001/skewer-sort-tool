using Sonat;
using UnityEngine;

namespace GrillSort.OnlineService
{
    [CreateAssetMenu(fileName = "OnlineServiceConfig", menuName = "Sonat Configs Custom/OnlineServiceConfig")]
    public class OnlineServiceConfig : ScriptableObject
    {
        public bool autoSignIn = true;
        public string serverProductionUrl = "https://services.sonatgame.com";
        public string serverDevUrl = "https://services-dev.sonatgame.com";
        public string emailDomain = "@grillsort.com";
        public int timeOut = 30;

        [Header("Test service - Set false when publish")]
        [Tooltip("⚠️ WARNING: Must be FALSE for production builds! Set to TRUE only for development/testing.")]
        public bool testService = true;
        public bool enableLog = true;

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (testService)
            {
                UnityEngine.Debug.LogWarning($"[OnlineServiceConfig] ⚠️ testService is ENABLED in '{name}'! Remember to set it to FALSE before publishing!", this);
            }
        }
#endif

        public string AppID()
        {
#if UNITY_IOS
            return "id" + SonatSdkManager.Settings.appID_IOS;
#else
            return Application.identifier;
#endif
        }
    }
}