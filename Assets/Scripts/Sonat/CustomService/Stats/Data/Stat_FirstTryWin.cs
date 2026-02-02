using GrillSort.OnlineService;
using SonatFramework.Systems.EventBus;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Stat_FirstTryWin", menuName = "My Services/StatGeneral/Stat_FirstTryWin")]
public class Stat_FirstTryWin : StatGeneral
{
    private EventBinding<LevelEndedEvent> levelEndEvent;

    public override void Initialize()
    {
        levelEndEvent = new EventBinding<LevelEndedEvent>(OnEndLevel);
    }

    private void OnEndLevel(LevelEndedEvent @event)
    {
        if (!@event.success) return;

        if (!analyticsService.levelPlayData.isFirstPlay) return;

        AddValue(1);
    }

    public override void AddValue(int amount)
    {
        GetGeneralStats().firstTryWin += amount;

        base.AddValue(amount);
    }

    public override int GetValue()
    {
        return GetGeneralStats().firstTryWin;
    }
}
