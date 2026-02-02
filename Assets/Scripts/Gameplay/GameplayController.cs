using System;
using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Gameplay.Entities;
using Gameplay.Entities.Orders;
using Gameplay.GameplayElement;
using Gameplay.LevelData;
using Gameplay.SceneManager;
using GrillSort.ConsecutiveWin;
using GrillSort.LoseRwdService;
using Manager;
using Sonat.Enums;
using SonatFramework.Scripts.Gameplay;
using SonatFramework.Scripts.SonatSDKAdapterModule;
using SonatFramework.Scripts.UIModule;
using SonatFramework.Scripts.Utils;
using SonatFramework.Systems;
using SonatFramework.Systems.ConfigManagement;
using SonatFramework.Systems.EventBus;
using SonatFramework.Systems.InventoryManagement.GameResources;
using SonatFramework.Systems.ObjectPooling;
using SonatFramework.Systems.SceneManagement;
using UnityEngine;

public class GameplayController : MonoBehaviour
{
    public static GameplayController instance;
    public static event Action<int> OnLoadLevel;
    public static event Action<bool> OnLevelEnd;
    public static event Action<Item> OnSelectItem;
    public static event Action<Item, bool> OnDropItem;
    public static event Action<Item, SlotBase> OnItemMoveSlot;
    public static event Action<GameResource> OnUseBooster;

    public static event Action<int> OnCollectItem;
    public static event Action<int, int> OnCollectItemCount;
    public static event Action OnReplay;
    public static Action<GameResource> OnUseBoosterSuccess;
    public static Action<PrimaryGrill> OnActionLockGrill;
    public static Action<PrimaryGrill> OnActionUnlockGrill;
    public static int gameplayCount = 0;
    public static int level;
    public GameState gameState;
    public Camera mainCamera;
    public LevelGenerator levelGenerator;
    public GameViewport gameViewport;
    [SerializeField] private Config<GamePlayConfig> gameConfig;
    [SerializeField] private GameplayScreen gameplayScreen;
    private ComboService comboService;
    private StarChestService starChestService;
    private GrillSort.LoseRwdService.LoseRwdService loseRwdService;

    [HideInInspector] public Item itemSelected;

    // private float timeRemaining;
    private int itemCount;
    private float timeCollectItem;
    public RewardData bonusRewardData = new();
    public TimeGameplayManager timeManager = new();
    public MoveGameplayManager moveManager = new();
    private EventBinding<GameStateChangeEvent> gameStateChangeEvent;
    [HideInInspector] public LevelType levelType;
    private bool swapped;
    private PrimaryGrill bonusTimeGrill;
    public static bool swapItem;

    [Header("Goals Mode")]
    public int moveOnRevive = 10;
    public UITargetItemCtl uiTargetItemCtl;
    private List<TargetData> targetData = new();
    public List<TargetData> TargetData => targetData;
    public static event Action<int, int> OnTargetUpdate; // id, remaining quantity

    private void Awake()
    {
        instance = this;
        timeManager.Initialize(this);
        moveManager.Initialize(this);
        gameViewport = new GameViewport();
        gameViewport.CalculateGameViewportBounds();
        comboService = MySonatFramework.GetService<ComboService>();
        starChestService = MySonatFramework.GetService<StarChestService>();
        loseRwdService = MySonatFramework.GetService<LoseRwdService>();

        EventBus<UpdateScreenEvent>.Raise(new() { screen = "IG" });
        EventBus<UpdatePlacementEvent>.Raise(new() { placement = "GP:::ingame" });
        swapItem = GameRemoteConfigValue.swapItem;

        gameConfig.config.winRewards =
            SonatSDKAdapter.GetRemoteConfig<List<GamePlayConfig.RewardByLevelDifficulty>>("game_play_reward_by_level_difficulty_config",
                gameConfig.config.winRewards);
        //ClearEvent();
    }

    private void OnPanelsUpdated()
    {
        if (PanelManager.Instance.HasAnyPopupPauseGame())
        {
            if (gameState is GameState.Playing)
                ChangeGameState(GameState.Paused);
        }
        else
        {
            if (gameState is GameState.Paused)
                ChangeGameState(GameState.Playing);
        }
    }

    private void ClearEvent()
    {
        OnLoadLevel = null;
        OnLevelEnd = null;
        OnSelectItem = null;
        OnDropItem = null;
        OnItemMoveSlot = null;
        OnUseBooster = null;
        OnCollectItem = null;
        OnCollectItemCount = null;
        OnReplay = null;
        OnUseBoosterSuccess = null;
        OnActionLockGrill = null;
        OnActionUnlockGrill = null;
        OnTargetUpdate = null;
    }

    public void SelectItem(Item item, bool unSelectLastItem = true)
    {
        //Debug.Log("Select Item Start");
        if (item != null && item == itemSelected) return;
        if (itemSelected != null && unSelectLastItem)
        {
            itemSelected.OnDeSelected();
        }

        itemSelected = item;
        OnSelectItem?.Invoke(item);
    }

    private Coroutine forceCheckWinCoroutine;

    private IEnumerator CheckWinCoroutine()
    {
        while (Time.time - timeCollectItem < 5)
        {
            if (gameState == GameState.GameOver)
            {
                forceCheckWinCoroutine = null;
                yield break;
            }

            yield return new WaitForSeconds(0.25f);
        }

        if (gameState == GameState.GameOver)
        {
            forceCheckWinCoroutine = null;
            yield break;
        }

        forceCheckWinCoroutine = null;
        ForceCheckWin();
    }

    private bool simulateError_CollectItem = false;
    private bool simulateError_ProcessGolden = false;

    public void CollectItem(int id, Vector3 position, int numSlot = 3, bool showTextCombo = true)
    {
        try
        {
            //Debug.Log("[CollectItem] Start");

            itemCount++;
            OnCollectItem?.Invoke(id);
            OnCollectItemCount?.Invoke(id, numSlot);

            comboService.AddCombo();
            int comboSound = 40 + comboService.Combo;
            if (comboSound > 50) comboSound = 50;
            MySonatFramework.audioService.PlaySound((AudioId)(comboSound));

            // Spawn combo text if this combo level has text
            if (showTextCombo)
                SpawnComboText(position);

            starChestService.OnCollectItem(position);

            // 🔴 Giả lập lỗi tại đây
            if (simulateError_CollectItem)
            {
                Debug.LogError("[CollectItem] Simulated error triggered!");
                throw new Exception("Simulated CollectItem crash!");
            }

            if (itemCount >= levelGenerator.MaxOrder)
            {
                var orderList = levelGenerator.CheckOrderRemain();
                if (orderList != null)
                {
                    orderList.OutOfItemOrder();
                    return;
                }

                Debug.Log("[CollectItem] All items collected -> Win()");
                Win().Forget();
            }
            else
            {
                ForceCheckWin();
            }

            timeCollectItem = Time.time;
            if (forceCheckWinCoroutine == null)
                forceCheckWinCoroutine = StartCoroutine(CheckWinCoroutine());
        }
        catch (Exception e)
        {
            Debug.LogError($"[CollectItem] Fatal error: {e}");
        }
    }

    private void ForceCheckWin()
    {
        if (gameState == GameState.GameOver) return;
        if (levelGenerator.IsWinLevel())
        {
            var orderList = levelGenerator.CheckOrderRemain();
            if (orderList != null)
            {

                orderList.OutOfItemOrder();
                Win().Forget();
                return;
            }
        }

        timeCollectItem = Time.time;
    }

    public void ProcessTarget(int id, int numSlot)
    {
        UpdateTargetProgress(id, numSlot);
        if (CheckTargetComplete())
        {
            Win().Forget();
        }
    }

    public void SwitchSlot(SlotBase slot)
    {
        if (itemSelected == null) return;
        //Debug.Log("Switched Slot Start");
        bool changed = slot != itemSelected.Slot;
        SlotBase oldSlot = itemSelected.Slot;
        Item oldItem = slot.GetItem();
        itemSelected.SwitchSlot(slot);
        swapped = false;
        if (swapItem && oldItem != null && oldSlot != null && oldItem != itemSelected)
        {
            swapped = true;
            oldItem.ForceOutSlot();
            oldItem.SwitchSlot(oldSlot);
        }

        OnDropItem?.Invoke(itemSelected, changed);
        OnItemMoveSlot?.Invoke(itemSelected, slot);
        SelectItem(null, false);
        //Debug.Log("Switched Slot End");
    }

    public bool CheckDoubleSwap()
    {
        return swapItem && GameRemoteConfigValue.bonusTime && swapped && Time.time - timeCollectItem < 0.32f;
    }

    public void ResetDoubleSwap()
    {
        swapped = false;
    }


    // Start is called before the first frame update
    void Start()
    {
        gameplayCount++;
        Initialize();
        _ = PlayLevel(level, false);
    }

    void OnDestroy()
    {
        EventBus<GameStateChangeEvent>.Deregister(gameStateChangeEvent);
        timeManager?.OnDestroy();
        moveManager?.OnDestroy();
    }

    private void Initialize()
    {
        level = MySonatFramework.userDataService.GetLevel();
        gameStateChangeEvent = new EventBinding<GameStateChangeEvent>(OnGameStateChangedEvent);
        PanelManager.Instance.OnPanelsUpdated += OnPanelsUpdated;
        levelGenerator.Init();
    }

    private void ChangeGameState(GameState newGameState)
    {
        EventBus<GameStateChangeEvent>.Raise(new GameStateChangeEvent() { gameState = newGameState });
    }

    private void OnGameStateChangedEvent(GameStateChangeEvent eventData)
    {
        gameState = eventData.gameState;
        timeManager.OnGameStateChanged(gameState);
        moveManager.OnGameStateChanged(gameState);
    }


    public async UniTask PlayLevel(int level, bool loading = true, bool isReplay = false, bool startPlay = true, Action OnLevelLoaded = null)
    {
        if (loading)
            PanelManager.Instance.OpenPanel<PopupLoading>(new UIData().Add("Time", 1f));

        ClearLevel();
        await levelGenerator.GenerateLevel(level, isReplay);

        targetData.Clear();
        targetData.AddRange(levelGenerator.LevelData.targetData);

        OnLoadLevel?.Invoke(level);

        // StartPlay();
        timeManager.timeCounting = false;
        moveManager.moveCounting = false;

        MySonatFramework.audioService.PlayMusic(GetBackgroundMusic(), true, 0.5f);
        await UniTask.Delay(1000);
        if (!loading && LoadingScreenInstance.Instance.IsShowing)
        {
            LoadingScreenInstance.Instance.Hide();
        }

        OnLevelLoaded?.Invoke();
        if (startPlay)
            StartPlay().Forget();
    }

    private static AudioId _bgMusic;
    public static AudioId BgMusic => _bgMusic;

    public static AudioId GetBackgroundMusic()
    {
        if (LocalizationUtils.IsJapanese())
        {
            var id = UnityEngine.Random.Range(0, 2);
            Debug.Log("anhnt: bgm ingame japan random = " + id);
            _bgMusic = id == 0 ? AudioId.BGM_Ingame_Japan_Grill_sort : AudioId.BGM_Ingame_Japan_01_Grill_sort;
        }
        else
        {
            _bgMusic = GameRemoteConfigValue.bgrMusic;
        }

        return _bgMusic;
    }

    public async UniTask StartPlay()
    {
        ChangeGameState(GameState.UsingBooster);
        var blockPanel = PanelManager.Instance.OpenPanel<BlockPanel>();
        EventBus<LevelStartedEvent>.Raise(new LevelStartedEvent() { level = level });

        await UniTask.WaitUntil(UIFlowController.CheckConditionStartPlay);
        await UniTask.Delay(200);
        blockPanel.Close();
        ChangeGameState(GameState.Playing);
    }

    // private void CheckLives(float delay = 0.5f)
    // {
    //     if (MySonatFramework.livesService.CanPlay())
    //     {
    //         StartPlay();
    //     }
    //     else
    //     {
    //         SonatUtils.DelayCall(delay, () =>
    //         {
    //             PanelManager.Instance.OpenPanel<PopupRefillLives>(new UIData().Add(UIDataKey.CallBackOnClose, (Action)(() => CheckLives(0))));
    //             PopupToast.Cretate("No more lives left!");
    //         }, this);
    //     }
    // }

    public void ClearLevel()
    {
        levelGenerator.ClearLevel();
        timeManager.ClearData();
        itemCount = 0;
        timeToAdd = 0;
        gameplayScreen.ClearLevel();
        bonusRewardData = new();
        if (forceCheckWinCoroutine != null)
        {
            StopCoroutine(forceCheckWinCoroutine);
            forceCheckWinCoroutine = null;
        }
    }

    public async UniTaskVoid NextLevel()
    {
        level = MySonatFramework.userDataService.GetLevel();
        _ = PlayLevel(level, true);
    }

    public async UniTask Win()
    {
        if (gameState == GameState.GameOver)
            return;

        try
        {

            PanelManager.Instance.CloseAllPanel();
            BlockPanel blockPanel = PanelManager.Instance.OpenPanel<BlockPanel>();
            ChangeGameState(GameState.GameOver);

            levelGenerator.OnGameOver();

            // ✅ Xử lý item vàng, an toàn
            try
            {
                ProcessGoldenBarItems();
            }
            catch (Exception e)
            {
                Debug.LogError($"[Win] Error in ProcessGoldenBarItems: {e}");
            }

            // Gọi event kết thúc màn
            try
            {
                OnLevelEnd?.Invoke(true);
            }
            catch (Exception e)
            {
                Debug.LogError($"[Win] Error invoking OnLevelEnd: {e}");
            }

            // ✅ Cộng thưởng bonus nếu có
            try
            {
                if (bonusRewardData?.resourceDatas != null && bonusRewardData.resourceDatas.Count > 0)
                    MySonatFramework.inventoryService.AddReward(bonusRewardData, null, true);
            }
            catch (Exception e)
            {
                Debug.LogError($"[Win] Error adding bonus reward: {e}");
            }

            // ✅ Raise event an toàn
            try
            {
                EventBus<LevelEndedEvent>.Raise(new LevelEndedEvent()
                {
                    gameMode = GameMode.Classic,
                    level = level,
                    success = true
                });
            }
            catch (Exception e)
            {
                Debug.LogError($"[Win] Error raising LevelEndedEvent: {e}");
            }

            await UniTask.Delay(1200);
            blockPanel.Close();
            MySonatFramework.audioService.StopMusic();

            // ✅ Mở PopupPreWin (nếu fail thì vẫn tiếp tục)
            PopupPreWin popupPreWin = await SafeOpenPanelAsync<PopupPreWin>();
            if (popupPreWin != null)
            {
                await UniTask.Delay((int)(popupPreWin.delay * 1000));
            }
            else
            {
                Debug.LogWarning("[Win] PopupPreWin failed to open, continue to WinPanel.");
            }

            if (forceCheckWinCoroutine != null)
            {
                StopCoroutine(forceCheckWinCoroutine);
                forceCheckWinCoroutine = null;
            }

            MySonatFramework.audioService.PlaySound(AudioId.Win_Music);

            // ✅ Mở WinPanel
            var data = new WinPanel.Data()
            {
                level = level,
                reward = gameConfig.config.GetWinReward(levelGenerator.LevelData.difficulty),
                nextLevel = () => NextLevel().Forget()
            };

            try
            {
                PanelManager.Instance.OpenForget<WinPanel>(data);
            }
            catch (Exception e)
            {
                Debug.LogError($"[Win] Failed to open WinPanel: {e}");
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"[Win] Unexpected error in Win(): {e}");
        }
    }

    // 🛡️ Helper: mở panel an toàn, không crash flow nếu lỗi
    private async UniTask<T> SafeOpenPanelAsync<T>() where T : Panel
    {
        try
        {
            return await PanelManager.Instance.OpenPanelAsync<T>();
        }
        catch (Exception e)
        {
            Debug.LogError($"[SafeOpenPanelAsync] Failed to open {typeof(T).Name}: {e}");
            return null;
        }
    }

    public async UniTaskVoid Stuck(StuckType stuckType)
    {
        await UniTask.WaitUntil(() => gameState == GameState.Playing);
        if (gameState == GameState.GameOver) return;
        gameState = GameState.GameOver;
        levelGenerator.OnGameOver();
        PanelManager.Instance.CloseAllPanel();
        ChangeGameState(GameState.GameOver);
        BlockPanel blockPanel = PanelManager.Instance.OpenPanel<BlockPanel>();
        EventBus<LevelStuckEvent>.Raise(new LevelStuckEvent() { gameMode = GameMode.Classic, level = level, cause = stuckType.ToString().ToLogString() });
        await UniTask.Delay(1000);
        PanelManager.Instance.CloseAllPanel();
        ChangeGameState(GameState.GameOver);

        PopupContinue.Data data = new PopupContinue.Data()
        {
            onPlayOn = (by, objectParams) => Revive(stuckType, by, objectParams).Forget(),
            onClose = () => Lose().Forget(),
            stuckType = stuckType
        };

        PanelManager.Instance.OpenForget<PopupWarningStuck>(data);
    }

    public async UniTaskVoid MissedOrder(OrderList.Data orderData, StuckType stuckType)
    {
        if (gameState == GameState.GameOver) return;
        await UniTask.WaitUntil(() => gameState == GameState.Playing);
        gameState = GameState.GameOver;
        levelGenerator.OnGameOver();
        PanelManager.Instance.CloseAllPanel();
        ChangeGameState(GameState.GameOver);
        BlockPanel blockPanel = PanelManager.Instance.OpenPanel<BlockPanel>();
        EventBus<LevelStuckEvent>.Raise(new LevelStuckEvent() { gameMode = GameMode.Classic, level = level, cause = "order_fail" });
        await UniTask.Delay(1000);
        PanelManager.Instance.CloseAllPanel();
        ChangeGameState(GameState.GameOver);

        PopupContinue.Data data = new PopupContinue.Data()
        {
            onPlayOn = (by, objectParams) => Revive(stuckType, by, objectParams).Forget(),
            onClose = () => Lose().Forget(),
            stuckType = stuckType
        };

        data.Add("OrderListData", orderData);

        if (stuckType == StuckType.OutOfItemOrder)
        {
            PanelManager.Instance.OpenPanelByName<PopupOutOfTimeOrder>("PopupOutOfItemOrder", data);
        }
        else
        {
            PanelManager.Instance.OpenForget<PopupOutOfTimeOrder>(data);
        }
    }

    private float lastTimeCheckOutOfMove;

    public async UniTaskVoid CheckOutOfMove()
    {
        if (gameState == GameState.GameOver || Time.time - lastTimeCheckOutOfMove < 0.5f) return;
        await UniTask.Delay(100);
        lastTimeCheckOutOfMove = Time.time;
        if (level == 6)
        {
            if (!levelGenerator.CheckHasMatch() && gameplayScreen.SuggestLockAds())
                return;
        }

        if (levelGenerator.CheckOutOfMove())
        {
            //Stuck(StuckType.OutOfMove).Forget();
            gameplayScreen.ForceCheckSuggest();
        }
    }

    public async UniTask ExplosiveBomb()
    {
        if (gameState == GameState.GameOver) return;
        await UniTask.WaitUntil(() => gameState == GameState.Playing);
        gameState = GameState.GameOver;
        ChangeGameState(GameState.GameOver);
        PanelManager.Instance.CloseAllPanel();
        ChangeGameState(GameState.GameOver);
        //MySonatFramework.livesService.ReduceLive(1, "explosive_bomb");
        MySonatFramework.livesService.ReduceLive(1, new() { earnType = "lose", earnId = "explosive_bomb" });
        BlockPanel blockPanel = PanelManager.Instance.OpenPanel<BlockPanel>();
        EventBus<LevelStuckEvent>.Raise(new LevelStuckEvent() { gameMode = GameMode.Classic, level = level, cause = "explosive_bomb" });
        await UniTask.Delay(1500);
        PanelManager.Instance.CloseAllPanel();
        ChangeGameState(GameState.GameOver);
        Lose().Forget();
    }

    public async UniTask OnOvercooked()
    {
        if (gameState == GameState.GameOver) return;
        await UniTask.WaitUntil(() => gameState == GameState.Playing);
        gameState = GameState.GameOver;
        ChangeGameState(GameState.GameOver);
        PanelManager.Instance.CloseAllPanel();
        ChangeGameState(GameState.GameOver);
        MySonatFramework.livesService.ReduceLive(1, new() { earnType = "lose", earnId = "overcooked" });
        BlockPanel blockPanel = PanelManager.Instance.OpenPanel<BlockPanel>();
        EventBus<LevelStuckEvent>.Raise(new LevelStuckEvent() { gameMode = GameMode.Classic, level = level, cause = "overcooked" });
        await UniTask.Delay(1500);
        PanelManager.Instance.CloseAllPanel();
        ChangeGameState(GameState.GameOver);
        Lose().Forget();
    }

    public async UniTask OnDynamiteExploded()
    {
        if (gameState == GameState.GameOver) return;
        await UniTask.WaitUntil(() => gameState == GameState.Playing);
        gameState = GameState.GameOver;
        ChangeGameState(GameState.GameOver);
        PanelManager.Instance.CloseAllPanel();
        ChangeGameState(GameState.GameOver);
        MySonatFramework.livesService.ReduceLive(1, new() { earnType = "lose", earnId = "dynamite_exploded" });
        BlockPanel blockPanel = PanelManager.Instance.OpenPanel<BlockPanel>();
        EventBus<LevelStuckEvent>.Raise(new LevelStuckEvent() { gameMode = GameMode.Classic, level = level, cause = "dynamite_exploded" });
        await UniTask.Delay(1500);
        PanelManager.Instance.CloseAllPanel();
        ChangeGameState(GameState.GameOver);
        Lose().Forget();
    }

    private async UniTaskVoid Lose()
    {
        OnLevelEnd?.Invoke(false);
        EventBus<LevelEndedEvent>.Raise(new LevelEndedEvent() { gameMode = GameMode.Classic, level = level, success = false });
        MySonatFramework.audioService.StopMusic();
        MySonatFramework.audioService.PlaySound(AudioId.Lose_Music);
        // ---old---
        // LosePanel.Data data = new LosePanel.Data()
        // {
        //     onRetryClick = () => ReplayLoadLevel()
        // };
        // PanelManager.Instance.OpenForget<LosePanel>(data);

        // ---new---
        var consecutiveWinService = MySonatFramework.GetService<ConsecutiveWinService>();
        if (consecutiveWinService.CheckStart(level))
        {
            var uiData = new UIData();
            uiData.Add("txtPlay", "Try Again");
            uiData.Add("OnPlay", (Action)(() =>
            {
                PanelManager.Instance.CloseAllPanel();
                _ = PlayLevel(level, true, true);
            }));
            uiData.Add("OnClose", (Action)(() =>
            {
                LoadingScreenInstance.Instance.Show(1.5f);
                SonatUtils.DelayCall(0.5f, () => { MySonatFramework.GetService<SceneService>().SwitchScene(GamePlacement.Home); });
            }));
            // uiData.Add("HideClose", true);
            PanelManager.Instance.OpenPanel<PopupPrePlay>(uiData);
        }
        else
        {
            _ = PlayLevel(level, true, true);
        }
    }
    private async UniTaskVoid Revive(StuckType stuckType, string by, object[] objectParams = null)
    {
        ChangeGameState(GameState.Playing);
        switch (by)
        {
            case "play_on_coin":
                switch (stuckType)
                {
                    case StuckType.OutOfTime:
                        // Nhận giá trị time từ objectParams (đã được tính toán với multiplier trong PopupContinue)
                        var timeReviveCoin = GameRemoteConfigValue.timeRevive;
                        if (objectParams != null && objectParams.Length > 0)
                        {
                            timeReviveCoin = (int)objectParams[0];
                        }
                        timeManager.AddTime(timeReviveCoin);
                        break;
                    case StuckType.OutOfMove:
                        if (levelGenerator.LevelMode == LevelMode.Target)
                        {
                            moveManager.AddMove(moveOnRevive);
                        }
                        else
                        {
                            _ = BoosterMagnet();
                            timeManager.CheckCountTime();
                        }
                        break;
                    case StuckType.OutOfTimeShipper:
                    case StuckType.OutOfMoveShipper:
                        SkipShipper();
                        timeManager.CheckCountTime();
                        break;
                }

                break;
            case "play_on_ads":
                switch (stuckType)
                {
                    case StuckType.OutOfTime:
                        if (loseRwdService.Config.active)
                            timeManager.AddTime(loseRwdService.GetCurrentStageData().addTimeSeconds);
                        else
                            timeManager.AddTime(GameRemoteConfigValue.timeRevive);
                        break;
                    case StuckType.OutOfMove:
                        if (levelGenerator.LevelMode == LevelMode.Target)
                        {
                            moveManager.AddMove(moveOnRevive);
                        }
                        else
                        {
                            _ = BoosterMagnet();
                            timeManager.CheckCountTime();
                        }
                        break;
                    case StuckType.OutOfTimeShipper:
                    case StuckType.OutOfMoveShipper:
                        SkipShipper();
                        timeManager.CheckCountTime();
                        break;
                }

                break;
            case "play_on_offer":
                switch (stuckType)
                {
                    case StuckType.OutOfTime:
                        var timeRevive = 60;
                        if (objectParams != null && objectParams.Length > 0)
                        {
                            timeRevive = (int)objectParams[0];
                        }

                        timeManager.AddTime(timeRevive);
                        break;
                    case StuckType.OutOfMove:
                        if (levelGenerator.LevelMode == LevelMode.Target)
                        {
                            moveManager.AddMove(moveOnRevive);
                        }
                        else
                        {
                            _ = BoosterMagnet();
                            timeManager.CheckCountTime();
                        }
                        break;
                    case StuckType.OutOfTimeShipper:
                    case StuckType.OutOfMoveShipper:
                        SkipShipper();
                        timeManager.AddTime(60);
                        break;
                }

                break;
        }

        EventBus<LevelContinueEvent>.Raise(new LevelContinueEvent() { by = by });
        //Continue
    }

    public async UniTaskVoid Replay()
    {
        if (gameState == GameState.GameOver) return;
        ChangeGameState(GameState.GameOver);
        string cause = levelGenerator.CheckOutOfMove() ? "retry_out_of_move" : "replay";
        EventBus<LevelQuitEvent>.Raise(new LevelQuitEvent() { cause = cause });
        EventBus<LevelEndedEvent>.Raise(new LevelEndedEvent() { gameMode = GameMode.Classic, level = level, success = false });
        OnReplay?.Invoke();

        var consecutiveWinService = MySonatFramework.GetService<ConsecutiveWinService>();
        if (consecutiveWinService.CheckStart(level))
        {
            _ = PlayLevel(level, true, true, false, () =>
            {
                ChangeGameState(GameState.Paused);
                var uiData = new UIData();
                uiData.Add("txtPlay", "Play");
                uiData.Add("OnPlay", (Action)(() => { StartPlay().Forget(); }));
                uiData.Add("OnClose", (Action)(() => { StartPlay().Forget(); }));
                PanelManager.Instance.OpenPanel<PopupPrePlay>(uiData);
            });
        }
        else
        {
            _ = PlayLevel(level, true, true);
        }
    }


    // private Coroutine countTimeCoroutine;
    // private bool timeCounting = false;
    //
    // private void StartCountTime()
    // {
    //     timeRemaining = levelGenerator.LevelData.time;
    //     if (timeRemaining < 10) timeRemaining = 180;
    //     if (countTimeCoroutine != null)
    //     {
    //         StopCoroutine(countTimeCoroutine);
    //     }
    //
    //     countTimeCoroutine = StartCoroutine(CountTime());
    // }
    //
    // public void AddTime(int seconds)
    // {
    //     timeRemaining += seconds;
    //     gameplayScreen.UpdateTime((int)timeRemaining);
    //     if (countTimeCoroutine == null)
    //     {
    //         countTimeCoroutine = StartCoroutine(CountTime());
    //     }
    // }

    // [HideInInspector] public bool isFreeze = false;
    // private int timeFreeze;
    // private Coroutine freezeCoroutine;
    //
    // private IEnumerator CountTime()
    // {
    //     yield return new WaitForSeconds(1);
    //     while (timeRemaining > 0)
    //     {
    //         if (gameState == GameState.Playing)
    //         {
    //             if (!isFreeze)
    //             {
    //                 gameplayScreen.UpdateTime(Mathf.FloorToInt(timeRemaining));
    //                 timeRemaining -= Time.deltaTime;
    //             }
    //         }
    //         else if (gameState == GameState.GameOver)
    //         {
    //             countTimeCoroutine = null;
    //             yield break;
    //         }
    //
    //         yield return null;
    //     }
    //
    //     yield return new WaitForSeconds(1);
    //     if (gameState == GameState.GameOver)
    //     {
    //         countTimeCoroutine = null;
    //         yield break;
    //     }
    //
    //     gameplayScreen.UpdateTime(0);
    //     countTimeCoroutine = null;
    //     Stuck(StuckType.OutOfTime).Forget();
    // }
    //
    // public int GetTimeRemaining()
    // {
    //     return Mathf.FloorToInt(timeRemaining);
    // }

    public async UniTask BoosterFreeze()
    {
        CheckCountTime();
        timeManager.Freeze();
        OnUseBooster?.Invoke(GameResource.BoosterFreeze);
        await UniTask.Delay(1000);
    }

    // private IEnumerator IEFreeze()
    // {
    //     gameplayScreen.SetFreezeState(true);
    //     while (timeFreeze > 0)
    //     {
    //         if (gameState == GameState.Playing)
    //         {
    //             timeFreeze--;
    //         }
    //         else if (gameState == GameState.GameOver)
    //         {
    //             freezeCoroutine = null;
    //             yield break;
    //         }
    //
    //         yield return new WaitForSeconds(1);
    //     }
    //
    //     freezeCoroutine = null;
    //     timeFreeze = 0;
    //     isFreeze = false;
    //     gameplayScreen.SetFreezeState(false);
    // }

    public void SkipShipper()
    {
        OrderEntity shipper = levelGenerator.GetOrderQueue().GetShipperOrderEntity();
        if (shipper != null)
        {
            _ = shipper.MoveOut(false);
        }
    }

    public async UniTask BoosterShuffle()
    {
        CheckCountTime();
        OnUseBooster?.Invoke(GameResource.BoosterShuffle);
        await levelGenerator.Shuffle();
    }

    public async UniTask BoosterMagnet()
    {
        CheckCountTime();
        OnUseBooster?.Invoke(GameResource.BoosterMagnet);
        await levelGenerator.Magnet();
    }

    public async UniTask BoosterMagicWand()
    {
        CheckCountTime();
        await levelGenerator.MagicWand();
    }

    public bool CheckAppearBooster(GameResource boosterType)
    {
        switch (boosterType)
        {
            // case GameResource.BoosterBlowTorch:
            //     return levelGenerator.CanUseBoosterBlowTorch();
            default:
                return true;
        }
    }

    public bool CanUseBooster(GameResource boosterType)
    {
        switch (boosterType)
        {
            case GameResource.BoosterMagicWand:
                return levelGenerator.CanUseBoosterMagicWand();
            case GameResource.BoosterMagicKey:
                return levelGenerator.CanUseBoosterMagicKey();
            case GameResource.BoosterBlowTorch:
                return levelGenerator.CanUseBoosterBlowTorch();
            default:
                return true;
        }
    }

    #region Booster Magic Key

    public async UniTask BoosterMagicKey()
    {
        CheckCountTime();
        OnUseBooster?.Invoke(GameResource.BoosterMagicKey);
        await levelGenerator.MagicKey();
    }

    #endregion

    #region Booster Blow Torch

    public async UniTask BoosterBlowTorch()
    {
        CheckCountTime();
        OnUseBooster?.Invoke(GameResource.BoosterBlowTorch);
        await levelGenerator.BlowTorch();
    }

    #endregion

    #region Booster Match3Tag

    public async UniTask BoosterMatch3Target()
    {
        CheckCountTime();
        OnUseBooster?.Invoke(GameResource.BoosterMagnet);
        await levelGenerator.Match3Target();
    }

    #endregion

    public int GetLevelTime()
    {
        return levelGenerator.LevelData.time + timeToAdd;
    }

    public int GetLevelMove()
    {
        return levelGenerator.LevelData.move;
    }

    private void CheckCountTime()
    {
        if (!timeManager.timeCounting)
        {
            timeManager.timeCounting = true;
            timeManager.StartCountTime(GetLevelTime());
        }
    }

    private int timeToAdd = 0;

    public void AddTimeWhenStart(int seconds)
    {
        if (timeManager.timeCounting == false)
        {
            timeToAdd += seconds;
            var time = levelGenerator.LevelData.time + timeToAdd;
            gameplayScreen.UpdateTime((int)time);
        }
        else
        {
            timeManager.AddTime(seconds);
        }
    }

    //#if UNITY_EDITOR
    private void Update()
    {
        // if (Input.GetKeyDown(KeyCode.Space))
        // {
        //     //BoosterShuffle();
        //     //Replay();
        //     BoosterMagnet();
        // }

        if (Input.GetMouseButton(0) && itemSelected != null)
        {
            itemSelected.OnMoveItem();
        }
        else if (Input.GetMouseButtonUp(0) && itemSelected != null)
        {
            itemSelected.OnUnSelectItem();
        }
    }
    //#endif

    public bool CanUseBooster(float timeGap)
    {
        return Time.time - timeCollectItem > timeGap;
    }

    public void SetLastTimeCollectItem(float timeCollectItem)
    {
        this.timeCollectItem = timeCollectItem;
    }

    public void SetBonusTime(PrimaryGrill grill)
    {
        this.bonusTimeGrill = grill;
    }

    public PrimaryGrill GetBonusTimeGrill()
    {
        return this.bonusTimeGrill;
    }

    /// <summary>
    /// Process items with id 1500 when winning level: hide them and add stars for each remaining item
    /// </summary>
    private void ProcessGoldenBarItems()
    {
        try
        {
            List<Vector3> goldenBarPositions = new List<Vector3>();
            var starChestService = MySonatFramework.GetService<StarChestService>();

            foreach (var primaryGrill in levelGenerator.GetPrimaryGrills())
            {
                if (primaryGrill == null) continue;
                foreach (var slot in primaryGrill.GetSlots())
                {
                    if (slot == null) continue;
                    var item = slot.GetItem();

                    if (item != null && item.id == 1500)
                    {
                        // 🔴 Giả lập lỗi giữa vòng lặp
                        if (simulateError_ProcessGolden && UnityEngine.Random.value < 0.5f)
                            throw new Exception("Simulated error inside ProcessGoldenBarItems!");

                        if (item.Visual != null)
                            item.Visual.gameObject.SetActive(false);

                        goldenBarPositions.Add(slot.transform.position);
                    }
                }
            }

            if (goldenBarPositions.Count > 0 && starChestService != null)
            {
                foreach (var pos in goldenBarPositions)
                {
                    starChestService.OnCollectItem(pos);
                }
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"[ProcessGoldenBarItems] Fatal error: {e}");
        }
    }

    #region Target Mode Methods

    /// <summary>
    /// Trừ số lượng item trong targetData khi collect
    /// </summary>
    /// <param name="itemId">ID của item được collect</param>
    /// <param name="amount">Số lượng item collect được (= size của grill)</param>
    private void UpdateTargetProgress(int itemId, int amount)
    {
        var target = targetData.Find(t => t.id == itemId);
        if (target != null && target.quantity > 0)
        {
            target.quantity -= amount;
            if (target.quantity < 0) target.quantity = 0; // Không cho âm
            OnTargetUpdate?.Invoke(itemId, target.quantity);
        }
    }

    /// <summary>
    /// Check xem tất cả target đã hoàn thành chưa
    /// </summary>
    private bool CheckTargetComplete()
    {
        foreach (var target in targetData)
        {
            if (target.quantity > 0)
                return false;
        }
        return true;
    }

    /// <summary>
    /// Get số lượng còn lại của một target item
    /// </summary>
    public int GetTargetRemaining(int itemId)
    {
        var target = targetData.Find(t => t.id == itemId);
        return target?.quantity ?? 0;
    }

    #endregion

    /// <summary>
    /// Spawn combo text at position if current combo level has text configured
    /// </summary>
    private void SpawnComboText(Vector3 position)
    {
        if (comboService == null || comboService.Config == null)
            return;

        int currentCombo = comboService.Combo;

        // Check if this combo level has a text to display
        if (currentCombo <= 0 || !comboService.Config.HasComboText(currentCombo))
            return;

        // Get prefab name for this combo level
        string prefabName = comboService.Config.GetComboPrefabName(currentCombo);

        if (string.IsNullOrEmpty(prefabName))
            return;

        // Get parent transform
        Transform parent = gameplayScreen.ComboTextContainer.transform;

        // Offset position to appear above the tray (adjust this value as needed)
        Vector3 offsetPosition = position + Vector3.up * 1.5f;  // Spawn 1.5 units above tray

        // Convert world position to local position in parent
        Vector3 localPosition = parent.InverseTransformPoint(offsetPosition);

        // Create combo text object from pool using the specific prefab
        var comboText = SonatSystem.GetService<PoolingService>().Create<ComboTextObject>(
            prefabName,
            parent  // Only pass parent, no position
                    // No args needed - prefab is pre-configured
        );

        // Set local position AFTER creation to avoid coordinate conflicts
        comboText.transform.localPosition = localPosition;
    }
}