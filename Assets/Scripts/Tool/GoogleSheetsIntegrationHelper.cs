using System;
using System.Collections.Generic;
using UnityEngine;
using Gameplay.LevelData;
using Sonat.Enums;

namespace Tool
{
    public static class GoogleSheetsIntegrationHelper
    {
        /// <summary>
        /// Apply Google Sheets data to the current level
        /// </summary>
        public static void ApplySheetDataToLevel(LevelDataFromSheet sheetData, LevelData levelData)
        {
            if (sheetData == null || levelData == null) return;

            // Update level properties based on sheet data
            levelData.level = sheetData.level;
            
            // You can add more logic here to apply sheet data to the level
            // For example, setting time limits, difficulty, etc.
            
            Debug.Log($"Applied sheet data for level {sheetData.level}");
        }

        /// <summary>
        /// Get grill type suggestions based on sheet data
        /// </summary>
        public static List<GrillType> GetSuggestedGrillTypes(LevelDataFromSheet sheetData)
        {
            List<GrillType> suggestions = new List<GrillType>();

            if (sheetData == null) return suggestions;

            // Add suggestions based on sheet data
            if (sheetData.lockCount > 0)
            {
                suggestions.Add(GrillType.Lock);
                suggestions.Add(GrillType.LockAndKey);
            }

            if (sheetData.dropLine > 0)
            {
                //suggestions.Add(GrillType.Drop7);
            }

            if (sheetData.vendingTray > 0)
            {
                suggestions.Add(GrillType.Vending);
            }

            if (sheetData.miniTray > 0)
            {
                suggestions.Add(GrillType.Normal);
            }

            if (sheetData.frozen > 0 || sheetData.ice > 0)
            {
                suggestions.Add(GrillType.Ice);
            }

            return suggestions;
        }

        /// <summary>
        /// Validate level against sheet data
        /// </summary>
        public static bool ValidateLevelAgainstSheet(LevelData levelData, LevelDataFromSheet sheetData)
        {
            if (levelData == null || sheetData == null) return false;

            // Count locks in current level
            int currentLocks = CountLocksInLevel(levelData);

            // Compare with sheet data
            bool locksMatch = currentLocks == sheetData.lockCount;

            if (!locksMatch)
            {
                Debug.LogWarning($"Lock count mismatch: Current={currentLocks}, Sheet={sheetData.lockCount}");
            }

            return locksMatch;
        }



        private static int CountLocksInLevel(LevelData levelData)
        {
            int count = 0;
            
            if (levelData.grillData != null)
            {
                foreach (var grillData in levelData.grillData)
                {
                    if (grillData.grillType == GrillType.Lock || 
                        grillData.grillType == GrillType.LockAndKey ||
                        grillData.grillType == GrillType.LockAndKey2 ||
                        grillData.grillType == GrillType.LockAds)
                    {
                        count++;
                    }
                }
            }

            return count;
        }

        /// <summary>
        /// Generate a summary report comparing level data with sheet data
        /// </summary>
        public static string GenerateComparisonReport(LevelData levelData, LevelDataFromSheet sheetData)
        {
            if (levelData == null || sheetData == null) return "Invalid data provided";

            int currentLocks = CountLocksInLevel(levelData);

            string report = $"Level {sheetData.level} Comparison Report:\n";
            report += $"Locks: Current={currentLocks}, Sheet={sheetData.lockCount} {(currentLocks == sheetData.lockCount ? "✓" : "✗")}\n";
            report += $"Drop Line: {sheetData.dropLine}\n";
            report += $"Mini Tray: {sheetData.miniTray}\n";
            report += $"Vending Tray: {sheetData.vendingTray}\n";
            report += $"Convey: {sheetData.convey}\n";

            return report;
        }
    }
} 