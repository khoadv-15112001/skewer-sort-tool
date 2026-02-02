using GrillSort.OnlineService;
using MyGame.Modules.CardCollection;
using SonatFramework.Scripts.Helper;
using SonatFramework.Systems;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Stat_SetsCompleted", menuName = "My Services/StatGeneral/Stat_SetsCompleted")]
public class Stat_SetsCompleted : StatGeneral
{
    private IntDataPref isInit;

    private CardCollectionService cardCollectionService => SonatSystem.GetService<CardCollectionService>();

    public override void Initialize()
    {
        isInit = new IntDataPref("stat_sets_completed_is_init");

        if (!isInit.BoolValue)
        {
            CheckSyncLocal();
            isInit.BoolValue = true;
        }

        cardCollectionService.OnCompleteAlbum += OnCompleteAlbum;
    }

    private void OnCompleteAlbum()
    {
        AddValue(1);
    }

    public override void SetValue(int amount)
    {
        GetGeneralStats().setsCompleted = amount;

        base.SetValue(amount);
    }

    public override void AddValue(int amount)
    {
        GetGeneralStats().setsCompleted += amount;

        base.AddValue(amount);
    }

    public override int GetValue()
    {
        return GetGeneralStats().setsCompleted;
    }

    public override void CheckSyncLocal()
    {
        if (GetValue() >= cardCollectionService.GetCountCompleteAlbum()) return;

        SetValue(cardCollectionService.GetCountCompleteAlbum());
    }
}
