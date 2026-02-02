using GrillSort.OnlineService;
using MyGame.Modules.CardCollection;
using SonatFramework.Scripts.Helper;
using SonatFramework.Systems;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

[CreateAssetMenu(fileName = "Stat_CollectionsCompleted", menuName = "My Services/StatGeneral/Stat_CollectionsCompleted")]
public class Stat_CollectionsCompleted : StatGeneral
{
    private IntDataPref isInit;

    private CardCollectionService cardCollectionService => SonatSystem.GetService<CardCollectionService>();

    public override void Initialize()
    {
        isInit = new IntDataPref("stat_collections_completed_is_init");

        if (!isInit.BoolValue)
        {
            CheckSyncLocal();
            isInit.BoolValue = true;
        }

        cardCollectionService.OnCompleteCollection += OnCompleteCollection;
    }

    private void OnCompleteCollection()
    {
        AddValue(1);
    }

    public override void SetValue(int amount)
    {
        GetGeneralStats().collectionsCompleted = amount;

        base.SetValue(amount);
    }

    public override void AddValue(int amount)
    {
        GetGeneralStats().collectionsCompleted += amount;

        base.AddValue(amount);
    }

    public override int GetValue()
    {
        return GetGeneralStats().collectionsCompleted;
    }

    public override void CheckSyncLocal()
    {
        if (GetValue() >= cardCollectionService.GetCountCompleteCollection()) return;

        SetValue(cardCollectionService.GetCountCompleteCollection());
    }
}
