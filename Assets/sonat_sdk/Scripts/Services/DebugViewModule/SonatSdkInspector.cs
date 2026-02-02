using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

namespace Sonat.DebugViewModule
{
    class SonatSdkContants
    {
    }

    class ClassFieldRequireData
    {
        public string name { get; set; }
        public string type { get; set; }
    }

    class ClassMethodRequireData
    {
        public string name { get; set; }
        public bool requireContext { get; set; }

        public bool isBool { get; set; }
    }

    class ClassRequireData
    {
        public string name { get; set; }
        public object[] fields { get; set; }
        public ClassMethodRequireData[] methods { get; set; }
    }

    class LibRequiredData
    {
        public string name { get; set; }
        public string[] classesName { get; set; }
        public string[] classesNameXOR { get; set; }
        public ClassRequireData[] classes { get; set; }

        public LibRequiredData[] deps { get; set; }

        public static List<LibRequiredData> fromJSONArray(string json)
        {
#if using_newtonsoft
            return JsonConvert.DeserializeObject<List<LibRequiredData>>(json);
#else
            return null;
#endif
        }
    }

    public class LibRequiredDataValidateResult
    {
        public string Name { get; set; }
        public bool IsValid { get; set; }
        public List<LibRequiredDataValidateResult> deps { get; set; }
    }

    public class SignatureInfo
    {
        public string sha1 { get; set; }
        public string sha256 { get; set; }
    }

    public class SonatSdkInspector
    {
        private static SonatSdkInspector _sInstance;
        static readonly object _sLock = new();

        private List<LibRequiredData> requiredData;

        private SonatSdkInspector()
        {
        }

        public static SonatSdkInspector GetInstance()
        {
            lock (_sLock)
            {
                _sInstance ??= new SonatSdkInspector();
                return _sInstance;
            }
        }

        public static void LogInfo(string message)
        {
            Debug.LogFormat(UnityEngine.LogType.Log, LogOption.NoStacktrace, null, "SonatSdkInspector: {0}", message);
        }

        public async Task<List<LibRequiredDataValidateResult>> Validate()
        {
            List<LibRequiredDataValidateResult> results = new();

            var firebaseBuildResult = await GetFirebaseOptions();
            results.Add(firebaseBuildResult);

            if (requiredData == null)
            {
                string data = await GetRequiredData();
                //string data = SonatSdkContants.DEFAULT;
                if (data != null)
                {
                    requiredData = LibRequiredData.fromJSONArray(data);
                }
            }

            if (requiredData != null)
            {
                foreach (var e in requiredData)
                {
                    var result = validateRequireLib(e);
                    results.Add(result);
                }
            }

            return results;
        }

        public async void LogValidate()
        {
            var results = await Validate();
            foreach (var result in results)
            {
                ShowLogValidate(result);
            }
        }

        private void ShowLogValidate(LibRequiredDataValidateResult result)
        {
            LogInfo("ShowLogValidate " + result.Name + ": " + result.IsValid);
            foreach (var dep in result.deps)
            {
                ShowLogValidate(dep);
            }
        }

        private async Task<LibRequiredDataValidateResult> GetFirebaseOptions()
        {
            var result = new LibRequiredDataValidateResult
            {
                Name = "Firebase Google Service",
                IsValid = false
            };
            try
            {
#if UNITY_EDITOR
                var projectId = "conquer-the-tower";
#else
                AndroidJavaClass cls = new AndroidJavaClass("com.google.firebase.FirebaseApp");
                var instance = cls.CallStatic<AndroidJavaObject>("getInstance");
                var opts = instance.Call<AndroidJavaObject>("getOptions");
                var projectId = opts.Call<string>("getProjectId");
                LogInfo("GetFirebaseOptions projectId : " + projectId);
#endif
                var gcloudProjectId = await GetGCPProjectOfApp();
                result.IsValid = gcloudProjectId == projectId;
            }
            catch (Exception ex)
            {
                LogInfo("GetFirebaseOptions err: " + ex.Message);
            }

            return result;
        }

        private LibRequiredDataValidateResult validateRequireLib(LibRequiredData lib)
        {
            LibRequiredDataValidateResult result = new()
            {
                Name = lib.name,
                IsValid = false,
                deps = new List<LibRequiredDataValidateResult>(),
            };

#if UNITY_EDITOR
            if (lib.classes != null && lib.classes.Length > 0)
            {
            }
            else if (lib.classesName != null && lib.classesName.Length > 0)
            {
                if (lib.classesNameXOR == null || lib.classesNameXOR.Length == 0)
                {
                    result.IsValid = false;
                }
                else
                {
                    result.IsValid = true;
                }
            }

            if (lib.deps != null)
            {
                foreach (var dep in lib.deps)
                {
                    var depResult = validateRequireLib(dep);
                    result.deps.Add(depResult);
                }
            }
#else
            if (lib.classes != null && lib.classes.Length > 0) {
                result.IsValid = IsClassesExistedFields(lib.classes);
            } else if (lib.classesName != null && lib.classesName.Length > 0) {
                if (lib.classesNameXOR == null || lib.classesNameXOR.Length == 0)
                {
                    result.IsValid = IsClassesNameExisted(lib.classesName);
                }
                else
                {
                    result.IsValid = IsClassesNameExisted(lib.classesName) ^ IsClassesNameExisted(lib.classesNameXOR);
                }
            }

            if (lib.deps != null)
            {
                foreach (var dep in lib.deps)
                {
                    var depResult = validateRequireLib(dep);
                    result.deps.Add(depResult);
                }
            }
#endif

            return result;
        }

        private bool IsClassesExistedFields(ClassRequireData[] classes)
        {
            foreach (var cls in classes)
            {
                try
                {
                    LogInfo("IsClassesExistedFields: " + cls.name + " , " + cls.fields);
                    var obj = new AndroidJavaClass(cls.name);
                    var rawCls = obj.GetRawClass();
                    if (rawCls == null || rawCls.ToInt64() == 0)
                    {
                        obj.Dispose();
                        return false;
                    }

                    if (cls.fields != null && cls.fields.Length > 0)
                    {
                        foreach (var _field in cls.fields)
                        {
                            if (_field.GetType() == typeof(string))
                            {
                                var f = obj.GetStatic<AndroidJavaObject>(_field as string);
                                LogInfo("IsClassesExistedFields: " + cls.name + " , " + _field + " , " + (f == null));
                                if (f == null)
                                {
                                    obj.Dispose();
                                    return false;
                                }
                            }
                            else
                            {
#if using_newtonsoft
                                Newtonsoft.Json.Linq.JObject field = _field as Newtonsoft.Json.Linq.JObject;
                                string fieldName = field.Value<string>("name");
                                string fieldType = field.Value<string>("type");
                                LogInfo("IsClassesExistedFields: " + cls.name + ", name: " + fieldName + ", type: " + fieldType);

                                if (fieldType != null && fieldType.Length > 0)
                                {
                                    switch (fieldType)
                                    {
                                        case "string":
                                        {
                                            var f = obj.GetStatic<string>(fieldName);
                                            if (f == null || f.Length == 0)
                                            {
                                                obj.Dispose();
                                                return false;
                                            }
                                            break;
                                        }
                                        case "bool":
                                        {
                                            var f = obj.GetStatic<bool>(fieldName);
                                            if (!f)
                                            {
                                                obj.Dispose();
                                                return false;
                                            }
                                            break;
                                        }
                                        case "int":
                                        {
                                            var f = obj.GetStatic<int>(fieldName);
                                            if (f == 0)
                                            {
                                                obj.Dispose();
                                                return false;
                                            }
                                            break;
                                        }
                                        default:
                                        {
                                            var f = obj.GetStatic<AndroidJavaObject>(fieldName);
                                            if (f == null)
                                            {
                                                obj.Dispose();
                                                return false;
                                            }
                                            break;
                                        }
                                    }

                                }
                                else
                                {
                                    var f = obj.GetStatic<AndroidJavaObject>(fieldName);
                                    LogInfo("IsClassesExistedFields: " + cls.name + " , " + fieldName + " , " + (f == null));
                                    if (f == null)
                                    {
                                        obj.Dispose();
                                        return false;
                                    }
                                }
#endif
                            }
                        }
                    }

                    if (cls.methods != null && cls.methods.Length > 0)
                    {
                        foreach (var method in cls.methods)
                        {
                            LogInfo("IsClassesExistedMethods: " + cls.name + " , " + method.name + " , " + method.requireContext + " , " + method.isBool);
                            bool isValid = false;
                            if (method.requireContext)
                            {
                                if (method.isBool)
                                {
                                    isValid = obj.CallStatic<bool>(method.name, getAndroidContext());
                                }
                                else
                                {
                                    AndroidJavaObject ins = obj.CallStatic<AndroidJavaObject>(method.name, getAndroidContext());
                                    isValid = ins != null;
                                }
                            }
                            else
                            {
                                try
                                {
                                    if (method.isBool)
                                    {
                                        isValid = obj.CallStatic<bool>(method.name);
                                    }
                                    else
                                    {
                                        AndroidJavaObject ins = obj.CallStatic<AndroidJavaObject>(method.name, new object[0]);
                                        isValid = ins != null;
                                    }
                                }
                                catch (Exception ex)
                                {
                                    LogInfo("IsClassesExistedMethods err: " + ex.Message);
                                }
                            }

                            LogInfo("IsClassesExistedMethods: " + cls.name + " , " + method.name + " , " + method.requireContext + " , " + isValid);
                            if (!isValid)
                            {
                                obj.Dispose();
                                return false;
                            }
                        }
                    }

                    obj.Dispose();
                }
                catch (Exception e)
                {
                    LogInfo("IsClassesExistedMethods err: " + e.ToString());
                    return false;
                }
            }


            return true;
        }

        private bool IsClassesNameExisted(string[] classesName)
        {
            foreach (var className in classesName)
            {
                try
                {
                    var obj = new AndroidJavaClass(className);
                    var cls = obj.GetRawClass();
                    if (cls == null || cls.ToInt64() == 0)
                    {
                        obj.Dispose();
                        return false;
                    }

                    obj.Dispose();
                }
                catch (Exception e)
                {
                    return false;
                }
            }

            return true;
        }

        public void ShowInfo()
        {
            Task<List<LibRequiredDataValidateResult>> task = Validate();
            TaskAwaiter<List<LibRequiredDataValidateResult>> awaiter = task.GetAwaiter();
            LoadingIndicatorWithoutMonoBehaviour loading = ShowLoadingIndicator();

            awaiter.OnCompleted(() =>
            {
                List<LibRequiredDataValidateResult> results = awaiter.GetResult();


                int screenWidth = Screen.width;
                int screenHeight = Screen.height;

                GameObject canvasGO = new GameObject("Canvas");
                Canvas canvas = canvasGO.AddComponent<Canvas>();
                canvas.renderMode = RenderMode.ScreenSpaceOverlay;
                CanvasScaler canvasScaler = canvasGO.AddComponent<CanvasScaler>();
                canvasScaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                canvasGO.AddComponent<GraphicRaycaster>();

                GameObject scrollViewGO = new GameObject("ScrollView");
                scrollViewGO.transform.SetParent(canvasGO.transform, false);

                RectTransform scrollViewRT = scrollViewGO.AddComponent<RectTransform>();
                scrollViewRT.anchorMin = new Vector2(0, 0);
                scrollViewRT.anchorMax = new Vector2(1, 1);
                scrollViewRT.sizeDelta = Vector2.zero;
                scrollViewRT.anchoredPosition = Vector2.zero;

                ScrollRect scrollRect = scrollViewGO.AddComponent<ScrollRect>();

                GameObject viewportGO = new GameObject("Viewport");
                viewportGO.transform.SetParent(scrollViewGO.transform, false);
                RectTransform viewportRT = viewportGO.AddComponent<RectTransform>();
                viewportRT.anchorMin = new Vector2(0.0f, 0.0f);
                viewportRT.anchorMax = new Vector2(1.0f, 1.0f);
                viewportRT.sizeDelta = Vector2.zero;
                viewportRT.pivot = new Vector2(0.5f, 0.5f);

                GameObject contentGO = new GameObject("Content");
                contentGO.transform.SetParent(viewportGO.transform, false);
                RectTransform contentRT = contentGO.AddComponent<RectTransform>();
                contentRT.anchorMin = new Vector2(0, 1);
                contentRT.anchorMax = new Vector2(1, 1);
                contentRT.pivot = new Vector2(0.5f, 1);
                contentRT.sizeDelta = new Vector2(0, 0);
                scrollRect.content = contentRT;

                viewportGO.AddComponent<Mask>().showMaskGraphic = true;
                viewportGO.AddComponent<Image>().color = new Color(0.5f, 0.5f, 0.5f, 1.0f);

                int itemHeight = 80;

                int alignTop = 100;
                int alignLeft = 20;

                int index = 0;
                for (int i = 0; i < results.Count; i++)
                {
                    var result = results[i];
                    index += CreateItem(contentGO, result, alignLeft, alignTop, itemHeight, index, 0);
                    ++index;
                }

                scrollRect.viewport = viewportRT;
                scrollRect.horizontal = false;
                scrollRect.vertical = true;
                contentRT.sizeDelta = new Vector2(0, itemHeight * (index + 1) + alignTop);


                CreateCloseButton(canvasGO);

                HideLoadingIndicator(loading);
            });
        }

        private LoadingIndicatorWithoutMonoBehaviour ShowLoadingIndicator()
        {
            var loading = new LoadingIndicatorWithoutMonoBehaviour();
            loading.StartLoading();
            return loading;
        }

        private void HideLoadingIndicator(LoadingIndicatorWithoutMonoBehaviour loading)
        {
            loading.StopLoading();
        }

        private int CreateItem(GameObject contentGO, LibRequiredDataValidateResult result, int alignLeft, int alignTop, int itemHeight, int index, int depth)
        {
            int childCount = 0;
            GameObject item = new GameObject("Item " + result.Name);
            item.transform.SetParent(contentGO.transform, false);
            Text itemText = item.AddComponent<Text>();
            itemText.text = result.Name;
            itemText.alignment = TextAnchor.MiddleLeft;
            AssignFont(itemText);
            if (result.IsValid)
            {
                itemText.color = Color.green;
            }
            else
            {
                itemText.color = Color.red;
                itemText.fontStyle = FontStyle.BoldAndItalic;
            }

            itemText.fontSize = 28;

            RectTransform itemRT = item.GetComponent<RectTransform>();
            itemRT.sizeDelta = new Vector2(Screen.width - 50, itemHeight);
            itemRT.anchorMin = new Vector2(0.0f, 1.0f);
            itemRT.anchorMax = new Vector2(0.0f, 1.0f);
            itemRT.pivot = new Vector2(0.0f, 1);
            itemRT.anchoredPosition = new Vector2(alignLeft + depth * 20, -alignTop - index * itemHeight);

            LogInfo("createItem " + result.Name + " , " + index + " , " + depth + " , " + Screen.width + " , " + Screen.height + " , " + Screen.dpi + " , " +
                    itemRT.anchorMin + " , " + itemRT.anchorMax);


            if (result.deps != null && result.deps.Count > 0)
            {
                for (int i = 0; i < result.deps.Count; ++i)
                {
                    childCount += CreateItem(contentGO, result.deps[i], alignLeft, alignTop, itemHeight, index + 1, depth + 1);
                    index += childCount + 1;
                }

                childCount += result.deps.Count;
            }


            return childCount;
        }

        private void CreateCloseButton(GameObject parent)
        {
            GameObject buttonObject = new GameObject("CloseButton");
            buttonObject.transform.SetParent(parent.transform, false);

            RectTransform rectTransform = buttonObject.AddComponent<RectTransform>();
            rectTransform.anchorMin = new Vector2(1.0f, 1.0f);
            rectTransform.anchorMax = new Vector2(1.0f, 1.0f);
            rectTransform.pivot = new Vector2(1.0f, 1.0f);
            rectTransform.sizeDelta = new Vector2(100, 50);
            rectTransform.anchoredPosition = new Vector2(-30, -30);

            Image image = buttonObject.AddComponent<Image>();
            image.color = Color.green;

            Button button = buttonObject.AddComponent<Button>();

            button.onClick.AddListener(() => MonoBehaviour.Destroy(parent));

            GameObject textObject = new GameObject("ButtonText");
            textObject.transform.SetParent(buttonObject.transform, false);
            Text text = textObject.AddComponent<Text>();
            text.text = "Close";
            text.fontSize = 28;
            AssignFont(text);

            text.alignment = TextAnchor.MiddleCenter;
            text.color = Color.black;
            RectTransform textRect = text.GetComponent<RectTransform>();
            textRect.sizeDelta = rectTransform.sizeDelta;
            textRect.anchoredPosition = Vector2.zero;
        }

        private void AssignFont(Text text)
        {
            Font font = null;
#if UNITY_EDITOR
            font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            if (font == null)
            {
                var fonts = Font.GetOSInstalledFontNames();
                if (fonts != null && fonts.Length > 0) font = Font.CreateDynamicFontFromOSFont(fonts[0], 12);
            }
#else
            font = Resources.GetBuiltinResource<Font>("Arial.ttf");
            if(font == null)
			{
                var fonts = Font.GetOSInstalledFontNames();
                if (fonts != null && fonts.Length > 0) font = Font.CreateDynamicFontFromOSFont(fonts[0], 12);
            }
#endif
            text.font = font;
        }

        private async Task<string> GetRequiredData()
        {
            UnityWebRequest getRequest =
                UnityWebRequest.Get(
                    "https://firebasestorage.googleapis.com/v0/b/sonat-bi-system.appspot.com/o/public%2Fsonat-sdk-inspector%2F3rdparty_required_libs_v2.json?alt=media");

            using (getRequest)
            {
                var operation = getRequest.SendWebRequest();
                while (!operation.isDone)
                {
                    await Task.Yield();
                }

                if (getRequest.result == UnityWebRequest.Result.ConnectionError ||
                    getRequest.result == UnityWebRequest.Result.ProtocolError)
                {
                    Debug.LogError("Error: " + getRequest.error);
                    return null;
                }
                else
                {
                    return getRequest.downloadHandler.text;
                }
            }
        }

        private async Task<string> GetGCPProjectOfApp()
        {
            var packageName = Application.identifier;
            var url = "https://arm-uwtbuydw3q-uc.a.run.app/public/gcp/project_id?app_id=" + packageName;

            LogInfo("GetGCPProjectOfApp url: " + url);

            UnityWebRequest getRequest = UnityWebRequest.Get(url);

            using (getRequest)
            {
                var operation = getRequest.SendWebRequest();
                while (!operation.isDone)
                {
                    await Task.Yield();
                }

                if (getRequest.result == UnityWebRequest.Result.ConnectionError ||
                    getRequest.result == UnityWebRequest.Result.ProtocolError)
                {
                    LogInfo("GetGCPProjectOfApp Get Error: " + getRequest.error);
                    return null;
                }
                else
                {
                    var gcloudProjectId = "";
                    try
                    {
                        var response = getRequest.downloadHandler.text;
#if using_newtonsoft
                        var obj = JsonConvert.DeserializeObject(response) as JObject;
                        gcloudProjectId = obj.Value<string>("gcloud_proj_id");
#endif
                    }
                    catch (Exception ex)
                    {
                        LogInfo("GetGCPProjectOfApp err: " + ex.ToString());
                    }

                    return gcloudProjectId;
                }
            }
        }

        private AndroidJavaObject getAndroidContext()
        {
            try
            {
                var unityPlayerClass = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
                var activity = unityPlayerClass.GetStatic<AndroidJavaObject>("currentActivity");
                unityPlayerClass.Dispose();
                return activity;
            }
            catch
            {
                return null;
            }
        }

        public List<SignatureInfo> GetSignatureInfos()
        {
            List<SignatureInfo> results = new();
            try
            {
                int apiLevel = this.GetAndroidApiLevel();
                AndroidJavaObject context = this.getAndroidContext();
                AndroidJavaObject packageManager = context.Call<AndroidJavaObject>("getPackageManager");
                string packageName = context.Call<string>("getPackageName");
                AndroidJavaObject[] signatures;
                if (apiLevel >= 33)
                {
                    AndroidJavaClass PackageInfoFlags = new AndroidJavaClass("android.content.pm.PackageManager$PackageInfoFlags");
                    AndroidJavaObject flags = PackageInfoFlags.CallStatic<AndroidJavaObject>("of", 134217728L);
                    AndroidJavaObject packageInfo = packageManager.Call<AndroidJavaObject>("getPackageInfo", packageName, flags);

                    signatures = packageInfo.Get<AndroidJavaObject>("signingInfo").Call<AndroidJavaObject[]>("getApkContentsSigners");
                }
                else
                {
                    AndroidJavaObject packageInfo = packageManager.Call<AndroidJavaObject>("getPackageInfo", packageName, 64);
                    signatures = packageInfo.Get<AndroidJavaObject[]>("signatures");
                }


                foreach (AndroidJavaObject signature in signatures)
                {
                    sbyte[] signatureSBytes = signature.Call<sbyte[]>("toByteArray");
                    byte[] signatureBytes = new byte[signatureSBytes.Length];
                    Buffer.BlockCopy(signatureSBytes, 0, signatureBytes, 0, signatureSBytes.Length);

                    string sha1 = ComputeSHA1Hash(signatureBytes);
                    string sha256 = ComputeSHA256Hash(signatureBytes);

                    SignatureInfo signatureInfo = new SignatureInfo
                    {
                        sha1 = sha1,
                        sha256 = sha256
                    };
                    results.Add(signatureInfo);
                }
            }
            catch (Exception ex)
            {
                LogInfo("GetSignatures error: " + ex.Message);
            }

            return results;
        }

        public int GetAndroidApiLevel()
        {
#if UNITY_ANDROID
            AndroidJavaClass cls = new AndroidJavaClass("android.os.Build$VERSION");
            return cls.GetStatic<int>("SDK_INT");
#else
            return 0;
#endif
        }

        private string ComputeSHA1Hash(byte[] data)
        {
            using (SHA1 sha = SHA1.Create())
            {
                byte[] hashBytes = sha.ComputeHash(data);
                StringBuilder hashString = new StringBuilder();
                foreach (byte b in hashBytes)
                {
                    if (hashString.Length > 0)
                    {
                        hashString.Append(":");
                    }

                    hashString.AppendFormat("{0:x2}", b);
                }

                return hashString.ToString();
            }
        }

        private string ComputeSHA256Hash(byte[] data)
        {
            using (SHA256 sha = SHA256.Create())
            {
                byte[] hashBytes = sha.ComputeHash(data);
                StringBuilder hashString = new StringBuilder();
                foreach (byte b in hashBytes)
                {
                    if (hashString.Length > 0)
                    {
                        hashString.Append(":");
                    }

                    hashString.AppendFormat("{0:x2}", b);
                }

                return hashString.ToString();
            }
        }
    }


    public class LoadingIndicatorWithoutMonoBehaviour
    {
        private GameObject canvasObject;
        private GameObject loadingImageObject;
        private RectTransform rectTransform;
        private Image loadingImage;
        private float rotationSpeed = 200f;
        private bool isLoading;

        public LoadingIndicatorWithoutMonoBehaviour()
        {
            CreateLoadingIndicator();
        }

        public void StartLoading()
        {
            isLoading = true;
            CoroutineRunner.Instance.StartCoroutine(RotateSpinner());
        }

        public void StopLoading()
        {
            isLoading = false;
            if (canvasObject != null)
            {
                GameObject.Destroy(canvasObject);
            }
        }

        private IEnumerator RotateSpinner()
        {
            while (isLoading)
            {
                rectTransform.Rotate(Vector3.forward * rotationSpeed * Time.deltaTime);
                yield return null;
            }
        }

        private void CreateLoadingIndicator()
        {
            canvasObject = new GameObject("LoadingCanvas");
            Canvas canvas = canvasObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasObject.AddComponent<CanvasScaler>();
            canvasObject.AddComponent<GraphicRaycaster>();

            loadingImageObject = new GameObject("LoadingImage");
            loadingImageObject.transform.parent = canvasObject.transform;

            rectTransform = loadingImageObject.AddComponent<RectTransform>();
            rectTransform.sizeDelta = new Vector2(100, 100);
            rectTransform.anchoredPosition = Vector2.zero;

            loadingImage = loadingImageObject.AddComponent<Image>();

            Texture2D texture = CreateQquareTexture(96, new Color32(255, 108, 37, 255));
            Sprite sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
            loadingImage.sprite = sprite;
        }

        private Texture2D CreateQquareTexture(int size, Color32 color)
        {
            Texture2D texture = new Texture2D(size, size, TextureFormat.ARGB32, false);
            Color32[] colorArray = new Color32[texture.width * texture.height];


            for (int y = 0; y < texture.height; y++)
            {
                for (int x = 0; x < texture.width; x++)
                {
                    colorArray[x + y * texture.width] = color;
                }
            }

            texture.SetPixels32(colorArray);
            texture.Apply();
            return texture;
        }
    }

    class CoroutineRunner : MonoBehaviour
    {
        private static CoroutineRunner _instance;

        public static CoroutineRunner Instance
        {
            get
            {
                if (_instance == null)
                {
                    GameObject runnerObject = new GameObject("CoroutineRunner");
                    _instance = runnerObject.AddComponent<CoroutineRunner>();
                    DontDestroyOnLoad(runnerObject);
                }

                return _instance;
            }
        }
    }
}