using System;
using Sonat.Attributes;
using UnityEngine;

namespace Sonat.FirebaseModule.RemoteConfig
{
    public enum ConfigFetchStatus
    {
        NotStarted,
        Fetching,
        Fetched,
        FetchedFail,
    }

    public enum DataType
    {
        String,
        Int,
        Float,
        Boolean,
        Json,
        //Vector2,
        //Vector3,
        //Point,
        //Point3,
    }

    public struct DummyRemoteConfig
    {
        public double DoubleValue;
        public bool BooleanValue;
        public long LongValue;
        public string StringValue;
    }

    [Serializable]
    public abstract class BaseRemoteConfigDefault
    {
        public bool active = true;
        public abstract string GetKey();
        public DataType dataType;


        [ConditionalField(nameof(dataType), new object[] {DataType.String}, false, false)] [SerializeField]
        public string defaultString;
        
        [ConditionalField(nameof(dataType), new object[] {DataType.Int}, false, false)] [SerializeField]
        public int defaultInt;
        
        [ConditionalField(nameof(dataType), new object[] {DataType.Float}, false, false)] [SerializeField]
        public float defaultFloat;
        
        [ConditionalField(nameof(dataType), new object[] {DataType.Boolean}, false, false)] [SerializeField]
        public bool defaultBoolean;
        
        [ConditionalField(nameof(dataType), new object[] {DataType.Json}, false, false)]
        public TextAsset jsonTextAsset;

        
        //public int DefaultLongValue;
        
        //public double DefaultDoubleValue;
        
        //public string DefaultStringValue;
        
        //public bool DefaultBooleanValue;

        //
        // public double DoubleValue => DefaultDoubleValue;
        // public int LongValue => DefaultLongValue;
        // public string StringValue => DefaultStringValue;
        // public bool BooleanValue => DefaultBooleanValue;

    
        private string PlayerPrefKey => "remote_value_" + GetKey();

        public int GetDefaultInt(bool playerPrefsOr)
        {
            if (playerPrefsOr && PlayerPrefs.HasKey(PlayerPrefKey))
                return PlayerPrefs.GetInt(PlayerPrefKey);

            return defaultInt;
        }

        public bool GetDefaultBoolean(bool playerPrefsOr)
        {
            if (playerPrefsOr && PlayerPrefs.HasKey(PlayerPrefKey))
                return PlayerPrefs.GetInt(PlayerPrefKey) != 0;

            return defaultBoolean;
        }


        public float GetDefaultFloat(bool playerPrefsOr)
        {
            if (playerPrefsOr && PlayerPrefs.HasKey(PlayerPrefKey))
                return PlayerPrefs.GetFloat(PlayerPrefKey);

            return defaultFloat;
        }

        public string GetDefaultString(bool playerPrefsOr)
        {
            if (playerPrefsOr && PlayerPrefs.HasKey(PlayerPrefKey))
                return PlayerPrefs.GetString(PlayerPrefKey);

            return defaultString;
        }


        public void SavePlayerPref()
        {
            if (SonatFirebaseRemoteConfig.FetchStatus == ConfigFetchStatus.Fetched)
            {
                if (SonatFirebase.remote.RemoteHasValue(GetKey()))
                    switch (dataType)
                    {
                        case DataType.String:
                            PlayerPrefs.SetString(PlayerPrefKey, SonatFirebase.remote.GetRemoteString(GetKey()));
                            break;
                        case DataType.Int:
                            PlayerPrefs.SetInt(PlayerPrefKey, SonatFirebase.remote.GetRemoteInt(GetKey()));
                            break;
                        case DataType.Float:
                            PlayerPrefs.SetFloat(PlayerPrefKey, SonatFirebase.remote.GetRemoteFloat(GetKey()));
                            break;
                        case DataType.Boolean:
                            PlayerPrefs.SetInt(PlayerPrefKey,  SonatFirebase.remote.GetRemoteBool(GetKey()) ? 1 : 0);
                            break;
                        case DataType.Json:
                            break;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                else
                {
                    // meaningless because event only local default still giving RemoteHasValue above
//                switch (dataType)
//                {
//                    case DataType.String:
//                        if(PlayerPrefs.GetString(PlayerPrefKey) != defaultString)
//                            PlayerPrefs.SetString(PlayerPrefKey, defaultString);
//                        break;
//                    case DataType.Int:
//                        if(PlayerPrefs.GetInt(PlayerPrefKey) != defaultInt)
//                            PlayerPrefs.SetInt(PlayerPrefKey, defaultInt);
//                        break;
//                    case DataType.Float:
//                        if(Math.Abs(PlayerPrefs.GetFloat(PlayerPrefKey) - defaultFloat) > 0.001f)
//                            PlayerPrefs.SetFloat(PlayerPrefKey, defaultFloat);
//                        break;
//                    case DataType.Boolean:
//                        if((PlayerPrefs.GetInt(PlayerPrefKey) != 0 ) != defaultBoolean)
//                            PlayerPrefs.SetInt(PlayerPrefKey, defaultBoolean ? 1 : 0);
//                        break;
//                    case DataType.Json:
//                        break;
//                    default:
//                        throw new ArgumentOutOfRangeException();
//                }
                }
            }
        }
    }


    [Serializable]
    public class RemoteConfigDefaultByKey : BaseRemoteConfigDefault
    {
        public RemoteConfigKey key;

        public override string GetKey() => key.ToString();

        public RemoteConfigDefaultByKey(RemoteConfigKey key, int defaultValue)
        {
            this.key = key;
            defaultInt = defaultValue;
            dataType = DataType.Int;
        }

        public RemoteConfigDefaultByKey(RemoteConfigKey key, string defaultValue)
        {
            this.key = key;
            defaultString = defaultValue;
            dataType = DataType.String;
        }

        public RemoteConfigDefaultByKey(RemoteConfigKey key, bool defaultValue)
        {
            this.key = key;
            defaultBoolean = defaultValue;
            dataType = DataType.Boolean;
        }

        public RemoteConfigDefaultByKey(RemoteConfigKey key, float defaultValue)
        {
            this.key = key;
            defaultFloat = defaultValue;
            dataType = DataType.Float;
        }
    }

    [Serializable]
    public class RemoteConfigDefaultByString : BaseRemoteConfigDefault
    {
        [ConditionalField(nameof(customKey), true, false, false)] [SerializeField]
        public string key;
        [ConditionalField(nameof(customKey), false, false, false)] [SerializeField]
        public RemoteConfigKey keyEnum;
        public bool customKey = false;
        public override string GetKey() => customKey ? key : keyEnum.ToString();

        // public RemoteConfigDefaultByString(string key, int defaultValue)
        // {
        //     this.key = key;
        //     defaultInt = defaultValue;
        //     dataType = DataType.Int;
        // }
        //
        // public RemoteConfigDefaultByString(string key, float defaultValue)
        // {
        //     this.key = key;
        //     defaultFloat = defaultValue;
        //     dataType = DataType.Float;
        // }
        //
        //
        // public RemoteConfigDefaultByString(string key, string defaultValue)
        // {
        //     this.key = key;
        //     defaultString = defaultValue;
        //     dataType = DataType.String;
        // }
        //
        // public RemoteConfigDefaultByString(string key, bool defaultValue)
        // {
        //     this.key = key;
        //     defaultBoolean = defaultValue;
        //     dataType = DataType.Boolean;
        // }
    }
}