using System;
using System.Collections.Generic;
using DG.Tweening;
using Sonat.Enums;
using SonatFramework.Scripts.UIModule;
using SonatFramework.Systems.EventBus;
using UnityEngine;
using UnityEngine.UI;

public class UIButtonSetting : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private PopupSettings popupSettings;
    [SerializeField] private List<RectTransform> buttonTrans;
    [SerializeField] private Image background;
    [SerializeField] private CanvasGroup canvasGroup;

    [Header("Animation Settings")]
    [SerializeField] private float offsetX = 200f;
    [SerializeField] private float goInDuration = 0.3f;
    [SerializeField] private float goOutDuration = 0.25f;
    [SerializeField] private float fadeDuration = 0.25f;
    [SerializeField] private float delayBetweenButtons = 0.08f;
    [SerializeField] private Ease goInEase = Ease.OutBack;
    [SerializeField] private Ease goOutEase = Ease.InBack;
    [SerializeField] private Ease fadeEase = Ease.OutQuad;
    
    private bool isActive = false;
    private List<float> originalPositions = new List<float>();
    private Sequence currentSequence;
    
    private void Awake()
    {
        // Cache original positions
        foreach (var button in buttonTrans)
        {
            originalPositions.Add(button.anchoredPosition.x);
        }
        
        GoOutImmediate();
    }

    private void OnEnable()
    {
        GoOutImmediate();
    }

    /// <summary>
    /// Toggle between GoIn and GoOut
    /// </summary>
    public void OnActive()
    {
        if (isActive)
            GoOut();
        else
            GoIn();
    }

    /// <summary>
    /// Animate buttons in with stagger effect
    /// </summary>
    public void GoIn()
    {
        if (isActive) return;

        KillCurrentSequence();
        
        // Pause game state
        EventBus<GameStateChangeEvent>.Raise(new GameStateChangeEvent
        {
            gameState = GameState.Paused
        });

        isActive = true;
        background.gameObject.SetActive(true);

        // Create animation sequence
        currentSequence = DOTween.Sequence();

        // Fade in background
        canvasGroup.alpha = 0f;
        currentSequence.Join(canvasGroup.DOFade(1f, fadeDuration).SetEase(fadeEase));

        // Animate buttons with stagger
        for (int i = 0; i < buttonTrans.Count; i++)
        {
            var button = buttonTrans[i];
            var targetX = originalPositions[i];
            
            button.gameObject.SetActive(true);
            button.anchoredPosition = new Vector2(targetX + offsetX, button.anchoredPosition.y);

            currentSequence.Insert(
                i * delayBetweenButtons,
                button.DOAnchorPosX(targetX, goInDuration).SetEase(goInEase)
            );
        }
    }

    /// <summary>
    /// Animate buttons out with reverse stagger effect
    /// </summary>
    public void GoOut()
    {
        if (!isActive) return;

        KillCurrentSequence();
        
        if (popupSettings != null)
            popupSettings.ResetClick();

        // Create animation sequence
        currentSequence = DOTween.Sequence();

        // Fade out background
        currentSequence.Join(canvasGroup.DOFade(0f, fadeDuration).SetEase(fadeEase));

        // Animate buttons out in reverse order
        for (int i = buttonTrans.Count - 1; i >= 0; i--)
        {
            var button = buttonTrans[i];
            var targetX = originalPositions[i] + offsetX;
            var delayIndex = buttonTrans.Count - 1 - i;

            currentSequence.Insert(
                delayIndex * delayBetweenButtons,
                button.DOAnchorPosX(targetX, goOutDuration).SetEase(goOutEase)
            );
        }

    // On complete
    currentSequence.OnComplete(() =>
    {
        OnAnimationCompleted();
        
        // Only resume game if no popup with pauseGame is open
        if (!PanelManager.Instance.HasAnyPopupPauseGame())
        {
            EventBus<GameStateChangeEvent>.Raise(new GameStateChangeEvent
            {
                gameState = GameState.Playing
            });
        }
    });
    }

    /// <summary>
    /// Immediately hide all buttons without animation
    /// </summary>
    public void GoOutImmediate()
    {
        KillCurrentSequence();
        
        for (int i = 0; i < buttonTrans.Count; i++)
        {
            var button = buttonTrans[i];
            button.anchoredPosition = new Vector2(originalPositions[i] + offsetX, button.anchoredPosition.y);
        }
        
        canvasGroup.alpha = 0f;
        OnAnimationCompleted();
    }
    
    /// <summary>
    /// Called when home button is clicked
    /// </summary>
    public void HomeClick()
    {
        GoOut();
    }

    /// <summary>
    /// Called when animation completes
    /// </summary>
    private void OnAnimationCompleted()
    {
        isActive = false;
        background.gameObject.SetActive(false);
        
        foreach (var button in buttonTrans)
        {
            button.gameObject.SetActive(false);
        }
    }

    /// <summary>
    /// Kill current animation sequence
    /// </summary>
    private void KillCurrentSequence()
    {
        if (currentSequence != null && currentSequence.IsActive())
        {
            currentSequence.Kill();
            currentSequence = null;
        }

        // Kill individual tweens as safety
        foreach (var button in buttonTrans)
        {
            button.DOKill();
        }
        
        canvasGroup.DOKill();
    }

    private void OnDisable()
    {
        KillCurrentSequence();
    }

    private void OnDestroy()
    {
        KillCurrentSequence();
    }
}
