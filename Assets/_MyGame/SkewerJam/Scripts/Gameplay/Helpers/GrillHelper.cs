using System.Collections.Generic;
using Manager;

namespace MyGame.SkewerJam.Gameplay.Helpers
{
    public static class GrillHelper
    {
        public static List<ItemId> GetItemIdListWithLayer(int layer, bool ignoreLock = false)
        {
            var listGrills = GameController.Instance.GameLogicHandler.GrillManager.ListGrills;
            var listItemIds = new List<ItemId>();

            foreach (var primaryGrill in listGrills)
            {
                if (ignoreLock == true && primaryGrill.IsLock) continue;

                // var shuffleLayerData = primaryGrill.GetShuffleLayerData();
                // if (shuffleLayerData?.layerData?.itemData == null) continue;
                // foreach (var itemData in shuffleLayerData.layerData.itemData)
                // {
                //     if (itemData is not { id: > 0 }) continue;
                //     listItemIds.Add((ItemId)itemData.id);
                // }
                if (primaryGrill.GetSlots() == null) continue;
                foreach (var slot in primaryGrill.GetSlots())
                {
                    var item = slot.GetItem();
                    if (item != null && item.id > 0)
                    {
                        if (ignoreLock == true && item.IsLocked) continue;
                        listItemIds.Add((ItemId)item.id);
                    }
                }

                var subLayerData = primaryGrill.GetSubsShuffleLayerData();
                if (subLayerData == null || subLayerData.Count == 0) continue;

                int currentLayer = 2;
                foreach (var subLayer in subLayerData)
                {
                    if (layer != -1 && currentLayer > layer) break;

                    currentLayer++;
                    if (subLayer?.layerData?.itemData == null) continue;
                    foreach (var itemData in subLayer.layerData.itemData)
                    {
                        if (itemData is not { id: > 0 }) continue;
                        listItemIds.Add((ItemId)itemData.id);
                    }
                }
            }
            return listItemIds;
        }

        public static Dictionary<int, int> GetGrillCountOrderItems()
        {
            // Đếm số order item trong các grill
            var listGrills = GameController.Instance.GameLogicHandler.GrillManager.ListGrills;
            var targetItemIds = GameController.Instance.GameLogicHandler.OrderManager.GetTargetItemIds();

            var dict = new Dictionary<int, int>();
            foreach (var primaryGrill in listGrills)
            {
                dict.TryAdd(primaryGrill.id, 0);
                foreach (var slot in primaryGrill.GetSlots())
                {
                    var item = slot.GetItem();
                    if (item != null && targetItemIds.Contains((ItemId)item.id))
                    {
                        dict[primaryGrill.id]++;
                    }
                }

                var subGrills = primaryGrill.GetSubGrills();
                if (subGrills == null || subGrills.Count == 0) continue;
                foreach (var slotInSubGrill in subGrills[0].GetSlots())
                {
                    var item = slotInSubGrill.GetItem();
                    if (item != null && targetItemIds.Contains((ItemId)item.id))
                    {
                        dict[primaryGrill.id]++;
                    }
                }
            }

            return dict;
        }
    }
}