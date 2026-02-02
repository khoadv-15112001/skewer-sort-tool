#if using_firebase_analytics
#endif

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
#if using_firebase_analytics
using Firebase.Analytics;
#endif
#if using_firebase_crashlytics
using Firebase.Crashlytics;
#endif
using Sonat.Data;
using Sonat.Debugger;
using Sonat.TrackingModule;
using UnityEngine;

namespace Sonat.FirebaseModule.Analytic
{
    public class SonatFirebaseAnalytic
    {
        public bool DependencyStatusAvailable;
        public event Action<EventLogData> OnLogEvent;

        public void SetUserProperty(string property, string value)
        {
#if using_firebase || using_firebase_analytics
            if (!DependencyStatusAvailable)
            {
                SonatDebugType.Tracking.LogError("Firebase Not ready to buyLog event " + Time.time);
                return;
            }

            FirebaseAnalytics.SetUserProperty(property, value);
#endif
        }


        public void Initialize()
        {
#if using_firebase || using_firebase_analytics
            FirebaseAnalytics.SetAnalyticsCollectionEnabled(true);
            SonatAnalyticTracker.FirebaseReady = true;
            LogEvent(EventNameEnum.app_loading);
            if (PlayerPrefs.GetInt(FireBaseDefaultEvent.my_first_open.ToString()) == 0)
            {
                LogEvent(FireBaseDefaultEvent.my_first_open.ToString());
                PlayerPrefs.SetInt(FireBaseDefaultEvent.my_first_open.ToString(), 1);
            }
#endif
        }

        private void LogEventName(string eventName)
        {
#if using_firebase || using_firebase_analytics
            if (!DependencyStatusAvailable)
            {
                SonatDebugType.Tracking.LogError("Firebase Not ready to buyLog event " + Time.time);
                return;
            }

            FirebaseAnalytics.LogEvent(eventName);
            OnLogEvent?.Invoke(new EventLogData(eventName));
#endif
        }

        public void LogEvent(string eventName, string paramName, string paramValue)
        {
#if using_firebase || using_firebase_analytics
            if (!DependencyStatusAvailable)
            {
                SonatDebugType.Tracking.LogError("Firebase Not ready to buyLog event " + Time.time);

                return;
            }

            FirebaseAnalytics.LogEvent(eventName, new Parameter(paramName, paramValue));
            OnLogEvent?.Invoke(new EventLogData(eventName, paramName, paramValue));
#endif
        }

#if using_firebase || using_firebase_analytics
        public void LogEvent(string eventName, params Parameter[] parameters)
        {
            if (!DependencyStatusAvailable)
            {
                SonatDebugType.Tracking.LogError("Firebase Not ready to buyLog event " + Time.time);
                return;
            }

            FirebaseAnalytics.LogEvent(eventName, parameters);
        }

        public void LogEvent(string eventName, Parameter parameter)
        {
            if (!DependencyStatusAvailable)
            {
                SonatDebugType.Tracking.LogError("Firebase Not ready to buyLog event " + Time.time);
                return;
            }

            FirebaseAnalytics.LogEvent(eventName, parameter);
        }

#endif

        public void LogEvent(string eventName, string paramName, int paramValue)
        {
#if using_firebase || using_firebase_analytics
            if (!DependencyStatusAvailable)
            {
                SonatDebugType.Tracking.LogError("Firebase Not ready to buyLog event " + Time.time);
                return;
            }

            FirebaseAnalytics.LogEvent(eventName, new Parameter(paramName, paramValue));
            OnLogEvent?.Invoke(new EventLogData(eventName, paramName, paramValue.ToString(CultureInfo.InvariantCulture)));
#endif
        }


        public void LogEvent(string eventName, string paramName, float paramValue)
        {
#if using_firebase || using_firebase_analytics
            if (!DependencyStatusAvailable)
            {
                SonatDebugType.Tracking.LogError("Firebase Not ready to buyLog event " + Time.time);
                return;
            }

            FirebaseAnalytics.LogEvent(eventName, new Parameter(paramName, paramValue));
            OnLogEvent?.Invoke(new EventLogData(eventName, paramName, paramValue.ToString(CultureInfo.InvariantCulture)));
#endif
        }


        public void LogEvent(EventNameEnum eventName, LogParameter[] parameters)
        {
#if using_firebase || using_firebase_analytics
            if (!DependencyStatusAvailable)
            {
                SonatDebugType.Tracking.LogError("Firebase Not ready to buyLog event " + Time.time);
                return;
            }

            Parameter[] fbParameters = new Parameter[parameters.Length];
            for (int i = 0; i < parameters.Length; i++)
                fbParameters[i] = parameters[i].Param;
            LogEvent(eventName.ToString(), fbParameters);
            OnLogEvent?.Invoke(new EventLogData(eventName.ToString(), parameters));
#endif
        }

        public void SetScreenView(string screenName, string screenClass)
        {
#if using_firebase || using_firebase_analytics
            if (!DependencyStatusAvailable)
            {
                SonatDebugType.Tracking.LogError("Firebase Not ready to buyLog event " + Time.time);
                return;
            }

            FirebaseAnalytics.LogEvent(
                FirebaseAnalytics.EventScreenView,
                new Parameter(FirebaseAnalytics.ParameterScreenName, screenName),
                new Parameter(FirebaseAnalytics.ParameterScreenClass, screenClass));
            OnLogEvent?.Invoke(new EventLogData(FirebaseAnalytics.EventScreenView, new LogParameter(FirebaseAnalytics.ParameterScreenName, screenName),
                new LogParameter(FirebaseAnalytics.ParameterScreenClass, screenClass)));
#endif
        }

        public void SetUserLevel(int level, string mode = "classic")
        {
            UserData.SetLevel(level, mode);
            UserData.SetMode(mode);

#if using_firebase || using_firebase_analytics
            if (!DependencyStatusAvailable)
            {
                SonatDebugType.Tracking.LogError("Firebase Not ready to buyLog event " + Time.time);
                return;
            }

            FirebaseAnalytics.SetUserProperty("level", level.ToString());
#endif
        }


        public void LogEvent(string eventName, LogParameter logParameter)
        {
#if using_firebase || using_firebase_analytics
            if (!DependencyStatusAvailable)
            {
                SonatDebugType.Tracking.LogError("Firebase Not ready to buyLog event " + Time.time);
                return;
            }

            LogEvent(eventName, logParameter.Param);
            OnLogEvent?.Invoke(new EventLogData(eventName, logParameter));
#endif
        }

        public void LogEvent<T>(string eventName, IList<T> parameters) where T : LogParameter
        {
#if using_firebase || using_firebase_analytics
            if (!DependencyStatusAvailable)
            {
                SonatDebugType.Tracking.LogError("Firebase Not ready to buyLog event " + Time.time);
                return;
            }

            LogEvent(eventName, parameters.Select(x => x.Param).ToArray());
            OnLogEvent?.Invoke(new EventLogData(eventName, parameters.ToArray<LogParameter>()));
#endif
        }


        public void LogEvent(string eventName)
        {
#if using_firebase || using_firebase_analytics
            if (!DependencyStatusAvailable)
            {
                SonatDebugType.Tracking.LogError("Firebase Not ready to buyLog event " + Time.time);
                return;
            }

            LogEventName(eventName);
#endif
        }

        public void LogEvent(EventNameEnum eventName)
        {
#if using_firebase || using_firebase_analytics
            if (!DependencyStatusAvailable)
            {
                SonatDebugType.Tracking.LogError("Firebase Not ready to buyLog event " + Time.time);
                return;
            }

            LogEvent(eventName.ToString());
#endif
        }

        public void LogCrash(string mess)
        {
#if using_firebase || using_firebase_crashlytics
            Crashlytics.Log(mess);
#endif
        }

        public void LogCrashException(Exception mess)
        {
#if using_firebase || using_firebase_crashlytics
            Crashlytics.LogException(mess);
#endif
        }
    }

    public class EventLogData
    {
        public string key;
        public Dictionary<string, string> parameters;
        public string tag = "firebase";

        public EventLogData()
        {
        }

        public EventLogData(string key)
        {
            this.key = key;
        }

        public EventLogData(string key, string paramKey, string paramValue)
        {
            this.key = key;
            parameters = new Dictionary<string, string>();
            parameters.Add(paramKey, paramValue);
        }

        public EventLogData(string eventName, params LogParameter[] _parameters)
        {
            key = eventName;
            if (_parameters != null)
            {
                parameters = new Dictionary<string, string>();
                for (int i = 0; i < _parameters.Length; i++)
                {
                    parameters.Add(_parameters[i].stringKey, _parameters[i].GetValueAsString());
                }
            }
        }
    }
}