using System.Collections;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using Gameplay.Entities;
using Gameplay.LevelData;
using GrillSort.SpeedUp;
using MyGame.SkewerJam.Gameplay;
using MyGame.SkewerJam.Scripts.SO.Behavior;
using Sirenix.Utilities;
using Sonat.Enums;
using SonatFramework.Scripts.Utils;
using SonatFramework.Systems;
using SonatFramework.Systems.EventBus;
using SonatFramework.Systems.ObjectPooling;
using UnityEngine;

public class ConveyorController : EntityBase, IPoolingObject
{
    [Header("Speed Settings")]
    public float normalSpeed = 1f;
    public float boostSpeed = 3f;

    [Header("Acceleration Settings")]
    public float accelerationTime = 1f;
    public float decelerationTime = 1f;

    public ConveyorVisual conveyorVisual;

    public override EntityType entityType => EntityType.Conveyor;
    [SerializeField] protected MoveType moveType;
    protected ConveyorData conveyData;
    [SerializeField] protected Transform container;
    protected Vector3 startPosition;
    protected Vector3 endPosition;
    protected List<PrimaryGrill> grills;
    protected float spacing = 4f;
    protected Vector3 direction;
    protected Vector3 conveySpeed;
    protected float maxConveySpeed = 1f;
    protected Vector3 visualSpeed;
    //protected Material materialVisual;
    [SerializeField] protected float visualRatio = -1.2f;

    [SerializeField] protected SpriteRenderer visual;
    //[SerializeField] private SpriteRenderer visual;
    //[SerializeField] private Sprite[] sprites;

    //private static int visualCount;
    protected bool paused;
    protected bool moving = true;
    protected EventBinding<GameStateChangeEvent> onGameStateChange;
    protected bool hasSubGrill = false;
    [SerializeField] protected Material visualMaterial;

    [Space(10)]
    [Header("Behavior SO")]
    [SerializeField] protected ConveyorControllerSO conveyorControllerSO;

    private CancellationTokenSource conveyorCTS;
    private readonly Service<SpeedUpService> speedUpService = new();

    private bool isInitialized = false;
    private bool isSpeedBoosted = false;

    private void OnEnable()
    {
        onGameStateChange = new EventBinding<GameStateChangeEvent>(OnGameStateChangedEvent);
    }

    private void OnDisable()
    {
        EventBus<GameStateChangeEvent>.Deregister(onGameStateChange);
        StopTweenIfAny();
        conveyorCTS?.Cancel();
        conveyorCTS?.Dispose();
        conveyorCTS = null;
    }

    private void OnDestroy()
    {
        StopTweenIfAny();
    }

    private void OnGameStateChangedEvent(GameStateChangeEvent eventData)
    {
        if (eventData.gameState == GameState.Paused || eventData.gameState == GameState.GameOver)
        {
            moving = false;
        }
        else
        {
            moving = true;
        }
    }

    public void SetData(ConveyorData conveyData)
    {
        if (conveyData.speed == 0) conveyData.speed = -1;
        this.conveyData = conveyData;
        transform.position = conveyData.position.ToVector3();
        //materialVisual = Instantiate(visualMaterial);
        CalculateStartPosition();
        GetGrills();
        CalculateSpacing();
        paused = false;
        moving = true;
        StartMovement();
        conveyorVisual.Initialize(conveySpeed * visualRatio);
        //visual.sprite = sprites[visualCount];
        //visualCount++;
        //if(visualCount >= sprites.Length) visualCount = 0;
    }

    protected virtual void CalculateStartPosition()
    {
        startPosition = container.position;
        endPosition = container.position;
        conveyorControllerSO.GetStartEndPositions(conveyData, moveType, conveyData.speed, ref startPosition, ref endPosition);

        direction = moveType == MoveType.Horizontal
            ? (conveyData.speed > 0 ? Vector3.right : Vector3.left)
            : (conveyData.speed > 0 ? Vector3.up : Vector3.down);
    }

    public virtual void GetGrills()
    {
        grills = new List<PrimaryGrill>();
        hasSubGrill = false;
        foreach (var grillId in conveyData.grillIds)
        {
            var grill = conveyorControllerSO.GetGrill(grillId);
            if (grill != null)
            {
                grills.Add(grill);
                grill.SetMaskVisible(moveType == MoveType.Vertical);
                if (!hasSubGrill && grill.HasSubGrills())
                {
                    hasSubGrill = true;
                }
            }
        }

        grills.Sort((a, b) => Vector3.SqrMagnitude(a.transform.position - endPosition).CompareTo(Vector3.SqrMagnitude(b.transform.position - endPosition)));
    }

    protected virtual void CalculateSpacing()
    {
        if (grills.Count == 0) return;

        spacing = 0.75f;
        float totalDistance = 0;
        for (int i = 0; i < grills.Count; i++)
        {
            totalDistance += grills[0].GrillVisual.GetGrillBounds().size.x;
        }

        if (totalDistance + spacing * (grills.Count - 1) < Vector3.Distance(startPosition, endPosition))
        {
            spacing = (Vector3.Distance(startPosition, endPosition) - totalDistance) / (grills.Count - 1) + 0.2f;
        }

        //spacing = Mathf.Clamp(spacing, 4, 8f);
        grills[0].transform.position = startPosition;
        if (!hasSubGrill)
        {
            grills[0].transform.SetLocalPositionY(startPosition.y - 0.35f);
        }

        for (int i = 1; i < grills.Count; i++)
        {
            float distance = grills[i].GrillVisual.GetGrillBounds().size.x / 2 + grills[i - 1].GrillVisual.GetGrillBounds().size.x / 2 + spacing;
            grills[i].transform.position = grills[i - 1].transform.position - direction * distance;
        }
    }

    protected virtual float GetReDistance(PrimaryGrill grill)
    {
        return grill.GrillVisual.GetGrillBounds().size.x / 2 + grills[^1].GrillVisual.GetGrillBounds().size.x / 2 + spacing;
    }

    private Vector3 GetRePosition(Vector3 position)
    {
        if (moveType == MoveType.Vertical) return position;
        if (conveyData.speed > 0)
        {
            if (position.x > startPosition.x) return startPosition;
        }
        else
        {
            if (position.x < startPosition.x) return startPosition;
        }

        return position;
    }

    public void Setup()
    {
    }

    public virtual void OnCreateObj(params object[] args)
    {
        paused = false;
    }

    public virtual void OnReturnObj()
    {
        StopAllCoroutines();
        paused = false;
    }

    #region Speed Control
    private bool isMouseDown = false;

    //Call from SetData
    private void StartMovement()
    {
        SetUpSpeed();
        conveyorCTS = new CancellationTokenSource();
        MoveConveyor(conveyorCTS.Token).Forget();
    }

    private void SetUpSpeed()
    {
        conveySpeed = direction * Mathf.Clamp(Mathf.Abs(conveyData.speed), -1, 1);
    }

    public async UniTask MoveConveyor(CancellationToken token)
    {
        //Vector3 movementDirection = direction; // * Mathf.Abs(conveyData.speed);
        PrimaryGrill firstGrill = grills[0];
        while (!token.IsCancellationRequested)
        {
            if (moving)
            {
                if (paused)
                {
                    paused = false;
                }

                foreach (var grill in grills)
                {
                    grill.transform.Translate(conveySpeed * Time.deltaTime);
                }

                if ((firstGrill.transform.position - endPosition).sqrMagnitude < 0.2f)
                {
                    grills.Remove(firstGrill);
                    Vector3 rePos = grills[^1].transform.position - direction * GetReDistance(firstGrill);
                    firstGrill.transform.position = GetRePosition(rePos);
                    firstGrill.OnResetConveyorCircle();
                    grills.Add(firstGrill);
                    firstGrill = grills[0];
                }
                conveyorVisual.UpdateVisual(conveySpeed * visualRatio);
            }
            else
            {
                if (!paused)
                {
                    paused = true;
                }
            }

            await UniTask.NextFrame(token);
        }
    }

    private Coroutine tweenCo;

    /// <summary>
    /// Called when user clicks on conveyor - boost speed if SpeedUp is active
    /// </summary>
    public void OnMouseDown()
    {
        // Chỉ cho phép boost nếu đã mua SpeedUp
        if (speedUpService.Instance == null || !speedUpService.Instance.IsSpeedUpActive)
        {
            Debug.Log("[ConveyorController] Speed up not active. Purchase speed up first!");
            return;
        }

        if (isSpeedBoosted) return;

        isSpeedBoosted = true;
        StopTweenIfAny();
        
        // Apply speed multiplier from service
        float multiplier = speedUpService.Instance.SpeedMultiplier;
        float targetX = direction.x * normalSpeed * multiplier;
        float targetY = direction.y * normalSpeed * multiplier;
        
        tweenCo = StartCoroutine(TweenSpeed(targetX, targetY, accelerationTime));
        
        // Switch visual to high speed
        conveyorVisual.SwitchSpeedSprite(true);
        
        Debug.Log($"[ConveyorController] Speed boosted! Multiplier: {multiplier}x");
    }

    /// <summary>
    /// Called when user releases mouse - return to normal speed
    /// </summary>
    public void OnMouseUp()
    {
        if (!isSpeedBoosted) return;

        isSpeedBoosted = false;
        StopTweenIfAny();
        
        float targetX = direction.x * normalSpeed;
        float targetY = direction.y * normalSpeed;
        
        tweenCo = StartCoroutine(TweenSpeed(targetX, targetY, decelerationTime));
        
        // Switch visual back to normal speed
        conveyorVisual.SwitchSpeedSprite(false);
        
        Debug.Log("[ConveyorController] Speed returned to normal");
    }

    private IEnumerator TweenSpeed(float targetX, float targetY, float duration)
    {
        float startX = conveySpeed.x;
        float startY = conveySpeed.y;

        if (duration <= 0f)
        {
            conveySpeed = new Vector3(targetX, targetY, 0f);
            yield break;
        }

        float t = 0f;

        while (t < 1f)
        {
            t += Time.deltaTime / duration;
            float s = Mathf.SmoothStep(0f, 1f, t); // smoothstep

            float newx = Mathf.Lerp(startX, targetX, s);
            float newy = Mathf.Lerp(startY, targetY, s);
            conveySpeed = new Vector3(newx, newy, 0f);

            //ApplyToRenderer(currentSpeedX, currentSpeedY);
            yield return null;
        }

        conveySpeed = new Vector3(targetX, targetY, 0f);
        //ApplyToRenderer(currentSpeedX, currentSpeedY);
        tweenCo = null;
    }

    private void StopTweenIfAny()
    {
        if (tweenCo != null) { StopCoroutine(tweenCo); tweenCo = null; }
    }
    #endregion

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            conveyorVisual.SwitchSpeedSprite(false);
        }
        else if(Input.GetKeyDown(KeyCode.RightArrow))
        {
            conveyorVisual.SwitchSpeedSprite(true);
        }
    }
}
