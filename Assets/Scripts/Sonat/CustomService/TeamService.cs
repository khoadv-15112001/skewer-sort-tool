using Cysharp.Threading.Tasks;
using GrillSort.OnlineService;
using GrillSort.RealTime;
using Helper;
using Newtonsoft.Json;
using Sonat.Enums;
using Sonat.FirebaseModule;
using SonatFramework.Scripts.SonatSDKAdapterModule;
using SonatFramework.Systems;
using SonatFramework.Systems.EventBus;
using SonatFramework.Systems.InventoryManagement.GameResources;
using SonatFramework.Systems.UserData;
using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Services.Core;
using UnityEngine;

[CreateAssetMenu(fileName = "TeamService", menuName = "Sonat Services/Team Service")]
public partial class TeamService : SonatServiceSo, IServiceInitialize
{
    public TeamConfig config;

    public int levelUnlock;
    public int createCost;

    public bool isUpdatingCache;

    private LiveOpsPackData liveOpsPackData;

    private TeamResponse currentTeamData;
    private List<string> pendingJoinRequests = new();

    private bool isCheckingTeam;

    private readonly Service<UserDataService> _userDataService = new();
    private readonly Service<OnlineService> _onlineService = new();
    private readonly Service<RealTimeService> _timeService = new();
    private readonly Service<ProfileService> _profileService = new();

    public static long LastHelpRequestCoolDown;
    public static bool forceUnlock = false;

    private const string DATA_KEY = "Team";

    public TeamResponse CurrentTeamData => currentTeamData;
    public List<TeamMessageData> ListMessage { get; private set; } = new();
    public string ChatListToken { get; private set; } = null;

    public void Initialize()
    {
        levelUnlock = SonatFirebase.remote.GetRemoteInt($"{DATA_KEY}_LevelUnlock", config.levelUnlock);
        createCost = SonatFirebase.remote.GetRemoteInt($"{DATA_KEY}_CreateCost", config.createCost);
        liveOpsPackData = SonatSDKAdapter.GetRemoteConfig($"{DATA_KEY}_liveOpsPackData", config.liveOpsPackData);

        new EventBinding<OnlineService.BIGetOrSignUpEvent>(OnBIGetOrSignUp);
        new EventBinding<OnlineService.MessageReceivedEvent>(OnMessageReceived);

        if (_onlineService.Instance.UserInfo != null && !IsJoinedTeam())
            CheckJoinTeam().Forget();
    }

    private void OnBIGetOrSignUp(OnlineService.BIGetOrSignUpEvent eventData)
    {
        if (eventData.success && !IsJoinedTeam())
            _ = CheckJoinTeam();
    }

    private void SetTeamData(TeamResponse teamData)
    {
        currentTeamData = teamData;
    }

    public async UniTask CheckJoinTeam()
    {
        if (isCheckingTeam) return;
        isCheckingTeam = true;

        LastHelpRequestCoolDown = _onlineService.Instance.UserInfo.helpRequestCoolDown == null
            ? 0
            : ((DateTimeOffset)_onlineService.Instance.UserInfo.helpRequestCoolDown.Value).ToUnixTimeSeconds();

        await UpdateTeamInfoCache();

        if (!IsJoinedTeam())
        {
            var response = await GetListUserPendingRequest();
            if (response.code == ResponseCode.SUCCESS)
                pendingJoinRequests = response.data.Select(d => d.id).ToList();
        }

        isCheckingTeam = false;
    }

    private async UniTask UpdateTeamInfoCache()
    {
        if (isUpdatingCache) return;
        isUpdatingCache = true;

        var data = await GetCurrentTeamInfo();

        SetTeamData(data);

        isUpdatingCache = false;
    }

    public bool CheckLiveOpsCondition()
    {
        if (forceUnlock) return true;
        return liveOpsPackData.CheckCondition();
    }

    public bool IsUnlockFeature()
    {
        if (IsJoinedTeam()) return true;

        return _userDataService.Instance.GetLevel() >= levelUnlock && CheckLiveOpsCondition();
    }

    public bool IsJoinedTeam()
    {
        if (currentTeamData == null) return false;

        return currentTeamData.code == ResponseCode.SUCCESS;
    }

    public bool IsTeamInfoLoaded()
    {
        return currentTeamData != null;
    }

    public bool CanSendLiveRequest()
    {
        if (LastHelpRequestCoolDown == 0) return true;
        return _timeService.Instance.GetCurrentTimeUnix() > LastHelpRequestCoolDown +
            BIGameConfigService.Instance.BIGameConfig.coolDownSecondsBetweenHelpRequest;
    }

    public string GetCountdownRequestStr(long time)
    {
        var ts = TimeSpan.FromSeconds(time);

        if (ts.TotalDays >= 1)
        {
            int days = ts.Days;
            int hours = ts.Hours;
            return $"{days}d {hours:00}h";
        }
        else if (ts.TotalHours >= 1)
        {
            int hours = (int)ts.TotalHours;
            int minutes = ts.Minutes;
            return $"{hours:00}h {minutes:00}m";
        }
        else
        {
            int minutes = ts.Minutes;
            int sec = ts.Seconds;
            return $"{minutes:00}m {sec:00}s";
        }
    }

    public TeamRole GetRole(TeamResponse data, string id)
    {
        if (data.leader.id == id)
            return TeamRole.Leader;

        if (data.coLeaders.Find(m => m.id == id) != null)
            return TeamRole.Co_Leader;

        if (data.members.Find(m => m.id == id) != null)
            return TeamRole.Member;

        return TeamRole.None;
    }

    public TeamRole GetMyRole()
    {
        if (!IsJoinedTeam()) return TeamRole.None;

        return GetRole(currentTeamData, _onlineService.Instance.UserID);
    }

    public static string GetRoleString(TeamRole role)
    {
        if (role == TeamRole.None) return "";
        else if (role == TeamRole.Co_Leader) return "Co-leader";
        else return "Leader";
    }

    public List<string> GetListTeamRequested()
    {
        return pendingJoinRequests;
    }

    public void RequestJoinTeam(string teamId)
    {
        if (!pendingJoinRequests.Contains(teamId))
            pendingJoinRequests.Add(teamId);
    }

    public void JoinTeam(TeamResponse teamData)
    {
        pendingJoinRequests.Clear();

        SetTeamData(teamData);
        EventBus<JoinTeamEvent>.Raise(new() { teamData = teamData });
    }

    public void LeaveTeam()
    {
        currentTeamData.code = ResponseCode.NOT_FOUND;
        EventBus<LeaveTeamEvent>.Raise(new());
    }

    public void UpdateTeam(TeamResponse teamData)
    {
        SetTeamData(teamData);
        EventBus<UpdateTeamEvent>.Raise(new());
    }

    #region Events

    public struct JoinTeamEvent : IEvent
    {
        public TeamResponse teamData;
    }

    public struct RequestJoinTeamEvent : IEvent
    {
        public string teamId;
    }

    public struct LeaveTeamEvent : IEvent
    {
    }

    public struct UpdateTeamEvent : IEvent
    {
    }

    public struct UpdateMessageEvent : IEvent
    {
        public TeamUpdateMessageType updateType;
        public TeamMessageData messageData;
        public bool refreshScroll;
    }

    #endregion

    #region Handle Message
    public void DeleteMessage(string messageId)
    {
        if (ListMessage == null || ListMessage.Count == 0) return;

        var find = ListMessage.Find(m => m.id == messageId);

        if (find != null)
        {
            ListMessage.Remove(find);

            EventBus<UpdateMessageEvent>.Raise(new UpdateMessageEvent()
            {
                updateType = TeamUpdateMessageType.Delete,
            });
        }
    }

    public async UniTask<List<TeamMessageData>> GetTeamChat(bool useChatListToken)
    {
        if (!useChatListToken)
            ChatListToken = null;

        if (string.IsNullOrEmpty(ChatListToken))
            ListMessage.Clear();

        string teamId = CurrentTeamData?.id;

        if (string.IsNullOrEmpty(teamId))
            return ListMessage;

        try
        {
            var teamMessageChatTask = GetTeamMessageChat(teamId,
                new TeamGetMessageRequest()
                {
                    cursor = ChatListToken,
                    limit = config.messageLimit,
                    target = TargetEnum.chat
                });

            var teamMessageRequestTask = GetTeamMessageChat(teamId,
                new TeamGetMessageRequest()
                {
                    cursor = ChatListToken,
                    limit = config.messageLimit,
                    target = TargetEnum.manage
                });

            var teamMessageSystemTask = GetTeamMessageChat(teamId,
                new TeamGetMessageRequest()
                {
                    cursor = ChatListToken,
                    limit = config.messageLimit,
                    target = TargetEnum.activity
                });

            var (teamMessageChat, teamMessageRequest, teamMessageSystem) =
                await UniTask.WhenAll(teamMessageChatTask, teamMessageRequestTask, teamMessageSystemTask);

            //Debug.LogError("abc teamMessageRequest count: " + teamMessageRequest?.data?.Count);
            //Debug.LogError("abc teamMessageSystem count: " + teamMessageSystem?.data?.Count);

            if (teamMessageRequest is { code: ResponseCode.SUCCESS, data: { Count: > 0 } })
            {
                foreach (var request in teamMessageRequest.data)
                {
                    if (ListMessage.Find(m => m.id == request.id) == null)
                        ListMessage.Add(request);
                }
            }

            if (teamMessageSystem is { code: ResponseCode.SUCCESS, data: { Count: > 0 } })
            {
                foreach (var request in teamMessageSystem.data)
                {
                    if (ListMessage.Find(m => m.id == request.id) == null)
                        ListMessage.Add(request);
                }
            }

            //Debug.LogError("abc ListMessage count: " + ListMessage.Count);

            ChatListToken = null;

            if (teamMessageChat is null)
            {
                Debug.Log("[TeamService] No data returned from server.");
                return ListMessage;
            }

            if (teamMessageChat.code == ResponseCode.NOT_FOUND)
                return ListMessage;

            if (teamMessageChat is not { code: ResponseCode.SUCCESS })
                return ListMessage;

            if (teamMessageChat.data is not { Count: > 0 })
                return ListMessage;

            ChatListToken = teamMessageChat.pageInfo.nextCursor;

            var reverseList = teamMessageChat.data.Where(x => x is not { sender: null }) /*.Reverse()*/.ToList();

            //Debug.LogError("abc reverseList count: " + reverseList.Count);

            ListMessage = reverseList.Concat(ListMessage).Distinct().OrderByDescending(x => x.created_at, StringComparer.Ordinal).ToList();

            //Debug.LogError("abc ListMessage count: " + ListMessage.Count);
            ValidateTeamRewardMessages();

            CheckLimitChat();

            return ListMessage;
        }
        catch (Exception ex)
        {
            Debug.LogError($"[TeamService] Error fetching team chat: {ex.Message}");
        }

        return ListMessage;
    }

    public async UniTask<List<TeamMessageData>> GetUpdatedListMessage()
    {
        if (string.IsNullOrEmpty(ChatListToken))
            ListMessage.Clear();

        string teamId = CurrentTeamData?.id;

        if (string.IsNullOrEmpty(teamId))
            return ListMessage;

        try
        {
            var teamMessageChat = await GetTeamMessageChat(teamId,
                new TeamGetMessageRequest()
                {
                    cursor = ChatListToken,
                    limit = config.messageLimit,
                    target = TargetEnum.chat,
                });


            if (teamMessageChat is null or { code: ResponseCode.NOT_FOUND } or not { code: ResponseCode.SUCCESS } or
                { data: { Count: <= 0 } })
            {
                Debug.LogWarning("[TeamService] No data returned from server.");
                return null;
            }

            var reverseList = teamMessageChat.data.Where(x => x is not { sender: null }).Reverse().ToList();

            var lastestMessageInList =
                ListMessage.LastOrDefault(x => x.type is TeamMessageType.text or TeamMessageType.help_request);

            var hasSetOfMessage = ListMessage.Select(x => x.id).ToHashSet();

            for (var i = 0; i < reverseList.Count; i++)
            {
                var messageItem = reverseList[i];
                if (lastestMessageInList == null)
                {
                    ListMessage.Add(messageItem);

                    EventBus<UpdateMessageEvent>.Raise(new UpdateMessageEvent()
                    {
                        updateType = TeamUpdateMessageType.Insert,
                        messageData = messageItem
                    });

                    continue;
                }

                if (TimeHelper.GetUtcTimeFromISO(lastestMessageInList.created_at) <= TimeHelper.GetUtcTimeFromISO(messageItem.created_at) &&
                    !hasSetOfMessage.Contains(messageItem.id))
                {
                    ListMessage.Add(messageItem);

                    EventBus<UpdateMessageEvent>.Raise(new UpdateMessageEvent()
                    {
                        updateType = TeamUpdateMessageType.Insert,
                        messageData = messageItem
                    });
                }
                else
                {
                    var messageIndex = ListMessage.FindIndex(x => x.id == messageItem.id);
                    if (messageIndex == -1) continue;
                    ListMessage[messageIndex] = messageItem;

                    EventBus<UpdateMessageEvent>.Raise(new UpdateMessageEvent()
                    {
                        updateType = TeamUpdateMessageType.Update,
                        messageData = messageItem
                    });
                }
            }

            reverseList.Reverse();
            return reverseList;
        }
        catch (Exception ex)
        {
            Debug.LogError($"[TeamService] Error fetching team chat: {ex.Message}");
        }

        return ListMessage;
    }

    public async UniTask<TeamMessageData> SendMessage(string content, TeamMessageType messageType)
    {
        if (currentTeamData == null)
            return null;

        try
        {
            var messageContent = messageType switch
            {
                TeamMessageType.text => new TeamSendMessageRequest()
                {
                    content = content,
                    type = TeamMessageType.text
                },
                TeamMessageType.help_request => new TeamSendMessageRequest()
                {
                    type = TeamMessageType.help_request,
                    resource = OnlineResourceType.Lives
                },
                _ => throw new ArgumentOutOfRangeException(nameof(messageType), messageType, null)
            };

            var response = await SendTeamMessage(currentTeamData.id, messageContent);

            if (response is not { code: ResponseCode.SUCCESS })
            {
                Debug.LogWarning("[TeamService] No data returned from server.");
                return response;
            }

            var newMessage = response;

            if (newMessage.type == TeamMessageType.help_request)
            {
                LastHelpRequestCoolDown = TimeHelper.GetUnixTimeFromISO(response.created_at);
                newMessage.resource = OnlineResourceType.Lives;
            }

            newMessage.updated_at = newMessage.created_at;
            newMessage.sender = new Member()
            {
                avatar = ProfileService.GetAvatarData(),
                name = _onlineService.Instance.UserInfo.name,
                id = _onlineService.Instance.UserID,
            };

            ListMessage.Insert(0, newMessage);
            CheckLimitChat();

            EventBus<UpdateMessageEvent>.Raise(new UpdateMessageEvent()
            {
                updateType = TeamUpdateMessageType.Insert,
                messageData = newMessage,
                refreshScroll = true
            });

            return newMessage;
        }
        catch (Exception ex)
        {
            Debug.LogError($"[TeamService] Error send message: {ex.Message}");
            throw;
        }
    }

    // Overload for sending resource requests (lives, coins, cards)
    public async UniTask<TeamMessageData> SendMessage(string resourceType, int? cardId = null)
    {
        if (currentTeamData == null)
            return null;

        try
        {
            string resource = resourceType;

            // If it's a card and cardId is provided, format as "card_0"
            if (resourceType == OnlineResourceType.Card && cardId.HasValue)
            {
                var cardType = cardId.Value;
                resource = OnlineResourceType.FormatCardResource(cardType.ToString());
            }

            var messageContent = new TeamSendMessageRequest()
            {
                type = TeamMessageType.help_request,
                resource = resource
            };

            var response = await SendTeamMessage(currentTeamData.id, messageContent);

            if (response is not { code: ResponseCode.SUCCESS })
            {
                return response;
            }

            var newMessage = response;

            if (newMessage.type == TeamMessageType.help_request)
            {
                if (resource == OnlineResourceType.Lives)
                    LastHelpRequestCoolDown = TimeHelper.GetUnixTimeFromISO(response.created_at);

                newMessage.resource = resource;
            }

            newMessage.updated_at = newMessage.created_at;
            newMessage.sender = new Member()
            {
                avatar = ProfileService.GetAvatarData(),
                name = _onlineService.Instance.UserInfo.name,
                id = _onlineService.Instance.UserID,
            };

            ListMessage.Insert(0, newMessage);

            EventBus<UpdateMessageEvent>.Raise(new UpdateMessageEvent()
            {
                updateType = TeamUpdateMessageType.Insert,
                messageData = newMessage,
                refreshScroll = true
            });

            CheckLimitChat();

            return newMessage;
        }
        catch
        {
            throw;
        }
    }

    public async UniTask<TeamMessageData> SendTeamRewards(string teamId, RewardData rewards,
        string content, string expiresAt, string source)
    {
        if (string.IsNullOrEmpty(teamId) || rewards == null || rewards.resourceDatas == null || rewards.resourceDatas.Count == 0)
            return null;

        try
        {
            var messageContent = new TeamSendMessageRequest()
            {
                content = content,
                type = TeamMessageType.team_reward,
                rewardsInfo = new()
                {
                    rewards = rewards.ToListRewardDataJson(),
                    expiresAt = expiresAt,
                    source = source
                }
            };

            var response = await SendTeamMessage(teamId, messageContent);

            if (response is not { code: ResponseCode.SUCCESS })
            {
                Debug.LogWarning($"[TeamService] No data returned from server.");
                return response;
            }

            return response;
        }
        catch (Exception ex)
        {
            Debug.LogError($"[TeamService] Error send message: {ex.Message}");
            throw;
        }
    }

    public async UniTask<BaseResponse> SendHelpRequest(string teamId, string messageId)
    {
        if (string.IsNullOrEmpty(teamId) || string.IsNullOrEmpty(messageId))
        {
            return null;
        }

        try
        {
            var response = await HelpRequest(teamId, messageId);

            if (response is null)
            {
                return null;
            }

            if (response.code == ResponseCode.NOT_FOUND)
            {
                return null;
            }

            if (response is not { code: ResponseCode.SUCCESS })
            {
                return response;
            }

            return response;
        }
        catch
        {
            throw;
        }
    }

    private void OnMessageReceived(OnlineService.MessageReceivedEvent eventData)
    {
        var messageData = ParseStringToMessage(eventData.messageString);

        if (messageData is null)
        {
            return;
        }

        if (eventData.type != MessageType.Update && IsDoubleMessage(messageData.id)) return;

        switch (eventData.type)
        {
            case MessageType.New:
                HandleTeamNewMessage(messageData);
                break;
            case MessageType.Update:
                HandleTeamUpdateMessage(messageData);
                break;
            case MessageType.TeamJoinSuccess:
                HandleTeamJoinSuccess(messageData);
                break;
            case MessageType.TeamJoinPending:
                HandleTeamJoinPending(messageData);
                break;
            case MessageType.TeamJoinAccepted:
                HandleTeamJoinAccepted(messageData);
                break;
            case MessageType.TeamJoinRejected:
                HandleTeamJoinRejected(messageData);
                break;
            case MessageType.TeamLeaveSuccess:
                HandleTeamLeaveSuccess(messageData);
                break;
            case MessageType.TeamCoLeaderPromote:
                HandleTeamCoLeaderPromote(messageData);
                break;
            case MessageType.TeamCoLeaderDemote:
                HandleTeamCoLeaderDemote(messageData);
                break;
            case MessageType.TeamMemberRemoved:
                HandleTeamMemberRemoved(messageData);
                break;
        }
    }

    private bool IsDoubleMessage(string id)
    {
        return ListMessage.FindIndex(e => e.id == id) > -1;
    }

    private void HandleTeamJoinSuccess(TeamMessageData messageData)
    {
        //Debug.LogError("abc join success: " + JsonConvert.SerializeObject(messageData));
        if (!IsJoinedTeam()) return;

        messageData.type = TeamMessageType.join_success;
        ListMessage.Insert(0, messageData);
        CheckLimitChat();

        EventBus<UpdateMessageEvent>.Raise(new UpdateMessageEvent()
        {
            updateType = TeamUpdateMessageType.Insert,
            messageData = messageData
        });

        UpdateTeamInfoCache().Forget();
    }

    private void HandleTeamJoinPending(TeamMessageData messageData)
    {
        //Debug.LogError("abc receive join request: " + JsonConvert.SerializeObject(messageData));
        if (!IsJoinedTeam()) return;

        messageData.type = TeamMessageType.join_request_pending;

        ListMessage.Insert(0, messageData);
        CheckLimitChat();

        EventBus<UpdateMessageEvent>.Raise(new UpdateMessageEvent()
        {
            updateType = TeamUpdateMessageType.Insert,
            messageData = messageData
        });
    }

    private async UniTask HandleTeamJoinAccepted(TeamMessageData messageData)
    {
        //Debug.LogError("abc join accepted: " + JsonConvert.SerializeObject(messageData));

        await UpdateTeamInfoCache();

        if (IsJoinedTeam())
            pendingJoinRequests.Clear();
    }

    private void HandleTeamJoinRejected(TeamMessageData messageData)
    {
        //Debug.LogError("abc join rejected: " + JsonConvert.SerializeObject(messageData));

        if (pendingJoinRequests.Contains(messageData.team))
            pendingJoinRequests.Remove(messageData.team);
    }

    private void HandleTeamLeaveSuccess(TeamMessageData messageData)
    {
        //Debug.LogError("abc leave team: " + JsonConvert.SerializeObject(messageData));
        if (!IsJoinedTeam()) return;

        messageData.type = TeamMessageType.leave_success;

        ListMessage.Insert(0, messageData);
        CheckLimitChat();

        EventBus<UpdateMessageEvent>.Raise(new UpdateMessageEvent()
        {
            updateType = TeamUpdateMessageType.Insert,
            messageData = messageData
        });

        UpdateTeamInfoCache().Forget();
    }

    private void HandleTeamCoLeaderPromote(TeamMessageData messageData)
    {
        //Debug.LogError("abc promote: " + JsonConvert.SerializeObject(messageData));
        if (!IsJoinedTeam()) return;

        messageData.type = TeamMessageType.co_leader_promote;

        ListMessage.Insert(0, messageData);
        CheckLimitChat();

        EventBus<UpdateMessageEvent>.Raise(new UpdateMessageEvent()
        {
            updateType = TeamUpdateMessageType.Insert,
            messageData = messageData
        });

        UpdateTeamInfoCache().Forget();
    }

    private void HandleTeamCoLeaderDemote(TeamMessageData messageData)
    {
        //Debug.LogError("abc demote: " + JsonConvert.SerializeObject(messageData));
        if (!IsJoinedTeam()) return;

        messageData.type = TeamMessageType.co_leader_demote;

        ListMessage.Insert(0, messageData);
        CheckLimitChat();

        EventBus<UpdateMessageEvent>.Raise(new UpdateMessageEvent()
        {
            updateType = TeamUpdateMessageType.Insert,
            messageData = messageData
        });

        UpdateTeamInfoCache().Forget();
    }

    private async UniTask HandleTeamMemberRemoved(TeamMessageData messageData)
    {
        //Debug.LogError("abc remove member: " + JsonConvert.SerializeObject(messageData));
        if (!IsJoinedTeam()) return;

        messageData.type = TeamMessageType.member_removed;

        ListMessage.Insert(0, messageData);
        CheckLimitChat();

        EventBus<UpdateMessageEvent>.Raise(new UpdateMessageEvent()
        {
            updateType = TeamUpdateMessageType.Insert,
            messageData = messageData
        });

        if (messageData.sender.id == _onlineService.Instance.UserID)
            LeaveTeam();
        else
            await UpdateTeamInfoCache();
    }

    private void HandleTeamNewMessage(TeamMessageData messageData)
    {
        if (messageData == null || messageData.id == null) return;
        if (!IsJoinedTeam()) return;

        if (messageData.type == TeamMessageType.help_request)
        {
            var messageUpdateIndex = ListMessage.FindIndex(x => x.sender.id == messageData.sender.id && 
                x.type == TeamMessageType.help_request && 
                MyStringComparer.Compare(x.resource, messageData.resource, MyStringComparison.Ordinal));

            if (messageUpdateIndex >= 0)
            {
                ListMessage.RemoveAt(messageUpdateIndex);

                EventBus<UpdateMessageEvent>.Raise(new UpdateMessageEvent()
                {
                    updateType = TeamUpdateMessageType.Delete,
                });
            }
        }

        ListMessage.Insert(0, messageData);
        CheckLimitChat();

        EventBus<UpdateMessageEvent>.Raise(new UpdateMessageEvent()
        {
            updateType = TeamUpdateMessageType.Insert,
            messageData = messageData
        });
    }

    private void HandleTeamUpdateMessage(TeamMessageData messageData)
    {
        //Debug.LogError("abc update: " + JsonConvert.SerializeObject(messageData));
        if (!IsJoinedTeam()) return;

        var messageUpdateIndex = ListMessage.FindIndex(x => x.id == messageData.id);
        if (messageUpdateIndex < 0) return;

        if (messageData.type == TeamMessageType.help_request)
        {
            if (messageData.helpRequest.status == RequestAcceptType.completed)
            {
                ListMessage.RemoveAt(messageUpdateIndex);

                EventBus<UpdateMessageEvent>.Raise(new UpdateMessageEvent()
                {
                    updateType = TeamUpdateMessageType.Delete,
                    messageData = messageData
                });
            }
            else
            {
                ListMessage[messageUpdateIndex] = messageData;

                EventBus<UpdateMessageEvent>.Raise(new UpdateMessageEvent()
                {
                    updateType = TeamUpdateMessageType.Update,
                    messageData = messageData
                });
            }
        }
    }

    private TeamMessageData ParseStringToMessage(string messageContent)
    {
        try
        {
            return JsonConvert.DeserializeObject<TeamMessageData>(messageContent);
        }
        catch (Exception ex)
        {
            Debug.LogError($"[TeamService] Error parse message: {ex.Message}");
            return null;
        }
    }

    public bool CanLoadMoreMessages()
    {
        return ListMessage == null || (ListMessage.Count + config.messagePerRequest <= config.limitChat);
    }

    private void CheckLimitChat()
    {
        if (ListMessage != null)
        {
            while (ListMessage.Count > config.limitChat)
                ListMessage.RemoveAt(ListMessage.Count - 1);
        }
    }

    private void ValidateTeamRewardMessages()
    {
        for (int i = 0; i < ListMessage.Count; i++)
        {
            var message = ListMessage[i];
            if (message.type != TeamMessageType.team_reward) continue;

            if (message.rewardsInfo == null ||
                 message.rewardsInfo.receiverIds == null ||
                 message.rewardsInfo.receiverIds.Contains(_onlineService.Instance.UserID) ||
                 message.sender.id == _onlineService.Instance.UserID)
            {
                ListMessage.Remove(message);
                i--;
            }
        }
    }

    #endregion
}