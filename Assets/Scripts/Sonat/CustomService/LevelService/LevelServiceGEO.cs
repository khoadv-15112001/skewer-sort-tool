using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using Sonat.Enums;
using SonatFramework.Scripts.SonatSDKAdapterModule;
using SonatFramework.Systems;
using SonatFramework.Systems.LevelManagement;
using SonatFramework.Systems.LoadObject;
using UnityEngine;

[CreateAssetMenu(fileName = "LevelServiceGEO", menuName = "Custom Services/Level Service/Level Service GEO")]
public class LevelServiceGEO : LevelService, IServiceWaitingRemoteConfig
{
    [SerializeField] protected string path = "Level";

    [BoxGroup("SERVICES")] [Required] [SerializeField]
    protected Service<LoadObjectService> loadObjectService = new();

    protected LevelData levelCache;

    [SerializeField] private SonatLevelService levelServiceFallback;

    public void OnRemoteConfigReady()
    {
        path = SonatSDKAdapter.GetRemoteString("level_id");
        Debug.Log($"current level id: {path}");
    }

    public override T GetLevelData<T>(int level, GameMode gameMode, bool force = false, bool loop = true, int category = 0)
    {
        if (!force && levelCache != null && levelCache.gameMode == gameMode && levelCache.level == level && levelCache.category == category)
            return levelCache as T;
        T levelData = GetLevel<T>(level, gameMode, category) ?? levelServiceFallback?.GetLevelData<T>(level, gameMode, force, loop, category);
        return levelData;
    }

    protected override T GetLevel<T>(int level, GameMode gameMode, int category)
    {
        string mergePath = category == 0 ? string.IsNullOrEmpty(path) ? $"{level}.json" : $"{path}/{level}.json" :
            string.IsNullOrEmpty(path) ? $"{level}.{category}.json" : $"{path}/{level}.{category}.json";
        var levelData = loadObjectService.Instance.LoadObject<T>(mergePath);
        if (levelData == null && category > 0)
        {
            levelData = GetLevel<T>(level, gameMode, category - 1);
        }
        return levelData;
    }

    public override void SaveLevel<T>(T levelData)
    {
        throw new System.NotImplementedException();
    }
}