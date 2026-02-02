using System;
using Gameplay.Entities;
using UnityEngine;

namespace MyGame.SkewerJam.Scripts.SO.Behavior
{
    [CreateAssetMenu(fileName = "EventSystemSO_GrillSort", menuName = "MyGame/GrillSort/EventSystemSO_GrillSort")]
    public class EventSystemSO_GrillSort : EventSystemSO
    {
        public override void RegisterEvents_OnCollectItem(Action<int> onCollectItem)
        {
            GameplayController.OnCollectItem += onCollectItem;
        }

        public override void UnregisterEvents_OnCollectItem(Action<int> onCollectItem)
        {
            GameplayController.OnCollectItem -= onCollectItem;
        }

        public override void RegisterEvents_OnDropItem(Action<Item, bool> onDropItem)
        {
            GameplayController.OnDropItem += onDropItem;
        }

        public override void UnregisterEvents_OnDropItem(Action<Item, bool> onDropItem)
        {
            GameplayController.OnDropItem -= onDropItem;
        }
    }
}