using System;
using System.Collections.Generic;
#if using_appsflyer
using AppsFlyerSDK;
#endif
using Sonat.FirebaseModule;
using UnityEngine;
#if using_appsflyer
#endif

namespace Sonat.TrackingModule
{
    public class CustomSonatLog : SonatLogBase
    {
        public override string EventName => _name;

        protected override List<LogParameter> GetParameters() => _logs;

        private readonly List<LogParameter> _logs;
        private readonly string _name;

        public CustomSonatLog(string name, List<LogParameter> logs)
        {
            _name = name;
            _logs = logs;
        }

        public CustomSonatLog(sonat_log_enum log, List<LogParameter> logs)
        {
            _name = log.ToString();
            _logs = logs;
        }
    }

    public abstract class SonatLogBase
    {
        protected abstract List<LogParameter> GetParameters();
        public abstract string EventName { get; }

        public static Action<string, List<LogParameter>> OnLog;

        public bool PostAf { get; set; }
        private List<LogParameter> _extra;


        public SonatLogBase AddExtraParameter(string name, string value)
        {
            if(_extra == null) _extra = new List<LogParameter>();
            _extra.Add(new LogParameter(name, value));
            return this;
        }
        
        public SonatLogBase AddExtraParameter(LogParameter parameter)
        {
            if(_extra == null) _extra = new List<LogParameter>();
            _extra.Add(parameter);
            return this;
        }

        public SonatLogBase SetExtraParameter(List<LogParameter> extra)
        {
            _extra = extra;
            return this;
        }
        

        public virtual void Post(bool logAf = false)
        {
            if (SonatAnalyticTracker.FirebaseReady)
            {
                var listParameters = GetParameters();

                if (listParameters == null)
                    listParameters = new List<LogParameter>();
                listParameters.Add(new LogParameter(nameof(network_connect_type), GetConnectionType().ToString()));
                if (_extra != null)
                {
                    listParameters.AddRange(_extra);
// #if !UNITY_STANDALONE_WIN && !UNITY_EDITOR && use_firebase_analytics
//                     FirebaseAnalytics.LogEvent(EventName, listParameters.Select(x => x.Param).ToArray());
// #endif
                }
// #if !UNITY_STANDALONE_WIN && !UNITY_EDITOR && use_firebase_analytics
//                 else
//                     FirebaseAnalytics.LogEvent(EventName, listParameters.Select(x => x.Param).ToArray());
// #endif
                SonatFirebase.analytic.LogEvent(EventName, listParameters);

                if (logAf)
                    LogAf(listParameters);

                OnLog?.Invoke(EventName, listParameters);
            }
            else
            {
                OnLog?.Invoke("firebase_not_ready", new List<LogParameter>()
                {
                    new LogParameter("message", "Firebase not ready : SonatAnalyticTracker.FirebaseReady, push to queued")
                });
                SonatTrackingHelper.NotReadyQueues.Add(this);
            }
        }

        protected virtual void LogAf(List<LogParameter> listParameters)
        {
            var dict = new Dictionary<string, string>();
            foreach (var parameter in listParameters)
            {
                if (!dict.ContainsKey(parameter.stringKey))
                    dict.Add(parameter.stringKey, parameter.GetValueAsString());
            }
#if using_appsflyer
            AppsFlyer.sendEvent(EventName, dict);
#endif
        }

        private network_connect_type GetConnectionType()
        {
            switch (Application.internetReachability)
            {
                case NetworkReachability.NotReachable:
                    return network_connect_type.none;
                    break;
                case NetworkReachability.ReachableViaCarrierDataNetwork:
                    return network_connect_type.mobile;
                    break;
                case NetworkReachability.ReachableViaLocalAreaNetwork:
                    return network_connect_type.wifi;
                    break;
                default:
                    return network_connect_type.other;
            }
        }
    }

    public abstract class SonatLogAppflyerBase
    {
        protected abstract Dictionary<string, string> GetParameters();
        public abstract string EventName { get; }

        public void Post()
        {
#if using_appsflyer
            AppsFlyer.sendEvent(EventName, GetParameters());
#endif
        }
    }
}