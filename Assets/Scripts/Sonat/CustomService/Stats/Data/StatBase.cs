using GrillSort.OnlineService;
using Sirenix.OdinInspector;
using SonatFramework.Systems;
using SonatFramework.Systems.TrackingModule;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class StatBase : ScriptableObject
{
    public StatsConfig.StatType StatType;
    [ValueDropdown(nameof(GetAllTerms))]
    public string Name;
    public Sprite icon;

    protected StatsService statsService => SonatSystem.GetService<StatsService>();
    protected GameplayAnalyticsService analyticsService => SonatSystem.GetService<GameplayAnalyticsService>();

    public abstract int GetValue();

    public abstract void SetValue(int value);

    public abstract void AddValue(int amount);

    public abstract void UploadToServer();

    public abstract void CheckSyncLocal();

    public abstract void Initialize();

    protected GeneralStats GetGeneralStats()
    {
        return statsService.GetGeneralStats();
    }

    public IEnumerable<string> GetAllTerms()
    {
        return LocalizationUtils.GetAllTerms();
    }
}
