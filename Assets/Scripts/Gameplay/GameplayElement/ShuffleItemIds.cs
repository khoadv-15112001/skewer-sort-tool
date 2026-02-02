using System.Collections.Generic;
using Gameplay.LevelData;

namespace Gameplay.GameplayElement
{
    public class ShuffleItemIds
    {
        private readonly Dictionary<int, int> itemIds = new();

        public void SetData(LevelData.LevelData levelData)
        {
            itemIds.Clear();
            List<int> itemIdList = new List<int>();
            foreach (var grill in levelData.grillData)
            {
                if (grill.layer == null) continue;
                foreach (var layerData in grill.layer)
                {
                    for (int i = 0; i < layerData.itemData.Length; i++)
                    {
                        if (layerData.itemData[i] != null && layerData.itemData[i].id > 0)
                        {
                            if (!itemIdList.Contains(layerData.itemData[i].id))
                                itemIdList.Add(layerData.itemData[i].id);
                        }
                    }
                }
            }

            List<int> newItemIdList = new List<int>(itemIdList);
            newItemIdList.Shuffle();
            for (int i = 0; i < itemIdList.Count; i++)
            {
                itemIds.Add(itemIdList[i], newItemIdList[i]);
            }
        }

        public void Clear()
        {
            itemIds.Clear();
        }

        public int GetItemId(int orgId)
        {
            return itemIds.GetValueOrDefault(orgId, orgId);
        }
    }
}