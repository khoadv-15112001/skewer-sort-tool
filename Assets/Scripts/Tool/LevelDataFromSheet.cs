using System;
using System.Collections.Generic;

namespace Tool
{
    [Serializable]
    public class LevelDataFromSheet
    {
        public int level;
        public int dropLine;
        public int miniTray;
        public int lockCount;
        public string napVung;
        public int vendingTray;
        public string hidden;
        public string lockAndKey;
        public int frozen;
        public int ice;
        public int boom;
        public int octoChef;
        public int totalIds;
        public int unique;
        public int variations;
        public int noMatch3;
        public string convey;

        public LevelDataFromSheet()
        {
        }

        public LevelDataFromSheet(string[] rowData)
        {
            if (rowData.Length >= 17)
            {
                level = int.TryParse(rowData[0], out int lvl) ? lvl : 0;
                convey = rowData[1];
                dropLine = int.TryParse(rowData[2], out int drop) ? drop : 0;
                miniTray = int.TryParse(rowData[3], out int mini) ? mini : 0;
                lockCount = int.TryParse(rowData[4], out int lockC) ? lockC : 0;
                napVung = rowData[5];
                vendingTray = int.TryParse(rowData[6], out int vend) ? vend : 0;
                hidden = rowData[7];
                lockAndKey = rowData[8];
                frozen = int.TryParse(rowData[9], out int frz) ? frz : 0;
                ice = int.TryParse(rowData[10], out int ic) ? ic : 0;
                boom = int.TryParse(rowData[11], out int bm) ? bm : 0;
                octoChef = int.TryParse(rowData[12], out int oct) ? oct : 0;
                totalIds = int.TryParse(rowData[13], out int total) ? total : 0;
                unique = int.TryParse(rowData[14], out int unq) ? unq : 0;
                variations = int.TryParse(rowData[15], out int var) ? var : 0;
                noMatch3 = int.TryParse(rowData[16], out int noMatch) ? noMatch : 0;
                
                // Debug logging
                UnityEngine.Debug.Log($"Parsed Level {level}: Convey={convey}, DropLine={dropLine}, MiniTray={miniTray}, Lock={lockCount}");
            }
            else
            {
                UnityEngine.Debug.LogWarning($"Row data length ({rowData.Length}) is less than expected (17). Row: {string.Join(",", rowData)}");
            }
        }

        public override string ToString()
        {
            return $"Level {level}: DropLine={dropLine}, MiniTray={miniTray}, Lock={lockCount}, Convey={convey}";
        }
    }
} 