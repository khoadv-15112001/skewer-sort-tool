using UnityEngine;

namespace SonatFramework.Systems.ObjectPooling
{
    public abstract class PoolingContainerService: SonatServiceSo
    {
        public abstract T CreateObject<T>(Transform container);
        public abstract void CleanContainer(Transform container);
    }
}