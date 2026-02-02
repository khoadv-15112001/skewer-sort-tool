using GrillSort.OnlineService;
using SonatFramework.Scripts.Helper;
using SonatFramework.Systems.EventBus;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Stat_LongestStreak", menuName = "My Services/StatGeneral/Stat_LongestStreak")]
public class Stat_LongestStreak : StatGeneral
{
    private EventBinding<LevelEndedEvent> levelEndEvent;

    private IntDataPref numWinstreak;

    public override void Initialize()
    {
        numWinstreak = new IntDataPref("stat_num_winstreak");

        levelEndEvent = new EventBinding<LevelEndedEvent>(OnEndLevel);
    }

    private void OnEndLevel(LevelEndedEvent @event)
    {
        if (!@event.success)
        {
            numWinstreak.Value = 0;
            return;
        }

        numWinstreak.Value++;

        if (numWinstreak.Value <= GetValue()) return;

        AddValue(numWinstreak.Value - GetValue());
    }

    public override void AddValue(int amount)
    {
        GetGeneralStats().longestStreak += amount;

        base.AddValue(amount);
    }

    public override int GetValue()
    {
        return GetGeneralStats().longestStreak;
    }
}
