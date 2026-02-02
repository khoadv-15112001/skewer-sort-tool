using System.IO;
using UnityEngine;
using UnityEngine.Networking;
using Cysharp.Threading.Tasks;
using System.ComponentModel;
using Sirenix.OdinInspector;
using SonatFramework.Scripts.UIModule;
using System;

namespace GrillSort.Story
{
    [CreateAssetMenu(fileName = "StoryServer", menuName = "My Services/StoryServer")]
    public class StoryServer : ScriptableObject
    {
        [SerializeField] private string endpoint_Android;
        [SerializeField] private string endpoint_IOS;

        [SerializeField] private string endpoint_Test;
        private const string URL_SERVER = "https://s3.sonatgame.com/browser";

        private const string VIDEO_FOLDER = "StoryVideos";

        public bool enableLog = true;
        public bool isTest;

        /// <summary>
        /// Download video từ URL và lưu vào persistentDataPath.
        /// Nếu file đã tồn tại hoặc giống file tử server, sẽ bỏ qua download.
        /// </summary>
        public async UniTask<string> DownloadVideoAsync(string url)
        {
            url = GetEndpoint() + url;

            string fileName = Path.GetFileName(url);
            string saveDir = Path.Combine(Application.persistentDataPath, VIDEO_FOLDER);
            string savePath = Path.Combine(saveDir, fileName);
            string tempPath = savePath + ".tmp";

            if (!Directory.Exists(saveDir))
                Directory.CreateDirectory(saveDir);

            // --- Kiểm tra file có tồn tại không ---
            if (File.Exists(savePath))
            {
                bool needRedownload = await CheckFileChangedAsync(url, savePath);
                if (!needRedownload)
                {
                    Log($"Video đã có sẵn và không thay đổi: {savePath}");
                    return savePath;
                }

                Log($"File đã thay đổi trên server, sẽ tải lại: {fileName}");
                File.Delete(savePath);
            }

            Log($"Bắt đầu tải video từ: {url}");

            // --- Tải xuống file tạm ---
            if (File.Exists(tempPath))
                File.Delete(tempPath);

            using (UnityWebRequest uwr = UnityWebRequest.Get(url))
            {
                uwr.downloadHandler = new DownloadHandlerFile(tempPath);

                try
                {
                    await uwr.SendWebRequest();
                }
                catch (System.Exception e)
                {
                    LogError($"Exception khi tải video: {e.Message}");
                    if (File.Exists(tempPath))
                        File.Delete(tempPath);
                    return null;
                }

                if (!IsHttpSuccess(uwr))
                {
                    LogError($"Lỗi tải video: {uwr.error} (HTTP {uwr.responseCode})");
                    if (File.Exists(tempPath))
                        File.Delete(tempPath);
                    return null;
                }

                // ✅ Ghi lại Last-Modified sau khi tải lần đầu
                var headers = uwr.GetResponseHeaders();
                if (headers != null && headers.TryGetValue("Last-Modified", out string serverModified))
                {
                    string localMetaPath = savePath + ".meta.txt";
                    File.WriteAllText(localMetaPath, serverModified);
                    Log($"Ghi metadata Last-Modified: {serverModified}");
                }
            }


            // --- Kiểm tra file tạm hợp lệ rồi đổi tên ---
            if (new FileInfo(tempPath).Length > 0)
            {
                File.Move(tempPath, savePath);
                Log($"Tải và lưu video thành công: {savePath}");
                return savePath;
            }
            else
            {
                LogError("File tải về rỗng hoặc hỏng, sẽ xóa file tạm.");
                File.Delete(tempPath);
                return null;
            }
        }

        /// <summary>
        /// Lấy đường dẫn video đã lưu trong máy (nếu có).
        /// </summary>
        public async UniTask<string> GetLocalVideoPath(string fileName, bool isCheating = false)
        {
            string savePath = Path.Combine(Application.persistentDataPath, VIDEO_FOLDER, fileName);

            if (!isCheating)
            {
                if (File.Exists(savePath))
                {
                    Log($"Tìm thấy video local: {savePath}");
                    return savePath;
                }

                LogError($"Không tìm thấy video: {fileName}");
            }

            BlockPanel.Set(true);

            string resultPath = null;

            try
            {
                // Timeout 30 giây — có thể chỉnh tùy nhu cầu
                resultPath = await DownloadVideoAsync(fileName)
                    .Timeout(TimeSpan.FromSeconds(5));
            }
            catch (TimeoutException)
            {
                LogError($"Tải video {fileName} bị timeout sau 5 giây");
            }
            catch (System.Exception e)
            {
                LogError($"Lỗi khi tải video {fileName}: {e.Message}");
            }
            finally
            {
                BlockPanel.Set(false);
            }

            if (string.IsNullOrEmpty(resultPath))
            {
                LogError($"Không thể tải video: {fileName}");
                return null;
            }

            Log($"Đã tải video thành công: {resultPath}");
            return resultPath;
        }

        private string GetEndpoint()
        {
            if (isTest) return endpoint_Test;

#if UNITY_ANDROID
            return endpoint_Android;
#elif UNITY_IOS
            return endpoint_IOS;
#else
            return endpoint_Android;
#endif
        }

        /// <summary>
        /// Kiểm tra xem file local có khác server không.
        /// So sánh bằng Content-Length hoặc Last-Modified.
        /// </summary>
        private async UniTask<bool> CheckFileChangedAsync(string url, string localPath)
        {
            long localSize = new FileInfo(localPath).Length;

            using (UnityWebRequest headRequest = UnityWebRequest.Head(url))
            {
                try
                {
                    await headRequest.SendWebRequest();
                }
                catch (System.Exception e)
                {
                    LogError($"Không thể kiểm tra file từ server: {e}");
                    return true;
                }

                if (!IsHttpSuccess(headRequest))
                {
                    LogError($"Không thể kiểm tra file từ server: {headRequest.error}");
                    return true;
                }

                //foreach (var request in headRequest.GetResponseHeaders())
                //{
                //    Debug.LogError(request.Key + ": " + request.Value);
                //}

                if (headRequest.GetResponseHeaders().TryGetValue("Content-Length", out string lengthStr) &&
                    long.TryParse(lengthStr, out long serverSize))
                {
                    if (serverSize != localSize)
                    {
                        return true;
                    }
                }

                if (headRequest.GetResponseHeaders().TryGetValue("Last-Modified", out string serverModified))
                {
                    string localMetaPath = localPath + ".meta.txt";

                    if (File.Exists(localMetaPath))
                    {
                        string localModified = File.ReadAllText(localMetaPath);
                        if (localModified != serverModified)
                        {
                            File.WriteAllText(localMetaPath, serverModified);
                            return true;
                        }
                    }
                    else
                    {
                        File.WriteAllText(localMetaPath, serverModified);
                    }
                }
            }

            return false;
        }

        private static bool IsHttpSuccess(UnityWebRequest uwr)
        {
            return uwr.result == UnityWebRequest.Result.Success &&
                   uwr.responseCode >= 200 &&
                   uwr.responseCode < 300;
        }

        // -------------------------------------------------------------
        // LOG HELPERS
        // -------------------------------------------------------------

        private void Log(string message)
        {
            if (enableLog)
                Debug.Log($"[StoryServer] {message}");
        }

        private void LogError(string message)
        {
            if (enableLog)
                Debug.LogError($"[StoryServer] {message}");
        }

        [Button]
        public void OpenServer()
        {
            Application.OpenURL(URL_SERVER);
        }
    }
}
