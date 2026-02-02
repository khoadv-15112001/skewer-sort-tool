using System;
using System.Collections.Generic;
using System.Linq;
using Gameplay.Entities;
using Manager;

namespace MyGame.SkewerJam.Gameplay.Helpers
{
    public static class ItemHelper
    {
        public static Dictionary<ItemId, int> GetItemIdDictInGameplay(int layer)
        {
            var grillManager = GameController.Instance.GameLogicHandler.GrillManager;
            var waitingGrillManager = GameController.Instance.GameLogicHandler.WaitingGrillManager;

            var listItemIds = GrillHelper.GetItemIdListWithLayer(layer);
            foreach (var waitingGrill in waitingGrillManager.ListWaitingGrills)
            {
                var slot = waitingGrill.GetSlots()[0];
                var item = slot.GetItem();
                if (item != null)
                {
                    listItemIds.Add((ItemId)item.id);
                }
            }

            return listItemIds.GroupBy(e => e).ToDictionary(e => e.Key, e => e.Count());
        }

        public static List<ItemId> GetItemIdsInLockedGrill()
        {
            var grillManager = GameController.Instance.GameLogicHandler.GrillManager;
            var listItemIds = new List<ItemId>();
            foreach (var grill in grillManager.ListGrills)
            {
                if (grill.IsLock)
                {
                    foreach (var slot in grill.GetSlots())
                    {
                        var item = slot.GetItem();
                        if (item != null)
                        {
                            listItemIds.Add((ItemId)item.id);
                        }
                    }
                }
                else
                {
                    foreach (var slot in grill.GetSlots())
                    {
                        var item = slot.GetItem();
                        if (item != null && item.IsLocked)
                        {
                            listItemIds.Add((ItemId)item.id);
                        }
                    }
                }
            }
            return listItemIds;
        }
    }
}