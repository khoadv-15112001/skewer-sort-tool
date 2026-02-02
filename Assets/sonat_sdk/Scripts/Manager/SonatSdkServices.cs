using System;
using System.Collections.Generic;
using Sonat.Debugger;
using Sonat.FirebaseModule;
using UnityEngine;
using UnityEngine.Serialization;

namespace Sonat
{
    [CreateAssetMenu(menuName = "SonatSDK/Create SonatSdk Services", fileName = "SonatSdkServices")]
    public class SonatSdkServices : ScriptableObject
    {
        [FormerlySerializedAs("firebaseService")] [SerializeField]
        private SonatFirebase sonatFirebase;

        [SerializeField] private List<SonatService> services;
        private Dictionary<Type, SonatService> serviceDictionary = new Dictionary<Type, SonatService>();
        public bool allServicesInitialized;
        private int servicesReadyCount = 0;

        public void Initialize()
        {
            allServicesInitialized = false;
            servicesReadyCount = 0;
            serviceDictionary = new();
#if UNITY_EDITOR
            var firebaseService = Instantiate(sonatFirebase);
#else
            var firebaseService = sonatFirebase;
#endif
            serviceDictionary.Add(typeof(SonatFirebase), firebaseService);
            firebaseService.Initialize(OnFirebaseInitialize);
        }

        private void OnFirebaseInitialize(ISonatService serviceInited)
        {
            SonatDebugType.Common.Log($"{serviceInited.ServiceType} initialize successfully");
            foreach (var _service in services)
            {
#if UNITY_EDITOR
                var service = Instantiate(_service);
#else
                var service = _service;
#endif
                serviceDictionary.Add(service.GetType(), service);
                service.Initialize(OnServiceInitialized);
            }
        }

        private void OnServiceInitialized(ISonatService serviceInited)
        {
            SonatDebugType.Common.Log($"{serviceInited.ServiceType} initialize successfully");
            servicesReadyCount++;
            if (servicesReadyCount == services.Count)
            {
                allServicesInitialized = true;
            }
        }

        public T GetService<T>() where T : SonatService
        {
            if (serviceDictionary != null)
            {
                return serviceDictionary.TryGetValue(typeof(T), out var service) ? service as T : null;
            }
            else
            {
                return services.Find(e => e.GetType() == typeof(T)) as T;
            }
        }

        public bool HasService(SonatService service)
        {
            return services.Contains(service);
        }

        public void TryAddService(SonatService service)
        {
            if (services.Contains(service)) return;
            services.Add(service);
        }

        public void TryRemoveService(SonatService service)
        {
            if (!services.Contains(service)) return;
            services.Remove(service);
        }

        public void OnApplicationFocus(bool focus)
        {
            foreach (var service in serviceDictionary.Values)
            {
                service.OnApplicationFocus(focus);
            }
        }

        public void OnApplicationPause(bool pause)
        {
            foreach (var service in serviceDictionary.Values)
            {
                service.OnApplicationPause(pause);
            }
        }

        public void OnApplicationQuit()
        {
            foreach (var service in serviceDictionary.Values)
            {
                service.OnApplicationQuit();
            }
        }
    }
}