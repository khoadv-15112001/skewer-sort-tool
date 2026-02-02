using System.Collections;
using System.Collections.Generic;
using Sonat;
using Sonat.Enums;
using SonatFramework.Scripts.UIModule;
using SonatFramework.Scripts.UIModule.UIElements;
using SonatFramework.Systems.InventoryManagement;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PopupStarChest : Panel
{
    [SerializeField] private Slider progressFill;
    [SerializeField] private TMP_Text txtProgress;
    [SerializeField] private UIRewardGrid uIRewardGrid;
    private StarChestService starChestService;
    private InventoryService inventoryService;
    private int currentStar = 0;
    private StarChest starChest;

    public override void Open(UIData uiData)
    {
        base.Open(uiData);
        inventoryService = MySonatFramework.GetService<InventoryService>();
        starChestService = MySonatFramework.GetService<StarChestService>();
        starChest = starChestService.CurrentChest;
        UpdateValue(inventoryService.GetResourceView(GameResource.Star));

        uIRewardGrid.SetReward(starChestService.GetCurrentReward());
    }

    private void UpdateValue(int value)
    {
        txtProgress.text = $"{value}/{starChest.starRequire}";
        float progress = value * 1.0f / starChest.starRequire;
        progressFill.value = progress;
    }
}