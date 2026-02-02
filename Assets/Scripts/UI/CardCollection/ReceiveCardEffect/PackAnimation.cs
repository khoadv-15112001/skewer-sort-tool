using System;
using SonatFramework.Scripts.Utils;
using Spine.Unity;
using UnityEngine;

namespace MyGame.Modules.CardCollection.Animation
{
    [RequireComponent(typeof(SkeletonGraphic))]
    public class PackAnimation : MonoBehaviour
    {
        [SerializeField] private string[] skins;
        [SerializeField] private string animAppear = "Appear";

        private SkeletonGraphic _packAnim;

        private void Awake()
        {
            GetAnimation();
        }

        private void GetAnimation()
        {
            _packAnim = GetComponent<SkeletonGraphic>();
        }

        public void Play(int skinIdx, bool loop, Action onComplete, float delay = 0.5f, bool forceHideAnim = false)
        {
            if (_packAnim == null) GetAnimation();

            _packAnim.AnimationState.ClearTracks();
            _packAnim.Skeleton.SetSkin(skins[skinIdx]);
            _packAnim.AnimationState.SetAnimation(0, animAppear, loop).Complete += (track) =>
            {
                _packAnim.gameObject.SetActive(false);
            };

            SonatUtils.DelayCall(delay, () =>
            {
                if (forceHideAnim && _packAnim.gameObject.activeSelf)
                {
                    _packAnim.gameObject.SetActive(false);
                }
                onComplete?.Invoke();
            });
        }
    }
}
