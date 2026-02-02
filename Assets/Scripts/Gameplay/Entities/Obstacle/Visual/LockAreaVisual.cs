using System;
using DG.Tweening;
using SonatFramework.Scripts.Utils;
using Spine.Unity;
using UnityEngine;

namespace Gameplay.Entities.Obstacle.Visual
{
    public class LockAreaVisual : MonoBehaviour
    {
        [SerializeField] private LockAreaObstacle obstacle;
        [SerializeField] private SpriteRenderer background;
        [SerializeField] private SkeletonAnimation skeletonAnimation;
        [SerializeField] private Transform lockPosition;

        public void Lock()
        {
            PlayAnimation("Idle", true, null);
            SetActiveBackground(true);
        }

        public void Unlock()
        {
            
            PlayAnimation("Destroy", false, () =>
            {
                obstacle.DestroyObstacle();
            });
            SetActiveBackground(false);
        }

        private void SetActiveBackground(bool active)
        {
            if (active)
            {
                background.SetAlpha(1);
            }
            else
            {
                background.DOFade(0, 0.7f);
            }
        }

        public Transform GetLockPosition()
        {
            return lockPosition;
        }

        private void PlayAnimation(string animationName, bool loop, Action _callback)
        {
            Action callback = _callback;
            skeletonAnimation.AnimationState.ClearTracks();
            skeletonAnimation.Initialize(true);
            if (loop)
            {
                skeletonAnimation.AnimationState.SetAnimation(0, animationName, true);
            }
            else
            {
                skeletonAnimation.AnimationState.SetAnimation(0, animationName, false).Complete += (track) =>
                {
                    callback?.Invoke();
                    callback = null;
                };
            }
        }
    }
}