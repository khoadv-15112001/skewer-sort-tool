using System.IO;
using Newtonsoft.Json;
using UnityEngine;

namespace SonatFramework.Systems.LoadObject
{
    [CreateAssetMenu(menuName = "Sonat Services/Load Service/Load Folder", fileName = "Load Folder")]
    public class SonatLoadFolder : LoadObjectService
    {
        public override T LoadObject<T>(string assetPath) where T : class
        {
            var fullPath = string.IsNullOrEmpty(path) ? assetPath : $"{path}/{assetPath}";
            if (!fullPath.EndsWith(".json")) fullPath += ".json";
            var data = File.ReadAllText(fullPath);
            if (string.IsNullOrEmpty(data)) return null;
            return JsonConvert.DeserializeObject<T>(data, Settings);
        }
    }
}