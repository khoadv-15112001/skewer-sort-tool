using System;
using System.Collections.Generic;
using System.Threading.Tasks;
#if using_firebase_remote
using Firebase.Extensions;
using Firebase.RemoteConfig;
#endif
#if using_newtonsoft
using Newtonsoft.Json;
#endif
using Sonat.AppsFlyerModule;
using Sonat.Data;
using Sonat.Debugger;
using Sonat.FirebaseModule.RemoteConfig;
using UnityEngine;
#if using_newtonsoft
#endif

namespace Sonat.FirebaseModule
{
    public sealed class SonatFirebaseRemoteConfig
    {
        public static ConfigFetchStatus FetchStatus = ConfigFetchStatus.NotStarted;

#if using_firebase || using_firebase_remote
        protected static FirebaseRemoteConfig firebaseRemote => FirebaseRemoteConfig.DefaultInstance;
#endif

        private Dictionary<string, RemoteDataByLevel> remoteDataByLevels = new();
        private Dictionary<string, Dictionary<string, RemoteDataByLevel>> remoteDataByLevelsSegment = new();

        //[SerializeField] private DefaultRemoteDatabase defaultRemoteDatabase;

        //[ArrayElementTitleSonatSdk(nameof(RemoteConfigDefaultByKey.key))] [SerializeField]
        //public List<RemoteConfigDefaultByKey> defaultConfigs = new List<RemoteConfigDefaultByKey>();

        //[ArrayElementTitleSonatSdk(nameof(RemoteConfigDefaultByKey.key))] [SerializeField]
        public List<RemoteConfigDefaultByString> defaultConfigs = new();

        private List<BaseRemoteConfigDefault> _allDefaultConfigs;
        private SonatFirebase sonatFirebase;

        //public FirebaseDefaultSettings setting => defaultRemoteDatabase.setting;

        public void Initialize(SonatFirebase sonatFirebase)
        {
            this.sonatFirebase = sonatFirebase;
            defaultConfigs = this.sonatFirebase.config.defaultConfigs;
            SetDefaults();
            FetchData(true);
        }

        private void FetchData(bool first)
        {
#if using_firebase || using_firebase_remote
            FetchDataAsync(first);
#endif
        }

#if using_firebase || using_firebase_remote
        Task FetchDataAsync(bool first)
        {
            if (first)
                FetchStatus = ConfigFetchStatus.Fetching;
            Task fetchTask = firebaseRemote.FetchAsync(
                TimeSpan.Zero);
            return fetchTask.ContinueWithOnMainThread(FetchComplete);
        }


        private void FetchComplete(Task fetchTask)
        {
            if (fetchTask.IsCanceled)
            {
                SonatDebugType.RemoteConfig.Log("Fetch canceled.");
            }
            else if (fetchTask.IsFaulted)
            {
                SonatDebugType.RemoteConfig.Log("Fetch encountered an error.");
                FetchStatus = ConfigFetchStatus.FetchedFail;
            }
            else if (fetchTask.IsCompleted)
            {
                SonatDebugType.RemoteConfig.Log("Fetch completed successfully!");
                FetchStatus = ConfigFetchStatus.Fetched;
            }

            var info = firebaseRemote.Info;

            switch (info.LastFetchStatus)
            {
                case LastFetchStatus.Success:
                    firebaseRemote.ActivateAsync();
                    foreach (var baseRemoteConfigDefault in AllDefaultConfigs)
                        baseRemoteConfigDefault.SavePlayerPref();
                    SonatDebugType.RemoteConfig.Log($"Remote data loaded and ready (last fetch time {info.FetchTime}).");

                    SonatFirebase.FirebaseRemoteReady = true;
                    break;
                case LastFetchStatus.Failure:
                    switch (info.LastFetchFailureReason)
                    {
                        case FetchFailureReason.Error:
                            SonatDebugType.RemoteConfig.Log("Fetch failed for unknown reason");
                            break;
                        case FetchFailureReason.Throttled:
                            SonatDebugType.RemoteConfig.Log("Fetch throttled until " + info.ThrottledEndTime);
                            break;
                    }

                    break;
                case LastFetchStatus.Pending:
                    SonatDebugType.RemoteConfig.Log("Latest Fetch call still pending.");
                    break;
            }

            SonatAppsFlyer.SendEvent("LaunchApp");
            SonatFirebase.analytic.LogEvent("app_loading");
        }
#endif


        private void SetDefaults()
        {
            var dict = new Dictionary<string, object>();
            for (var index = 0; index < AllDefaultConfigs.Count; index++)
            {
                var item = AllDefaultConfigs[index];
                var key = item.GetKey();
                if (dict.ContainsKey(key))
                {
                    Debug.LogError("duplicate key " + key + " at " + index);
                    Debug.Break();
                }

                switch (item.dataType)
                {
                    case DataType.String:
                        dict.Add(key, item.defaultString);
                        break;
                    case DataType.Int:
                        dict.Add(key, item.defaultInt);
                        break;
                    case DataType.Float:
                        dict.Add(key, item.defaultFloat);
                        break;
                    case DataType.Boolean:
                        dict.Add(key, item.defaultBoolean);
                        break;
                    case DataType.Json:
                        dict.Add(key, item.jsonTextAsset.text);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
#if using_firebase || using_firebase_remote
            firebaseRemote.SetDefaultsAsync(dict);
#endif
        }

        private int fetchTime;

        public void OnApplicationPause(bool pauseStatus)
        {
            if (pauseStatus)
            {
                int seg = Mathf.FloorToInt(Time.realtimeSinceStartup / 300f);
                if (seg > fetchTime)
                {
                    fetchTime = seg;
#if using_firebase || using_firebase_remote
                    FetchData(!SonatFirebase.FirebaseRemoteReady);
#endif
                }
            }
        }


        public T GetRemoteConfig<T>(string key, T defaultValue)
        {
            string value = GetRemoteString(key);
            if (!string.IsNullOrEmpty(value))
            {
                if (typeof(T).IsEnum)
                {
                    return (T)Enum.Parse(typeof(T), value, true);
                }
#if using_newtonsoft
                T t = JsonConvert.DeserializeObject<T>(value);
                return t;
#endif
            }

            return defaultValue;
        }

        public int GetRemoteInt(string key, int defaultValue = 0)
        {
#if using_firebase || using_firebase_remote
            if ((FetchStatus == ConfigFetchStatus.Fetched ||
                 FetchStatus == ConfigFetchStatus.FetchedFail)
                && RemoteHasValue(key))
                return (int)GetValue(key).LongValue;
#endif

            var defaultConfig = GetDefault(key);
            if (defaultConfig != null) return defaultConfig.GetDefaultInt(true);

            //SonatDebugType.RemoteConfig.Log($"not found {key} default config, returning default value :{defaultValue}");
            return defaultValue;
        }

        public float GetRemoteFloat(string key, float defaultValue = 0)
        {
#if using_firebase || using_firebase_remote
            if ((FetchStatus == ConfigFetchStatus.Fetched ||
                 FetchStatus == ConfigFetchStatus.FetchedFail)
                && RemoteHasValue(key))
                return (float)GetValue(key).DoubleValue;
#endif

            var defaultConfig = GetDefault(key);
            if (defaultConfig != null) return defaultConfig.GetDefaultFloat(true);

            //SonatDebugType.RemoteConfig.Log($"not found {key} default config, returning default value :{defaultValue}");

            return defaultValue;
        }

        public double GetRemoteDouble(string key, double defaultValue = 0)
        {
#if using_firebase || using_firebase_remote
            if ((FetchStatus == ConfigFetchStatus.Fetched ||
                 FetchStatus == ConfigFetchStatus.FetchedFail)
                && RemoteHasValue(key))
                return GetValue(key).DoubleValue;
#endif

            var defaultConfig = GetDefault(key);
            if (defaultConfig != null) return defaultConfig.GetDefaultFloat(true);

            //SonatDebugType.RemoteConfig.Log($"not found {key} default config, returning default value :{defaultValue}");
            return defaultValue;
        }

        public bool GetRemoteBool(string key, bool defaultValue = false)
        {
#if using_firebase || using_firebase_remote
            if ((FetchStatus == ConfigFetchStatus.Fetched ||
                 FetchStatus == ConfigFetchStatus.FetchedFail)
                && RemoteHasValue(key))
                return GetValue(key).BooleanValue;
#endif

            var defaultConfig = GetDefault(key);
            if (defaultConfig != null) return defaultConfig.GetDefaultBoolean(true);

            //SonatDebugType.RemoteConfig.Log($"not found {key} default config, returning default value :{defaultValue}");
            return defaultValue;
        }

        public string GetRemoteString(string key, string defaultValue = null)
        {
#if using_firebase || using_firebase_remote
            if ((FetchStatus == ConfigFetchStatus.Fetched ||
                 FetchStatus == ConfigFetchStatus.FetchedFail)
                && RemoteHasValue(key))
                return GetValue(key).StringValue;
#endif

            var defaultConfig = GetDefault(key);
            if (defaultConfig != null) return defaultConfig.GetDefaultString(true);

            //SonatDebugType.RemoteConfig.Log($"not found {key} default config, returning default value :{defaultValue}");

            return defaultValue;
        }


        private List<BaseRemoteConfigDefault> AllDefaultConfigs
        {
            get
            {
                if (_allDefaultConfigs == null)
                {
                    _allDefaultConfigs = new List<BaseRemoteConfigDefault>();
                    //_allDefaultConfigs.AddRange(defaultConfigs);
                    _allDefaultConfigs.AddRange(GetDefaultConfig2CheckNull());
                    //_allDefaultConfigs.AddRange(GetDistinctDefaultByKernelConfig());


#if UNITY_EDITOR
                    for (var i = 0; i < _allDefaultConfigs.Count; i++)
                    for (var i1 = i + 1; i1 < _allDefaultConfigs.Count; i1++)
                        if (_allDefaultConfigs[i].GetKey() == _allDefaultConfigs[i1].GetKey())
                        {
                            Debug.LogError("duplicate " + _allDefaultConfigs[i].GetKey());
                        }
#endif
                }

                return _allDefaultConfigs;
            }
        }

        private IEnumerable<RemoteConfigDefaultByString> GetDefaultConfig2CheckNull()
        {
            foreach (var remoteConfigDefaultByString in defaultConfigs)
            {
                if (remoteConfigDefaultByString.dataType == DataType.Json)
                {
                    if (remoteConfigDefaultByString.jsonTextAsset != null)
                        yield return remoteConfigDefaultByString;
                }
                else
                    yield return remoteConfigDefaultByString;
            }
        }


        public BaseRemoteConfigDefault GetDefault(RemoteConfigKey key)
        {
            return GetDefault(key.ToString());
        }

        public BaseRemoteConfigDefault GetDefault(string key)
        {
            BaseRemoteConfigDefault find = AllDefaultConfigs.Find(x => x.GetKey() == key);
            if (find == null)
            {
                find = defaultConfigs.Find(x => x.key.ToString() == key);
                //if (find == null)
                //SonatDebugType.RemoteConfig.LogWarning("not found remote config " + key);
            }

            return find;
        }


        public T GetValueByLevel<T>(string key, int level, T defaultValue)
        {
            if (!remoteDataByLevels.TryGetValue(key, out var data))
            {
                string json = GetRemoteString(key);
                if (!string.IsNullOrEmpty(json))
                {
#if using_newtonsoft
                    data = JsonConvert.DeserializeObject<RemoteDataByLevel>(json);
                    remoteDataByLevels.Add(key, data);
#endif
                }
            }

            return data == null ? defaultValue : data.GetValueByLevel<T>(level, defaultValue);
        }


        public T GetValueByLevel<T>(string key, T defaultValue)
        {
            return GetValueByLevel<T>(key, UserData.GetLevel("classic"), defaultValue);
        }

        public T GetValueByLevel<T>(RemoteConfigKey key, T defaultValue)
        {
            return GetValueByLevel<T>(key.ToString(), UserData.GetLevel("classic"), defaultValue);
        }

        public T GetValueByLevelSegment<T>(string key, string segment, int level, T defaultValue)
        {
            if (!remoteDataByLevelsSegment.TryGetValue(key, out var data))
            {
                string json = GetRemoteString(key);
                if (!string.IsNullOrEmpty(json))
                {
#if using_newtonsoft
                    data = JsonConvert.DeserializeObject<Dictionary<string, RemoteDataByLevel>>(json);
                    remoteDataByLevelsSegment.Add(key, data);
#endif
                }
            }

            if (data == null) return defaultValue;
            return data.TryGetValue(segment, out var value) ? value.GetValueByLevel<T>(level, defaultValue) : defaultValue;
        }

        public T GetValueBySegment<T>(string key, string segment, T defaultValue)
        {
            string json = GetRemoteString(key);
            if (!string.IsNullOrEmpty(json))
            {
#if using_newtonsoft
                var data = JsonConvert.DeserializeObject<Dictionary<string, object>>(json);
                if (data.TryGetValue(segment, out var value))
                    return (T)Convert.ChangeType(value, typeof(T), System.Globalization.CultureInfo.InstalledUICulture.NumberFormat);
#endif
            }
            return defaultValue;
        }


        public bool RemoteHasValue(RemoteConfigKey key)
        {
            return RemoteHasValue(key.ToString());
        }

        public bool RemoteHasValue(string key)
        {
#if using_firebase || using_firebase_remote
            if (!(FetchStatus == ConfigFetchStatus.Fetched || FetchStatus == ConfigFetchStatus.FetchedFail))
                return false;
            var config = GetValue(key);
            if (string.IsNullOrEmpty(config.StringValue))
                return false;
            return true;
#else
            return false;
#endif
        }

#if using_firebase || using_firebase_remote
        public ConfigValue GetValue(RemoteConfigKey key)
        {
            if (!(FetchStatus == ConfigFetchStatus.Fetched || FetchStatus == ConfigFetchStatus.FetchedFail))
            {
                SonatDebugType.RemoteConfig.LogWarning("don't call if not initialized'");
            }

            return GetValue(key.ToString());
        }

        public ConfigValue GetValue(string key)
        {
            return firebaseRemote.GetValue(key);
        }
#endif
    }
}