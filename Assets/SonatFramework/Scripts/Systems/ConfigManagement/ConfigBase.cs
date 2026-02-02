using Sirenix.OdinInspector;
using System;
using UnityEngine;
using Newtonsoft.Json;

namespace SonatFramework.Systems.ConfigManagement
{
    public interface IConfig
    {
    }

    [Serializable]
    public class ConfigSo : ScriptableObject, IConfig
    {

        [Button("Print JSON Config")]
        private void PrintJsonConfig()
        {
            Debug.Log(JsonConvert.SerializeObject(this));
        }
    }


}