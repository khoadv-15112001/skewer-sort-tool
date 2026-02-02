using System;
using System.Collections;
using System.Collections.Generic;
using Sonat.Enums;
using UnityEngine;

public class RewardHelper
{
    private static List<GameResource> listBoosters = new List<GameResource>();
    private static List<GameResource> listPreBoosters = new List<GameResource>()
    {
        GameResource.PreBoosterFreeze,
        GameResource.PreBoosterMagnet,
        GameResource.PreBoosterDoubleStar,
    };
    public static GameResource GetRandomBooster()
    {
        if (listBoosters.Count == 0)
        {
            CreateListBooster();
        }
        return listBoosters[UnityEngine.Random.Range(0, listBoosters.Count)];
    }

    public static GameResource GetRandomPreBooster()
    {
        return listPreBoosters[UnityEngine.Random.Range(0, listPreBoosters.Count)];
    }

    internal static GameResource GetCardMultiplier(int v)
    {
        throw new NotImplementedException();
    }

    private static void CreateListBooster()
    {
        for (GameResource resource = GameResource.None; resource < GameResource.MAX; resource++)
        {
            if (GameResourceHelper.ResourceType(resource) == GameResourceType.Booster)
            {
                listBoosters.Add(resource);
            }
        }
    }
}
