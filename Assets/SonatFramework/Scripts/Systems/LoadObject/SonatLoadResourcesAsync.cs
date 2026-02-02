using Cysharp.Threading.Tasks;
using Newtonsoft.Json;
using UnityEngine;

namespace SonatFramework.Systems.LoadObject
{
    [CreateAssetMenu(menuName = "Sonat Services/Load Service/Load Resources Async",
        fileName = "Load Resources Async")]
    public class SonatLoadResourcesAsync : LoadObjectServiceAsync
    {
        public override async UniTask<T> LoadAsync<T>(string assetPath) where T : class
        {
            var fullPath = string.IsNullOrEmpty(path) ? assetPath : $"{path}/{assetPath}";
            if (fullPath.EndsWith(".json")) fullPath = fullPath.Replace(".json", "");
            var data = await Resources.LoadAsync(fullPath);
            if (data is TextAsset textAsset) return JsonConvert.DeserializeObject<T>(textAsset.text, Settings);

            return data as T;
        }
    }
}