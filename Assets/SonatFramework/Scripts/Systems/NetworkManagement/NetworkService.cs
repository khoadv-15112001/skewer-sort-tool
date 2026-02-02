using Cysharp.Threading.Tasks;
using UnityEngine;

namespace SonatFramework.Systems.NetworkManagement
{
    public abstract class NetworkService : SonatServiceSo
    {
        public abstract bool IsInternetConnection();
        public abstract UniTask<T> Get<T>(string endpoint);
        public abstract UniTask<T> Post<T>(string endpoint, object payload);
        public abstract UniTask<T> Patch<T>(string endpoint, object payload);
        public abstract UniTask<T> Put<T>(string endpoint, object payload);
        public abstract UniTask<T> Delete<T>(string endpoint, object payload);

        public abstract UniTask<Texture> DownloadImage(string mediaUrl);
    }

    public enum RequestType
    {
        GET = 0,
        POST = 1,
        PATCH = 2,
        PUT = 3,
        DELETE = 4
    }


    public enum HTTPStatus
    {
        OK = 200,
        Unauthorized = 401,
        NotFound = 404
    }
}