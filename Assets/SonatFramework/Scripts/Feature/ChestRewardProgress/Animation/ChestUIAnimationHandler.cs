using System;
using DG.Tweening;
using UnityEngine;

namespace SonatFramework.Scripts.Feature.ChestRewardProgress
{
    public class ChestUIAnimationHandler : ChestAnimationInterface
    {
        [Header("Components")] [SerializeField]
        private Animator animator;

        [Header("Animation Parameters")] [SerializeField]
        private string idleAnimName = "Idle";

        [SerializeField] private string openAnimName = "Open";
        [SerializeField] private string idleOpenAnimName = "Open";
        private RectTransform chestRect;

        private void Awake()
        {
            if (animator == null) animator = GetComponent<Animator>();
            chestRect = GetComponent<RectTransform>();
        }

        public override void PlayIdleAnimation()
        {
            if (animator != null && animator.enabled) animator.Play(idleAnimName);
        }

        public override void PlayIdleOpenAnimation()
        {
            if (animator != null && animator.enabled) animator.Play(idleOpenAnimName);
        }


        public override void PlayOpenAnimation(Action onComplete = null)
        {
            if (animator != null && animator.enabled)
            {
                animator.Play(openAnimName);
                var animationLength = GetAnimationLength(openAnimName);
                DOVirtual.DelayedCall(animationLength, () => onComplete?.Invoke());
            }
        }

        public override void SetChestState(ChestState state)
        {
        }

        private float GetAnimationLength(string animationName)
        {
            if (animator == null) return 0f;

            var clips = animator.runtimeAnimatorController.animationClips;
            foreach (var clip in clips)
                if (clip.name == animationName)
                    return clip.length;
            return 0f;
        }

        public override void Cleanup()
        {
            DOTween.Kill(chestRect);
        }
    }
}