using System;
using Cysharp.Threading.Tasks;
using Gameplay.Entities;
using SonatFramework.Systems.ObjectPooling;
using UnityEngine;

namespace MyGame.SkewerJam.Scripts.SO.Behavior
{
    public abstract class GameFactorySO : ScriptableObject
    {
        public abstract UniTask<T> CreateItem<T>(string name, Transform parent = null) where T : IPoolingObject;

        public abstract UniTask<T> CreateItem<T>(string name, Vector3 position, Transform parent = null) where T : IPoolingObject;

        public abstract void ReturnEntity<T>(T entity) where T : IPoolingObject;

    }
}