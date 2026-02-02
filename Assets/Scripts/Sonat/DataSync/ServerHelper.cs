using UnityEngine;
using Cysharp.Threading.Tasks;
using System.Collections.Generic;

namespace GrillSort.OnlineService
{
    public static class ServerHelper
    {
        public const string LOG_TAG = "[ServerHelper]";
        public static void DeleteUser()
        {
            PlayerPrefs.DeleteKey("SONAT_LOCAL_USER_ID");
            PlayerPrefs.DeleteKey("SONAT_LOCAL_USER_UID");
            PlayerPrefs.DeleteKey("CHECK_CONNECT_SERVER_USER_ID");
            PlayerPrefs.DeleteKey("SONAT_GENERATED_USER_NAME");
            MySonatFramework.GetService<OnlineService>().BIDeleteUser().Forget();
        }

        public static void CheatServer(DataSyncKey dataSyncKey, DataType dataType, string value)
        {
            if (dataType == DataType.None)
            {
                var paths = new string[] { dataSyncKey.ToString() };
                GetValueFromServer(paths).Forget();
            }
            else
            {

            }

        }

        public static async UniTaskVoid GetValueFromServer(string[] paths)
        {
            var value = await MySonatFramework.GetService<OnlineService>().BIGetUserData(paths);
            if (value.data.TryGetValue(paths[0], out var serverVal))
            {
                Debug.Log($"{LOG_TAG} Value from server: " + serverVal.ToString());
                PopupToast.Cretate("Value from server: " + serverVal.ToString());
                return;
            }
            else
            {
                Debug.Log($"{LOG_TAG} Key not found on server: " + paths[0]);
                PopupToast.Cretate("Key not found on server: " + paths[0]);
                return;
            }
        }

        public static async UniTaskVoid SetValueToServer(string dataSyncKey, DataType dataType, string value)
        {
            // set server data
            var updateData = new Dictionary<string, object>();
            switch (dataType)
            {
                case DataType.String:
                    updateData.Add(dataSyncKey.ToString(), value);
                    break;
                case DataType.Int:
                    if (int.TryParse(value, out var intValue))
                    {
                        updateData.Add(dataSyncKey.ToString(), intValue);
                    }
                    else
                    {
                        Debug.Log($"{LOG_TAG} Invalid int value: " + value);
                        PopupToast.Cretate("Invalid int value: " + value);
                        return;
                    }
                    break;
                case DataType.Float:
                    if (float.TryParse(value, out var floatValue))
                    {
                        updateData.Add(dataSyncKey.ToString(), floatValue);
                    }
                    else
                    {
                        Debug.Log($"{LOG_TAG} Invalid float value: " + value);
                        PopupToast.Cretate("Invalid float value: " + value);
                        return;
                    }
                    break;
            }

            MySonatFramework.GetService<OnlineService>().BIUpdateUserData(updateData).Forget();
        }
    }
}