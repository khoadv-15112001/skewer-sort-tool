using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using Sonat;
using Sonat.Enums;
using SonatFramework.Systems;
using SonatFramework.Systems.EventBus;
using SonatFramework.Systems.InventoryManagement;
using SonatFramework.Systems.InventoryManagement.GameResources;
using Unity.VisualScripting;
using UnityEngine;

[CreateAssetMenu(fileName = "StarChestService", menuName = "Sonat Services/Star Chest Service")]
public class StarChestService : SonatServiceSo, IServiceInitializeAsync
{
    private ComboService comboService;
    private InventoryService inventoryService;
    private int combo;
    private int star;
    protected PlayerPrefInt currentStarMilestone = new PlayerPrefInt("CurrentStarMilestone");
    [SerializeField] protected StarChestConfig starChestConfig;
    [SerializeField] protected List<StarChestRemoteConfig> starChestRemoteConfigs;

    public StarChest CurrentChest => starChestConfig.GetStarChest(currentStarMilestone.Value);
    [SerializeField] private List<int> starByCombo = new List<int>();

    public int Star => star;
    private int multiplier = 1;
    internal Action<int> onMultiplierChanged;

    public async UniTaskVoid InitializeAsync()
    {
        await LoadConfig();
        comboService = SonatSystem.GetService<ComboService>();
        inventoryService = SonatSystem.GetService<InventoryService>();
        comboService.OnComboChange += OnComboChange;
        new EventBinding<LevelStartedEvent>(OnLevelStarted);
        new EventBinding<LevelEndedEvent>(OnLevelEnded);
    }

    private async UniTask LoadConfig()
    {
        // ConfigType configType = global::Services.UserRemoteService.GetConfigType();
        // var remoteConfig = starChestRemoteConfigs.FirstOrDefault(x => x.configType == configType);
        // if (remoteConfig != null)
        // {
        //    starChestConfig = remoteConfig.config;
        // }
    }

    private void OnComboChange()
    {
        combo = comboService.Combo;
    }

    public int StarByCombo()
    {
        if (combo - 1 >= starByCombo.Count) return starByCombo[^1];
        return starByCombo[combo - 1];
    }

    public void OnCollectItem(Vector3 position)
    {
        int starAdd = StarByCombo() * multiplier;
        star += starAdd;
        EventBus<AddItemEvent>.Raise(new AddItemEvent() { resource = GameResource.Star, quantity = starAdd, position = position });
    }

    protected void OnLevelStarted(LevelStartedEvent levelStartedEvent)
    {
        star = 0;
        combo = comboService.Combo;
    }

    protected void OnLevelEnded(LevelEndedEvent eventData)
    {
        if (eventData.success)
        {
            EarnResourceLogData earnData = new()
            {
                spendId = "win",
                spendType = "win"
            };

            inventoryService.AddResource(GameResource.Star, star, earnData, false);
        }
    }

    public void NextStarChest()
    {
        currentStarMilestone.Value++;
    }

    public void SetMultiplier(int multiplier)
    {
        this.multiplier = multiplier;
        onMultiplierChanged?.Invoke(multiplier);
    }

    public RewardData GetCurrentReward()
    {
        return CurrentChest.GetRewardData();
    }
}

[Serializable]
public class StarChestRemoteConfig
{
    public ConfigType configType;
    public StarChestConfig config;
}

public enum ConfigType
{
    Default,
    Custom
}

