using Sonat.Enums;
using SonatFramework.Systems.InventoryManagement;
using UnityEngine;

namespace SonatFramework.Systems.EventBus
{
    public interface IEvent
    {
    }

    public struct OpenGameEvent : IEvent
    {
    }

    public struct LevelStartedEvent : IEvent
    {
        public GameMode gameMode;
        public int level;
        public int phase;
    }

    public struct LevelEndedEvent : IEvent
    {
        public GameMode gameMode;
        public int level;
        public bool success;
        public int phase;
    }

    public struct LevelQuitEvent : IEvent
    {
        public string cause;
    }

    public struct LevelStuckEvent : IEvent
    {
        public GameMode gameMode;
        public int level;
        public string cause;
    }

    public struct LevelContinueEvent : IEvent
    {
        public string by;
        public string currencyName;
        public string packId;
        public int valueRevive;
    }

    public struct GameStateChangeEvent : IEvent
    {
        public GameState gameState;
    }

    public struct PhaseStartedEvent : IEvent
    {
        public GameMode gameMode;
        public int level;
        public int phase;
    }

    public struct PhaseEndedEvent : IEvent
    {
        public GameMode gameMode;
        public int level;
        public int phase;
        public bool success;
    }

    public struct UseBoosterEvent : IEvent
    {
        public GameResource booster;
    }

    public struct SwitchPlacementEvent : IEvent
    {
        public GamePlacement from;
        public GamePlacement to;
        public Status status;

        public enum Status : byte
        {
            Start = 0,
            End = 1,
        }
    }

    public struct UpdatePlacementEvent : IEvent
    {
        public string placement;
    }

    public struct UpdateScreenEvent : IEvent
    {
        public string screen;
    }

    public struct ClickShortcutEvent : IEvent
    {
        public string shortcut;
    }

    public struct EarnResourceEvent : IEvent
    {
        public GameResource resource;
        public int value;
        public string spendType;
        public string spendId;
        public bool isFirstBuy;
        public string source;
        public int price;

        public EarnResourceEvent(GameResource gameResource, int value, EarnResourceLogData logData)
        {
            resource = gameResource;
            this.value = value;
            spendType = logData.spendType;
            spendId = logData.spendId;
            isFirstBuy = logData.isFirstBuy;
            source = logData.source;
            price = logData.price;
        }
    }

    public struct SpendResourceEvent : IEvent
    {
        public GameResource resource;
        public int value;
        public string earnType;
        public string earnId;
        public string source;
        public int price;

        public SpendResourceEvent(GameResource gameResource, int value, SpendResourceLogData logData)
        {
            resource = gameResource;
            this.value = value;
            earnType = logData.earnType;
            earnId = logData.earnId;
            source = logData.source;
            price = logData.price;
        }
    }

    public struct AddItemEvent : IEvent
    {
        public GameResource resource;
        public int quantity;
        public Vector3 position;
        public SonatCollectEffect collectEffect;
    }

    public struct ReduceItemEvent : IEvent
    {
        public GameResource resource;
        public int quantity;
    }

    public struct AddLimitItemEvent : IEvent
    {
        public GameResource resource;
        public int quantity;
    }
}