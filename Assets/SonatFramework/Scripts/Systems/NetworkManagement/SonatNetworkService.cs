using System;
using System.Text;
using Cysharp.Threading.Tasks;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.Networking;

namespace SonatFramework.Systems.NetworkManagement
{
    [CreateAssetMenu(fileName = "SonatNetworkService", menuName = "Sonat Services/Network Service")]
    public class SonatNetworkService : NetworkService
    {
        private const int TIMEOUT_SECONDS = 30;
        private readonly string token = "";

        public override bool IsInternetConnection()
        {
            return Application.internetReachability != NetworkReachability.NotReachable;
        }

        public override UniTask<T> Get<T>(string endpoint)
        {
            return SendRequest<T>(endpoint, "GET");
        }

        public override UniTask<T> Post<T>(string endpoint, object payload)
        {
            return SendRequest<T>(endpoint, "POST", payload);
        }

        public override UniTask<T> Patch<T>(string endpoint, object payload)
        {
            return SendRequest<T>(endpoint, "PATCH", payload);
        }

        public override UniTask<T> Put<T>(string endpoint, object payload)
        {
            return SendRequest<T>(endpoint, "PUT", payload);
        }

        public override UniTask<T> Delete<T>(string endpoint, object payload)
        {
            return SendRequest<T>(endpoint, "DELETE", payload);
        }

        public override async UniTask<Texture> DownloadImage(string mediaUrl)
        {
            using var request = UnityWebRequestTexture.GetTexture(mediaUrl);
            request.timeout = TIMEOUT_SECONDS;
            try
            {
                if (!string.IsNullOrEmpty(token))
                    request.SetRequestHeader("Authorization", $"Bearer {token}");
                await request.SendWebRequest();
                ValidateResponse(request);
                return ((DownloadHandlerTexture)request.downloadHandler).texture;
            }
            finally
            {
                request.Dispose();
            }
        }

        private async UniTask<T> SendRequest<T>(string endpoint, string method, object payload = null)
        {
            using var request = CreateRequest(endpoint, method, payload);

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

        private UnityWebRequest CreateRequest(string endpoint, string method, object payload = null)
        {
            var request = new UnityWebRequest(endpoint, method);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.timeout = TIMEOUT_SECONDS;
            request.SetRequestHeader("Content-Type", "application/json");
            if (!string.IsNullOrEmpty(token))
                request.SetRequestHeader("Authorization", $"Bearer {token}");

            if (payload != null)
            {
                var json = JsonConvert.SerializeObject(payload);
                var bodyRaw = Encoding.UTF8.GetBytes(json);
                request.uploadHandler = new UploadHandlerRaw(bodyRaw);
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
    }
}