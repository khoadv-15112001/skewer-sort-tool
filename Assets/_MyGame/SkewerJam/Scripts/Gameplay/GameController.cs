using System;
using System.Linq;
using Cysharp.Threading.Tasks;
using GrillSort.Winstreak;
using MyGame.SkewerJam.Gameplay.Helpers;
using MyGame.SkewerJam.Scripts.SO.SkewerJam.Gameplay;
using Sonat.CustomService;
using Sonat.Enums;
using SonatFramework.Scripts.SonatSDKAdapterModule;
using SonatFramework.Scripts.UIModule;
using SonatFramework.Scripts.Utils;
using SonatFramework.Systems.AudioManagement;
using SonatFramework.Systems.EventBus;
using SonatFramework.Systems.InventoryManagement;
using SonatFramework.Systems.InventoryManagement.GameResources;
using UnityEngine;

namespace MyGame.SkewerJam.Gameplay
{
    public class GameController : MonoBehaviour
    {
        public static GameController Instance;

        [SerializeField] private LevelGenerator levelGenerator;
        [SerializeField] private GameLogicHandler gameLogicHandler;

        [Header("UI")][SerializeField] private GameplayScreen gameplayScreen;
        [SerializeField] private GameViewport gameViewport;

        [Header("Game config")]
        [SerializeField]
        private GameConfigHLW gameConfig;

        public GameConfigHLW GameConfig => gameConfig;


        public LevelGenerator LevelGenerator => levelGenerator;
        public GameLogicHandler GameLogicHandler => gameLogicHandler;
        public GameViewport GameViewport => gameViewport;
        public GameState GameState => gameState;
        private int level;

        public int Level => level;
        private GameState gameState;
        private EventBinding<GameStateChangeEvent> gameStateChangeEvent;

        private void Awake()
        {
            Instance = this;
        }

        void Start()
        {
            Initialize();
            PlayLevel(level).Forget();
            PanelManager.Instance.OnPanelsUpdated += OnPanelsUpdated;
        }

        private void OnPanelsUpdated()
        {
            if (PanelManager.Instance.HasAnyPopupPauseGame())
            {
                ChangeGameState(GameState.Paused);
            }
        }

        void OnDestroy()
        {
            if (PanelManager.Instance != null)
                PanelManager.Instance.OnPanelsUpdated -= OnPanelsUpdated;
        }

        private void Initialize()
        {
            gameConfig = SonatSDKAdapter.GetRemoteConfig<GameConfigHLW>("hlw_obstacle_price_config", gameConfig); 
            gameViewport.Init();
            level = MySonatFramework.userDataService.GetLevel(GameMode.SkewerJam);
            gameStateChangeEvent = new EventBinding<GameStateChangeEvent>(OnGameStateChangedEvent);
            PanelManager.Instance.OpenPanelByName<BasePanel>("GameplayPanel_SkewerJam");
        }

        private void OnGameStateChangedEvent(GameStateChangeEvent @event)
        {
            gameState = @event.gameState;
            Debug.Log("<color=green>[GameController]</color> OnGameStateChangedEvent: " + gameState);
        }

        #region Load level
        public async UniTask PlayLevel(int level, bool force = false)
        {
            ClearLevel();


            PanelManager.Instance.OpenPanelByName<PopupLoading>("PopupLoading_SkewerJam", new UIData().Add("Time", 1f));
            Debug.Log("<color=green>[GameController]</color> PlayLevel: " + level);
            SonatUtils.DelayCall(0.75f, () =>
            {
                var bgm = UnityEngine.Random.Range(0, 2) == 0 ? AudioId.BGM_Ingame_Halloween_Grill_sort : AudioId.BGM_Ingame_Halloween_01_Grill_sort;
                MySonatFramework.GetService<AudioService>().PlayMusic(bgm);
            }, this);


            this.level = level;
            gameplayScreen.InitLevel(level);
            ChangeGameState(GameState.Loading);

            Debug.Log("<color=green>[GameController]</color> PlayLevel: " + level);
            InitLevel();

            var backup = GameplayStateSaver.Instance.CheckBackup();
            if (backup == false || force == true)
            {
                await levelGenerator.GenerateLevel(level);
            }
            else
            {
                await levelGenerator.GenerateLevel(level, true);
            }

            ChangeGameState(GameState.Playing);
            EventBus<LevelStartedEvent_HLW>.Raise(new LevelStartedEvent_HLW() { level = level, gameMode = GameMode.SkewerJam });

            // if (GameplayHelper.CheckStart() == false)
            // {
            //     PanelManager.Instance.OpenPanel<PopupWarningEnergy_SkewerJam>(new UIData().Add("GamePlacement", GamePlacement.Gameplay_SkewerJam));
            // }
        }

        public void InitLevel()
        {
            levelGenerator.Init();
            gameLogicHandler.Init();
        }

        public void ClearLevel()
        {
            levelGenerator.Clear();
            gameLogicHandler.Clear();
        }

        #endregion

        public void ChangeGameState(GameState newGameState)
        {
            EventBus<GameStateChangeEvent>.Raise(new GameStateChangeEvent() { gameState = newGameState });
        }

        public async UniTaskVoid Win()
        {
            if (gameState == GameState.GameOver) return;

            EventBus<LevelEndedEvent_HLW>.Raise(new LevelEndedEvent_HLW() { level = level, gameMode = GameMode.SkewerJam, success = true });
            var newLevel = MySonatFramework.userDataService.GetLevel(GameMode.SkewerJam) + 1;
            MySonatFramework.userDataService.SaveLevel(newLevel, GameMode.SkewerJam);

            GameplayStateSaver.Instance.SetStatus(); // không lưu trạng thái
            ChangeGameState(GameState.GameOver);
            GameplayHelper.IsWin = true;
            // PopupToast.Cretate("You Win");

            await UniTask.Delay(2000);
            MySonatFramework.audioService.StopMusic();
            PopupPreWin popupPreWin = await PanelManager.Instance.OpenPanelAsync<PopupPreWin_SkewerJam>();
            await UniTask.Delay((int)(popupPreWin.delay * 1000));

            gameplayScreen.HideCurrencies();

            var log = new EarnResourceLogData()
            {
                spendType = "pumpkin",
                spendId = "pumpkin",
                source = "gameplay"
            };
            MySonatFramework.GetService<InventoryService>().AddResource(GameResource.Pumpkin, gameLogicHandler.Pumpkin, log, false);

            var data = new WinPanelBase.Data()
            {
                level = level,
                reward = new ResourceData() { resource = GameResource.Pumpkin, quantity = gameLogicHandler.Pumpkin },
                nextLevel = () => NextLevel()
            };
            PanelManager.Instance.OpenForget<WinPanel_SkewerJam>(data);
            // NextLevel();
        }

        public async UniTaskVoid Stuck(StuckType stuckType)
        {
            if (gameState == GameState.GameOver) return;

            EventBus<LevelEndedEvent_HLW>.Raise(new LevelEndedEvent_HLW() { level = level, gameMode = GameMode.SkewerJam, success = false, loseCause = stuckType.ToString(), lose = false });
            GameplayStateSaver.Instance.SetStatus();
            ChangeGameState(GameState.GameOver);

            var showPopupContinue = GameLogicHandler.WaitingGrillManager.ListWaitingGrills.Where(e => e.IsActive == false).Count() > 0;
            PopupContinue.Data data = new PopupContinue.Data()
            {
                onPlayOn = (by, objectParams) => Revive(stuckType, by, objectParams).Forget(),
                onClose = () => Lose(stuckType).Forget(),
                stuckType = stuckType
            };
            data.Add("PopupContinueName", "PopupContinue_SkewerJam");
            data.Add("UseUILevel", false);
            data.Add("ShowPopupContinue", showPopupContinue);

            PanelManager.Instance.OpenForget<PopupWarningStuck>(data);
        }

        private async UniTaskVoid Revive(StuckType stuckType, string by, object[] objectParams = null)
        {
            ChangeGameState(GameState.Playing);

            EventBus<LevelStartedEvent_HLW>.Raise(new LevelStartedEvent_HLW() { level = level, gameMode = GameMode.SkewerJam });
            await UniTask.Delay(1000);
            switch (stuckType)
            {
                case StuckType.SkewerJam_OutOfSpace:
                    switch (by)
                    {
                        case "play_on_add_trays":
                            // var orderManager = GameLogicHandler.OrderManager;
                            // orderManager.Unlock();

                            // cộng thêm 2 plates

                            var waitingManager = GameLogicHandler.WaitingGrillManager;
                            waitingManager.Unlock();
                            await UniTask.Delay(1000);
                            waitingManager.Unlock();
                            break;
                    }

                    break;
                case StuckType.SkewerJam_OutOfEnergy:
                    switch (by)
                    {
                        case "play_on_add_energies":
                            PopupToast.Cretate("Add Energies!");
                            break;
                    }

                    break;
            }
        }

        public async UniTaskVoid Lose(StuckType stuckType)
        {
            EventBus<LevelEndedEvent_HLW>.Raise(new LevelEndedEvent_HLW() { level = level, gameMode = GameMode.SkewerJam, success = false, lose = true });

            GameplayStateSaver.Instance.SetStatus(); // không lưu trạng thái
            PanelManager.Instance.OpenPanelByName<PopupLose_SkewerJam>("PopupLose_SkewerJam");
            GameplayHelper.IsWin = false;
        }

        private void NextLevel()
        {
            level = MySonatFramework.userDataService.GetLevel(GameMode.SkewerJam);
            PlayLevel(level).Forget();
        }

        public void Replay()
        {
            PlayLevel(level).Forget();
        }

        public void Continue()
        {
            ChangeGameState(GameState.Playing);
            GameplayStateSaver.Instance.SetStatus();
        }

#if UNITY_EDITOR
        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.W))
            {
                Win();
            }

            if (Input.GetKeyDown(KeyCode.L))
            {
                Stuck(StuckType.SkewerJam_OutOfSpace);
            }

            if (Input.GetKeyDown(KeyCode.Space))
            {
                PopupPreWin popupPreWin = PanelManager.Instance.OpenPanelByName<PopupPreWin>("PopupPreWin_SkewerJam");
            }
        }

#endif
        public void SetPumpkin(int currentPumpkin)
        {
            gameLogicHandler.SetPumpkin(currentPumpkin);
            EventBus<AddItemEvent>.Raise(new AddItemEvent()
            {
                resource = GameResource.Pumpkin,
                quantity = currentPumpkin,
                position = Vector3.zero,
                collectEffect = null
            });
        }
    }
}