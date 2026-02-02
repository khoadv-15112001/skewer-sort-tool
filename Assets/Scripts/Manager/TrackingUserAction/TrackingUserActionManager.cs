using System;
using System.Collections.Generic;
using Gameplay.Entities;
using Newtonsoft.Json;
using Sonat.Enums;
using SonatFramework.Systems.EventBus;
using SonatFramework.Systems.LoadObject;
using UnityEngine;

namespace Manager.TrackingUserAction
{
    public class TrackingUserActionManager: MonoBehaviour
    {
        private TrackingUserActionData trackingUserActionData;
        [SerializeField] private SaveObjectService saveObjectService;
        private EventBinding<LevelStartedEvent> levelStartedEvent;
        private EventBinding<LevelEndedEvent> levelEndedEvent;
        private EventBinding<GameStateChangeEvent> changeGameStateEvent;
        private float startTime;
        private bool tracking;

        private void Awake()
        {
            DontDestroyOnLoad(gameObject);
        }

        private void Start()
        {
            levelStartedEvent = new EventBinding<LevelStartedEvent>(OnLevelStarted);
            levelEndedEvent = new EventBinding<LevelEndedEvent>(OnLevelEndEvent);
            changeGameStateEvent = new EventBinding<GameStateChangeEvent>(OnChangeGameState);
            
            GameplayController.OnSelectItem += OnSelectItem;
            GameplayController.OnDropItem += OnDropItem;
            GameplayController.OnItemMoveSlot += OnItemMoved;
            GameplayController.OnUseBooster += OnUseBooster;
        }

        private void OnLevelStarted(LevelStartedEvent eventData)
        {
            trackingUserActionData = new TrackingUserActionData();
            trackingUserActionData.level = eventData.level;
            trackingUserActionData.screenWidth = Screen.width;
            trackingUserActionData.screenHeight = Screen.height;
            for (GameResource gameResource = GameResource.Coin; gameResource < GameResource.Star; gameResource++)
            {
                trackingUserActionData.userResources.Add(gameResource, MySonatFramework.inventoryService.GetResource(gameResource));
            }
            startTime = Time.time;
            tracking = true;
        }

        private void OnSelectItem(Item item)
        {
            if(!tracking || item == null) return;
            var actionData = new UserActionData()
            {
                actionType = UserActionType.SelectItem,
                id = item.Slot.id,
                time = Time.time - startTime,
            };
            trackingUserActionData.AddAction(actionData);
        }

        private void OnDropItem(Item item, bool changeSlot)
        {
            
        }

        private void OnItemMoved(Item item, SlotBase slot)
        {
            if(!tracking) return;
            var actionData = new UserActionData()
            {
                actionType = UserActionType.DropItem,
                id = slot.id,
                time = Time.time - startTime,
            };
            trackingUserActionData.AddAction(actionData);
        }
        
        

        private void OnUseBooster(GameResource booster)
        {
            if(!tracking) return;
            var actionData = new UserActionData()
            {
                actionType = UserActionType.UseBooster,
                id = (int)booster,
                time = Time.time - startTime,
            };
            trackingUserActionData.AddAction(actionData);
            if (booster == GameResource.BoosterShuffle)
            {
                tracking = false;
            }
        }

        private void OnChangeGameState(GameStateChangeEvent eventData)
        {
            if(!tracking) return;
            if (eventData.gameState != GameState.Playing && eventData.gameState != GameState.Paused) return;
            var actionData = new UserActionData()
            {
                actionType = UserActionType.ChangeGameState,
                id = (int)eventData.gameState,
                time = Time.time - startTime,
            };
            trackingUserActionData.AddAction(actionData);
        }

        private void OnLevelEndEvent(LevelEndedEvent eventData)
        {
            if(!tracking) return;
            saveObjectService.SaveObject(trackingUserActionData, $"track_{trackingUserActionData.level}");
        }
    }

    public class TrackingUserActionData
    {
        public int level;
        public int session;
        public int screenWidth;
        public int screenHeight;
        public Dictionary<GameResource, int> userResources = new Dictionary<GameResource, int>();
        public List<UserActionData> actionDataList = new List<UserActionData>();

        public void AddAction(UserActionData actionData)
        {
            actionDataList.Add(actionData);
        }
    }

    public class UserActionData
    {
        public UserActionType actionType;
        public int id;
        public float time;
    }

    public enum UserActionType: byte
    {
        None = 0,
        SelectItem = 1,
        DropItem = 2,
        UseBooster = 3,
        ChangeGameState = 4,
    }
}