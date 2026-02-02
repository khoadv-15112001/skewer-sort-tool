using System;
using GrillSort.OnlineService;
using Sonat;
using Sonat.CustomService;
using Sonat.Enums;
using SonatFramework.Scripts.Feature.Lives;
using SonatFramework.Scripts.SonatSDKAdapterModule;
using SonatFramework.Systems;
using SonatFramework.Systems.AudioManagement;
using SonatFramework.Systems.BoosterManagement;
using SonatFramework.Systems.InventoryManagement;
using SonatFramework.Systems.LevelManagement;
using SonatFramework.Systems.LoadObject;
using SonatFramework.Systems.NetworkManagement;
using SonatFramework.Systems.ObjectPooling;
using SonatFramework.Systems.TrackingModule;
using SonatFramework.Systems.UserData;
using UnityEngine;

public class MySonatFramework : SonatSystem
{
    public static SonatPoolingService poolingService;
    public static SonatPoolingContainer poolingContainer;
    public static SonatLoadAddressableAsync sonatLoadAddressableAsync;
    public static SonatLevelService sonatLevelService;
    public static AudioService audioService;
    public static UserDataService userDataService;
    public static LivesService livesService;
    public static InventoryService inventoryService;
    public static LevelService levelService;
    public static CustomTrackingService customTrackingService;
    public static GameplayAnalyticsService gameplayAnalyticsService;
    public static SonatBoosterService sonatBoosterService;

    private void Start()
    {
        QualitySettings.vSyncCount = 0;
        Application.targetFrameRate = 60;
        Input.multiTouchEnabled = false;
        InitService();
    }

    public static void InitService()
    {
        poolingService = GetService<SonatPoolingService>();
        poolingContainer = GetService<SonatPoolingContainer>();
        sonatLoadAddressableAsync = GetService<SonatLoadAddressableAsync>();
        sonatLevelService = GetService<SonatLevelService>();
        audioService = GetService<AudioService>();
        userDataService = GetService<UserDataService>();
        livesService = GetService<LivesService>();
        inventoryService = GetService<InventoryService>();
        levelService = GetService<LevelRemoteService>();
        customTrackingService = GetService<CustomTrackingService>();
        gameplayAnalyticsService = GetService<GameplayAnalyticsService>();
        sonatBoosterService = GetService<SonatBoosterService>();
    }

    public static LevelDifficulty GetLevelDifficulty(int level)
    {
        try
        {

            return LevelGenerator.levelService != null
                ? LevelGenerator.levelService.GetLevelData<Gameplay.LevelData.LevelData>(level, GameMode.Classic).difficulty
                    : levelService.GetLevelData<Gameplay.LevelData.LevelData>(level, GameMode.Classic).difficulty;
        }
        catch (Exception)
        {
            return LevelDifficulty.Easy;
        }
    }

    public static LevelType GetLevelType(int level)
    {
        return levelService.GetLevelData<Gameplay.LevelData.LevelData>(level, GameMode.Classic).levelType;
    }
    public static bool IsNetworkAvailable()
    {
        return SonatSdkManager.IsInternetConnection();
    }
    internal static void ShowRewardAds(Action OnSuccess, string item_type = "", string item_id = "")
    {
        if (!SonatSDKAdapter.IsRewardAdsReady() && !IsNetworkAvailable())
        {
            Debug.Log("No Internet Connection - Show Reward Ads");
            NoInternet.Instance.ForceShowPopup();
            return;
        }
        SonatSDKAdapter.ShowRewardAds(() =>
        {
            OnSuccess?.Invoke();
        }, item_type, item_id);
    }
}