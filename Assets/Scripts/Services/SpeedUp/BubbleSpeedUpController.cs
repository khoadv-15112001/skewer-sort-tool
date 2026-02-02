using System;
using DG.Tweening;
using GrillSort.SpeedUp;
using SonatFramework.Scripts.UIModule;
using SonatFramework.Systems;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Controller quản lý bubble speed up conveyor - hiển thị và animation
/// </summary>
public class BubbleSpeedUpController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private RectTransform bubbleSpeedConveyor;
    [SerializeField] private Button btnBubble;
    [SerializeField] private GameplayController gameplayController;
    
    [Header("Visual Effects")]
    [Tooltip("Effect hiển thị khi đã mua và đang apply speed up")]
    [SerializeField] private GameObject speedUpActiveEffect;

    [Header("Animation Settings")]
    [Tooltip("X position khi bubble slide in")]
    [SerializeField] private float slideInXPosition = -300f;
    
    [Tooltip("Y position khi bubble bounce")]
    [SerializeField] private float bounceYPosition = -50f;
    
    [Tooltip("Duration cho animation slide in")]
    [SerializeField] private float slideInDuration = 0.5f;
    
    [Tooltip("Duration cho animation bounce")]
    [SerializeField] private float bounceDuration = 2f;
    
    [Tooltip("Delay trước khi bắt đầu bounce")]
    [SerializeField] private float bounceDelay = 0.1f;
    
    [Header("Debug")]
    [SerializeField] private bool enableDebugLog = false;
    [SerializeField] private bool enableDebugKey = true;
    
    private bool isShowing;
    private bool hasAppliedSpeedUpThisLevel = false; // Track nếu đã apply speed up trong level này
    private bool hasCheckedShowCondition = false; // Track nếu đã check điều kiện show
    
    private readonly Service<SpeedUpService> speedUpService = new();
    
    public bool IsShowing => isShowing;
    
    private void OnEnable()
    {
        btnBubble.onClick.AddListener(OnBubbleClick);
        GameplayController.OnLoadLevel += OnLevelLoad;
        
        // Subscribe to SpeedUp service events
        if (speedUpService.Instance != null)
        {
            speedUpService.Instance.OnSpeedUpStateChanged += OnSpeedUpStateChanged;
        }
        
        // Subscribe to TickService để check điều kiện mỗi giây
        TickService.Register(CheckShowBubbleCondition, TickService.TickPhase.Tick);
    }
    
    private void OnDisable()
    {
        btnBubble.onClick.RemoveListener(OnBubbleClick);
        GameplayController.OnLoadLevel -= OnLevelLoad;
        
        // Unsubscribe from SpeedUp service events
        if (speedUpService.Instance != null)
        {
            speedUpService.Instance.OnSpeedUpStateChanged -= OnSpeedUpStateChanged;
        }
        
        // Unsubscribe from TickService
        TickService.UnRegister(CheckShowBubbleCondition, TickService.TickPhase.Tick);
        
        // Hide effect when disabled
        HideSpeedUpEffect();
    }
    
    
#if UNITY_EDITOR
    private void Update()
    {
        // Debug: Press Space to test animation
        if (enableDebugKey && Input.GetKeyDown(KeyCode.Space))
        {
            ShowBubble();
        }
    }
#endif
    
    /// <summary>
    /// Called when level loads - reset bubble state
    /// </summary>
    private void OnLevelLoad(int level)
    {        
        // Reset flag khi load level mới hoặc replay
        hasAppliedSpeedUpThisLevel = false;
        hasCheckedShowCondition = false;
        
        // Deactivate speed up khi load level mới
        if (speedUpService.Instance != null)
        {
            speedUpService.Instance.DeactivateSpeedUp();
        }

        HideBubbleImmediate();
        
        // Hide effect at start of level
        HideSpeedUpEffect();
    }
    
    /// <summary>
    /// Check điều kiện để show bubble dựa trên thời gian gameplay
    /// </summary>
    private void CheckShowBubbleCondition()
    {
        // Không check nếu đã show rồi
        if (hasCheckedShowCondition)
            return;
            
        // Không show nếu đã áp dụng speed up trong level này
        if (hasAppliedSpeedUpThisLevel)
            return;
        
        // Không show nếu bubble đang hiển thị
        if (isShowing || bubbleSpeedConveyor.gameObject.activeSelf)
            return;
        
        // Không show nếu game không đang chơi
        if (gameplayController.gameState != Sonat.Enums.GameState.Playing)
            return;
        
        // Không show nếu service chưa được khởi tạo
        if (speedUpService.Instance == null || speedUpService.Instance.Config == null)
            return;
        
        // Check điều kiện dựa trên thời gian gameplay (lấy từ config)
        bool hasConveyors = gameplayController.levelGenerator.AnyConveyors();
        float playTime = gameplayController.timeManager.GetTimeRunning();
        float minSeconds = speedUpService.Instance.Config.minSecondsToShow;
        float maxSeconds = speedUpService.Instance.Config.maxSecondsToShow;
        bool shouldShow = hasConveyors 
            && playTime >= minSeconds 
            && playTime <= maxSeconds;
        
        if (shouldShow)
        {
            hasCheckedShowCondition = true; // Mark đã check để không check lại nữa
            ShowBubble();
            Log($"CheckShowBubbleCondition: Showing bubble at playTime={playTime:F1}s (min={minSeconds}s, max={maxSeconds}s)");
        }
    }
    
    /// <summary>
    /// Show bubble với animation
    /// </summary>
    public void ShowBubble()
    {
        if (bubbleSpeedConveyor == null)
        {
            Debug.LogError("[BubbleSpeedUpController] bubbleSpeedConveyor is null!");
            return;
        }
        
        isShowing = true;
        bubbleSpeedConveyor.gameObject.SetActive(true);
        
        // Kill existing tweens
        bubbleSpeedConveyor.DOKill();
        
        // Reset position
        bubbleSpeedConveyor.anchoredPosition = Vector2.zero;
        
        // Animation: Slide in X
        bubbleSpeedConveyor.DOAnchorPosX(slideInXPosition, slideInDuration).SetEase(Ease.OutQuad);
        
        // Animation: Slide down Y then bounce loop
        bubbleSpeedConveyor.DOAnchorPosY(bounceYPosition, slideInDuration).SetEase(Ease.Linear)
            .OnComplete(() =>
            {
                bubbleSpeedConveyor.DOAnchorPosY(0, bounceDuration)
                    .SetDelay(bounceDelay)
                    .SetEase(Ease.Linear)
                    .SetLoops(-1, LoopType.Yoyo);
            });
        
        Log("ShowBubble: Animation started");
    }
    
    /// <summary>
    /// Hide bubble với animation scale out
    /// </summary>
    public void HideBubble(Action onComplete = null)
    {
        if (bubbleSpeedConveyor == null) return;
        
        isShowing = false;
        
        // Kill existing tweens
        bubbleSpeedConveyor.DOKill();
        
        // Scale out animation
        bubbleSpeedConveyor.transform.DOScale(Vector3.zero, 0.3f).SetEase(Ease.InBack)
            .OnComplete(() =>
            {
                bubbleSpeedConveyor.gameObject.SetActive(false);
                bubbleSpeedConveyor.transform.localScale = Vector3.one; // Reset scale
                onComplete?.Invoke();
            });
        
        Log("HideBubble: Animation started");
    }
    
    /// <summary>
    /// Hide bubble ngay lập tức không có animation
    /// </summary>
    public void HideBubbleImmediate()
    {
        if (bubbleSpeedConveyor == null) return;
        
        isShowing = false;
        
        // Kill existing tweens
        bubbleSpeedConveyor.DOKill();
        
        // Reset
        bubbleSpeedConveyor.anchoredPosition = Vector2.zero;
        bubbleSpeedConveyor.transform.localScale = Vector3.one;
        bubbleSpeedConveyor.gameObject.SetActive(false);
        
        Log("HideBubbleImmediate: Bubble hidden");
    }
    
    /// <summary>
    /// Called when user clicks bubble - open popup and hide bubble
    /// </summary>
    public void OnBubbleClick()
    {
        Log("OnBubbleClick: Opening PopupSpeedUp");
        
        UIData uiData = new UIData();
        uiData.Add("OnSuccess", (Action)(() =>
        {
            // Mark rằng đã áp dụng speed up trong level này
            hasAppliedSpeedUpThisLevel = true;
            
            HideBubble();
            Log("OnBubbleClick: Speed up success - bubble hidden, hasAppliedSpeedUpThisLevel set to true");
        }));
        
        PanelManager.Instance.OpenForget<PopupSpeedUp>(uiData);
    }
    
    private void Log(string message)
    {
        if (enableDebugLog)
        {
            Debug.Log($"[BubbleSpeedUpController] {message}");
        }
    }
    
    #region Speed Up Effect Management
    
    /// <summary>
    /// Called when SpeedUp state changes (active/inactive)
    /// </summary>
    private void OnSpeedUpStateChanged(bool isActive)
    {
        if (isActive)
        {
            ShowSpeedUpEffect();
        }
        else
        {
            HideSpeedUpEffect();
        }
        
        Log($"OnSpeedUpStateChanged: isActive={isActive}");
    }
    
    /// <summary>
    /// Show speed up active effect
    /// </summary>
    private void ShowSpeedUpEffect()
    {
        if (speedUpActiveEffect != null)
        {
            speedUpActiveEffect.SetActive(true);
            Log("Speed up effect shown");
        }
    }
    
    /// <summary>
    /// Hide speed up active effect
    /// </summary>
    private void HideSpeedUpEffect()
    {
        if (speedUpActiveEffect != null)
        {
            speedUpActiveEffect.SetActive(false);
            Log("Speed up effect hidden");
        }
    }
    
    #endregion
}

