using System;
using Gameplay.Entities;
using Sonat.Enums;
using SonatFramework.Systems.EventBus;
using SonatFramework.Systems.LoadObject;
using Spine;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Manager.TrackingUserAction
{
    public class ReplayUserAction : MonoBehaviour
    {
        [SerializeField] private LoadObjectService loadObjectService;

        private TrackingUserActionData trackingUserActionData;
        private EventBinding<LevelStartedEvent> levelStartedEvent;
        private EventBinding<LevelEndedEvent> levelEndedEvent;
        private float startTime;

        private void Awake()
        {
            DontDestroyOnLoad(gameObject);
        }

        private void Start()
        {
            levelStartedEvent = new EventBinding<LevelStartedEvent>(OnLevelStarted);
            levelEndedEvent = new EventBinding<LevelEndedEvent>(OnLevelEndEvent);
        }

        private void OnLevelStarted(LevelStartedEvent eventData)
        {
            startTime = Time.time;
            GameplayController.instance.mainCamera.eventMask = 0;
            FindObjectOfType<EventSystem>()?.gameObject.SetActive(false);
            trackingUserActionData = loadObjectService.LoadObject<TrackingUserActionData>($"track_{eventData.level}");
        }

        private void Update()
        {
            if (trackingUserActionData == null || trackingUserActionData.actionDataList.Count == 0) return;
            if (Time.time - startTime >= trackingUserActionData.actionDataList[0].time)
            {
                PlayAction(trackingUserActionData.actionDataList[0]);
                trackingUserActionData.actionDataList.RemoveAt(0);
            }
        }

        public void PlayAction(UserActionData actionData)
        {
            switch (actionData.actionType)
            {
                case UserActionType.SelectItem:
                    SelectItem(actionData.id);
                    break;
                case UserActionType.DropItem:
                    DropItem(actionData.id);
                    break;
                case UserActionType.UseBooster:
                    UseBooster((GameResource)actionData.id);
                    break;
                case UserActionType.ChangeGameState:
                    OnChangeGameState((GameState)actionData.id);
                    break;
            }
        }


        private void SelectItem(int id)
        {
            Item item = FindSlot(id)?.GetItem();
            if (item != null)
            {
                item.OnMouseDown();
            }
        }

        private void DropItem(int slotId)
        {
            SlotBase slot = FindSlot(slotId);
            if (slot != null)
            {
                GameplayController.instance.SwitchSlot(slot);
            }
        }

        private void UseBooster(GameResource booster)
        {
            switch (booster)
            {
                case GameResource.BoosterFreeze:
                    GameplayController.instance.BoosterFreeze();
                    break;
                case GameResource.BoosterMagnet:
                    GameplayController.instance.BoosterMagnet();
                    break;
                case GameResource.BoosterShuffle:
                    GameplayController.instance.BoosterShuffle();
                    break;
            }
        }

        private void OnChangeGameState(GameState gameState)
        {
            if (gameState is GameState.Playing or GameState.Paused)
            {
                EventBus<GameStateChangeEvent>.Raise(new GameStateChangeEvent() { gameState = gameState });
            }
        }

        private void OnLevelEndEvent(LevelEndedEvent eventData)
        {
        }


        public SlotBase FindSlot(int slotID)
        {
            LevelGenerator levelGenerator = GameplayController.instance.levelGenerator;
            int grillId = slotID / 1000;
            int slotIndex = slotID % 1000;
            foreach (var primaryGrill in levelGenerator.GetPrimaryGrills())
            {
                if (primaryGrill.id == grillId)
                {
                    return primaryGrill.GetSlot(slotIndex);
                }
            }

            return null;
        }
    }
}