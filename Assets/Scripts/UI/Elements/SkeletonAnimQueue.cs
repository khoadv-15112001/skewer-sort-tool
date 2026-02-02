using System;
using DG.Tweening;
using Sirenix.OdinInspector;
using Sonat.Enums;
using SonatFramework.Scripts.Utils;
using SonatFramework.Systems.AudioManagement;
using Spine.Unity;
using System.Collections.Generic;
using SonatFramework.Systems;
using UnityEngine;

public class SkeletonAnimQueue : MonoBehaviour
{
    public SkeletonGraphic skeletonGraphic;
    public List<AnimTrack> animTracks;
    private int trackIndex;
    public bool reInit = true;
    public bool isHide = true;

#if UNITY_EDITOR

    private void OnValidate()
    {
        if (skeletonGraphic == null)
            skeletonGraphic = GetComponent<SkeletonGraphic>();
    }

#endif

    private void Awake()
    {
        if (skeletonGraphic == null)
            skeletonGraphic = GetComponent<SkeletonGraphic>();
    }

    private void OnEnable()
    {
        if (skeletonGraphic == null) return;

        if (reInit)
        {
            trackIndex = 0;
            skeletonGraphic.AnimationState.ClearTracks();
            skeletonGraphic.Initialize(true);
        }

        PlayQueue();
    }


    public void PlayQueue()
    {
        if (animTracks == null || trackIndex >= animTracks.Count) return;

        if (animTracks[trackIndex].delay > 0 && isHide)
        {
            skeletonGraphic.enabled = false;
        }

        DOVirtual.DelayedCall(animTracks[trackIndex].delay, () =>
        {
            if (animTracks[trackIndex].playAudio)
            {
                SonatUtils.DelayCall(animTracks[trackIndex].delayAudio,
                    () => { SonatSystem.GetService<AudioService>().PlaySound(animTracks[trackIndex].AudioName); });
            }

            if (animTracks[trackIndex].delay > 0)
            {
                skeletonGraphic.enabled = true;
            }

            skeletonGraphic.AnimationState.ClearTracks();
            skeletonGraphic.Initialize(true);
            if (animTracks[trackIndex].loop)
            {
                skeletonGraphic.AnimationState.SetAnimation(0, animTracks[trackIndex].animationName, animTracks[trackIndex].loop);
            }
            else
            {
                skeletonGraphic.AnimationState.SetAnimation(0, animTracks[trackIndex].animationName, animTracks[trackIndex].loop).Complete += (track) =>
                {
                    trackIndex++;
                    PlayQueue();
                };
            }
        });
    }
}

[System.Serializable]
public class AnimTrack
{
    public string animationName;
    public float delay;
    public bool loop;

    public bool playAudio = false;

    [ShowIf("playAudio"), FoldoutGroup("Audio", expanded: true)]
    public float delayAudio = 0;

    [ShowIf("playAudio"), FoldoutGroup("Audio"), SerializeField]
    private AudioId audioId;
    
    [ShowIf("playAudio"), FoldoutGroup("Audio"), SerializeField]
    private string audioName;
    
    public string AudioName => audioId == AudioId.None? audioName : audioId.ToString();
    
}