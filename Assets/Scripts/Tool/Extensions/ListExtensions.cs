using System.Collections.Generic;
using UnityEngine;

namespace Tool.Extensions
{
    /// <summary>
    /// Extension methods for List<T>
    /// </summary>
    public static class ListExtensions
    {
        /// <summary>
        /// Shuffle a list using Fisher-Yates algorithm
        /// </summary>
        public static void Shuffle<T>(this List<T> list)
        {
            int n = list.Count;
            while (n > 1)
            {
                n--;
                int k = Random.Range(0, n + 1);
                T value = list[k];
                list[k] = list[n];
                list[n] = value;
            }
        }
        
        /// <summary>
        /// Get a random item from the list
        /// </summary>
        public static T GetRandom<T>(this List<T> list)
        {
            if (list == null || list.Count == 0)
                return default(T);
                
            return list[Random.Range(0, list.Count)];
        }
        
        /// <summary>
        /// Get multiple random items from the list
        /// </summary>
        public static List<T> GetRandomItems<T>(this List<T> list, int count)
        {
            List<T> result = new List<T>();
            List<T> tempList = new List<T>(list);
            
            count = Mathf.Min(count, tempList.Count);
            
            for (int i = 0; i < count; i++)
            {
                int randomIndex = Random.Range(0, tempList.Count);
                result.Add(tempList[randomIndex]);
                tempList.RemoveAt(randomIndex);
            }
            
            return result;
        }
    }
} 