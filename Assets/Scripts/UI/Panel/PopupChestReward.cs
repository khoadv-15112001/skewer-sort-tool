using System.Collections;
using System.Collections.Generic;
using Sonat;
using Sonat.Enums;
using SonatFramework.Scripts.UIModule;
using SonatFramework.Systems.InventoryManagement;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PopupChestReward : Panel
{
    //[SerializeField] private Slider progressFill;
    [SerializeField] private Transform progress;
    [SerializeField] private TMP_Text txtProgress;
    private ChestRewardService chestRewardService;

    public override void Open(UIData uiData)
    {
        base.Open(uiData);
        chestRewardService = MySonatFramework.GetService<ChestRewardService>();
        UpdateValue(chestRewardService.data.currentProgress);
    }

    private void UpdateValue(int value)
    {
        var max = chestRewardService.configs.GetChestRewardData(chestRewardService.data.currentChestIndex).levelRequired;
        txtProgress.text = $"{value}/{max}";
        //float progress = value * 1.0f / max;
        //progressFill.value = progress;
        for (int i = 0; i < progress.childCount; i++)
        {
            progress.GetChild(i).gameObject.SetActive(i < value);
        }
    }
}