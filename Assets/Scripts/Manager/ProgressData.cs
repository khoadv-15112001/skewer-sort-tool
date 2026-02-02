using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

[CreateAssetMenu(fileName = "ProgressData", menuName = "GameData/ProgressData")]
public class ProgressData : ScriptableObject
{
    public List<ProgressMilestone> progressMilestone;

    public ProgressMilestone GetMilestoneData(int level)
    {
        int lastLevel = 0;
        foreach (var progressMilestone in progressMilestone)
        {
            if (progressMilestone.level >= level)
            {
                progressMilestone.progress = progressMilestone.level - lastLevel;
                return progressMilestone;
            }
            lastLevel = progressMilestone.level;
        }
        return null;
    }
}

[Serializable]
public class ProgressMilestone
{
    public int level;
    [HideInInspector] public int progress;
    public ProgressType type;
    public string description;
}

public enum ProgressType : byte
{
    Item,
    Resource,
    Feature
}
