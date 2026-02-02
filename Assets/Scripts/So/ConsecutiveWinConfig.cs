using Newtonsoft.Json;
using Sirenix.OdinInspector;
using System;
using UnityEngine;

namespace GrillSort.ConsecutiveWin
{
    [CreateAssetMenu(fileName = "ConsecutiveWinConfig", menuName = "Sonat Configs Custom/ConsecutiveWinConfig")]
    public class ConsecutiveWinConfig : ScriptableObject
    {
        public int appearLevel = 6;
        public int appearDay = 0;
        public int[] timeToAdd;
        public int timeToRemove = 0;
        public int GetMaxConsecutiveWins()
        {
            return timeToAdd.Length;
        }

        public int GetTimeToAdd(int consecutiveWins)
        {
            return timeToAdd[consecutiveWins - 1];
        }

        internal int GetTimeToRemove(int level)
        {
            if (level < appearLevel) return 0;

            return timeToRemove;
        }

        [Button("Print JSON Config")]
        private void PrintJsonConfig()
        {
            Debug.Log(JsonConvert.SerializeObject(this));
        }
    }
}
