using Cysharp.Threading.Tasks;
using UnityEngine;

namespace SonatFramework.Scripts.UIModule
{
    public class UITweenElement : MonoBehaviour
    {
        public TweenData tweenData;
        public bool playOnAwake = true;


        private void OnEnable()
        {
            if (tweenData.target == null) tweenData.target = transform;
            if (playOnAwake) Play();
        }


        public void Play()
        {
            UITween.Play(tweenData).Forget();
        }
    }
}