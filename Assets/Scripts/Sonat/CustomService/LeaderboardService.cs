using Cysharp.Threading.Tasks;
using GrillSort.OnlineService;
using Sonat.FirebaseModule;
using SonatFramework.Scripts.SonatSDKAdapterModule;
using SonatFramework.Systems;
using SonatFramework.Systems.EventBus;
using SonatFramework.Systems.NetworkManagement;
using SonatFramework.Systems.UserData;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

[CreateAssetMenu(fileName = "LeaderboardService", menuName = "Sonat Services/Leaderboard Service")]
public class LeaderboardService : SonatServiceSo, IServiceInitialize
{
    private const string DATA_KEY = "Leaderboard";
    public LeaderboardConfig config;

    public int LevelUnlock => levelUnlock;
    private int levelUnlock;
    private LiveOpsPackData liveOpsPackData;

    public static int limitTopLeaderboard;
    public static bool forceUnlock = false;

    private readonly Service<OnlineService> _onlineService = new();
    private readonly Service<UserDataService> _userDataService = new();

    public void Initialize()
    {
        levelUnlock = SonatFirebase.remote.GetRemoteInt($"{DATA_KEY}_LevelUnlock", config.levelUnlock);
        liveOpsPackData = SonatSDKAdapter.GetRemoteConfig<LiveOpsPackData>($"{DATA_KEY}_liveOpsPackData", config.liveOpsPackData);

        new EventBinding<LevelEndedEvent>(OnLevelEnded);
        //new EventBinding<OnlineService.BIGetOrSignUpEvent>(OnBIGetOrSignUp);
        if (!PlayerPrefs.HasKey("BackupLeaderboardScore"))
        {
            UpdateUserScore(_userDataService.Instance.GetLevel()).Forget();
            PlayerPrefs.SetInt("BackupLeaderboardScore", 1);
        }

    }

    private void OnLevelEnded(LevelEndedEvent eventData)
    {
        if (eventData.success && eventData.gameMode == Sonat.Enums.GameMode.Classic)
            _ = SubmitScore(eventData.level + 1);
    }

    private void OnBIGetOrSignUp(OnlineService.BIGetOrSignUpEvent eventData)
    {
        if (eventData.success)
            _ = UpdateUserScore(_userDataService.Instance.GetLevel());
    }

    public async UniTask<LeaderboardResponseData> FetchLeaderboard(LeaderboardGroup group, LeaderboardFilterType filterType, int limit = 99)
    {
        LeaderboardRequestData leaderboardRequestData = new LeaderboardRequestData();
        leaderboardRequestData.grouping = group;
        leaderboardRequestData.limit = limit;
        leaderboardRequestData.filterType = filterType;

        try
        {
            return await GetLeaderboard(leaderboardRequestData);
        }
        catch (Exception ex)
        {
            Debug.LogError("Can't fetch leaderboard: " + ex.Message);
            return null;
        }
    }

    public async UniTask<LeaderboardTeamResponse> FetchTeamsLeaderboard(LeaderboardGroup group, LeaderboardFilterType filterType, int limit = 99)
    {
        LeaderboardRequestData leaderboardRequestData = new()
        {
            grouping = group,
            limit = limit,
            filterType = filterType
        };

        try
        {
            return await GetTeamsLeaderboard(leaderboardRequestData);
        }
        catch (Exception ex)
        {
            Debug.LogError("Can't fetch teams leaderboard: " + ex.Message);
            return null;
        }
    }

    public async UniTask UpdateUserScore(int score)
    {
        await SubmitScore(score);
    }

    public bool IsUnlockFeature()
    {
        if (forceUnlock) return true;
        return _userDataService.Instance.GetLevel() >= levelUnlock && CheckLiveOpsCondition();
    }

    public bool CheckLiveOpsCondition()
    {
        if (forceUnlock) return true;
        return liveOpsPackData.CheckCondition();
    }

    #region BI API
    private async UniTask<LeaderboardResponseData> GetLeaderboard(LeaderboardRequestData requestData)
    {
        return await _onlineService.Instance.Post<LeaderboardResponseData>("leaderboard/v2", requestData);
    }

    private async UniTask<LeaderboardTeamResponse> GetTeamsLeaderboard(LeaderboardRequestData requestData)
    {
        return await _onlineService.Instance.Post<LeaderboardTeamResponse>("leaderboard/team", requestData);
    }

    private async UniTask<LeaderboardResponseData> GetContestLeaderboard(bool previous)
    {
        LeaderboardContestRequest request = new LeaderboardContestRequest()
        {
            previous = previous
        };

        return await _onlineService.Instance.Post<LeaderboardResponseData>("leaderboard/contest", request);
    }

    private async UniTask<UnityWebRequest.Result> JoinContest()
    {
        return await _onlineService.Instance.SendRequest("leaderboard/contest/join", RequestType.POST, null);
    }

    private async UniTask<LeaderboardTrackingData> SubmitScore(int score)
    {
#if !UNITY_EDITOR
        var data = new Dictionary<string, int> { { "level", score } };
        return await _onlineService.Instance.Post<LeaderboardTrackingData>("leaderboard/v2/tracking", data);
#else
        return null;
#endif
    }
    #endregion
}
