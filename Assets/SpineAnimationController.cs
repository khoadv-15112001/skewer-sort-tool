using System;
using UnityEngine;
using Spine;
using Spine.Unity;

public class SpineAnimationController : MonoBehaviour
{
    public SkeletonGraphic skeletonGraphic;

    public void PlayOnceThenLoop(AnimState animState, Action onOnceEnd = null)
    {
        if (skeletonGraphic == null)
        {
            Debug.LogError("SpineAnimationController: skeletonGraphic is null");
            return;
        }

        skeletonGraphic.AnimationState.ClearTracks();

        TrackEntry entry = skeletonGraphic.AnimationState.SetAnimation(0, animState.onceAnim, false);

        entry.Complete += (TrackEntry finishedEntry) =>
        {
            onOnceEnd?.Invoke();

            skeletonGraphic.AnimationState.SetAnimation(0, animState.loopAnim, true);
        };
    }

    public void Play(string animName, bool loop = false, Action onComplete = null)
    {
        if (skeletonGraphic == null)
        {
            Debug.LogError("SpineAnimationController: skeletonGraphic is null");
            return;
        }

        skeletonGraphic.AnimationState.ClearTracks();
        var entry = skeletonGraphic.AnimationState.SetAnimation(0, animName, loop);
        if (!loop && onComplete != null)
        {
            entry.Complete += (TrackEntry e) =>
            {
                onComplete();
            };
        }
    }

    internal void Play(object loopAnim, bool v)
    {
        throw new NotImplementedException();
    }
}
[Serializable]
public class AnimState
{
    public string onceAnim;
    public string loopAnim;
}
