using Cysharp.Threading.Tasks;
using GrillSort.OnlineService;
using Newtonsoft.Json;
using Sirenix.OdinInspector;
using SonatFramework.Systems;
using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "StatsService", menuName = "My Services/StatsService")]
public class StatsService : SonatServiceSo, IServiceInitialize
{
    private const string PREF_GENERAL_STATS = "general_stats";

    [SerializeField] private List<StatBase> stats = new();
    public List<StatBase> Stats => stats;

    private OnlineService _onlineService => SonatSystem.GetService<OnlineService>();
    private GeneralStats _currentStats;

    // ----------------------------------------
    // INITIALIZE
    // ----------------------------------------
    public void Initialize() => CheckAndUpdateUserInfo().Forget();

    private async UniTaskVoid CheckAndUpdateUserInfo()
    {
        bool ready = await UniTask.WhenAny(
            UniTask.WaitUntil(() => _onlineService != null && _onlineService.Ready && _onlineService.UserInfo != null),
            UniTask.Delay(TimeSpan.FromSeconds(10))
        ) == 0;

        if (!ready)
        {
            Debug.LogWarning("[StatsService] OnlineService not ready after timeout.");
            LoadLocal();
            return;
        }

        //Debug.LogError("[StatsService]: GeneralStats: " + JsonConvert.SerializeObject(_onlineService.UserInfo));

        if (_onlineService.UserInfo.generalStats != null)
        {
            //Debug.LogError("[StatsService]: GeneralStats: " + JsonConvert.SerializeObject(_onlineService.UserInfo.generalStats));
            _currentStats = _onlineService.UserInfo.generalStats;
            SaveLocal();
        }
        else
        {
            LoadLocal();
            await UploadToServer();
        }

        await UniTask.Yield();

        foreach (var stat in stats)
        {
            stat.Initialize();
        }
    }

    // ----------------------------------------
    // PUBLIC API
    // ----------------------------------------

    public StatBase GetStat(StatsConfig.StatType type)
        => stats.Find(s => s.StatType == type);

    public GeneralStats GetGeneralStats()
        => _currentStats ??= new GeneralStats();

    public void SetGeneralStats(GeneralStats stats)
        => _currentStats = stats;

    [Button]
    public async UniTask UploadToServer()
    {
        if (_onlineService == null || !_onlineService.Ready)
            return;

        var userRequestData = new UserRequestData
        {
            generalStats = GetGeneralStats()
        };

        await _onlineService.BIUpdateUserInfo(userRequestData);
        Debug.Log("[StatsService] Uploaded GeneralStats to server.");
    }

    public void SaveLocal()
    {
        string json = JsonUtility.ToJson(GetGeneralStats());
        PlayerPrefs.SetString(PREF_GENERAL_STATS, json);
        PlayerPrefs.Save();
    }

    public void LoadLocal()
    {
        if (PlayerPrefs.HasKey(PREF_GENERAL_STATS))
        {
            string json = PlayerPrefs.GetString(PREF_GENERAL_STATS);
            _currentStats = JsonUtility.FromJson<GeneralStats>(json);
        }
        else
        {
            _currentStats = new GeneralStats();
        }

        Debug.Log("[StatsService] Loaded GeneralStats from local.");
    }
}
