using System;
using System.Collections;
using Sonat.Debugger;
using Sonat.FirebaseModule;
using UnityEngine;

namespace Sonat
{
    public class SonatSdkManager : MonoBehaviour
    {
        public static SonatSdkManager instance;
        public static bool ready;
        private static SonatSdkSettings settings;
        private static SonatSdkServices sonatServices;

        public static SonatSdkSettings Settings => settings ??= Resources.Load<SonatSdkSettings>("SonatSdkSettings");
        public static SonatSdkServices SonatServices => sonatServices ??= Resources.Load<SonatSdkServices>("SonatSdkServices");
        public static event Action onInitializeComplete;
        public static Action onRemoteConfigReady;

        public static void Initialize(Action onComplete)
        {
            onInitializeComplete += onComplete;
            var gameObject = Instantiate(new GameObject("SonatSdkManager"), new Vector3(0, 0, 0), Quaternion.identity);
            DontDestroyOnLoad(gameObject);
            instance = gameObject.AddComponent<SonatSdkManager>();
            SonatServices.Initialize();
            instance.StartCoroutine(Loading());
            instance.StartCoroutine(WaitForRemoteConfig());
        }

        private static void OnInitializeComplete()
        {
            ready = true;
            SonatDebugType.Common.Log($"Sonat SDK initialize complete");
            onInitializeComplete?.Invoke();
            onInitializeComplete = null;
            OnRemoteConfigReady();
        }

        private static void OnRemoteConfigReady()
        {
            onRemoteConfigReady?.Invoke();
            onRemoteConfigReady = null;
        }

        private static IEnumerator WaitForRemoteConfig()
        {
            while (!SonatFirebase.FirebaseRemoteReady)
            {
                yield return null;
            }
            OnRemoteConfigReady();
        }

        private static IEnumerator Loading()
        {
            float timeLoading = 0;
            while (timeLoading < Settings.timeout && !SonatServices.allServicesInitialized)
            {
                timeLoading += Time.deltaTime;
                yield return null;
            }

            OnInitializeComplete();
        }

        public static bool IsInternetConnection()
        {
#if UNITY_EDITOR
            if(settings != null) return settings.internetConnection;
#endif
            return Application.internetReachability != NetworkReachability.NotReachable;
        }


        private void OnApplicationFocus(bool hasFocus)
        {
            sonatServices.OnApplicationFocus(hasFocus);
        }

        private void OnApplicationPause(bool pauseStatus)
        {
            sonatServices.OnApplicationPause(pauseStatus);
        }

        private void OnApplicationQuit()
        {
            sonatServices.OnApplicationQuit();
        }

        public static void DelayCall(float seconds, Action callback)
        {
            if (seconds <= 0)
            {
                callback?.Invoke();
                return;
            }

            instance.StartDelayCall(seconds, callback);
        }

        private void StartDelayCall(float seconds, Action callback)
        {
            StartCoroutine(IEDelayCall(seconds, callback));
        }

        private IEnumerator IEDelayCall(float seconds, Action callback)
        {
            yield return new WaitForSeconds(seconds);
            callback?.Invoke();
        }
    }
}