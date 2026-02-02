using UnityEngine;

namespace SonatFramework.Systems.ObjectPooling
{
    public interface IPoolingObject
    {
        public Transform transform { get; }
        public void Setup();
        public void OnCreateObj(params object[] args);
        public void OnReturnObj();
    }
}