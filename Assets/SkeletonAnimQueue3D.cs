using System.Collections.Generic;
using DG.Tweening;
using Sirenix.OdinInspector;
using Sonat.Enums;
using SonatFramework.Scripts.Utils;
using SonatFramework.Systems;
using SonatFramework.Systems.AudioManagement;
using Spine.Unity;
using UnityEngine;

public class SkeletonAnimQueue3D : MonoBehaviour
{
    [Header("Spine Skeleton")]
    public SkeletonAnimation skeletonAnimation;

    [Header("Animation Queue")]
    public List<AnimTrack> animTracks;
    private int trackIndex;
    public bool reInit = true;
    public bool isHide = true;

#if UNITY_EDITOR
    private void OnValidate()
    {
        if (skeletonAnimation == null)
            skeletonAnimation = GetComponent<SkeletonAnimation>();
    }
#endif

    private void Awake()
    {
        if (skeletonAnimation == null)
            skeletonAnimation = GetComponent<SkeletonAnimation>();
    }

    private void OnEnable()
    {
        if (skeletonAnimation == null) return;

        if (reInit)
        {
            trackIndex = 0;
            skeletonAnimation.AnimationState.ClearTracks();
            skeletonAnimation.Initialize(true);
        }

        PlayQueue();
    }

    public void PlayQueue()
    {
        if (animTracks == null || trackIndex >= animTracks.Count) return;

        if (animTracks[trackIndex].delay > 0 && isHide)
        {
            skeletonAnimation.gameObject.SetActive(false);
        }

        DOVirtual.DelayedCall(animTracks[trackIndex].delay, () =>
        {
            if (animTracks[trackIndex].playAudio)
            {
                SonatUtils.DelayCall(animTracks[trackIndex].delayAudio,
                    () => SonatSystem.GetService<AudioService>().PlaySound(animTracks[trackIndex].AudioName));
            }

            if (animTracks[trackIndex].delay > 0)
            {
                skeletonAnimation.gameObject.SetActive(true);
            }

            skeletonAnimation.AnimationState.ClearTracks();
            skeletonAnimation.Initialize(true);

            if (animTracks[trackIndex].loop)
            {
                skeletonAnimation.AnimationState.SetAnimation(0, animTracks[trackIndex].animationName, true);
            }
            else
            {
                var entry = skeletonAnimation.AnimationState.SetAnimation(0, animTracks[trackIndex].animationName, false);
                entry.Complete += _ =>
                {
                    trackIndex++;
                    PlayQueue();
                };
            }
        });
    }
}
