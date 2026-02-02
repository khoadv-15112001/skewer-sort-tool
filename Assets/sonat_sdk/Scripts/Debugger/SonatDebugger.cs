using UnityEngine;

namespace Sonat.Debugger
{
    public static class SonatDebugger
    {
        public static void Log(this SonatDebugType debugType, object message, string color = "white")
        {
#if !ignore_log && !ignore_log_messages
            if (!CheckLogType(debugType)) return;
            Debug.Log($"<color={color}>[SONAT] {debugType}: {message}</color>");
#endif
        }

        public static void LogWarning(this SonatDebugType debugType, object message, string color = "yellow")
        {
#if !ignore_log && !ignore_log_warnings
            if (!CheckLogType(debugType)) return;
            Debug.LogWarning($"<color={color}>[SONAT] {debugType}: {message}</color>");
#endif
        }

        public static void LogError(this SonatDebugType debugType, object message, string color = "red")
        {
#if !ignore_log && !ignore_log_errors
            if (!CheckLogType(debugType)) return;
            Debug.LogError($"<color={color}>[SONAT] {debugType}: {message}</color>");
#endif
        }

        private static bool CheckLogType(SonatDebugType debugType)
        {
            return SonatSdkManager.Settings.logTypes.Contains(SonatDebugType.All) || SonatSdkManager.Settings.logTypes.Contains(debugType);
        }
    }

    public enum SonatDebugType
    {
        Common = 0,
        Ads,
        Iap,
        Tracking,
        RemoteConfig,
        Editor,
        Firebase,
        Development,
        Gameplay,
        Tool,
        All
    }
}