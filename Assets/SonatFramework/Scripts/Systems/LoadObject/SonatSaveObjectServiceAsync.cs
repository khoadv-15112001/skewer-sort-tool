using System.IO;
using Cysharp.Threading.Tasks;
using Newtonsoft.Json;
using UnityEditor;
using UnityEngine;

namespace SonatFramework.Systems.LoadObject
{
    [CreateAssetMenu(fileName = "Save Folder Async",
        menuName = "Sonat Services/Save Service/Sonat Save Folder Async")]
    public class SonatSaveObjectServiceAsync : SaveObjectServiceAsync
    {
        private readonly JsonSerializerSettings settings = new() { TypeNameHandling = TypeNameHandling.Auto };

        public override async UniTask<T> SaveObject<T>(T data, string fileName)
        {
            var json = JsonConvert.SerializeObject(data, settings);

            if (Directory.Exists(path) == false) Directory.CreateDirectory(path);
            var fullPath = string.IsNullOrEmpty(path) ? fileName : $"{path}/{fileName}";
            if (!fullPath.EndsWith(".json")) fullPath += ".json";
            await File.WriteAllTextAsync(fullPath, json);
#if UNITY_EDITOR
            AssetDatabase.Refresh();
#endif
            return data;
        }
        
    }
}