using System;
using Base.Singleton;
using Cysharp.Threading.Tasks;
using MyGame.SkewerJam.Objects.Entities;
using SonatFramework.Systems.ObjectPooling;
using UnityEngine;

namespace MyGame.SkewerJam.Gameplay
{
    public class GameFactory : Singleton<GameFactory>
    {
        // Start is called before the first frame update
        [SerializeField] private PoolingServiceAsync poolingService;
        private const string PREFAB_PREFIX = "_SkewerJam_";
        private const string PREFAB_SUFFIX = ".prefab";

        protected override void OnAwake()
        {
            poolingService.Initialize();
        }

        public async UniTask<T> CreateEntityAsync<T>(string name, Transform parent = null) where T : IPoolingObject
        {
            return await poolingService.CreateAsync<T>(PREFAB_PREFIX + name + PREFAB_SUFFIX, parent);
        }

        public async UniTask<T> CreateEntityAsync<T>(string name, Vector3 position, Transform parent = null) where T : IPoolingObject
        {
            return await poolingService.CreateAsync<T>(PREFAB_PREFIX + name + PREFAB_SUFFIX, position, parent);
        }

        public void ReturnEntity<T>(T order) where T : IPoolingObject
        {
            poolingService.ReturnObj(order, true);
        }
    }
}