using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public static class OrderEntityConfig
{
    public const int MAX_NORMAL_VISUAL = 16;
    public const int MAX_NORMAL_SHIPPER = 5;

    public static List<int> GetOrderVisuals()
    {
        if (LocalizationUtils.IsJapanese())
        {
            return Enumerable.Range(16, 9).ToList();
        }
        else
        {
            return Enumerable.Range(0, MAX_NORMAL_VISUAL).ToList();
        }
    }

    public static int GetRandomShipper()
    {
        if (LocalizationUtils.IsJapanese())
        {
            return 105;
        }
        else
        {
            return 100 + Random.Range(0, MAX_NORMAL_SHIPPER);
        }
    }
}
