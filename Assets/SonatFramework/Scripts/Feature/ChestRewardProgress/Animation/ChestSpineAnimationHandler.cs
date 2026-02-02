using System;
using DG.Tweening;
using Spine.Unity;
using UnityEngine;

namespace SonatFramework.Scripts.Feature.ChestRewardProgress
{
    [RequireComponent(typeof(SkeletonGraphic))]
    public class ChestSpineUIAnimationHandler : ChestAnimationInterface
    {
        [Header("Animation Names")]
        [SpineAnimation]
        [SerializeField]
        private string idleAnimation = "idle";

        [SpineAnimation][SerializeField] private string openAnimation = "open";

        [SpineAnimation][SerializeField] private string idleOpenAnimation = "idleOpen";

        [Header("Skins")]
        [SpineSkin][SerializeField] private string lockedSkin = "locked";

        [SpineSkin][SerializeField] private string openingSkin = "opened";

        [SpineSkin][SerializeField] private string openedSkin = "opened";

        private RectTransform chestRect;

        [Header("Components")] private SkeletonGraphic skeletonGraphic;

        private void Awake()
        {
            if (skeletonGraphic == null)
                skeletonGraphic = GetComponent<SkeletonGraphic>();
            if (chestRect == null)
                chestRect = GetComponent<RectTransform>();
        }

        public override void PlayIdleAnimation()
        {
            if (skeletonGraphic)
            {
                SetChestState(ChestState.Locked);
                skeletonGraphic.AnimationState.SetAnimation(0, idleAnimation, true);
            }
        }

        public override void PlayIdleOpenAnimation()
        {
                Debug.Log("PlayIdleOpenAnimation");
            if (skeletonGraphic)
            {
                SetChestState(ChestState.Opened);
                skeletonGraphic.AnimationState.SetAnimation(0, idleOpenAnimation, true);
            }
        }

        public override void PlayOpenAnimation(Action onComplete = null)
        {
            if (skeletonGraphic)
            {
                SetChestState(ChestState.Opening);
                var trackEntry = skeletonGraphic.AnimationState.SetAnimation(0, openAnimation, false);
                trackEntry.Complete += trackEntry =>
                {
                    onComplete?.Invoke();
                    PlayIdleOpenAnimation();
                };
            }
        }

        public override void SetChestState(ChestState state)
        {
            if (skeletonGraphic == null || state == ChestState.None) return;

            var targetSkin = state switch
            {
                ChestState.Locked => lockedSkin,
                ChestState.Opening => openingSkin,
                ChestState.Opened => openedSkin,
                _ => lockedSkin
            };

            skeletonGraphic.Skeleton.SetSkin(targetSkin);
            skeletonGraphic.Skeleton.SetToSetupPose();
        }

        public override void Cleanup()
        {
            if (skeletonGraphic != null) skeletonGraphic.AnimationState.ClearTracks();
            DOTween.Kill(chestRect);
        }
    }
}