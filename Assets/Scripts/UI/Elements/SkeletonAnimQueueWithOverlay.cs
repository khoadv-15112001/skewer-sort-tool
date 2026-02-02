using System;
using DG.Tweening;
using Sirenix.OdinInspector;
using Sonat.Enums;
using SonatFramework.Scripts.Utils;
using SonatFramework.Systems.AudioManagement;
using Spine.Unity;
using System.Collections.Generic;
using System.Linq;
using SonatFramework.Systems;
using UnityEngine;

public class SkeletonAnimQueueWithOverlay : MonoBehaviour
{
    #region Inspector Fields

    [SerializeField] private SkeletonGraphic skeletonGraphic;
    [SerializeField] private List<AnimTrackWithOverlay> animTracks;
    [SerializeField] private bool reInit = true;
    [SerializeField] private bool isHide = true;

#if UNITY_EDITOR
    public SkeletonGraphic SkeletonGraphic => skeletonGraphic;
    public List<AnimTrackWithOverlay> AnimTracks => animTracks;
#endif

    #endregion

    #region Private Fields

    private int trackIndex;
    private Tween overlayTween;
    private Tween overlayAlphaTween; 
    private bool isPlayingOverlay = false;

    #endregion

    #region Unity Lifecycle

#if UNITY_EDITOR
    private void OnValidate()
    {
        skeletonGraphic ??= GetComponent<SkeletonGraphic>();
    }
#endif

    private void Awake()
    {
        skeletonGraphic ??= GetComponent<SkeletonGraphic>();
    }

    private void OnEnable()
    {
        if (skeletonGraphic == null) return;

        if (reInit)
            InitializeAnimation();

        PlayQueue();
    }

    private void OnDisable()
    {
        CleanupOverlay();
        ClearAnimationTracks();
    }

    #endregion

    #region Public Methods

    public void PlayQueue()
    {
        if (!IsValidTrackIndex()) return;

        var currentTrack = animTracks[trackIndex];
        PrepareForAnimation(currentTrack);

        DOVirtual.DelayedCall(currentTrack.delay, () => ExecuteAnimation(currentTrack));
    }

    #endregion

    #region Animation Execution

    private void ExecuteAnimation(AnimTrackWithOverlay track)
    {
        PlayAudioIfNeeded(track);
        SetupSkeletonForAnimation();

        if (track.loop)
            PlayLoopingAnimation(track);
        else
            PlaySingleAnimation(track);
    }

    private void PlayLoopingAnimation(AnimTrackWithOverlay track)
    {
        skeletonGraphic.AnimationState.SetAnimation(0, track.animationName, true);

        if (HasOverlayAnimation(track))
            StartOverlayCycle();
    }

    private void PlaySingleAnimation(AnimTrackWithOverlay track)
    {
        skeletonGraphic.AnimationState.SetAnimation(0, track.animationName, false)
            .Complete += _ => AdvanceToNextTrack();
    }

    private void AdvanceToNextTrack()
    {
        trackIndex++;
        PlayQueue();
    }

    #endregion

    #region Overlay System

    private void StartOverlayCycle()
    {
        overlayTween?.Kill();

        float delay = GetRandomDuration(animTracks[trackIndex].mainTrackDurationRange);
        overlayTween = DOVirtual.DelayedCall(delay, TryPlayOverlay);
    }

    private void TryPlayOverlay()
    {
        if (CanPlayOverlay())
            PlayOverlay();
    }

    private void PlayOverlay()
    {
        if (!IsValidTrackIndex()) return;

        var track = animTracks[trackIndex];
        isPlayingOverlay = true;

        SetupOverlayAnimation(track);
        ScheduleOverlayEnd(track);
    }

    private void SetupOverlayAnimation(AnimTrackWithOverlay track)
    {
        // Mỗi lần setup overlay sẽ random lại animation
        string randomOverlayAnim = track.GetRandomOverlayAnimation();
        if (string.IsNullOrEmpty(randomOverlayAnim)) return;
    
        skeletonGraphic.AnimationState.Data.SetMix(track.animationName, randomOverlayAnim, 0.3f);

        var overlayEntry = skeletonGraphic.AnimationState.SetAnimation(1, randomOverlayAnim, track.overlayLoop);
        overlayEntry.MixDuration = 0.3f;
        overlayEntry.Alpha = 0f;
        overlayAlphaTween?.Kill();
        overlayAlphaTween = DOTween.To(() => overlayEntry.Alpha, x => overlayEntry.Alpha = x, 1f, 0.3f);
    }
    

    private void ScheduleOverlayEnd(AnimTrackWithOverlay track)
    {
        float duration = GetRandomDuration(track.overlayDurationRange);

        overlayTween?.Kill();
        overlayTween = DOVirtual.DelayedCall(duration, EndOverlay);
    }

    private void EndOverlay()
    {
        if (!CanEndOverlay()) return;

        var overlayTrack = skeletonGraphic.AnimationState.GetCurrent(1);
        if (overlayTrack != null)
        {
            overlayAlphaTween?.Kill();  
            overlayAlphaTween = DOTween.To(() => overlayTrack.Alpha, x => overlayTrack.Alpha = x, 0f, 0.3f)
                .OnComplete(() => {
                    skeletonGraphic.AnimationState.SetEmptyAnimation(1, 0.3f);
                    isPlayingOverlay = false;
                
                    if (ShouldContinueOverlayCycle())
                        StartOverlayCycle();
                });
        }
        else
        {
            skeletonGraphic.AnimationState.SetEmptyAnimation(1, 0.3f);
            isPlayingOverlay = false;
        
            if (ShouldContinueOverlayCycle())
                StartOverlayCycle();
        }
    }

    #endregion

    #region Helper Methods

    private void InitializeAnimation()
    {
        trackIndex = 0;
        skeletonGraphic.AnimationState.ClearTracks();
        skeletonGraphic.Initialize(true);
    }

    private void PrepareForAnimation(AnimTrackWithOverlay track)
    {
        if (track.delay > 0 && isHide)
            skeletonGraphic.enabled = false;
    }

    private void SetupSkeletonForAnimation()
    {
        if (animTracks[trackIndex].delay > 0)
            skeletonGraphic.enabled = true;

        skeletonGraphic.AnimationState.ClearTracks();
        skeletonGraphic.Initialize(true);
    }

    private void PlayAudioIfNeeded(AnimTrackWithOverlay track)
    {
        if (!track.playAudio) return;

        SonatUtils.DelayCall(track.delayAudio, () =>
            SonatSystem.GetService<AudioService>().PlaySound(track.AudioName));
    }

    private void CleanupOverlay()
    {
        overlayTween?.Kill();
        overlayTween = null;
        overlayAlphaTween?.Kill(); 
        overlayAlphaTween = null;
        isPlayingOverlay = false;
    }

    private void ClearAnimationTracks()
    {
        if (skeletonGraphic?.AnimationState != null)
            skeletonGraphic.AnimationState.ClearTracks();
    }

    private float GetRandomDuration(Vector2 range) =>
        UnityEngine.Random.Range(range.x, range.y);

    #endregion

    #region Validation Methods

    private bool IsValidTrackIndex() =>
        animTracks != null && trackIndex >= 0 && trackIndex < animTracks.Count;

    private bool HasOverlayAnimation(AnimTrackWithOverlay track) =>
        track.HasValidOverlayAnimation;

    private bool CanPlayOverlay() =>
        !isPlayingOverlay && IsValidTrackIndex() && this != null;

    private bool CanEndOverlay() =>
        skeletonGraphic?.AnimationState != null;

    private bool ShouldContinueOverlayCycle() =>
        IsValidTrackIndex() && animTracks[trackIndex].loop && this != null;

    #endregion
    
#if UNITY_EDITOR
    [Button("Refresh Animation List")]
    private void RefreshAnimationDropdowns()
    {
        // Force refresh inspector
        UnityEditor.EditorUtility.SetDirty(this);
    }
#endif
}

[System.Serializable]
public class AnimTrackWithOverlay
{
    [Header("Basic Animation")]
    [ValueDropdown("GetAvailableAnimations")]
    public string animationName;
    public float delay;
    public bool loop;

    [Header("Overlay Animation")] 
    [ShowIf("loop")]
    [ValueDropdown("GetAvailableAnimations")]
    public List<string> overlayAnimationNames = new List<string>();
    
    [ShowIf("@loop && HasValidOverlayAnimation")] public bool overlayLoop = false;

    [ShowIf("@loop && overlayLoop && HasValidOverlayAnimation")] public Vector2 mainTrackDurationRange = new Vector2(2f, 5f);

    [ShowIf("@loop && overlayLoop && HasValidOverlayAnimation")] public Vector2 overlayDurationRange = new Vector2(2f, 5f);

    [Header("Audio")] public bool playAudio = false;

    [ShowIf("playAudio"), FoldoutGroup("Audio", expanded: true)]
    public float delayAudio = 0;

    [ShowIf("playAudio"), FoldoutGroup("Audio"), SerializeField]
    private AudioId audioId;

    [ShowIf("playAudio"), FoldoutGroup("Audio"), SerializeField]
    private string audioName;

    public string AudioName => audioId == AudioId.None ? audioName : audioId.ToString();
    public bool HasValidOverlayAnimation => overlayAnimationNames != null && 
                                            overlayAnimationNames.Count > 0 && 
                                            overlayAnimationNames.Any(name => !string.IsNullOrEmpty(name) && name != "None");
    public string GetRandomOverlayAnimation()
    {
        if (!HasValidOverlayAnimation) return null;
    
        var validAnimations = overlayAnimationNames
            .Where(name => !string.IsNullOrEmpty(name) && name != "None")
            .ToList();
    
        if (validAnimations.Count == 0) return null;
    
        return validAnimations[UnityEngine.Random.Range(0, validAnimations.Count)];
    }
#if UNITY_EDITOR
    private IEnumerable<string> GetAvailableAnimations()
    {
        var selected = UnityEditor.Selection.activeGameObject;
        var component = selected?.GetComponent<SkeletonAnimQueueWithOverlay>();

        if (component?.SkeletonGraphic?.SkeletonData == null)
            return new string[] { "No Skeleton Data" };

        // Thêm option "None" ở đầu list
        var animations = new List<string> { "None" };
        animations.AddRange(component.SkeletonGraphic.SkeletonData.Animations.Items
            .Select(anim => anim.Name));

        return animations;
    }
#endif

}