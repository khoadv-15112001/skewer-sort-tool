using GrillSort.Winstreak;
using Newtonsoft.Json;
using SonatFramework.Scripts.SonatSDKAdapterModule;
using SonatFramework.Systems.InventoryManagement.GameResources;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "LineConfig", menuName = "Sonat Configs Custom/LineConfig", order = 0)]
public class LineConfig : ScriptableObject
{
    public BannerName bannerName;
    public PackConfig packConfig;
    public int cooldown = 60;

    public int MaxLevel => packConfig.levels != null ? packConfig.levels.Count : 0;
    public void LoadRemoteConfig()
    {
        var packKey = $"pack_{bannerName.ToString().ToLower()}_config";
        packConfig = SonatSDKAdapter.GetRemoteConfig<PackConfig>(packKey, packConfig);
        Debug.Log($"anhnt: pack Key={packKey} - str={JsonConvert.SerializeObject(packConfig)}");
    }
    public PackLevelConfig GetConfigForLevel(int level)
    {
        if (packConfig.levels == null) return null;
        if (level < 1 || level > packConfig.levels.Count) return null;
        return packConfig.levels[level - 1];
    }
}

[System.Serializable]
public class PackConfig
{
    public List<PackLevelConfig> levels;  // index 0 → level1, 1 → level2, …
}
[System.Serializable]
public class PackLevelConfig
{
    public int level;
    public int purchaseLimit;
    public int membershipDays;
    public int timeBonus;
    public float saleOff;
    [Range(1, 100)] public int price;
}

public enum BannerName
{
    Revive,
    //Huge,
    //Amazing,
    Michelin
}
