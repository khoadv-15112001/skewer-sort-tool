using System;
using System.Collections;
using Sonat.Debugger;
using UnityEngine;

namespace Sonat
{
    public abstract class SonatService : ScriptableObject, ISonatService
    {
        public abstract SonatServiceType ServiceType { get; }
        public abstract bool Ready { get; set; }
        
        public Action<ISonatService> OnInitialized { get; set; }

        public virtual void Initialize(Action<ISonatService> onInitialized)
        {
            Ready = false;
            OnInitialized += onInitialized;
            SonatDebugType.Common.Log($"Start Initializing {ServiceType}");
        }
        

        public virtual void OnApplicationFocus(bool focus)
        {
        }

        public virtual void OnApplicationPause(bool pause)
        {
        }

        public virtual void OnApplicationQuit()
        {
        }

        protected Coroutine StartCoroutine(IEnumerator iEnumerator)
        {
           return SonatSdkManager.instance.StartCoroutine(iEnumerator);
        }
        protected Coroutine StartCoroutine(string coroutineName)
        {
            return SonatSdkManager.instance.StartCoroutine(coroutineName);
        }
        
        protected void StopCoroutine(string coroutineName)
        {
            SonatSdkManager.instance.StopCoroutine(coroutineName);
        }
        
        protected void StopCoroutine(Coroutine coroutine)
        {
            SonatSdkManager.instance.StopCoroutine(coroutine);
        }
    }
}