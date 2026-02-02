using System.IO;
using Cysharp.Threading.Tasks;
using Newtonsoft.Json;
using UnityEngine;

namespace SonatFramework.Systems.LoadObject
{
    [CreateAssetMenu(menuName = "Sonat Services/Load Service/Load Folder Async", fileName = "Load Folder Async")]
    public class SonatLoadFolderAsync : LoadObjectServiceAsync
    {
        public override async UniTask<T> LoadAsync<T>(string assetPath) where T : class
        {
            string fullPath = string.IsNullOrEmpty(path) ? assetPath : $"{path}/{assetPath}";
            if (!fullPath.EndsWith(".json")) fullPath += ".json";
            var data = await File.ReadAllTextAsync(fullPath);
            return JsonConvert.DeserializeObject<T>(data, Settings);
        }
    }
}