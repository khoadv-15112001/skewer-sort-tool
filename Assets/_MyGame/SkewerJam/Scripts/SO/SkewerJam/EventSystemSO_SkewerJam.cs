using System;
using Gameplay.Entities;
using MyGame.SkewerJam.Gameplay;
using UnityEngine;

namespace MyGame.SkewerJam.Scripts.SO.Behavior
{
    [CreateAssetMenu(fileName = "EventSystemSO_SkewerJam", menuName = "MyGame/SkewerJam/EventSystemSO_SkewerJam")]
    public class EventSystemSO_SkewerJam : EventSystemSO
    {
        public override void RegisterEvents_OnCollectItem(Action<int> onCollectItem)
        {
            GameController.Instance.GameLogicHandler.OnCollectItem += onCollectItem;
        }

        public override void UnregisterEvents_OnCollectItem(Action<int> onCollectItem)
        {
            GameController.Instance.GameLogicHandler.OnCollectItem -= onCollectItem;
        }

        public override void RegisterEvents_OnDropItem(Action<Item, bool> onDropItem)
        {
            GameController.Instance.GameLogicHandler.OnItemStartSwitch += onDropItem;
        }

        public override void UnregisterEvents_OnDropItem(Action<Item, bool> onDropItem)
        {
            GameController.Instance.GameLogicHandler.OnItemStartSwitch -= onDropItem;
        }
    }
}