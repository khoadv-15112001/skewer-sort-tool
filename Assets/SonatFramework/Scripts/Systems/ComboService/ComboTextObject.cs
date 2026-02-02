using DG.Tweening;
using I2.Loc;
using Sonat.Enums;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Combo text object that can be pooled
/// Each prefab is pre-configured with its own text and light sprite
/// Structure: 
/// - Root (this object) - positioned at spawn location
///   └── Content (child) - contains all UI elements, animates upward
///       └── Text
/// </summary>
public class ComboTextObject : EffectPoolBase
{
    [SerializeField] private ComboTextPrefab comboTextType;

    [Header("References")]
    [SerializeField] private Transform contentTransform;  // Parent of all UI elements

    [Header("Animation Settings")]
    [SerializeField] private float displayDuration = 1.5f;
    [SerializeField] private float moveUpDistance = 2f;
    [SerializeField] private Ease moveEase = Ease.OutCubic;
    [SerializeField] private Ease fadeEase = Ease.InCubic;

    [Header("Scale Animation")]
    [SerializeField] private float scaleInDuration = 0.3f;
    [SerializeField] private float scaleInTarget = 1.2f;  // Scale lên 1.2x
    [SerializeField] private Ease scaleInEase = Ease.OutBack;  // Bounce effect

    private Sequence animationSequence;
    private CanvasGroup canvasGroup;
    public override void Setup()
    {
        base.Setup();
        // Get CanvasGroup on the content transform (where UI elements are)
        if (contentTransform != null && canvasGroup == null)
        {
            canvasGroup = contentTransform.GetComponent<CanvasGroup>();
            if (canvasGroup == null)
            {
                canvasGroup = contentTransform.gameObject.AddComponent<CanvasGroup>();
            }
        }
    }

    public override void OnCreateObj(params object[] args)
    {
        base.OnCreateObj(args);

        // Reset content position and scale
        if (contentTransform != null)
        {
            contentTransform.localPosition = Vector3.zero;
            contentTransform.localScale = Vector3.zero;  // Start from scale 0
        }

        // Reset alpha
        if (canvasGroup != null)
        {
            canvasGroup.alpha = 1f;
        }

        // Start animation
        PlayAnimation();

        PlaySoundCombo();
    }

    private void PlaySoundCombo()
    {
        // Always use female voice (only female voices available)
        Sonat.Enums.AudioId audioId = GetComboSoundId(comboTextType);

        if (audioId != Sonat.Enums.AudioId.None)
        {
            //Debug.Log($"[ComboTextObject] Playing sound: {audioId} for combo: {comboTextType}");
            MySonatFramework.audioService.PlaySound(audioId);
        }
        else
        {
            //Debug.LogWarning($"[ComboTextObject] No audio found for combo type: {comboTextType}, language: {languageCode}");
        }
    }

    /// <summary>
    /// Get the appropriate audio ID based on combo type (female voice only, English)
    /// </summary>
    private Sonat.Enums.AudioId GetComboSoundId(ComboTextPrefab comboType)
    {
        //Debug.Log($"[ComboTextObject] Language code: {languageCode}, Combo type: {comboType}");

        if (LocalizationManager.CurrentLanguageCode != "en")
        {
            //Debug.LogWarning($"[ComboTextObject] Non-English language ({languageCode}), skipping combo voice");
            return AudioId.None;
        }
        
        // Map combo text types to female voice audio IDs
        return comboType switch
        {
            ComboTextPrefab.ComboText_Good => Sonat.Enums.AudioId.Voice_Char_woman_Combo_Good,
            ComboTextPrefab.ComboText_Excellent => Sonat.Enums.AudioId.Voice_Char_woman_Combo_Excellent,
            ComboTextPrefab.ComboText_Amazing => Sonat.Enums.AudioId.Voice_Char_woman_Combo_Amazing,
            ComboTextPrefab.ComboText_Tasty => Sonat.Enums.AudioId.Voice_Char_woman_Combo_Tasty,
            
            _ => Sonat.Enums.AudioId.None,
        };
    }

    public override void OnReturnObj()
    {
        base.OnReturnObj();

        // Kill any running animations
        if (animationSequence != null && animationSequence.IsActive())
        {
            animationSequence.Kill();
            animationSequence = null;
        }

        // Reset transforms
        transform.localScale = Vector3.one;
        if (contentTransform != null)
        {
            contentTransform.localPosition = Vector3.zero;
            contentTransform.localScale = Vector3.one;  // Reset scale
        }

        // Reset alpha
        if (canvasGroup != null)
        {
            canvasGroup.alpha = 0f;
        }

        // Note: Parent will be changed by PoolingService.ReturnObj() to pool.transform
        // This is normal pooling behavior - inactive objects stay in pool for reuse
    }

    private void PlayAnimation()
    {
        if (contentTransform == null)
        {
            Debug.LogError("[ComboTextObject] Content Transform is not assigned!");
            return;
        }

        // Kill existing animation
        if (animationSequence != null && animationSequence.IsActive())
        {
            animationSequence.Kill();
        }

        // Simple calculation: move up from 0 to moveUpDistance in local space
        float endY = moveUpDistance;

        // Create animation sequence
        animationSequence = DOTween.Sequence();

        // 1. Scale in animation (bung ra) - 0 → 1.2 → 1
        animationSequence.Append(
            contentTransform.DOScale(scaleInTarget, scaleInDuration * 0.6f)
                .SetEase(scaleInEase)
        );
        animationSequence.Append(
            contentTransform.DOScale(1f, scaleInDuration * 0.4f)
                .SetEase(Ease.OutQuad)
        );

        // 2. Move content up (local Y only) - starts after scale in
        animationSequence.Append(
            contentTransform.DOLocalMoveY(endY, displayDuration)
                .SetEase(moveEase)
        );

        // 3. Fade out animation - runs parallel with move up
        if (canvasGroup != null)
        {
            animationSequence.Insert(
                scaleInDuration,  // Start fade after scale in
                canvasGroup.DOFade(0f, displayDuration * 0.7f)
                    .SetDelay(displayDuration * 0.3f)
                    .SetEase(fadeEase)
            );
        }

        // Return to pool after animation
        animationSequence.OnComplete(() =>
        {
            Destroy(); // Use EffectPoolBase's Destroy method
        });
    }

    private void OnDestroy()
    {
        // Clean up animation
        if (animationSequence != null && animationSequence.IsActive())
        {
            animationSequence.Kill();
            animationSequence = null;
        }
    }
}

