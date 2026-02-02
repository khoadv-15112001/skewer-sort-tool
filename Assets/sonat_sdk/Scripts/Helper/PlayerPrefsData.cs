using System;
using System.Collections.Generic;
using System.Linq;
using Sonat.FirebaseModule;
using Sonat.FirebaseModule.RemoteConfig;
using UnityEngine;

namespace Sonat
{
    [Serializable]
    public class PlayerPrefRemoteInt
    {
        [SerializeField] private string key;
        [SerializeField] private int value; //default

        private int SavedData
        {
            get => PlayerPrefs.GetInt(key, value);
            set => PlayerPrefs.SetInt(key, value);
        }

        public int Value
        {
            get
            {
                if (SonatFirebaseRemoteConfig.FetchStatus == ConfigFetchStatus.Fetched ||
                    SonatFirebaseRemoteConfig.FetchStatus == ConfigFetchStatus.FetchedFail)
                    if (SonatFirebase.remote.RemoteHasValue(key) &&
                        SonatFirebase.remote.GetRemoteInt(key) != SavedData)
                        SavedData = SonatFirebase.remote.GetRemoteInt(key);

                return SavedData;
            }
        }

        public PlayerPrefRemoteInt(string key, int value)
        {
            this.key = key;
            this.value = value;
        }


        public PlayerPrefRemoteInt(RemoteConfigKey key, int value)
        {
            this.key = key.ToString();
            this.value = value;
        }
    }

    [Serializable]
    public class PlayerPrefRemoteString
    {
        [SerializeField] private string key;
        [SerializeField] private string value; //default

        private string SavedData
        {
            get => PlayerPrefs.GetString(key, value);
            set => PlayerPrefs.SetString(key, value);
        }

        public string DefaultValueWhenEmpty() => string.IsNullOrEmpty(Value) ? value : Value;

        public string Value
        {
            get
            {
                if (SonatFirebaseRemoteConfig.FetchStatus == ConfigFetchStatus.Fetched ||
                    SonatFirebaseRemoteConfig.FetchStatus == ConfigFetchStatus.FetchedFail)
                    if (SonatFirebase.remote.RemoteHasValue(key) && SonatFirebase.remote.GetRemoteString(key) != SavedData)
                        SavedData = SonatFirebase.remote.GetRemoteString(key);

                return SavedData;
            }
        }

        public PlayerPrefRemoteString(RemoteConfigKey key, string value)
        {
            this.key = key.ToString();
            this.value = value;
        }

        public PlayerPrefRemoteString(string key, string value)
        {
            this.key = key;
            this.value = value;
        }
    }


    [Serializable]
    public class PlayerPrefRemoteBool
    {
        [SerializeField] private string key;
        [SerializeField] private bool value; //default

        private bool SavedData
        {
            get => PlayerPrefs.GetInt(key, value ? 1 : 0) != 0;
            set => PlayerPrefs.SetInt(key, value ? 1 : 0);
        }

        public bool Value
        {
            get
            {
                if (SonatFirebaseRemoteConfig.FetchStatus == ConfigFetchStatus.Fetched ||
                    SonatFirebaseRemoteConfig.FetchStatus == ConfigFetchStatus.FetchedFail)
                    if (SonatFirebase.remote.RemoteHasValue(key) && SonatFirebase.remote.GetRemoteBool(key) != SavedData)
                        SavedData = SonatFirebase.remote.GetRemoteBool(key);

                return SavedData;
            }
        }


        public PlayerPrefRemoteBool(string key, bool value)
        {
            this.key = key;
            this.value = value;
        }

        public PlayerPrefRemoteBool(RemoteConfigKey key, bool value)
        {
            this.key = key.ToString();
            this.value = value;
        }
    }


    [Serializable]
    public class PlayerPrefRemoteFloat
    {
        [SerializeField] private string key;
        [SerializeField] private float value; //default

        private float SavedData
        {
            get => PlayerPrefs.GetFloat(key, value);
            set => PlayerPrefs.SetFloat(key, value);
        }

        public float Value
        {
            get
            {
                if (SonatFirebaseRemoteConfig.FetchStatus == ConfigFetchStatus.Fetched ||
                    SonatFirebaseRemoteConfig.FetchStatus == ConfigFetchStatus.FetchedFail)
                    if (SonatFirebase.remote.RemoteHasValue(key) &&
                        Math.Abs((float)SonatFirebase.remote.GetRemoteDouble(key) - SavedData) > 0.001f)
                        SavedData = (float)SonatFirebase.remote.GetRemoteDouble(key);

                return SavedData;
            }
        }


        public PlayerPrefRemoteFloat(RemoteConfigKey key, float value)
        {
            this.key = key.ToString();
            this.value = value;
        }

        public PlayerPrefRemoteFloat(string key, float value)
        {
            this.key = key;
            this.value = value;
        }
    }

    public class PlayerPrefInt
    {
        public static Action<string, int> OnChangedValue;
        public Action<int> OnChanged;

        public int Value
        {
            get => PlayerPrefs.GetInt(_name);
            set
            {
                if (value != Value)
                {
                    PlayerPrefs.SetInt(_name, value);
                    OnChanged?.Invoke(value);
                    OnChangedValue?.Invoke(_name, value);
                }
            }
        }

        public bool BoolValue
        {
            get => PlayerPrefs.GetInt(_name) != 0;
            set
            {
                if (value == (Value != 0)) return;
                PlayerPrefs.SetInt(_name, value ? 1 : 0);
                OnChanged?.Invoke(value ? 1 : 0);
                OnChangedValue?.Invoke(_name, value ? 1 : 0);
            }
        }

        private readonly string _name;

        public PlayerPrefInt(string name)
        {
            _name = name;
        }

        public bool HasKey() => PlayerPrefs.HasKey(_name);

        public PlayerPrefInt(string name, int defaultValue)
        {
            if (!PlayerPrefs.HasKey(name))
                PlayerPrefs.SetInt(name, defaultValue);
            _name = name;
        }
    }


    public class PlayerPrefLong
    {
        public Action<long> OnChanged;

        private long _currentValue;

        public long Value
        {
            get => _currentValue;
            set
            {
                if (value != Value)
                {
                    _currentValue = value;
                    PlayerPrefs.SetString(_name, value.ToString());
                    OnChanged?.Invoke(value);
                }
            }
        }


        private readonly string _name;

        public PlayerPrefLong(string name)
        {
            _name = name;
            _currentValue = 0;
            try
            {
                if (!string.IsNullOrEmpty(PlayerPrefs.GetString(_name)))
                {
                    _currentValue = long.Parse(PlayerPrefs.GetString(_name));
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }
        
        public PlayerPrefLong(string name, long defaultValue)
        {
            _name = name;
            try
            {
                _currentValue = !string.IsNullOrEmpty(PlayerPrefs.GetString(_name)) ? long.Parse(PlayerPrefs.GetString(_name)) : defaultValue;
            }
            catch (Exception e)
            {
                _currentValue = defaultValue;
                Console.WriteLine(e);
                throw;
            }
        }
    }


    public class PlayerPrefString
    {
        public bool Exist => PlayerPrefs.HasKey(_name) && !string.IsNullOrEmpty(PlayerPrefs.GetString(_name, _default));

        public string Value
        {
            get => PlayerPrefs.GetString(_name, _default);
            set
            {
                if (value != Value)
                    PlayerPrefs.SetString(_name, value);
            }
        }

        private readonly string _name;

        private string _default;

        public PlayerPrefString(string name, string defaultValue)
        {
            _name = name;
            _default = defaultValue;
        }

        public void Clear()
        {
            PlayerPrefs.DeleteKey(_name);
        }
    }


    [System.Serializable]
    public class IntegerSerializableDictionary2 : BaseSerializableDictionary2
    {
        private int _currentValue;
        private readonly int _defaultValue;
        public List<int> items = new List<int>();
        private Dictionary<int, int> _dictionary = new Dictionary<int, int>();

        public int Count => items.Count;

        public int CountValue => items.Sum();

        public int Max => keys.Count == 0 ? -1 : keys.Max();

        public string Key { get; set; }

        public void Save()
        {
            if (!string.IsNullOrEmpty(Key))
                PlayerPrefs.SetString(Key, JsonUtility.ToJson(this));
        }
    
        public static IntegerSerializableDictionary2 Load(string key)
        {
            if (PlayerPrefs.HasKey(key))
            {
                try
                {
                    var load = JsonUtility.FromJson<IntegerSerializableDictionary2>(PlayerPrefs.GetString(key));
                    load.Key = key;
                    return load;
                }
                catch
                {
                }
            }
            var newDict = new IntegerSerializableDictionary2();
            newDict.Key = key;
            return newDict;
        }
    
        public IntegerSerializableDictionary2()
        {
        }

        public IntegerSerializableDictionary2(int currValue, int defaultVal = -1)
        {
            _currentValue = currValue;
            _defaultValue = defaultVal;
        }

        public int Current
        {
            get => _currentValue;
            set
            {
                if (_currentValue != value)
                {
                    _currentValue = value;
                }
            }
        }

        // id/index
        // value of subject index
        public int this[int key]
        {
            get
            {
                if (Exist(key))
                    return items[_dictionary[key]];

                return _defaultValue;
            }
            set
            {
                if (Exist(key))
                {
                    if (items[_dictionary[key]] != value)
                    {
                        items[_dictionary[key]] = value;
                    }
                }
                else
                {
                    keys.Add(key);
                    items.Add(value);
                    _dictionary.Add(key, keys.IndexOf(key));
                }
            }
        }

        public override bool Exist(int key)
        {
            if (_dictionary.Count > items.Count)
            {
                Debug.LogError("What the fuck 2");
                //Debug.LogError(key + "_dictionary[key]" + _dictionary[key] + " Items" + Items.Count);
                items.ForEach(x => Debug.Log("Item" + x));
                keys.ForEach(x => Debug.Log("Key" + x));
                foreach (var keyValuePair in _dictionary)
                {
                    Debug.Log(keyValuePair.Key + "/" + keyValuePair.Value);
                }

                Debug.Break();
            }

            if (_dictionary.ContainsKey(key))
                return true;
            // for existing
            var index = keys.IndexOf(key);
            if (index >= 0)
            {
                _dictionary.Add(key, index);
                return true;
            }

            return false;
        }


        public IntegerSerializableDictionary2 Clone()
        {
            return new IntegerSerializableDictionary2()
            {
                keys = keys.ToList(),
                items = items.ToList(),
            };
        }

        public override void ClearDict()
        {
            keys.Clear();
            items.Clear();
            _dictionary = new Dictionary<int, int>();
        }

        public int Sum() => items.Sum();

        public bool AnyValue()
        {
            return items.Sum() > 0;
        }
    }

    [System.Serializable]
    public abstract class BaseSerializableDictionary2
    {
        [SerializeField]
        protected List<int> keys = new List<int>();
        public abstract void ClearDict();

        public List<int> Keys => keys;

        public virtual bool Exist(int key)
        {
            return keys.Contains(key);
        }
    }
}