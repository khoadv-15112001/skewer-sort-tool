using System;
using Sirenix.OdinInspector;
using UnityEngine;

namespace SonatFramework.Systems.ConfigManagement
{
    [Serializable]
    public class Config<T> where T : class
    {
        [EnumToggleButtons] public LoadConfigFrom loadConfigFrom = LoadConfigFrom.Automatic;
        [ShowIf("@loadConfigFrom == LoadConfigFrom.Manual")] [SerializeField]
        private T manualConfig;

        private static T configCache;
        public T config => loadConfigFrom == LoadConfigFrom.Manual ? manualConfig : configCache ??= SonatSystem.GetConfig<T>();
    }

    public enum LoadConfigFrom : byte
    {
        Manual = 0,
        Automatic
    }
}