using System.Collections;
using System.Collections.Generic;
using MyGame.SkewerJam.Scripts.Service;
using Sonat.Enums;
using SonatFramework.Scripts.UIModule;
using SonatFramework.Systems.EventBus;
using SonatFramework.Systems.InventoryManagement;
using TMPro;
using UnityEngine;

public class PopupRewardEnergyUpdate : Panel
{
    [SerializeField] private TMP_Text txtEnergy;
    [SerializeField] private Transform spawnPoint;

    public override void Open(UIData uiData)
    {
        base.Open(uiData);

        int level = MySonatFramework.userDataService.GetLevel(GameMode.SkewerJam);
        var energy = MySonatFramework.GetService<HLWEventService>().HLWEventConfig.GetEnergyGift(level);
        txtEnergy.text = energy.ToString();
    }

    public void OnClickCollectEnergy()
    {
        base.Close();

        // quà unlock
        var log = new EarnResourceLogData()
        {
            spendType = "energy",
            spendId = "energy",
            source = "unlock_event"
        };
        int level = MySonatFramework.userDataService.GetLevel(GameMode.SkewerJam);
        MySonatFramework.GetService<InventoryService>().AddResource(
            GameResource.Energy,
            MySonatFramework.GetService<HLWEventService>().HLWEventConfig.GetEnergyGift(level),
            log,
            false);

        EventBus<AddItemEvent>.Raise(new AddItemEvent()
        {
            position = spawnPoint.position,
            resource = GameResource.Energy,
            quantity = 50,
            collectEffect = new CollectEffectMultiple()
        });
    }
}
