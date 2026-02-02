using System;
using System.Collections.Generic;
using System.Linq;
using Gameplay.Entities;
using Manager;
using MyGame.SkewerJam.Level;
using UnityEngine;

namespace MyGame.SkewerJam.Gameplay.Helpers
{
    public static class OrderHelper
    {
        public static bool CheckCreateNextOrder()
        {
            var orderManager = GameController.Instance.GameLogicHandler.OrderManager;

            var dictAllItemIds = ItemHelper.GetItemIdDictInGameplay(-1);
            var orderItemsDict = orderManager.GetOrderItemsDict();

            // số item còn lại <= số item còn lại tạo order
            foreach (var id in dictAllItemIds.Keys)
            {
                if (orderItemsDict.ContainsKey(id) == true)
                {
                    // Nếu item target + item còn lại cùng loại nó > maxItems thì cần tạo thêm order
                    if (dictAllItemIds[id] + orderItemsDict[id].num > orderItemsDict[id].maxItems) return true;
                }
                else
                {
                    // Nếu item còn lại không có trong target thì cần tạo order
                    return true;
                }
            }
            return false;
        }

        #region logic order
        private static int rescueGap = 0;
        private static int currentNumberRescues = 0;

        private static int stepGap = 0;
        public static int maxStep2Gap = 3;

        public static bool forward = false;

        public static void Reset()
        {
            rescueGap = 0;
            currentNumberRescues = 0;

            stepGap = 0;
        }

        public static (ItemId itemId, int num) GetItemOrder()
        {
            // // kiểm tra có sử dụng rescue không
            // // Sử dụng khi còn lại hàng chờ chỉ còn <= 2 khay trống
            // var levelData = GameController.Instance.LevelGenerator.LevelData;
            // var rescueCondition = levelData.rescueCondition;
            // var logicOrderConfigs = levelData.logicOrderConfigs;


            // if (CheckUseRescue(rescueCondition))
            // {
            //     rescueGap = rescueCondition.maxRescueGap;
            //     currentNumberRescues += 1;
            //     Debug.Log("<color=green>OrderHelper:</color> Use Rescue");
            //     return GetItemOrderToRescue();
            // }


            // var logicOrderConfig = GetLogicOrderConfig(logicOrderConfigs);
            // var gameplayInfo = GetGameplayInfoForOrder(); // lấy order info mỗi layer (2 layer đầu) --> OPTIMIZE: giảm tính toán

            // var (itemId, num, step) = GetItemOrderBasic(logicOrderConfig.minNumberSteps, gameplayInfo);
            // if (itemId != ItemId.None)
            // {
            //     if (rescueGap > 0) rescueGap--;
            //     if (step >= 2 && stepGap <= 0)
            //     {
            //         stepGap = maxStep2Gap;
            //         return (itemId, num);
            //     }

            //     if (step < 2)
            //     {
            //         stepGap -= 1;
            //         stepGap = Mathf.Min(stepGap, maxStep2Gap);
            //         return (itemId, num);
            //     }
            // }
            // stepGap -= 1;
            // stepGap = Mathf.Min(stepGap, maxStep2Gap);
            // var (itemId2, num2, step2) = ForceGetItemOrderBasic(gameplayInfo);
            // if (step2 >= 2) stepGap = maxStep2Gap;
            // return (itemId2, num2);
            return (ItemId.None, 0);
        }

        // private static bool CheckUseRescue(RescueCondition rescueCondition)
        // {
        //     if (rescueGap > 0) return false;
        //     if (currentNumberRescues >= rescueCondition.maxNumberRescues) return false;

        //     var waitingGrillManager = GameController.Instance.GameLogicHandler.WaitingGrillManager;
        //     var maxWaitingGrill = waitingGrillManager.ListWaitingGrills.Where(e => e.IsActive).ToList().Count;
        //     var threshold = maxWaitingGrill - rescueCondition.remainingWaitingGrill;

        //     return waitingGrillManager.ListWaitingGrills.Where(e => e.GetSlots()[0].GetItem() != null).Count() >= threshold;
        // }

        private static LogicOrderConfig GetLogicOrderConfig(List<LogicOrderConfig> logicOrderConfigs)
        {
            // var itemManager = GameController.Instance.GameLogicHandler.ItemManager;
            // var currentRegion = 1 - (float)itemManager.CurrentItems / itemManager.TotalItems;
            // Debug.Log("<color=green>OrderHelper:</color> GetLogicOrderConfig: " + currentRegion);
            // return logicOrderConfigs.FirstOrDefault(config => config.region >= currentRegion);
            return null;
        }

        private static (ItemId itemId, int num, int step) GetItemOrderBasic(int minStep, GameplayInfo gameplayInfo)
        {
            var dictItems = gameplayInfo.dictNeededSlots;

            if (forward == true)
            {
                for (int numItems = 3; numItems >= 1; numItems--)
                {
                    // chuyển về dict<numStep, List<itemId>>
                    var dictSteps = new Dictionary<int, List<ItemId>>();
                    foreach (var item in dictItems)
                    {
                        if (item.Value.ContainsKey(numItems))
                        {
                            var step = item.Value[numItems];
                            dictSteps.TryAdd(step, new List<ItemId>());
                            dictSteps[step].Add(item.Key);
                        }
                    }

                    foreach (var curStep in dictSteps.Keys)
                    {
                        if (curStep >= minStep)
                        {
                            var filteredItems = dictSteps[curStep];
                            if (filteredItems.Count == 0) continue;

                            var randomItemId = filteredItems[UnityEngine.Random.Range(0, filteredItems.Count)];
                            numItems = numItems > 3 ? 3 : numItems;
                            Debug.Log("<color=green>OrderHelper:</color> GetItemOrderBasic: " + randomItemId + " " + numItems + " minStep: " + minStep + " steps: " + dictItems[randomItemId][numItems]);
                            return (randomItemId, numItems, curStep);
                        }
                    }
                }
            }
            else
            {
                var dictNeededSlots = gameplayInfo.dictNeededSlots;
                // không lấy các item trong khay đang bị khóa
                var idsInLockedGrill = ItemHelper.GetItemIdsInLockedGrill();

                for (int step = minStep; step < 6; step++)
                {
                    var randomItemIds = dictNeededSlots.Keys.Where(e => dictNeededSlots[e].ContainsValue(minStep) && idsInLockedGrill.Contains(e) == false).ToList();
                    var itemIdsList = new List<ItemId>();
                    var maxNum = 0;
                    foreach (var itemId in randomItemIds)
                    {
                        foreach (var num in dictNeededSlots[itemId].Keys.Where(e => dictNeededSlots[itemId][e] == minStep))
                        {
                            if (num > maxNum)
                            {
                                maxNum = num;
                                itemIdsList.Clear();
                                itemIdsList.Add(itemId);
                            }
                            else if (num == maxNum)
                            {
                                itemIdsList.Add(itemId);
                            }
                        }
                    }
                    if (itemIdsList.Count > 0)
                    {
                        var randomItemId = itemIdsList[UnityEngine.Random.Range(0, itemIdsList.Count)];
                        Debug.Log("<color=green>OrderHelper:</color> GetItemOrderBasic: " + randomItemId + " " + maxNum + " minStep: " + minStep);
                        return (randomItemId, maxNum, minStep);
                    }
                    // // lấy ra item có num lớn nhất
                    // if (randomItemIds.Count > 0)
                    // {
                    //     // lấy id có số num lớn nhất với min step
                    //     var maxNum = randomItemIds.Max(e => dictNeededSlots[e].Keys.Max());
                    //     var ids = randomItemIds.Where(e => dictNeededSlots[e].Keys.Max() == maxNum).ToList();
                    //     var randomId = ids[UnityEngine.Random.Range(0, ids.Count)];
                    //     // var randomItemId = randomItemIds[UnityEngine.Random.Range(0, randomItemIds.Count)];
                    //     Debug.Log("<color=green>OrderHelper:</color> GetItemOrderBasic: " + randomId + " " + maxNum + " minStep: " + minStep);
                    //     return ((ItemId)randomId, maxNum, minStep);
                    // }
                }

            }


            return (ItemId.None, 0, 0);
        }

        private static (ItemId itemId, int num, int step) ForceGetItemOrderBasic(GameplayInfo gameplayInfo)
        {
            // lấy step nhỏ nhất
            var dictNeededSlots = gameplayInfo.dictNeededSlots;
            var minStep = dictNeededSlots.Values.Min(e => e.Values.Min());

            var randomItemIds = dictNeededSlots.Keys.Where(e => dictNeededSlots[e].ContainsValue(minStep)).ToList();
            var maxNum = 0;
            var itemIdsList = new List<ItemId>();
            foreach (var itemId in randomItemIds)
            {
                foreach (var num in dictNeededSlots[itemId].Keys.Where(e => dictNeededSlots[itemId][e] == minStep))
                {
                    if (num > maxNum)
                    {
                        maxNum = num;
                        itemIdsList.Clear();
                        itemIdsList.Add(itemId);
                    }
                    else if (num == maxNum)
                    {
                        itemIdsList.Add(itemId);
                    }
                }
            }
            // var ids = randomItemIds.Where(e => dictNeededSlots[e].Keys.Max() == maxNum).ToList();
            // var randomId = ids[UnityEngine.Random.Range(0, ids.Count)];

            var randomId = itemIdsList[UnityEngine.Random.Range(0, itemIdsList.Count)];
            Debug.Log("<color=red>OrderHelper:</color> ForceGetItemOrderBasic: " + randomId + " " + maxNum + " minStep: " + minStep);
            return ((ItemId)randomId, maxNum, minStep);

            // var randomItemId = randomItemIds[UnityEngine.Random.Range(0, randomItemIds.Count)];
            // var nums = dictNeededSlots[randomItemId].Keys.Where(e => dictNeededSlots[randomItemId][e] == minStep).ToList();
            // var maxNum = nums.Max();
            // Debug.Log("<color=red>OrderHelper:</color> ForceGetItemOrderBasic: " + randomItemId + " " + maxNum + " minStep: " + minStep);
            // return (randomItemId, maxNum, minStep);
        }

        private static GameplayInfo GetGameplayInfoForOrder()
        {
            return new GameplayInfo() { dictNeededSlots = GetDictNeededSlots() };
        }

        private static Dictionary<ItemId, Dictionary<int, int>> GetDictNeededSlots()
        {
            // dynamic programming
            var dp = new Dictionary<ItemId, Dictionary<int, int>>(); // itemId/numItems/neededSlot

            // order: Chọn item tối ưu giúp clear order
            var orderManager = GameController.Instance.GameLogicHandler.OrderManager;
            var orderItemsDict = orderManager.GetOrderItemsDict();
            // order: Chọn item tối ưu giúp clear order
            var neededItemsForCurrentOrder = orderItemsDict.ToDictionary(e => e.Key, e => e.Value.maxItems - e.Value.num);

            // waiting grill
            var waitingGrillManager = GameController.Instance.GameLogicHandler.WaitingGrillManager;
            foreach (var waitingGrill in waitingGrillManager.ListWaitingGrills)
            {
                var item = waitingGrill.GetSlots()[0].GetItem();
                if (item != null)
                {
                    if (neededItemsForCurrentOrder.ContainsKey((ItemId)item.id))
                    {
                        neededItemsForCurrentOrder[(ItemId)item.id]--;
                        if (neededItemsForCurrentOrder[(ItemId)item.id] <= 0)
                        {
                            neededItemsForCurrentOrder.Remove((ItemId)item.id);
                        }
                        continue;
                    }

                    if (dp.ContainsKey((ItemId)item.id) == false)
                    {
                        dp[(ItemId)item.id] = new Dictionary<int, int>() { { 1, 0 } };
                    }
                    else
                    {
                        var numInDp = dp[(ItemId)item.id].Count;
                        if (numInDp < 3)
                        {
                            dp[(ItemId)item.id][numInDp + 1] = 0;
                        }
                    }
                }
            }

            // tính số slot trống cần tối đa để lấy ra item
            var grillManager = GameController.Instance.GameLogicHandler.GrillManager;
            var dictCountSlotByGrill = new Dictionary<int, int>(); // số slot mỗi grill
            var dictNumItemsInUpperLayer = new Dictionary<int, List<Item>>(); // số lượng item ở layer trên đó
            // duyệt qua layer 0
            foreach (var primaryGrill in grillManager.ListGrills)
            {
                var slots = primaryGrill.GetSlots();
                if (slots == null) continue;

                var listCurrentItems = slots.Select(e => e.GetItem()).Where(e => e != null).ToList();
                var dictItems = listCurrentItems.GroupBy(e => (ItemId)e.id).ToDictionary(e => e.Key, e => e.Count());

                foreach (var itemId in dictItems.Keys)
                {
                    var numItems = dictItems[itemId];
                    // cần chọn item tối ưu cho order
                    if (neededItemsForCurrentOrder.ContainsKey(itemId))
                    {
                        var neededItems = neededItemsForCurrentOrder[itemId];
                        neededItemsForCurrentOrder[itemId] -= numItems;
                        if (neededItemsForCurrentOrder[itemId] <= 0)
                        {
                            neededItemsForCurrentOrder.Remove(itemId);
                        }

                        numItems = numItems - neededItems;
                    }
                    if (numItems <= 0) continue;

                    var numInDp = dp.ContainsKey(itemId) ? dp[itemId].Count : 0;
                    dp.TryAdd(itemId, new Dictionary<int, int>());
                    for (int j = 1; j <= numItems; j++)
                    {
                        if (numInDp + j > 3) break;
                        dp[itemId].TryAdd(numInDp + j, 0);
                    }
                }
                dictNumItemsInUpperLayer[primaryGrill.id] = listCurrentItems;
                dictCountSlotByGrill[primaryGrill.id] = listCurrentItems.Count;
            }

            // duyệt qua layer 1
            // Duyệt lần 1 để lấy item tối ưu còn lại cho order
            var dictMinItemForOrder = new Dictionary<ItemId, List<(List<Item>, int)>>(); // đếm các item (num, neededSlot)
            foreach (var primaryGrill in grillManager.ListGrills)
            {
                var subGrills = primaryGrill.GetSubGrills();
                if (subGrills == null || subGrills.Count == 0) continue;
                var slots = subGrills[0].GetSlots();
                if (slots == null) continue;

                var listCurrentItems = slots.Select(e => e.GetItem()).Where(e => e != null).ToList();
                var dictItems = listCurrentItems.GroupBy(e => (ItemId)e.id).ToDictionary(e => e.Key, e => e.Count());
                foreach (var itemId in dictItems.Keys)
                {
                    if (neededItemsForCurrentOrder.ContainsKey(itemId))
                    {
                        dictMinItemForOrder.TryAdd(itemId, new List<(List<Item>, int)>());

                        var items = listCurrentItems.Where(e => (ItemId)e.id == itemId).ToList();
                        var neededSlot = dictCountSlotByGrill.GetValueOrDefault(primaryGrill.id, 0);
                        dictMinItemForOrder[itemId].Add((items, neededSlot));
                    }
                }
            }

            var listIgnoreItems = new List<Item>();
            foreach (var itemId in neededItemsForCurrentOrder.Keys)
            {
                if (dictMinItemForOrder.ContainsKey(itemId))
                {
                    var neededItems = neededItemsForCurrentOrder[itemId];
                    listIgnoreItems.AddRange(ChooseItemToIgnore(dictMinItemForOrder[itemId], neededItems));
                }
            }

            // duyệt lần 2 để tính toán
            foreach (var primaryGrill in grillManager.ListGrills)
            {
                var subGrills = primaryGrill.GetSubGrills();
                if (subGrills == null || subGrills.Count == 0) continue;
                var slots = subGrills[0].GetSlots();
                if (slots == null) continue;

                var listCurrentItems = slots.Select(e => e.GetItem()).Where(e => e != null).ToList();
                var remainItems = listCurrentItems.Where(e => listIgnoreItems.Contains(e) == false).ToList();
                var dictItems = remainItems.GroupBy(e => (ItemId)e.id).ToDictionary(e => e.Key, e => e.Count());

                foreach (var itemId in dictItems.Keys)
                {
                    var numItems = dictItems[itemId];
                    var currentNeededSlot = dictCountSlotByGrill.GetValueOrDefault(primaryGrill.id, 0);

                    // nếu ở layer bên trên có item này thì trừ đi số lượng itme ở layer trên đó
                    var numItemsInUpperLayer = dictNumItemsInUpperLayer.GetValueOrDefault(primaryGrill.id, new List<Item>()).Where(e => (ItemId)e.id == itemId).Count();
                    currentNeededSlot = currentNeededSlot - numItemsInUpperLayer;

                    if (dp.ContainsKey(itemId) == false)
                    {
                        dp[itemId] = new Dictionary<int, int>();
                        for (int j = 1; j <= Mathf.Min(numItems, 3); j++)
                        {
                            dp[itemId].TryAdd(j, currentNeededSlot);
                        }
                    }
                    else
                    {
                        var numInDp = dp[itemId].Count;
                        for (int j = numInDp; j >= 1; j--)
                        {
                            var currentValue = dp[itemId][j] + currentNeededSlot;
                            for (int k = 1; k <= numItems; k++)
                            {
                                if (j + k > 3) continue;
                                dp[itemId].TryAdd(j + k, currentValue);
                                dp[itemId][j + k] = Mathf.Min(dp[itemId][j + k], currentValue);
                            }
                        }
                    }
                }
            }

            return dp;
        }

        private static List<Item> ChooseItemToIgnore(List<(List<Item>, int)> list, int neededItems)
        {
            // dp[i] = (số item đạt được, chi phí, danh sách item đã chọn)
            var dp = new (int maxItems, int minCost, List<Item> chosen)[neededItems + 1];

            for (int i = 0; i <= neededItems; i++)
            {
                dp[i].maxItems = 0;
                dp[i].minCost = int.MaxValue;
                dp[i].chosen = new List<Item>();
            }

            dp[0].minCost = 0;

            foreach (var (items, cost) in list)
            {
                int itemCount = items.Count;

                // duyệt ngược để không overwrite trạng thái cũ
                for (int cap = neededItems; cap >= 0; cap--)
                {
                    if (dp[cap].minCost == int.MaxValue) continue;

                    int newCap = cap + itemCount;
                    if (newCap > neededItems) newCap = neededItems;

                    int newItems = dp[cap].maxItems + itemCount;
                    int newCost = dp[cap].minCost + cost;

                    bool shouldUpdate = false;

                    if (newItems > dp[newCap].maxItems) shouldUpdate = true;
                    else if (newItems == dp[newCap].maxItems && newCost < dp[newCap].minCost) shouldUpdate = true;

                    if (shouldUpdate)
                    {
                        dp[newCap].maxItems = newItems;
                        dp[newCap].minCost = newCost;
                        dp[newCap].chosen = dp[cap].chosen.Concat(items).ToList();
                    }
                }
            }

            // tìm trạng thái tốt nhất
            var best = dp[0];
            for (int i = 1; i <= neededItems; i++)
            {
                if (dp[i].maxItems > best.maxItems ||
                    (dp[i].maxItems == best.maxItems && dp[i].minCost < best.minCost))
                {
                    best = dp[i];
                }
            }

            return best.chosen;
        }

        private static (ItemId itemId, int num) GetItemOrderToRescue()
        {
            // ---Tạo order để giải cứu---
            // Item được lấy từ hàng chờ --> Làm giảm số lượng khay trống nhiều nhất
            var waitingGrillManager = GameController.Instance.GameLogicHandler.WaitingGrillManager;

            var itemsInWaiting = waitingGrillManager.ListWaitingGrills.Select(e => e.GetSlots()[0].GetItem());
            var items = itemsInWaiting.Where(e => e != null).ToList();

            var dictItems = items.GroupBy(e => e.id).ToDictionary(e => e.Key, e => e.Count());
            var itemId = dictItems.OrderByDescending(e => e.Value).First().Key;
            var num = dictItems[itemId] > 3 ? 3 : dictItems[itemId];
            return ((ItemId)itemId, num);
        }
        #endregion
    }

    public class GameplayInfo
    {
        // số step để ăn numItems tối ưu
        public Dictionary<ItemId, Dictionary<int, int>> dictNeededSlots;
    }
}