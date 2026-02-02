using System.Collections.Generic;
using Gameplay.LevelData;
using UnityEngine;

namespace Tool
{
    public static class ItemIdSwapHelper
    {
        /// <summary>
        /// Swaps all item IDs in a level from old IDs to new IDs based on the mapping
        /// </summary>
        /// <param name="levelData">The level data to process</param>
        /// <param name="swapService">The ItemIdSwapService instance</param>
        /// <returns>Number of items that were swapped</returns>
        public static int SwapItemIdsInLevel(LevelData levelData, ItemIdSwapService swapService = null)
        {
            if (swapService == null)
            {
                swapService = ItemIdSwapService.Instance;
            }

            if (swapService == null || !swapService.IsDataLoaded)
            {
                UnityEngine.Debug.LogWarning("ItemIdSwapService not available or data not loaded");
                return 0;
            }

            int swappedCount = 0;

            // Swap items in grill data (main level structure)
            if (levelData.grillData != null)
            {
                foreach (var grill in levelData.grillData)
                {
                    if (grill != null && grill.layer != null)
                    {
                        foreach (var layer in grill.layer)
                        {
                            if (layer != null && layer.itemData != null)
                            {
                                for (int i = 0; i < layer.itemData.Length; i++)
                                {
                                    if (layer.itemData[i] != null && layer.itemData[i].id > 0)
                                    {
                                        int oldId = layer.itemData[i].id;
                                        int newId = swapService.GetNewId(oldId);
                                        
                                        if (oldId != newId)
                                        {
                                            layer.itemData[i].id = newId;
                                            swappedCount++;
                                            UnityEngine.Debug.Log($"Swapped grill item at grill {grill.id}, layer item {i}: {oldId} -> {newId}");
                                        }
                                    }
                                }
                            }
                        }

                        if (grill is GrillLidData grillLidData)
                        {
                            int oldId = grillLidData.itemCondition;
                            int newId = swapService.GetNewId(oldId);
                                        
                            if (oldId != newId)
                            {
                                grillLidData.itemCondition = newId;
                                swappedCount++;
                                UnityEngine.Debug.Log($"Swapped grill item at grill {grill.id}, lid item: {oldId} -> {newId}");
                            }
                        }
                    }
                }
            }

            // Swap items in order data if they exist
            if (levelData.orderData != null)
            {
                foreach (var order in levelData.orderData)
                {
                    if (order != null && order.ids != null)
                    {
                        for (int i = 0; i < order.ids.Count; i++)
                        {
                            int oldId = order.ids[i];
                            int newId = swapService.GetNewId(oldId);
                            
                            if (oldId != newId)
                            {
                                order.ids[i] = newId;
                                swappedCount++;
                                UnityEngine.Debug.Log($"Swapped order item at index {i}: {oldId} -> {newId}");
                            }
                        }
                    }
                }
            }

            UnityEngine.Debug.Log($"Item ID swapping completed. Total items swapped: {swappedCount}");
            return swappedCount;
        }

        /// <summary>
        /// Swaps item IDs in reverse (new IDs to old IDs)
        /// </summary>
        /// <param name="levelData">The level data to process</param>
        /// <param name="swapService">The ItemIdSwapService instance</param>
        /// <returns>Number of items that were swapped</returns>
        public static int ReverseSwapItemIdsInLevel(LevelData levelData, ItemIdSwapService swapService = null)
        {
            if (swapService == null)
            {
                swapService = ItemIdSwapService.Instance;
            }

            if (swapService == null || !swapService.IsDataLoaded)
            {
                UnityEngine.Debug.LogWarning("ItemIdSwapService not available or data not loaded");
                return 0;
            }

            int swappedCount = 0;

            // Reverse swap items in grill data (main level structure)
            if (levelData.grillData != null)
            {
                foreach (var grill in levelData.grillData)
                {
                    if (grill != null && grill.layer != null)
                    {
                        foreach (var layer in grill.layer)
                        {
                            if (layer != null && layer.itemData != null)
                            {
                                for (int i = 0; i < layer.itemData.Length; i++)
                                {
                                    if (layer.itemData[i] != null && layer.itemData[i].id > 0)
                                    {
                                        int newId = layer.itemData[i].id;
                                        int oldId = swapService.GetOldId(newId);
                                        
                                        if (oldId != newId)
                                        {
                                            layer.itemData[i].id = oldId;
                                            swappedCount++;
                                            UnityEngine.Debug.Log($"Reverse swapped grill item at grill {grill.id}, layer item {i}: {newId} -> {oldId}");
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }

            // Reverse swap items in order data if they exist
            if (levelData.orderData != null)
            {
                foreach (var order in levelData.orderData)
                {
                    if (order != null && order.ids != null)
                    {
                        for (int i = 0; i < order.ids.Count; i++)
                        {
                            int newId = order.ids[i];
                            int oldId = swapService.GetOldId(newId);
                            
                            if (oldId != newId)
                            {
                                order.ids[i] = oldId;
                                swappedCount++;
                                UnityEngine.Debug.Log($"Reverse swapped order item at index {i}: {newId} -> {oldId}");
                            }
                        }
                    }
                }
            }

            UnityEngine.Debug.Log($"Reverse item ID swapping completed. Total items swapped: {swappedCount}");
            return swappedCount;
        }

        /// <summary>
        /// Gets a summary of all item ID mappings
        /// </summary>
        /// <param name="swapService">The ItemIdSwapService instance</param>
        /// <returns>Formatted string with mapping information</returns>
        public static string GetMappingSummary(ItemIdSwapService swapService = null)
        {
            if (swapService == null)
            {
                swapService = ItemIdSwapService.Instance;
            }

            if (swapService == null || !swapService.IsDataLoaded)
            {
                return "ItemIdSwapService not available or data not loaded";
            }

            var mappings = swapService.GetAllOldToNewMappings();
            string summary = $"Item ID Mapping Summary (Total: {mappings.Count} old IDs, {swapService.GetTotalNewIds()} total new IDs):\n";
            
            foreach (var mapping in mappings)
            {
                if (mapping.Value.Count == 1)
                {
                    summary += $"  Old ID {mapping.Key} -> New ID {mapping.Value[0]}\n";
                }
                else
                {
                    summary += $"  Old ID {mapping.Key} -> New IDs [{string.Join(", ", mapping.Value)}] (random selection)\n";
                }
            }

            return summary;
        }
    }
} 