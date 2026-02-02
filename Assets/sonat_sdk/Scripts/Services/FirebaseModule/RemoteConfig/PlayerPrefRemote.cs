using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Sonat.FirebaseModule.RemoteConfig
{
    [Serializable]
     public class PlayerPrefListInt
    {
        public static Action<string, List<int>> OnChangedValue;

        [SerializeField] private List<int> defaultValue; //default
        [SerializeField] private string key;
        public List<int> Current
        {
            get
            {
                return PlayerPrefs.HasKey(key) ? Get() : defaultValue.ToList();
            }
        }

        private List<int> Get()
        {
            return SonatSdkUtils
                .ListIntFromString(PlayerPrefs.GetString(key, SonatSdkUtils.ToString(defaultValue)))
                .ToList();
        }

        public int Count => Current.Count;

        public bool Contains(int value) => Current.Contains(value);

        public bool AddDistinct(int add)
        {
            if (!Contains(add))
            {
                Add(add);
                return true;
            }

            return false;
        }

        public void Add(int add)
        {
            var temp = Current;
            temp.Add(add);
            Save(temp);
        }

        public void Remove(int remove)
        {
            var temp = Current;
            temp.Remove(remove);
            Save(temp);
        }

        public void RemoveAll(int remove)
        {
            var temp = Current;
            temp.RemoveAll(x => x == remove);
            Save(temp);
        }

        public void Clear()
        {
            var temp = Current;
            Current.Clear();
            Save(temp);
        }

        public void Save(List<int> temp)
        {
            PlayerPrefs.SetString(key, SonatSdkUtils.ToString(temp));
            OnChangedValue?.Invoke(key, temp);
        }


        //    public void Set(IEnumerable<int> set)
        //    {
        //        current = set.ToList();
        //        PlayerPrefs.SetString(key, DuongSerializationExtensions.ToString(current));
        //    }

        public PlayerPrefListInt(string key, List<int> value)
        {
            this.key = key;
            defaultValue = value;
            // Current = PlayerPrefs.HasKey(key) ? Get() : defaultValue.ToList();
        }
    }


    /// <summary>
    /// value only can override not edit
    /// </summary>
    [Serializable]
    public class PlayerPrefRemoteArrayInt
    {
        [SerializeField] private int[] defaultValue; //default
        [SerializeField] private string key;

        private int[] SavedData => SonatSdkUtils.ListIntFromString(PlayerPrefs.GetString(key, SonatSdkUtils.ToString(defaultValue)))
            .ToArray();

        public int[] Value
        {
            get
            {
                if (SonatFirebaseRemoteConfig.FetchStatus == ConfigFetchStatus.Fetched
                    && SonatFirebase.remote.RemoteHasValue(key)
                   )
                {
                    if (SonatFirebase.remote.GetRemoteString(key) != PlayerPrefs.GetString(key, SonatSdkUtils.ToString(defaultValue)))
                        PlayerPrefs.SetString(key, SonatFirebase.remote.GetRemoteString(key));
                }


                return SavedData;
            }
        }

        public PlayerPrefRemoteArrayInt(RemoteConfigKey key, int[] value)
        {
            this.key = key.ToString();
            defaultValue = value;
        }

        public PlayerPrefRemoteArrayInt(string key, int[] value)
        {
            this.key = key;
            defaultValue = value;
        }
    }
}