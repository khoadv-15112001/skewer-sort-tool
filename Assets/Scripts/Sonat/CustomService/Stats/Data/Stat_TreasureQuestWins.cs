using GrillSort.LavaQuest;
using GrillSort.OnlineService;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Stat_TreasureQuestWins", menuName = "My Services/StatGeneral/Stat_TreasureQuestWins")]
public class Stat_TreasureQuestWins : StatGeneral
{
    public override void Initialize()
    {
        LavaQuestService.OnWinEvent += OnWin;
    }

    private void OnWin()
    {
        AddValue(1);
    }

    public override void AddValue(int amount)
    {
        GetGeneralStats().treasureQuestWins += amount;

        base.AddValue(amount);
    }

    public override int GetValue()
    {
        return GetGeneralStats().treasureQuestWins;
    }
}
