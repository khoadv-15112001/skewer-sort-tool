using Cysharp.Threading.Tasks;
using Firebase;
using Newtonsoft.Json;
using Sonat.Data;
using Sonat.FirebaseModule;
using SonatFramework.Systems;
using SonatFramework.Systems.EventBus;
using SonatFramework.Systems.NetworkManagement;
using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.Networking;
using Newtonsoft.Json.Linq;
using System.Text;
using Firebase.Installations;
#if UNITY_IOS
using System.Runtime.InteropServices;
#endif

namespace GrillSort.OnlineService
{
    [CreateAssetMenu(fileName = "OnlineService", menuName = "Sonat Services/Online Service")]
    public class OnlineService : NetworkService, IServiceInitialize
    {
        [SerializeField] private OnlineServiceConfig _osConfig;

        private string fcm_token;
        private string fid;
        private string email;
        private string password;
        private bool firebaseReady;

        private static bool isInitilized;

        private const string SONAT_LOCAL_USER_EMAIL = "backup_user_email";
        private const string SONAT_LOCAL_USER_ID = "SONAT_LOCAL_USER_ID";
        private const string SONAT_LOCAL_USER_UID = "SONAT_LOCAL_USER_UID";
        private const string SONAT_GENERATED_USER_NAME = "SONAT_GENERATED_USER_NAME";
        private const float INIT_FIREBASE_TIMEOUT = 10.0f;

        //Public
        public bool Ready { get; private set; }
        public UserInfo UserInfo { get; private set; }

#if UNITY_IOS && !UNITY_EDITOR
        public delegate void LoadFIDCallback(string fid);
        private static UniTaskCompletionSource<string> _tcs;
        private static readonly LoadFIDCallback _callback = OnFIDLoaded;

        [AOT.MonoPInvokeCallback(typeof(LoadFIDCallback))]
        private static void OnFIDLoaded(string fid)
        {
            _tcs?.TrySetResult(fid);
        }

        [DllImport("__Internal")]
        private static extern void saveFID(string fid);

        [DllImport("__Internal")]
        private static extern void loadFID(LoadFIDCallback callback);
#endif

#if UNITY_ANDROID && !UNITY_EDITOR
        private static AndroidJavaObject GetUnityActivity()
        {
            var unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
            return unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
        }

        public static void SaveAccId(string accId)
        {
            var activity = GetUnityActivity();
            using var accBackupManager = new AndroidJavaClass("com.grill.sorting.food.match.puzzle.AccBackupManager");
            accBackupManager.CallStatic("saveAccId", activity, accId);
        }

        public static string LoadAccId()
        {
            var activity = GetUnityActivity();
            using var accBackupManager = new AndroidJavaClass("com.grill.sorting.food.match.puzzle.AccBackupManager");
            return accBackupManager.CallStatic<string>("loadAccId", activity);
        }

        public static void ClearAccId()
        {
            var activity = GetUnityActivity();
            using var accBackupManager = new AndroidJavaClass("com.grill.sorting.food.match.puzzle.AccBackupManager");
            accBackupManager.CallStatic("clearAccId", activity);
        }
#endif

        public struct BIGetOrSignUpEvent : IEvent
        {
            public bool success;
            public bool isNewUser;
        }

        public struct CheckNewUserEvent : IEvent
        {
            public bool isNewUser;
        }

        public struct MessageReceivedEvent : IEvent
        {
            public MessageType type;
            public string messageString;
        }

        public void Initialize()
        {
#if !UNITY_EDITOR
            if (isInitilized) return;
            isInitilized = true;
#endif

            Ready = false;
            firebaseReady = false;

            GenerateDefaultName();
            InitializeOnlineService().Forget();

            new EventBinding<BIGetOrSignUpEvent>(OnBIGetOrSignUp);
        }

        private void OnBIGetOrSignUp(BIGetOrSignUpEvent eventData)
        {
            if (!eventData.success) return;

            CheckFcmTokenAsync().Forget();
        }

        private async UniTask CheckFcmTokenAsync()
        {
            var userInfoTask = BIGetUserInfo();
            var fcmTokenTask = Firebase.Messaging.FirebaseMessaging.GetTokenAsync().AsUniTask();

            var (userInfo, fcmToken) = await UniTask.WhenAll(userInfoTask, fcmTokenTask);

            Device found = null;
            userInfo.devices ??= new List<Device>();

            for (int i = 0; i < userInfo.devices.Count; i++)
            {
                if (userInfo.devices[i].fb_instance_id == UserData.FirebaseInstanceId.Value)
                {
                    found = userInfo.devices[i]; 
                    break;
                }
            }

            if (found == null)
            {
                userInfo.devices.Add(new()
                {
                    fb_instance_id = UserData.FirebaseInstanceId.Value,
                    fb_analytics_instance_id= UserData.AnalyticsInstanceId.Value,
                    fcmToken = fcmToken,
                });

                await BIUpdateUserInfo(new()
                {
                    name = userInfo.name,
                    avatar = userInfo.avatar,
                    devices = userInfo.devices
                });

                if (_osConfig.enableLog)
                    Debug.LogError("[Sonat OnlineService] Added new device to UserInfo");
            }
            else
            {
                if (fcmToken != found.fcmToken)
                {
                    found.fcmToken = fcmToken;

                    await BIUpdateUserInfo(new()
                    {
                        name = userInfo.name,
                        avatar = userInfo.avatar,
                        devices = userInfo.devices
                    });

                    if (_osConfig.enableLog)
                        Debug.LogError("[Sonat OnlineService] Fcm token updated");
                }
            }
        }

        #region Basic Authen
        public async UniTask InitializeOnlineService()
        {
            fid = await LoadFID();

            bool isNewUser = string.IsNullOrEmpty(fid);
            EventBus<CheckNewUserEvent>.Raise(new CheckNewUserEvent() { isNewUser = isNewUser });

            if (_osConfig.autoSignIn)
            {
                InitializeBasicAuth();
            }
        }

        private async UniTask InitializeBasicAuth(Action<bool> onResult = null)
        {
#if using_firebase_analytics
            bool isFBAvailable = await UniTask.WhenAny(
                UniTask.WaitUntil(() => SonatFirebase.dependencyStatus == DependencyStatus.Available),
                UniTask.Delay(TimeSpan.FromSeconds(INIT_FIREBASE_TIMEOUT))
            ) == 0;
#else
            bool isFBAvailable = false;
#endif

            if (!isFBAvailable)
            {
                if (_osConfig.enableLog)
                    Debug.LogError("[Sonat OnlineService] Firebase dependencies check timed out.");

                onResult?.Invoke(false);
                return;
            }

            Firebase.Messaging.FirebaseMessaging.TokenReceived += OnTokenReceived;
            Firebase.Messaging.FirebaseMessaging.MessageReceived += OnMessageReceived;
            firebaseReady = true;
            await PrepareForSignInOrCreate(onResult);
        }

        private async UniTask PrepareForSignInOrCreate(Action<bool> onResult = null)
        {
            //string fid = await LoadFID();

            if (string.IsNullOrEmpty(fid))
            {
#if UNITY_EDITOR
                string tmpFid = Regex.Replace(GetGeneratedName(), "[^a-zA-Z0-9]", "");
                fid = tmpFid;
#else
                fid = await FirebaseInstallations.DefaultInstance.GetIdAsync();
#endif
                await SaveFID(fid);
            }

            string email = fid + _osConfig.emailDomain;
            string password = GeneratePassword(email);

            await SignInOrCreateV2(email, password, onResult);
        }

        private string GeneratePassword(string email)
        {
            string salt = "Sonat@111";
            string combined = email + salt;

            using (var sha256 = SHA256.Create())
            {
                byte[] bytes = Encoding.UTF8.GetBytes(combined);
                byte[] hash = sha256.ComputeHash(bytes);

                StringBuilder sb = new StringBuilder();
                foreach (byte b in hash)
                    sb.Append(b.ToString("x2"));
                return sb.ToString();
            }
        }

        private async UniTask SignInOrCreateV2(string email, string password, Action<bool> onResult = null)
        {
            this.email = email;
            this.password = password;

            try
            {
                bool success = await BIGetOrSignUpUserInfo();

                if (success)
                {
                    if (_osConfig.enableLog)
                        Debug.LogFormat("[Sonat OnlineService] User signed in successfully: {0}", email);
                    Ready = true;
                    onResult?.Invoke(true);
                }
                else
                {
                    if (_osConfig.enableLog)
                        Debug.LogFormat("[Sonat OnlineService] User signed in Failed: {0}", email);
                    Ready = false;
                    onResult?.Invoke(false);
                }
            }
            catch (FirebaseException ex)
            {
                if (_osConfig.enableLog)
                    Debug.LogError("[Sonat OnlineService] Auth failed: " + email);
                if (_osConfig.enableLog)
                    Debug.LogError("[Sonat OnlineService] Auth failed: " + ex.ErrorCode + "_" + ex.Message);

                onResult?.Invoke(false);
            }
        }

        private UniTask SaveFID(string fid)
        {
#if UNITY_IOS && !UNITY_EDITOR
            saveFID(fid);
#elif UNITY_ANDROID && !UNITY_EDITOR
            SaveAccId(fid);
#endif
            if (_osConfig.enableLog)
                Debug.Log("[Sonat OnlineService] SaveFID " + fid);

            PlayerPrefs.SetString(SONAT_LOCAL_USER_EMAIL, fid);
            PlayerPrefs.Save();
            return UniTask.CompletedTask;
        }

        private async UniTask<string> LoadFID()
        {
            string fid = null;

#if UNITY_IOS && !UNITY_EDITOR
            _tcs = new UniTaskCompletionSource<string>();
            loadFID(_callback);
            fid = await _tcs.Task;
#elif UNITY_ANDROID && !UNITY_EDITOR
            fid = LoadAccId();
#endif

            if (string.IsNullOrEmpty(fid) && (PlayerPrefs.HasKey(SONAT_LOCAL_USER_EMAIL) || Application.isEditor))
            {
                fid = PlayerPrefs.GetString(SONAT_LOCAL_USER_EMAIL, null);
            }

            if (_osConfig.enableLog)
                Debug.Log("[Sonat OnlineService] LoadFID " + fid);

            return fid;
        }

        private void OnTokenReceived(object sender, Firebase.Messaging.TokenReceivedEventArgs token)
        {
            if (_osConfig.enableLog)
                Debug.Log("[Sonat OnlineService] Received Registration Token FCM: " + token.Token);
            fcm_token = token.Token;
        }

        private string GetBasicToken()
        {
            string credentials = $"{email}:{password}";
            string base64Credentials = Convert.ToBase64String(Encoding.UTF8.GetBytes(credentials));
            return $"{base64Credentials}";
        }
        #endregion

        #region UserInfo
        public string UserID
        {
            get { return PlayerPrefs.GetString(SONAT_LOCAL_USER_ID); }
            set { PlayerPrefs.SetString(SONAT_LOCAL_USER_ID, value); }
        }

        public string UserUID
        {
            get { return PlayerPrefs.GetString(SONAT_LOCAL_USER_UID); }
            set { PlayerPrefs.SetString(SONAT_LOCAL_USER_UID, value); }
        }

        public async UniTask<bool> BIGetOrSignUpUserInfo()
        {
            bool tokenReady = await UniTask.WhenAny(
                UniTask.WaitUntil(() => !string.IsNullOrEmpty(GetBasicToken())),
                UniTask.Delay(TimeSpan.FromSeconds(INIT_FIREBASE_TIMEOUT))
            ) == 0;

            if (!tokenReady)
            {
                if (_osConfig.enableLog)
                    Debug.LogError("[Sonat OnlineService] Token is empty");
                return false;
            }

            try
            {
                if (string.IsNullOrEmpty(UserID))
                {
                    return await DoSignUp(isNewUser: true);
                }
                else
                {
                    var result = await BITryFetchUserUID();

                    if (result.Success)
                    {
                        return true;
                    }
                    else
                    {
                        if (result.StatusCode == 409)
                        {
                            if (_osConfig.enableLog)
                                Debug.LogWarning("[Sonat OnlineService] User not found on server (409), signing up again...");

                            return await DoSignUp(isNewUser: true);
                        }
                        else
                        {
                            if (_osConfig.enableLog)
                                Debug.LogError($"[Sonat OnlineService] Problem in BIGetOrSignUpUserInfo with statusCode: {result.StatusCode}");

                            return false;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                if (_osConfig.enableLog)
                    Debug.LogError($"[Sonat OnlineService] Problem in BIGetOrSignUpUserInfo: {ex}");

                var result = await BITryFetchUserUID();

                if (result.Success)
                {
                    return true;
                }

                return false;
            }
        }

        private async UniTask<bool> DoSignUp(bool isNewUser)
        {
            if (_osConfig.enableLog)
                Debug.Log($"[Sonat OnlineService] SignUp BI user: {GetGeneratedName()}");

            UserInfo = await BISignUp(GetGeneratedName(), ProfileService.GetAvatarData());

            if (UserInfo is not null)
            {
                UserID = UserInfo.id;
                UserUID = UserInfo.uid;

                EventBus<BIGetOrSignUpEvent>.Raise(new BIGetOrSignUpEvent()
                {
                    success = true,
                    isNewUser = isNewUser
                });

                if (_osConfig.enableLog)
                    Debug.Log($"[Sonat OnlineService] SignUp BI user info successfully: {UserInfo.name}");

                return true;
            }

            return false;
        }

        private bool IsUserNotFoundError(Exception error)
        {
            if (_osConfig.enableLog)
                Debug.Log($"[Sonat OnlineService] IsUserNotFoundError: {error.Message}");

            try
            {
                var obj = JObject.Parse(error.Message);
                int code = (int?)obj["statusCode"] ?? -1;
                return code == 409;
            }
            catch (Exception ex)
            {
                if (_osConfig.enableLog)
                    Debug.LogError($"[Sonat OnlineService] Can't get statusCode: {ex}");

                return false;
            }
        }

        public async UniTask<UserInfo> BIGetUserInfo()
        {
            return await Get<UserInfo>("auth/me");
        }

        public async UniTask<UserInfo> BIUpdateUserInfo(string name, string avatar)
        {
            if (UserInfo == null)
                return null;

            if (string.IsNullOrEmpty(name) && string.IsNullOrEmpty(avatar))
                return null;

            UserRequestData data = new UserRequestData();

            if (!string.IsNullOrEmpty(name))
                data.name = name;

            if (!string.IsNullOrEmpty(avatar))
                data.avatar = avatar;

            return UserInfo = await Patch<UserInfo>("auth/me", data);
        }

        public async UniTask<UserInfo> BIUpdateUserInfo(UserRequestData data)
        {
            if (UserInfo == null || data == null)
                return null;

            return UserInfo = await Patch<UserInfo>("auth/me", data);
        }

        public async UniTask<FetchUserResult> BITryFetchUserUID()
        {
            if (_osConfig.enableLog)
                Debug.Log($"[Sonat OnlineService] Fetch BI user info with email: {email}");

            if (UserInfo is not null)
            {
                UserID = UserInfo.id;
                UserUID = UserInfo.uid;
                return new FetchUserResult { Success = true, StatusCode = null };
            }

            try
            {
                UserInfo = await BIGetUserInfo();
                UserID = UserInfo.id;
                UserUID = UserInfo.uid;

                EventBus<BIGetOrSignUpEvent>.Raise(new BIGetOrSignUpEvent()
                {
                    success = true,
                    isNewUser = false
                });

                if (_osConfig.enableLog)
                    Debug.Log($"[Sonat OnlineService] Fetch user info successfully: {UserInfo.name}");

                return new FetchUserResult { Success = true, StatusCode = null };
            }
            catch (Exception ex)
            {
                EventBus<BIGetOrSignUpEvent>.Raise(new BIGetOrSignUpEvent()
                {
                    success = false,
                    isNewUser = false
                });

                if (_osConfig.enableLog)
                    Debug.LogError($"[Sonat OnlineService] Problem getting user info: {ex}");

                int? statusCode = TryGetStatusCodeFromException(ex);

                return new FetchUserResult
                {
                    Success = false,
                    StatusCode = statusCode
                };
            }
        }

        private int? TryGetStatusCodeFromException(Exception error)
        {
            try
            {
                if (_osConfig.enableLog)
                    Debug.LogError($"[Sonat OnlineService] TryGetStatusCodeFromException: {error.Message}");

                var match = Regex.Match(error.Message, @"\{.*\}", RegexOptions.Singleline);

                if (match.Success)
                {
                    var jsonPart = match.Value;
                    var jObj = JObject.Parse(jsonPart);

                    if (jObj.TryGetValue("statusCode", out var token))
                        return token.Value<int>();
                }
            }
            catch (Exception ex)
            {
                if (_osConfig.enableLog)
                    Debug.LogError($"[Sonat OnlineService] Problem getting user info: {ex}");
            }

            return null;
        }

        private async UniTask<UserInfo> BISignUp(string name, string avatar)
        {
            bool idsReady = await UniTask.WhenAny(
                UniTask.WaitUntil(() =>
                    !string.IsNullOrEmpty(UserData.FirebaseInstanceId.Value) &&
                    !string.IsNullOrEmpty(UserData.AnalyticsInstanceId.Value)
                ),
                UniTask.Delay(TimeSpan.FromSeconds(INIT_FIREBASE_TIMEOUT))
            ) == 0;

            if (!idsReady)
            {
                if (_osConfig.enableLog)
                    Debug.LogError("[Sonat OnlineService] BISignUp failed: Missing FirebaseInstanceId/AnalyticsInstanceId");
                return null;
            }

            Device deviceData = new Device()
            {
                fb_instance_id = UserData.FirebaseInstanceId.Value,
                fb_analytics_instance_id = UserData.AnalyticsInstanceId.Value,
                fcmToken = string.IsNullOrEmpty(fcm_token) ? "empty" : fcm_token
            };

            UserRequestData data = new UserRequestData();
            data.name = name;
            data.devices = new List<Device>() { deviceData };

            if (!string.IsNullOrEmpty(avatar))
                data.avatar = avatar;

            if (_osConfig.enableLog)
                Debug.Log("[Sonat OnlineService] BISignUp Call API: auth/sign-up");

            return await Post<UserInfo>("auth/sign-up", data);
        }
        #endregion

        #region User progress
        public async UniTask<UserGameDataResponse> BIGetUserData(params string[] paths)
        {
            string endpoint = "users/v2/data";

            if (paths != null && paths.Length > 0)
            {
                List<string> queryParams = new List<string>();

                foreach (var p in paths)
                {
                    queryParams.Add("paths=" + UnityWebRequest.EscapeURL(p));
                }

                string query = string.Join("&", queryParams);
                endpoint += "?" + query;
            }

            try
            {
                var response = await Get<UserGameDataResponse>(endpoint);

                if (_osConfig.enableLog)
                {
                    string keys = (response?.data?.Keys != null) ? string.Join(",", response.data.Keys) : "null";
                    Debug.Log($"[Sonat OnlineService] GetUserData success, keys: {keys}");
                }

                return response;
            }
            catch (Exception ex)
            {
                if (_osConfig.enableLog)
                    Debug.LogError($"[Sonat OnlineService] GetUserData failed: {ex}");

                return null;
            }
        }

        public async UniTask<UserGameDataResponse> BIUpdateUserData(Dictionary<string, object> updateDatas)
        {
            if (updateDatas == null || updateDatas.Count == 0)
            {
                if (_osConfig.enableLog)
                    Debug.LogError("[Sonat OnlineService] BIUpdateUserData failed: updates is empty.");
                return null;
            }

            string endpoint = "users/v2/data/paths";

            try
            {
                var items = new List<UserDataUpdateItem>();
                foreach (var kvp in updateDatas)
                {
                    items.Add(new UserDataUpdateItem
                    {
                        path = kvp.Key,
                        value = kvp.Value
                    });
                }

                var payload = new UserGameDataUpdateRequest { data = items };
                var response = await Put<UserGameDataResponse>(endpoint, payload);

                if (_osConfig.enableLog && response != null)
                {
                    string keys = (response.data != null) ? string.Join(",", response.data.Keys) : "null";
                    Debug.Log($"[Sonat OnlineService] UpdateUserData success, keys: {keys}");
                }
                return response;
            }
            catch (Exception ex)
            {
                if (_osConfig.enableLog)
                    Debug.LogError($"[Sonat OnlineService] UpdateUserData failed: {ex}");
                return null;
            }
        }

        public async UniTask BIDeleteUser()
        {
            var endpoint = "users/force";
            await Delete<UserGameDataResponse>(endpoint, null);
        }
        #endregion

        #region Network
        public override bool IsInternetConnection()
        {
            return Application.internetReachability != NetworkReachability.NotReachable;
        }

        public override UniTask<T> Get<T>(string endpoint)
        {
            return SendRequest<T>(endpoint, RequestType.GET);
        }

        public override UniTask<T> Post<T>(string endpoint, object payload)
        {
            return SendRequest<T>(endpoint, RequestType.POST, payload);
        }

        public override UniTask<T> Patch<T>(string endpoint, object payload)
        {
            return SendRequest<T>(endpoint, RequestType.PATCH, payload);
        }

        public override UniTask<T> Put<T>(string endpoint, object payload)
        {
            return SendRequest<T>(endpoint, RequestType.PUT, payload);
        }

        public override UniTask<T> Delete<T>(string endpoint, object payload)
        {
            return SendRequest<T>(endpoint, RequestType.DELETE, payload);
        }

        public override async UniTask<Texture> DownloadImage(string mediaUrl)
        {
            throw new NotImplementedException();
        }

        public async UniTask<UnityWebRequest.Result> SendRequest(string endpoint, RequestType requestType, object payload)
        {
            var postRequest = CreateRequest(endpoint, requestType, payload);
            await postRequest.SendWebRequest();

            while (!postRequest.isDone)
                await UniTask.Delay(10);

            return postRequest.result;
        }

        private async UniTask<T> SendRequest<T>(string endpoint, RequestType requestType, object payload = null)
        {
            using var request = CreateRequest(endpoint, requestType, payload);

            try
            {
                await request.SendWebRequest();
                ValidateResponse(request);
                return DeserializeResponse<T>(request);
            }
            finally
            {
                request.Dispose();
            }
        }

        private UnityWebRequest CreateRequest(string endpoint, RequestType requestType, object payload = null)
        {
            string host = _osConfig.testService ? _osConfig.serverDevUrl : _osConfig.serverProductionUrl;
            string path = $"{host}/{_osConfig.AppID()}/{endpoint}";

            var request = new UnityWebRequest(path, requestType.ToString());
            request.downloadHandler = new DownloadHandlerBuffer();
            request.timeout = _osConfig.timeOut;
            request.SetRequestHeader("Content-Type", "application/json");

            string tmpToken = GetBasicToken();

            if (!string.IsNullOrEmpty(tmpToken))
                request.SetRequestHeader("Authorization", $"Basic {tmpToken}");

            if (payload != null)
            {
                var json = JsonConvert.SerializeObject(payload);
                var bodyRaw = Encoding.UTF8.GetBytes(json);
                request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            }

            if (_osConfig.enableLog)
            {
                Debug.Log($"[Sonat OnlineService] CreateRequest Token: {tmpToken}");
                Debug.Log($"[Sonat OnlineService] CreateRequest Path: {path}");
            }

            return request;
        }

        private void ValidateResponse(UnityWebRequest request)
        {
            if (request.result != UnityWebRequest.Result.Success)
                throw new Exception($"Network error: {request.error}");
        }

        private T DeserializeResponse<T>(UnityWebRequest request)
        {
            var json = request.downloadHandler.text;
            return JsonConvert.DeserializeObject<T>(json);
        } 
        #endregion

        #region Message Handle
        private void OnMessageReceived(object sender, Firebase.Messaging.MessageReceivedEventArgs e)
        {
            var messageContent = e.Message.Data;

            if (_osConfig.enableLog)
                Debug.Log("[Sonat OnlineService] MessageReceive " + JsonConvert.SerializeObject(e.Message));

            if (!messageContent.TryGetValue("type", out var messageType))
                return;

            MessageType type = OnlineMessage.FromString(messageType);

            if (type == MessageType.Unknown)
                return;

            if (!messageContent.TryGetValue("stringify", out var messageString))
            {
                return;
            }

            EventBus<MessageReceivedEvent>.Raise(new MessageReceivedEvent() { type = type, messageString = messageString });
        }
        #endregion

        #region Ultil
        public static void GenerateDefaultName()
        {
            if (PlayerPrefs.HasKey(SONAT_GENERATED_USER_NAME))
            {
                return;
            }

            uint randomNumber = BitConverter.ToUInt32(Guid.NewGuid().ToByteArray(), 0);
            string digits = randomNumber.ToString("D10");
            PlayerPrefs.SetString(SONAT_GENERATED_USER_NAME, $"Player#{digits}");
        }

        public static string GetGeneratedName()
        {
            return PlayerPrefs.GetString(SONAT_GENERATED_USER_NAME, null);
        }
        #endregion
    }
}
