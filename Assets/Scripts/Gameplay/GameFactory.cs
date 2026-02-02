using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using SonatFramework.Systems;
using SonatFramework.Systems.ObjectPooling;
using UnityEngine;

namespace Gameplay
{
    public class GameFactory : MonoBehaviour
    {
        [SerializeField] private SonatPoolingService poolingService;
        [SerializeField] private SonatPoolingServiceAsync poolingServiceAsync;
        private static SonatPoolingService poolingServiceInstance;
        private static SonatPoolingServiceAsync poolingServiceInstanceAsync;

        private static List<IPoolingObject> spawnedObjectsAsync = new List<IPoolingObject>();

        private void Awake()
        {
            // if (poolingServiceInstance != null)
            // {
            //     Destroy(poolingServiceInstance);
            // }
            //
            // poolingServiceInstance = Instantiate(poolingService);
            // poolingServiceInstance.InitializePool(this.gameObject);
            //
            // if (poolingServiceInstanceAsync != null)
            // {
            //     Destroy(poolingServiceInstanceAsync);
            // }
            // poolingServiceInstanceAsync = Instantiate(poolingServiceAsync);
            // poolingServiceInstanceAsync.InitializePool(this.gameObject);
            if (poolingServiceInstance == null)
                poolingServiceInstance = Instantiate(poolingService);
            if (poolingServiceInstanceAsync == null)
                poolingServiceInstanceAsync = Instantiate(poolingServiceAsync);
        }

        // private void OnDestroy()
        // {
        //     if (poolingServiceInstance != null)
        //     {
        //         Destroy(poolingServiceInstance);
        //     }
        // }

        public static T CreateEntity<T>(string name) where T : IPoolingObject
        {
            return poolingServiceInstance.Create<T>(name);
        }

        public static T CreateEntity<T>(string name, Transform parent) where T : IPoolingObject
        {
            return poolingServiceInstance.Create<T>(name, parent: parent);
        }

        public static T CreateEntity<T>(string name, Vector3 position) where T : IPoolingObject
        {
            return poolingServiceInstance.Create<T>(name, position);
        }

        public static T CreateEntity<T>(string name, Vector3 position, Transform parent) where T : IPoolingObject
        {
            return poolingServiceInstance.Create<T>(name, position, parent);
        }

        public static void ReturnEntity(IPoolingObject entity)
        {
            if (spawnedObjectsAsync.Contains(entity))
            {
                spawnedObjectsAsync.Remove(entity);
                poolingServiceInstanceAsync.ReturnObj(entity);
            }
            else
            {
                poolingServiceInstance.ReturnObj(entity);
            }
        }

        public static async UniTask<T> CreateEntityAsync<T>(string name) where T : IPoolingObject
        {
            var obj = await poolingServiceInstanceAsync.CreateAsync<T>(name);
            spawnedObjectsAsync.Add(obj);
            return obj;
        }

        public static async UniTask<T> CreateEntityAsync<T>(string name, Vector3 position) where T : IPoolingObject
        {
            var obj = await poolingServiceInstanceAsync.CreateAsync<T>(name, position);
            spawnedObjectsAsync.Add(obj);
            return obj;
        }

        public static async UniTask<T> CreateEntityAsync<T>(string name, Vector3 position, Transform parent) where T : IPoolingObject
        {
            var obj = await poolingServiceInstanceAsync.CreateAsync<T>(name, position, parent);
            spawnedObjectsAsync.Add(obj);
            return obj;
        }
    }
}