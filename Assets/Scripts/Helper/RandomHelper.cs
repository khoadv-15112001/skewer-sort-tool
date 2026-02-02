using System;
using System.Collections.Generic;
using System.Linq;

public static class RandomHelper
{

    static Random _rnd = new Random();

    static object randomLocker = new object();

    static public int Next(int min, int max)
    {
        lock (randomLocker)
        {
            return _rnd.Next(min, max);
        }
    }

    public static double NextDouble()
    {
        lock (randomLocker)
        {
            return _rnd.NextDouble();
        }
    }

    public static double NextDouble(double min, double max)
    {
        lock (randomLocker)
        {
            return _rnd.NextDouble() * (max - min) + min;
        }
    }

    public static int Next(int max)
    {
        lock (randomLocker)
        {
            return _rnd.Next(max);
        }
    }
    public static bool GetRandomChancePercent(float percent)
    {
        var rand = NextDouble() * 100;
        return rand < percent;
    }

    public static float GetRandomFloat()
    {
        return (float)NextDouble();
    }

    public static int GetRandomIndexInList(List<float> chances)
    {
        float total = 0;
        foreach (float c in chances)
        {
            total += c;
        }
        float rg = GetRandomFloat() * total;
        int index = 0;
        float checkChance = 0;
        foreach (float c in chances)
        {
            checkChance += c;
            if (rg < checkChance) return index;
            index++;
        }
        return -1;
    }

    public static T GetRandomKeyInDictionary<T>(Dictionary<T, float> dictionary)
    {
        float total = 0;
        foreach (float c in dictionary.Values)
        {
            total += c;
        }
        float rg = GetRandomFloat() * total;
        int index = 0;
        float checkChance = 0;
        foreach (var c in dictionary)
        {
            checkChance += c.Value;
            if (rg < checkChance) return c.Key;
            index++;
        }
        return default;
    }
    
    public static T GetRandomKeyInDictionary<T>(Dictionary<T, int> dictionary)
    {
        int total = 0;
        foreach (int c in dictionary.Values)
        {
            total += c;
        }
        float rg = GetRandomFloat() * total;
        int index = 0;
        float checkChance = 0;
        foreach (var c in dictionary)
        {
            checkChance += c.Value;
            if (rg < checkChance) return c.Key;
            index++;
        }
        return default;
    }
    

    public static List<int> GetRandomIndexsInList(List<float> chances, int count)
    {
        List<int> indexs = new List<int>();
        List<int> allIndexs = new List<int>(Enumerable.Range(0, chances.Count));
        for (int i = 0; i < count; i++)
        {
            int rdIndex = GetRandomIndexInList(chances);
            if (rdIndex >= 0)
            {
                indexs.Add(allIndexs[rdIndex]);
                chances.RemoveAt(rdIndex);
                allIndexs.RemoveAt(rdIndex);
            }
        }
        return indexs;
    }

    public static List<int> GetRandomIndexsInList(int count, int total)
    {
        List<int> indexs = new List<int>();
        List<int> allIndexs = new List<int>(Enumerable.Range(0, total));
        List<float> chances = new List<float>(Enumerable.Repeat(1f, total));

        for (int i = 0; i < count; i++)
        {
            int rdIndex = GetRandomIndexInList(chances);
            if (rdIndex >= 0)
            {
                indexs.Add(allIndexs[rdIndex]);
                chances.RemoveAt(rdIndex);
                allIndexs.RemoveAt(rdIndex);
            }
        }
        return indexs;
    }

    public static List<T> GetRandomElemntsInList<T>(List<T> list, int count)
    {
        List<T> org = new List<T>(list);
        List<T> t = new List<T>();
        // List<int> indexs = new List<int>();
        List<int> allIndexs = new List<int>(Enumerable.Range(0, list.Count));
        List<float> chances = new List<float>(Enumerable.Repeat(1f, list.Count));

        for (int i = 0; i < count; i++)
        {
            int rdIndex = GetRandomIndexInList(chances);
            if (rdIndex >= 0)
            {
                t.Add(org[allIndexs[rdIndex]]);
                // indexs.Add(allIndexs[rdIndex]);
                chances.RemoveAt(rdIndex);
                allIndexs.RemoveAt(rdIndex);
            }
        }
        return t;
    }

    public static List<T> Shuffle<T>(List<T> _list)
    {
        for (int i = 0; i < _list.Count; i++)
        {
            T temp = _list[i];
            int randomIndex = UnityEngine.Random.Range(i, _list.Count);
            _list[i] = _list[randomIndex];
            _list[randomIndex] = temp;
        }

        return _list;
    }
}
