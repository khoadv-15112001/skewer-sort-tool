using System;
using Base.Singleton;
#if sonat_sdk_v2
using Sonat;
#endif
using SonatFramework.Systems.ConfigManagement;
using UnityEngine;

namespace SonatFramework.Systems
{
    public class SonatSystem : SingletonSimple<SonatSystem>
    {
        [SerializeField] protected SonatServicesManager serviceManager;
        [SerializeField] protected Service<ConfigService> configService = new();

        protected override void OnAwake()
        {
            DontDestroyOnLoad(gameObject);
            serviceManager.Resolve();
#if sonat_sdk_v2
            if (SonatSdkManager.ready)
            {
                OnSonatSDKInitialized();
                OnRemoteConfigReady();
            }
            else
            {
                SonatSdkManager.onInitializeComplete += OnSonatSDKInitialized;
                SonatSdkManager.onRemoteConfigReady += OnRemoteConfigReady;
            }
#endif
        }

        public static void ReInitializeAllServices()
        {
            Instance.serviceManager.Resolve();
        }

        public static T GetService<T>() where T : class
        {
            return Instance.serviceManager?.Get<T>();
        }

        public static T GetConfig<T>() where T : class
        {
            return Instance.configService.Instance.Get<T>();
        }

        private void OnRemoteConfigReady()
        {
            serviceManager.OnRemoteConfigReady();
        }

        private void OnSonatSDKInitialized()
        {
            serviceManager.OnSonatSdkInitialized();
        }

        private void OnApplicationFocus(bool hasFocus)
        {
            serviceManager.OnApplicationFocus(hasFocus);
        }
    }
}