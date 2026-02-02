using Cysharp.Threading.Tasks;
using Newtonsoft.Json.Linq;
using Sirenix.OdinInspector;
using Sonat;
using Sonat.Enums;
using Sonat.FirebaseModule.RemoteConfig;
using SonatFramework.Scripts.Feature.Lives;
using SonatFramework.Systems;
using SonatFramework.Systems.EventBus;
using SonatFramework.Systems.GameDataManagement;
using SonatFramework.Systems.InventoryManagement;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace GrillSort.OnlineService
{
    [CreateAssetMenu(fileName = "HybridDataService",
        menuName = "Sonat Services/Data Service/HybridDataService", order = 2)]
    public class HybridDataService : DataService, IServiceInitialize
    {
        // set thì lưu local + server
        // get thì ưu tiên 
        // nếu đã kết nối server và giống tài khoản server:
        [SerializeField] public HybridDataServiceConfig config;

        [BoxGroup("SERVICES")]
        [Required]
        [SerializeField]
        private PlayerPrefsDataService local;

        [BoxGroup("SERVICES")]
        [Required]
        [SerializeField]
        private OnlineService online;

        public static List<PendingResourceResponse> pendingResources = new();

        private bool isLoadingResource;

        public static bool Ready { get; set; } = false;
        public static string LOG_TAG = "[Sonat HybridDataService]";
        public static string DATA_KEY = "HYBRID_DATA_SERVICE";

        public void Initialize()
        {
            PlayerPrefInt.OnChangedValue += PlayerPrefInt_OnChangedValue;
            PlayerPrefListInt.OnChangedValue += PlayerPrefListInt_OnChangedValue;

            new EventBinding<OnlineService.BIGetOrSignUpEvent>(OnBIGetOrSignUpEvent);
        }

        private void PlayerPrefInt_OnChangedValue(string key, int value)
        {
            SetDataToServer(key, PlayerPrefs.GetInt(key, value));
        }

        private void PlayerPrefListInt_OnChangedValue(string key, List<int> value){
            SetDataToServer(key, SonatSdkUtils.ToString(value));
        }

        private void OnBIGetOrSignUpEvent(OnlineService.BIGetOrSignUpEvent eventData)
        {
            GetPendingResources().Forget();
        }

        #region Implement DataService
        public override bool HasKey(string key)
        {
            return local.HasKey(key);
        }

        public override void DeleteKey(string key)
        {
            local.DeleteKey(key);
        }

        public override void SetInt(string key, int value)
        {
            local.SetInt(key, value);
            SetDataToServer(key, value);
        }

        public override int GetInt(string key, int defaultValue = 0)
        {
            return local.GetInt(key, defaultValue);
        }

        public override void SetFloat(string key, float value)
        {
            local.SetFloat(key, value);
            SetDataToServer(key, value);
        }

        public override float GetFloat(string key, float defaultValue = 0f)
        {
            return local.GetFloat(key, defaultValue);
        }

        public override void SetString(string key, string value)
        {
            local.SetString(key, value);
            SetDataToServer(key, value);
        }
        public override string GetString(string key, string defaultValue = null)
        {
            return local.GetString(key, defaultValue);
        }
        public override void SetBool(string key, bool value)
        {
            local.SetBool(key, value);
            SetDataToServer(key, value ? 1 : 0);
        }

        public override bool GetBool(string key, bool defaultValue = false)
        {
            return local.GetBool(key, defaultValue);
        }

        public override T GetData<T>(string key)
        {
            return local.GetData<T>(key);
        }

        public override void SetData<T>(string key, T data)
        {
            // luôn lưu local
            local.SetData(key, data);
            SetDataToServer(key, data);
        }

        private void SetDataToServer<T>(string key, T data)
        {
            if (Ready == false) return;

            //Debug.Log($"[Sonat OnlineService] SetDataToServer: {key} {data}");
            var dict = new Dictionary<string, object> { { key, data } };
            online.BIUpdateUserData(dict).Forget();
        }

        private T GetDataFromServer<T>(string key)
        {
            try
            {
                var response = online.BIGetUserData(key).GetAwaiter().GetResult();
                if (response != null && response.data.ContainsKey(key))
                {
                    // Convert JSON/string → object
                    var json = response.data[key].ToString();
                    var serverData = JsonUtility.FromJson<T>(json);

                    // cache local
                    local.SetData(key, serverData);
                    return serverData;
                }
            }
            catch { /* ignore */ }

            return default;
        }
        #endregion

        #region Implement connect to server
        public async UniTask UploadDataToServer()
        {
            var dict = new Dictionary<string, object>();
            foreach (var key in Enum.GetValues(typeof(DataSyncKey)))
            {
                var _obj = GetLocalData((DataSyncKey)key);
                if (_obj == null || string.IsNullOrEmpty(_obj.ToString()))
                {
                    continue;
                }
                dict.Add(key.ToString(), _obj);
            }
            var response = await online.BIUpdateUserData(dict);
            if (response != null)
            {
                Ready = true;
            }
        }

        private object GetLocalData(DataSyncKey key)
        {
            var type = DataSyncHelper.GetType(key);
            if (type == typeof(int))
            {
                return local.GetInt(key.ToString(), 0);
            }
            else if (type == typeof(string))
            {
                return local.GetString(key.ToString(), "");
            }
            else if (type == typeof(float))
            {
                return local.GetFloat(key.ToString(), 0f);
            }
            else
            {
                return local.GetData<object>(key.ToString());
            }
        }



        public async UniTask FetchDataFromServer()
        {
            string[] keys = Enum.GetValues(typeof(DataSyncKey)).Cast<DataSyncKey>().Select(k => k.ToString()).ToArray();
            var response = await online.BIGetUserData(keys);
            if (response != null)
            {
                if (response.data == null || response.data.Count == 0) return;
                foreach (var item in response.data)
                {
                    if (string.IsNullOrEmpty(item.Value.ToString())) continue;
                    Debug.Log($"[Sonat OnlineService] FetchDataFromServer: {item.Key} {item.Value}");
                    SaveLocalData(item.Key, item.Value);
                }
            }
            if (response != null)
            {
                Ready = true;
            }
        }
        private void SaveLocalData(string key, object value)
        {
            Debug.Log($"[Sonat OnlineService] SaveLocalData: {key} {value} ");

            try
            {
                JValue jv = value as JValue;
                switch (jv.Type)
                {
                    case JTokenType.String:
                        if (long.TryParse(jv.ToString().Replace("\"", ""), out long result))
                        {
                            local.SetString(key, result.ToString());
                            return;
                        }
                        else
                        {
                            local.SetString(key, jv.ToString());
                        }
                        break;
                    case JTokenType.Integer:
                        local.SetInt(key, (int)jv);
                        break;
                    case JTokenType.Float:
                        local.SetFloat(key, (float)jv);
                        break;
                    default:
                        local.SetString(key, jv.ToString());
                        break;
                }
            }
            catch (Exception e)
            {
                Debug.Log($"[Sonat OnlineService] <color=red>Error convert value to JValue</color>: {key} {value} {e}");
                var v = value.ToString();
                local.SetString(key, v);
            }
        }
        #endregion

        private async UniTask GetPendingResources()
        {
            if (isLoadingResource) return;
            isLoadingResource = true;

            try
            {
                pendingResources.Clear();

                var response = await online.Get<List<PendingResourceResponse>>($"users/resource-events?status=pending");

                if (response == null || response.Count == 0)
                {
                    isLoadingResource = false;
                    return;
                }

                pendingResources.AddRange(response);

                isLoadingResource = false;
            }
            catch
            {
                isLoadingResource = false;
            }
        }

        public async UniTask<int> GetPendingResource(string resource, MyStringComparison comparison = MyStringComparison.Ordinal)
        {
            try
            {
                if (isLoadingResource)
                    await UniTask.WaitUntil(() => !isLoadingResource);
                else
                    await GetPendingResources();

                if (pendingResources == null || pendingResources.Count == 0) return 0;

                var list = pendingResources.Where(
                    r => r.status == ResourceStatus.pending &&
                    MyStringComparer.Compare(r.key, resource, comparison) &&
                    r.eventType == VariableEventType.increase).ToList();

                if (list == null || list.Count == 0) return 0;

                return list.Sum(r => r.amountChange);
            }
            catch
            {
                return 0;
            }
        }

        public async UniTask<List<PendingResourceResponse>> GetPendingResourceResponses
        (string resource, MyStringComparison comparison = MyStringComparison.Ordinal)
        {
            try
            {
                if (isLoadingResource)
                    await UniTask.WaitUntil(() => !isLoadingResource);
                else
                    await GetPendingResources();

                if (pendingResources == null || pendingResources.Count == 0) return pendingResources;

                var list = pendingResources.Where(
                    r => r.status == ResourceStatus.pending &&
                    MyStringComparer.Compare(r.key, resource, comparison) &&
                    r.eventType == VariableEventType.increase).ToList();

                return list;
            }
            catch
            {
                return new List<PendingResourceResponse>();
            }
        }

        public async UniTask<bool> ClaimPendingLives()
        {
            await UniTask.WaitUntil(() => !isLoadingResource);

            var list = pendingResources.Where(
                r => r.status == ResourceStatus.pending &&
                r.key == OnlineResourceType.Lives &&
                r.eventType == VariableEventType.increase).ToList();

            int amount;
            if (list == null || list.Count == 0)
                amount = 0;
            else
                amount = list.Sum(r => r.amountChange);

            if (amount > 0)
            {
                if (MySonatFramework.GetService<LivesService>().IsFullLives())
                {
                    PopupToast.Cretate("Your lives are full!!");
                    return false;
                }

                try
                {
                    var response = await MakePendingSourceDone(new()
                    {
                        ids = list.Select(r => r.id).ToList()
                    });

                    MySonatFramework.GetService<LivesService>().RefillLive(amount, new()
                    {
                        spendId = "help",
                        spendType = "team"
                    });

                    for (int i = 0; i < pendingResources.Count; i++)
                    {
                        if (pendingResources[i].status == ResourceStatus.pending &&
                            pendingResources[i].key == OnlineResourceType.Lives &&
                            pendingResources[i].eventType == VariableEventType.increase)
                        {
                            pendingResources.RemoveAt(i);
                            i--;
                        }
                    }

                    return true;
                }
                catch
                {
                    return false;
                }
            }

            return false;
        }

        public async UniTask<bool> ClaimPendingResource(string id)
        {
            await UniTask.WaitUntil(() => !isLoadingResource);

            var found = pendingResources.Find(r => r.id == id);

            if (found == null || found.amountChange <= 0) return false;

            try
            {
                await MakePendingSourceDone(new()
                {
                    ids = new List<string> { id },
                });

                // Re-validate the resource still exists before removing
                var stillExists = pendingResources.Find(r => r.id == id);
                if (stillExists != null)
                {
                    pendingResources.Remove(stillExists);
                }

                return true;
            }
            catch (Exception ex)
            {
                //Debug.LogError($"Error claiming pending resource {id}: {ex.Message}");
                return false;
            }
        }

        public async UniTask<SendResourceResponse> SendResource(SendResourceRequest request)
        {
            try
            {
                var response = await online.Post<SendResourceResponse>($"users/resource-events", request);

                if (response is null)
                {
                    Debug.LogWarning($"No data returned from server.");
                    return null;
                }

                if (response is not { code: ResponseCode.SUCCESS })
                {
                    return response;
                }

                return response;
            }
            catch (Exception ex)
            {
                Debug.LogError($"Error send resource detail: {ex.Message}");
                throw;
            }
        }

        private async UniTask<UpdatePendingResourceResponse> MakePendingSourceDone(UpdatePendingResourceRequest request)
        {
            return await online.Put<UpdatePendingResourceResponse>($"users/resource-events/done", request);
        }
    }
}
