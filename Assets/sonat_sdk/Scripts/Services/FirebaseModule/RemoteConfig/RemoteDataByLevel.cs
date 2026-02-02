using System;
using System.Collections.Generic;

namespace Sonat.FirebaseModule.RemoteConfig
{
    public class RemoteDataByLevel
    {
        public Dictionary<int, object> data = new();

        public T GetValueByLevel<T>(int level, T defaultValue)
        {
            var result = defaultValue;
            object remoteObj = null;
            foreach (var remoteData in data)
            {
                if (remoteData.Key <= level)
                {
                    remoteObj = remoteData.Value;
                }
                else
                {
                    break;
                }
            }

            switch (remoteObj)
            {
                case null:
                    return result;
                case T value:
                    return value;
                default:
                    try
                    {
                        return (T)Convert.ChangeType(remoteObj, typeof(T), System.Globalization.CultureInfo.InstalledUICulture.NumberFormat);
                    }
                    catch (InvalidCastException)
                    {
                        return defaultValue;
                    }

                    return result;
            }
        }
    }
}

// using System;
// using sonat_sdk_v2.Enums;
// using sonat_sdk_v2.Helper;
// using UnityEngine;
//
// [Serializable]
// public class KeyConditionBase
// {
//     public int key;
// }
//
//
// [Serializable]
// public class IntKeyCondition : KeyConditionBase
// {
//     public int value;
// }
//
// [Serializable]
// public class StringByCondition : KeyConditionBase
// {
//     public string value;
// }
//
// [Serializable]
// public class IntByLevelCollection
// {
// //    public int defaultValue;
//     public IntKeyCondition[] data;
// }
//
//
// [Serializable]
// public class StringByLevelCollection
// {
// //    public int defaultValue;
//     public StringByCondition[] data;
// }
//
// [Serializable]
// public class IntByLevelCollectionRemote : IntByLevelCollection
// {
//     public RemoteConfigKey key;
//
//     public IntByLevelCollectionRemote(RemoteConfigKey key)
//     {
//         this.key = key;
//     }
//
//     public void Load()
//     {
//         var remoteStr = key.GetString();
//         if (remoteStr != String.Empty)
//         {
//             IntByLevelCollection collection = JsonUtility.FromJson<IntByLevelCollection>(remoteStr);
//             data = collection.data;
//         }
//     }
// }
//
// [Serializable]
// public class StringByLevelCollectionRemote : StringByLevelCollection
// {
//     public RemoteConfigKey key;
//
//     public StringByLevelCollectionRemote(RemoteConfigKey key)
//     {
//         this.key = key;
//     }
//
//     public void Load()
//     {
//         var remoteStr = key.GetString();
//         if (remoteStr != String.Empty)
//         {
//             StringByLevelCollection collection = JsonUtility.FromJson<StringByLevelCollection>(remoteStr);
//             data = collection.data;
//         }
//         else
//         {
//             data = null;
//         }
//     }
//
//     public string GetKey() => key.ToString();
//     public string GetString() => key.GetString();
// }