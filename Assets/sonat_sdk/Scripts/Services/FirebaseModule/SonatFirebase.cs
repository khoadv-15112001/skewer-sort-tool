#if using_firebase_installation
#endif
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
#if using_firebase_analytics
using Firebase;
using Firebase.Analytics;
using Firebase.Extensions;
#endif
#if using_firebase_crashlytics
using Firebase.Crashlytics;
#endif
#if using_firebase_installation
using Firebase.Installations;
#endif
using Sonat.Data;
using Sonat.Debugger;
using Sonat.FirebaseModule.Analytic;
using Sonat.FirebaseModule.Message;
using Sonat.TrackingModule;
using UnityEngine;

namespace Sonat.FirebaseModule
{
    [CreateAssetMenu(menuName = "SonatSDK/Services/Firebase Service", fileName = nameof(SonatFirebase))]
    public class SonatFirebase : SonatService
    {
        private static SonatFirebase instance;
        public override SonatServiceType ServiceType => SonatServiceType.FirebaseService;
        public override bool Ready { get; set; }

        public SonatFirebaseConfig config;

        public static readonly SonatFirebaseRemoteConfig remote = new();
        public static readonly SonatFirebaseMessage message = new();
        public static readonly SonatFirebaseAnalytic analytic = new();
#if using_firebase_installation
        private static FirebaseInstallations installations;
#endif

        public static bool FirebaseRemoteReady { get; set; }


        public override void Initialize(Action<ISonatService> onInitialized)
        {
            FirebaseRemoteReady = false;
            instance = this;
            base.Initialize(onInitialized);
            Register();
        }


        private void Register()
        {
            StartCoroutine(WaitRemoteConfig());
#if using_firebase_analytics
            FirebaseApp.CheckAndFixDependenciesAsync().ContinueWithOnMainThread<DependencyStatus>(task =>
            {
                dependencyStatus = task.Result;

                SonatDebugType.Common.Log("CheckAndFixDependenciesAsync " + dependencyStatus);
                if (dependencyStatus == DependencyStatus.Available)
                {
                    analytic.DependencyStatusAvailable = true;
                    InitializeFirebase();
                    SonatSdkManager.instance.StartCoroutine(WaitToLog());
                }
                else
                {
                    analytic.DependencyStatusAvailable = false;
                    SonatDebugType.Common.LogError("Could not resolve all Fire base dependencies: " + dependencyStatus);
                    SonatSdkManager.instance.StartCoroutine(WaitToLog());
                }
            });

#endif
        }


        protected virtual void InitializeFirebase()
        {
            
            analytic.Initialize();
            remote.Initialize(this);
#if using_firebase_installation
            installations = FirebaseInstallations.DefaultInstance;
#endif
#if using_firebase_analytics
            GetIdAsync();
            GetAnalyticsInstanceIdAsync();
#endif
            message.Initialize();
           
        }

        IEnumerator WaitRemoteConfig()
        {
            float t = 0;
            while (!FirebaseRemoteReady && t < 5)
            {
                t += Time.deltaTime;
                yield return null;
            }

            yield return new WaitForSeconds(0.1f);
            Initialized();
        }

        private void Initialized()
        {
            Ready = true;
            OnInitialized?.Invoke(this);
        }

        public static void SetCustomUserId(string customUserId)
        {
#if using_firebase || using_firebase_analytics
            FirebaseAnalytics.SetUserId(customUserId);
#endif
#if using_firebase || using_firebase_crashlytics
            Crashlytics.SetUserId(customUserId);
#endif
        }


#if using_firebase_analytics
        public static DependencyStatus dependencyStatus = DependencyStatus.UnavailableOther;

        private Firebase.FirebaseApp App => FirebaseApp.DefaultInstance;


        protected Task GetIdAsync()
        {
#if using_firebase_installation
            return installations.GetIdAsync().ContinueWithOnMainThread<string>(task =>
            {
                if (LogTaskCompletion(task, "GetIdAsync"))
                {
                    UserData.FirebaseInstanceId.Value = task.Result;
                    analytic.SetUserProperty("fb_instance_id", task.Result);
                    SonatDebugType.Common.Log($"Installations {task.Result}");
                }
            });
#else
            return null;
#endif
        }

        protected Task GetAnalyticsInstanceIdAsync()
        {
#if using_firebase_analytics
            return FirebaseAnalytics.GetAnalyticsInstanceIdAsync().ContinueWithOnMainThread<string>(task =>
            {
                if (LogTaskCompletion(task, "GetAnalyticsInstanceIdAsync"))
                {
                    UserData.AnalyticsInstanceId.Value = task.Result;
                    SonatDebugType.Common.Log($"GetAnalyticsInstanceIdAsync {task.Result}");
                }
            });
#endif
        }


        private bool LogTaskCompletion(Task task, string operation)
        {
            bool complete = false;
            if (task.IsCanceled)
            {
                SonatDebugType.Common.Log(operation + " canceled.");
            }
            else if (task.IsFaulted)
            {
                SonatDebugType.Common.Log(operation + " encounted an error.");
                foreach (Exception exception in task.Exception.Flatten().InnerExceptions)
                {
                    string errorCode = "";
#if using_firebase_analytics
                    if (exception is FirebaseException firebaseException)
                    {
                        errorCode = $"Error code={firebaseException.ErrorCode}: {firebaseException.Message}";
                    }
#endif
                    SonatDebugType.Common.Log(errorCode + exception.ToString());
                }
            }
            else if (task.IsCompleted)
            {
                SonatDebugType.Common.Log(operation + " completed");
                complete = true;
            }

            return complete;
        }
#endif


        public IEnumerable<LogParameter> GetAdmobParameter(int step, IEnumerable<LogParameter> input, AdTypeLog adType)
        {
            return input;
        }

        private static IEnumerator WaitToLog()
        {
            for (int i = 0; i < 5; i++)
            {
                yield return new WaitForSeconds(5);
                bool done = SonatTrackingHelper.TryToPostQueues();
                if (done)
                    break;
            }
        }


        public static SonatFirebaseConfig GetConfig()
        {
            if (instance == null) return null;
            return instance.config;
        }

        public override void OnApplicationPause(bool pause)
        {
            base.OnApplicationPause(pause);
            remote.OnApplicationPause(pause);
        }
    }
}