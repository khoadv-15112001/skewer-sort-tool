using Newtonsoft.Json;
using UnityEngine;

namespace SonatFramework.Systems.LoadObject
{
    [CreateAssetMenu(menuName = "Sonat Services/Load Service/Load Resources", fileName = "Load Resources")]
    public class SonatLoadResources : LoadObjectService
    {
        public override T LoadObject<T>(string assetName) where T : class
        {
            string fullPath = string.IsNullOrEmpty(path) ? assetName : $"{path}/{assetName}";
            if (fullPath.EndsWith(".json")) fullPath = fullPath.Replace(".json", "");
            var obj = Resources.Load(fullPath);
            if (obj == null) return null;
            if (obj is TextAsset textAsset)
            {
                return string.IsNullOrEmpty(textAsset.text) ? null : JsonConvert.DeserializeObject<T>(textAsset.text, Settings);
            }
            else
            {
                return obj as T;
            }
        }
    }
}