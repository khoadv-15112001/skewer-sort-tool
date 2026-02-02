using System.IO;
using Newtonsoft.Json;
using UnityEditor;
using UnityEngine;

namespace SonatFramework.Systems.LoadObject
{
    [CreateAssetMenu(fileName = "Save Folder",
        menuName = "Sonat Services/Save Service/Save Folder")]
    public class SonatSaveObjectService : SaveObjectService
    {
        private readonly JsonSerializerSettings settings = new() { TypeNameHandling = TypeNameHandling.Auto };

        public override void SaveObject<T>(T data, string fileName)
        {
            var json = JsonConvert.SerializeObject(data, settings);
            if (Directory.Exists(path) == false) Directory.CreateDirectory(path);
            var fullPath = string.IsNullOrEmpty(path) ? fileName : $"{path}/{fileName}";
            if (!fullPath.EndsWith(".json")) fullPath += ".json";
            File.WriteAllText(fullPath, json);
#if UNITY_EDITOR
            AssetDatabase.Refresh();
#endif
        }

        public void SaveTextFile(string data, string fileName)
        {
            if (Directory.Exists(path) == false) Directory.CreateDirectory(path);
            var fullPath = string.IsNullOrEmpty(path) ? fileName : $"{path}/{fileName}";
            if (!fullPath.EndsWith(".txt")) fullPath += ".txt";
            File.WriteAllText(fullPath, data);
#if UNITY_EDITOR
            AssetDatabase.Refresh();
#endif
        }
    }
}