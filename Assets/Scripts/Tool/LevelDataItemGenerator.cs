using System;
using System.Collections.Generic;
using System.Linq;
using Gameplay.LevelData;
using Sonat;
using SonatFramework.Scripts.UIModule;
using TMPro;
using UnityEngine;
using Tool.Extensions;

namespace Tool
{
    /// <summary>
    /// Tool for generating random items for all grills in LevelData
    /// </summary>
    public class LevelDataItemGenerator : MonoBehaviour
    {
        [Header("Generation Settings")]
        [SerializeField] private int totalItems = 30;
        [SerializeField] private string itemIdsString = "";
        [SerializeField] private int layersPerGrill = 3;
        
        [Header("Validation")]
        [SerializeField] private bool ensureValidCombinations = true;
        [SerializeField] private bool ensureTripleMatches = true;
        
        [Header("References")]
        [SerializeField] private UIToolPanel toolPanel;
        
        private List<int> availableItemIds = new List<int>();
        [SerializeField] private TMP_InputField inputTotalItems;
        [SerializeField] private TMP_InputField inputLayersPerGrill;
        [SerializeField] private TMP_InputField inputItemIds;
        
        private void Start()
        {
            if (toolPanel == null)
                toolPanel = FindObjectOfType<UIToolPanel>();
            
            ParseItemIds();
        }
        
        /// <summary>
        /// Parse the item IDs string into a list
        /// </summary>
        private void ParseItemIds()
        {
            availableItemIds.Clear();
            
            if (string.IsNullOrEmpty(itemIdsString))
            {
                Debug.LogWarning("Item IDs string is empty!");
                return;
            }

            availableItemIds = SonatSdkHelper.GetIntListFromRegex(itemIdsString);
        }

        public void PickIds()
        {
            itemIdsString = inputItemIds.text;
            ParseItemIds();
            ToolItemSelectorPanel.Instance.Open(availableItemIds, OnSelectItemIds);
        }

        private void OnSelectItemIds(List<int> selectedIds)
        {
            string ids = string.Join(",", selectedIds);
            inputItemIds.text = ids;
        }
        
        /// <summary>
        /// Generate random items for all grills in the current LevelData
        /// </summary>
        public void GenerateRandomItems()
        {
            if (toolPanel == null)
            {
                Debug.LogError("UIToolPanel reference not found!");
                return;
            }
            
            LevelData levelData = toolPanel.LevelData;
            if (levelData?.grillData == null || levelData.grillData.Count == 0)
            {
                Debug.LogWarning("No grills found in LevelData!");
                return;
            }

            if (!int.TryParse(inputTotalItems.text, out totalItems))
            {
                Debug.LogWarning("Input total item IDs are not integer!");
                return;
            }
            
            if (!int.TryParse(inputLayersPerGrill.text, out layersPerGrill))
            {
                Debug.LogWarning("Input layer per grill is not integer!");
                return;
            }

            itemIdsString = inputItemIds.text;
            
            ParseItemIds();
            if (availableItemIds.Count == 0)
            {
                Debug.LogError("No valid item IDs available!");
                return;
            }
            
            // Generate items based on the strategy
            List<int> allItems = GenerateItemList();
            
            // Distribute items to grills
            DistributeItemsToGrills(levelData, allItems);
            
            ToolManager.Instance.SetMainLayer(ToolManager.currentLayer);
            // Update the tool panel
            toolPanel.UpdateItemCount();
            
            Debug.Log($"Generated {allItems.Count} items across {levelData.grillData.Count} grills with {layersPerGrill} layers each");
        }
        
        /// <summary>
        /// Generate the complete list of items based on settings
        /// </summary>
        private List<int> GenerateItemList()
        {
            List<int> items = new List<int>();
            
            if (ensureTripleMatches)
            {
                // Ensure items come in groups of 3 for valid matches
                int groupsOfThree = totalItems / 3;
                int remainder = totalItems % 3;
                
                for (int i = 0; i < groupsOfThree; i++)
                {
                    int randomId = availableItemIds[UnityEngine.Random.Range(0, availableItemIds.Count)];
                    items.Add(randomId);
                    items.Add(randomId);
                    items.Add(randomId);
                }
                
                // Add remaining items (if any)
                for (int i = 0; i < remainder; i++)
                {
                    int randomId = availableItemIds[UnityEngine.Random.Range(0, availableItemIds.Count)];
                    items.Add(randomId);
                }
            }
            else
            {
                // Generate completely random items
                for (int i = 0; i < totalItems; i++)
                {
                    int randomId = availableItemIds[UnityEngine.Random.Range(0, availableItemIds.Count)];
                    items.Add(randomId);
                }
            }
            
            // Calculate total slots needed for all grills
            int totalSlots = CalculateTotalSlots();
            
            // Add empty items (id = 0) to fill remaining slots
            int emptySlotsNeeded = totalSlots - items.Count;
            for (int i = 0; i < emptySlotsNeeded; i++)
            {
                items.Add(0); // 0 represents empty slot
            }
            
            // Shuffle the items (including empty slots)
            items.Shuffle();
            
            Debug.Log($"Generated {items.Count - emptySlotsNeeded} items + {emptySlotsNeeded} empty slots = {items.Count} total slots");
            
            return items;
        }
        
        /// <summary>
        /// Calculate total number of slots across all grills
        /// </summary>
        private int CalculateTotalSlots()
        {
            if (toolPanel == null) return 0;
            
            LevelData levelData = toolPanel.LevelData;
            if (levelData?.grillData == null) return 0;
            
            int totalSlots = 0;
            foreach (GrillData grillData in levelData.grillData)
            {
                int itemsPerLayer = grillData.grillType == GrillType.Normal ? 3 : 1;
                totalSlots += layersPerGrill * itemsPerLayer;
            }
            
            return totalSlots;
        }
        
        /// <summary>
        /// Distribute items to grills in layers
        /// </summary>
        private void DistributeItemsToGrills(LevelData levelData, List<int> allItems)
        {
            int itemIndex = 0;
            
            foreach (GrillData grillData in levelData.grillData)
            {
                // Initialize layer list if null
                if (grillData.layer == null)
                    grillData.layer = new List<LayerData>();
                else
                    grillData.layer.Clear();
                
                // Create layers for this grill
                for (int layerIndex = 0; layerIndex < layersPerGrill; layerIndex++)
                {
                    int itemsPerLayer = grillData.grillType == GrillType.Normal ? 3 : 1;
                    LayerData layerData = new LayerData(itemsPerLayer);
                    
                    // Fill this layer with items
                    for (int slotIndex = 0; slotIndex < itemsPerLayer; slotIndex++)
                    {
                        if (itemIndex < allItems.Count)
                        {
                            layerData.itemData[slotIndex] = new ItemData
                            {
                                id = allItems[itemIndex],
                                hidden = false
                            };
                            itemIndex++;
                        }
                        else
                        {
                            // Fallback: fill with empty slot if we somehow run out of items
                            layerData.itemData[slotIndex] = new ItemData
                            {
                                id = 0,
                                hidden = false
                            };
                        }
                    }
                    
                    // Only add layer if it has at least one valid item
                    if (HasValidItems(layerData))
                    {
                        grillData.layer.Add(layerData);
                    }
                }
            }
            
            Debug.Log($"Distributed {itemIndex} items across {levelData.grillData.Count} grills");
        }
        
        /// <summary>
        /// Check if a layer has any valid items (not null and id > 0)
        /// </summary>
        private bool HasValidItems(LayerData layerData)
        {
            if (layerData?.itemData == null) return false;
            
            foreach (ItemData itemData in layerData.itemData)
            {
                if (itemData != null && itemData.id > 0)
                {
                    return true;
                }
            }
            
            return false;
        }
        
        /// <summary>
        /// Generate items with custom parameters
        /// </summary>
        public void GenerateRandomItems(int totalItems, string itemIdsString, int layersPerGrill)
        {
            this.totalItems = totalItems;
            this.itemIdsString = itemIdsString;
            this.layersPerGrill = layersPerGrill;
            
            GenerateRandomItems();
        }
        
        /// <summary>
        /// Clear all items from all grills
        /// </summary>
        public void ClearAllItems()
        {
            if (toolPanel == null) return;
            
            LevelData levelData = toolPanel.LevelData;
            if (levelData?.grillData == null) return;
            
            foreach (GrillData grillData in levelData.grillData)
            {
                if (grillData.layer != null)
                {
                    grillData.layer.Clear();
                }
            }
            
            toolPanel.UpdateItemCount();
            Debug.Log("Cleared all items from all grills");
        }
        
        /// <summary>
        /// Validate the current LevelData for valid item combinations
        /// </summary>
        public bool ValidateLevelData()
        {
            if (toolPanel == null) return false;
            
            LevelData levelData = toolPanel.LevelData;
            if (levelData?.grillData == null) return false;
            
            Dictionary<int, int> itemCounts = new Dictionary<int, int>();
            bool hasLock = false, haskey = false;
            
            foreach (GrillData grillData in levelData.grillData)
            {
                if (grillData.layer != null)
                {
                    foreach (LayerData layerData in grillData.layer)
                    {
                        if (layerData.itemData != null)
                        {
                            foreach (ItemData itemData in layerData.itemData)
                            {
                                if (itemData != null && itemData.id > 0)
                                {
                                    if (!itemCounts.TryAdd(itemData.id, 1))
                                    {
                                        itemCounts[itemData.id]++;
                                    }
                                    if(itemData.itemType == ItemType.Key) haskey = true;
                                }
                            }
                        }
                    }
                }

                if (grillData.grillType == GrillType.LockAndKey) hasLock = true;
            }

            if (haskey != hasLock)
            {
                PanelManager.Instance.OpenForget<NotifyPanel>(new NotifyPanel.Data() { content = "Lock and key is not validate!" });
                return false;
            }
            
            // Check if all items have valid counts (multiples of 3 for matching)
            foreach (var count in itemCounts)
            {
                if (count.Value % 3 != 0)
                {
                    Debug.LogWarning($"Item {count.Key} appears {count.Value} times (not a multiple of 3)");
                    return false;
                }
            }
            
            Debug.Log("LevelData validation passed!");
            return true;
        }
        
        /// <summary>
        /// Get statistics about the current LevelData
        /// </summary>
        public void PrintLevelStatistics()
        {
            if (toolPanel == null) return;
            
            LevelData levelData = toolPanel.LevelData;
            if (levelData?.grillData == null) return;
            
            int totalGrills = levelData.grillData.Count;
            int totalLayers = 0;
            int totalItems = 0;
            Dictionary<int, int> itemCounts = new Dictionary<int, int>();
            
            foreach (GrillData grillData in levelData.grillData)
            {
                if (grillData.layer != null)
                {
                    totalLayers += grillData.layer.Count;
                    foreach (LayerData layerData in grillData.layer)
                    {
                        if (layerData.itemData != null)
                        {
                            foreach (ItemData itemData in layerData.itemData)
                            {
                                if (itemData != null && itemData.id > 0)
                                {
                                    totalItems++;
                                    if (!itemCounts.TryAdd(itemData.id, 1))
                                    {
                                        itemCounts[itemData.id]++;
                                    }
                                }
                            }
                        }
                    }
                }
            }
            
            Debug.Log($"Level Statistics:");
            Debug.Log($"- Total Grills: {totalGrills}");
            Debug.Log($"- Total Layers: {totalLayers}");
            Debug.Log($"- Total Items: {totalItems}");
            Debug.Log($"- Unique Item Types: {itemCounts.Count}");
            
            foreach (var count in itemCounts)
            {
                Debug.Log($"  Item {count.Key}: {count.Value} times");
            }
        }
        
        /// <summary>
        /// Set the generation parameters
        /// </summary>
        public void SetParameters(int totalItems, string itemIdsString, int layersPerGrill)
        {
            this.totalItems = totalItems;
            this.itemIdsString = itemIdsString;
            this.layersPerGrill = layersPerGrill;
            
            ParseItemIds();
        }
    }
} 