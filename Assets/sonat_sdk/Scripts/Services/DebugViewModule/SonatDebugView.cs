using System;
using System.Text;
using Sonat.FirebaseModule;
using Sonat.FirebaseModule.Analytic;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Sonat.DebugViewModule
{
    public class SonatDebugView
    {
        private static DebugLogClient client;
        private static bool active = false;
        public static string ip;
        public static UInt16 port;

        private static OnScreenDebugLog onScreenDebugLog;

        public static void ShowSdkInfo()
        {
            SonatSdkInspector.GetInstance().ShowInfo();
        }

        public static void StartTransportDebugView(string _ip, UInt16 _port = 8282)
        {
            ip = _ip;
            port = _port;

            if (client != null)
            {
                Object.Destroy(client.gameObject);
            }
            
            GameObject clientObj = new GameObject("DebugLogClient");
            client = clientObj.AddComponent<DebugLogClient>();
            Object.DontDestroyOnLoad(clientObj);
            active = true;
            SonatFirebase.analytic.OnLogEvent -= OnLogEventTransport;
            SonatFirebase.analytic.OnLogEvent += OnLogEventTransport;
        }


        public static void StopTransportDebugView()
        {
            SonatFirebase.analytic.OnLogEvent -= OnLogEventTransport;
            if (client != null)
            {
                Object.Destroy(client.gameObject);
            }
        }


        private static void OnLogEventTransport(EventLogData data)
        {
            client.SendData(data);
        }


        public static void OpenDebugLogScreen()
        {
            if (OnScreenDebugLog.Instance == null)
            {
                if (onScreenDebugLog == null)
                {
                    var newGo = new GameObject("ScreenDebugLog");
                    onScreenDebugLog = newGo.AddComponent<OnScreenDebugLog>();
                    Object.DontDestroyOnLoad(newGo);
                }

                onScreenDebugLog.IsShow = true;
                onScreenDebugLog.gameObject.SetActive(true);
            }
            else
            {
                OnScreenDebugLog.Instance.gameObject.SetActive(true);
                OnScreenDebugLog.Instance.IsShow = true;
            }

            SonatFirebase.analytic.OnLogEvent += OnLogEventScreen;
        }

        public static void CloseDebugLogScreen()
        {
            if (OnScreenDebugLog.Instance != null)
            {
                OnScreenDebugLog.Instance.gameObject.SetActive(false);
                OnScreenDebugLog.Instance.IsShow = false;
            }

            SonatFirebase.analytic.OnLogEvent -= OnLogEventScreen;
        }

        private static void OnLogEventScreen(EventLogData data)
        {
            var s = new StringBuilder("LogEvent: ");
            s.Append(data.key + "\t ");
            if (data.parameters != null && data.parameters.Count > 0)
            {
                foreach (var parameter in data.parameters)
                {
                    s.Append($" - [{parameter.Key}: {parameter.Value}]");
                }
            }

            OnScreenDebugLog.Log(s.ToString());
        }
    }
}