using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Sonat
{
    public static class SonatSdkUtils
    {

        public static void DoActionDelay(Action action, float delay)
        {
            SonatSdkManager.instance.StartCoroutine(ActionDelay(action, delay));
        }

        private static IEnumerator ActionDelay(Action action, float delay)
        {
            yield return new WaitForSeconds(delay);
            action?.Invoke();
        }
        
        public static void WaitActionUntil(Func<bool> condition, Action action)
        {
            SonatSdkManager.instance.StartCoroutine(ActionCondition(action, condition));
        }

        private static IEnumerator ActionCondition(Action action, Func<bool> condition)
        {
            yield return new WaitUntil(condition);
            action?.Invoke();
        }
    
        public static int GetEpochDate(DateTime date)
        {
            DateTime epochStart = new DateTime(1970, 1, 1, 0, 0, 0);
            return (int) (date - epochStart).TotalDays;
        }
    
    
        public static int GetEpochDate(Vector3Int date)
        {
            DateTime epochStart = new DateTime(1970, 1, 1, 0, 0, 0);
            return (int) (new  DateTime(date.z,date.y,date.x) - epochStart).TotalDays;
        }
        public static int GetEpochDate()
        {
            return GetEpochDate(DateTime.Today);
        }
        
        public static string ToString(IEnumerable<int> list)
        {
            return string.Join(",", list);
        }
        
        public static IEnumerable<int> ListIntFromString(string str)
        {
            if (string.IsNullOrEmpty(str)) return new List<int>();
            try
            {
                var splits = str.Split(',');
                return splits.Select(int.Parse);
            }
            catch (Exception e)
            {
                Debug.LogError("Parse err");
                return new List<int>();
            }

        }
    }
}
