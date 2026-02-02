using System;
using Cysharp.Threading.Tasks;
using SonatFramework.Scripts.Utils;
using SonatFramework.Systems.ObjectPooling;
using UnityEngine;

namespace SonatFramework.Scripts.UIModule.UIElements
{
    public class UIEffect : MonoBehaviour, IPoolingObject
    {
        [SerializeField] private TweenData[] tweenData;
        [SerializeField] private bool playOnAwake = true;
        private Action callback;
        public Transform Transform => Transform;

        public virtual void Setup()
        {
        }

        public virtual void OnCreateObj(params object[] args)
        {
            if (playOnAwake) PlayEffect();
        }

        public virtual void OnReturnObj()
        {
            callback = null;
        }

        public virtual void Setup(Action callback)
        {
            this.callback = callback;
        }

        public virtual void PlayEffect()
        {
            SonatUtils.PlayTweens(tweenData, callback).Forget();
        }
    }
}