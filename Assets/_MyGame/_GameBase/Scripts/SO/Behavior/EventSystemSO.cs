using System;
using Gameplay.Entities;
using UnityEngine;

namespace MyGame.SkewerJam.Scripts.SO.Behavior
{
    public abstract class EventSystemSO : ScriptableObject
    {
        public abstract void RegisterEvents_OnCollectItem(Action<int> onCollectItem);

        public abstract void UnregisterEvents_OnCollectItem(Action<int> onCollectItem);

        public abstract void RegisterEvents_OnDropItem(Action<Item, bool> onDropItem);

        public abstract void UnregisterEvents_OnDropItem(Action<Item, bool> onDropItem);
    }
}