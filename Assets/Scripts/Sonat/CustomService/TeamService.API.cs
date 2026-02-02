using Cysharp.Threading.Tasks;

public partial class TeamService
{
    #region BI API
    public async UniTask<TeamResponse> CreateTeam(TeamRequest teamRequest)
    {
        return await _onlineService.Instance.Post<TeamResponse>("teams", teamRequest);
    }

    public async UniTask<TeamResponse> UpdateTeam(string teamId, TeamRequest teamRequest)
    {
        return await _onlineService.Instance.Put<TeamResponse>($"teams/{teamId}", teamRequest);
    }

    public async UniTask<GetListTeamsResponse> GetListTeams(GetListTeamsRequest getListTeamsRequest)
    {
        return await _onlineService.Instance.Get<GetListTeamsResponse>($"teams{getListTeamsRequest.ToQueryString()}");
    }

    public async UniTask<TeamResponse> GetTeamInfo(string teamId)
    {
        return await _onlineService.Instance.Get<TeamResponse>($"teams/{teamId}");
    }

    public async UniTask<TeamResponse> GetCurrentTeamInfo()
    {
        return await _onlineService.Instance.Get<TeamResponse>($"teams/me");
    }

    public async UniTask<TeamUserModifyResponse> JoinTeam(string teamId)
    {
        return await _onlineService.Instance.Post<TeamUserModifyResponse>($"teams/{teamId}/join", null);
    }

    public async UniTask<BaseResponse> ProcessJoinRequest(TeamProcessJoinRequest requestType,
        string teamId,
        string requestId)
    {
        return await _onlineService.Instance.Put<BaseResponse>($"teams/{teamId}/join/{requestId}", requestType);
    }

    public async UniTask<TeamPendingRequestsResponse> GetListTeamPendingRequest(string teamId)
    {
        return await _onlineService.Instance.Get<TeamPendingRequestsResponse>($"teams/{teamId}/join");
    }

    public async UniTask<UserPendingRequestResponse> GetListUserPendingRequest()
    {
        return await _onlineService.Instance.Get<UserPendingRequestResponse>("teams/join");
    }

    public async UniTask<TeamUserModifyResponse> AssignCoLeader(string teamId, TeamUserModifyRequest userId)
    {
        return await _onlineService.Instance.Post<TeamUserModifyResponse>($"teams/{teamId}/assign", userId);
    }

    public async UniTask<TeamUserModifyResponse> RemoveCoLeader(string teamId, TeamUserModifyRequest userId)
    {
        return await _onlineService.Instance.Delete<TeamUserModifyResponse>($"teams/{teamId}/assign", userId);
    }

    public async UniTask<TeamUserModifyResponse> RemoveUser(string teamId, TeamUserModifyRequest userId)
    {
        return await _onlineService.Instance.Delete<TeamUserModifyResponse>($"teams/{teamId}/members", userId);
    }

    public async UniTask<TeamUserModifyResponse> LeaveTeam(TeamUserModifyRequest userId)
    {
        return await _onlineService.Instance.Delete<TeamUserModifyResponse>("teams/leave", userId);
    }

    public async UniTask<BaseResponse> HelpRequest(string teamId, string messageId)
    {
        return await _onlineService.Instance.Post<BaseResponse>($"teams/{teamId}/messages/{messageId}/help", null);
    }

    public async UniTask<TeamMessagesResponse> GetTeamMessageChat(string teamId, TeamGetMessageRequest teamRequest)
    {
        return await _onlineService.Instance.Get<TeamMessagesResponse>($"teams/{teamId}/messages{teamRequest.ToQueryString()}");
    }

    public async UniTask<TeamMessageData> SendTeamMessage(string teamId, TeamSendMessageRequest messageRequest)
    {
        return await _onlineService.Instance.Post<TeamMessageData>($"teams/{teamId}/messages", messageRequest);
    }
    #endregion
}
