using GrillSort.OnlineService;
using Helper;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Sonat.Enums;
using SonatFramework.Systems.InventoryManagement.GameResources;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;

public class TeamRequest
{
    public string name;
    public string description;
    public string logo;

    [JsonConverter(typeof(StringEnumConverter))]
    public TeamType type;
    public Requirements requirements;
}

public class Requirements
{
    public int minLevel;
}

public class TeamResponse
{
    public string appId;
    public string name;
    public string description;
    public string logo;

    [JsonConverter(typeof(StringEnumConverter))]
    public TeamType type { get; set; }

    public string geo;
    public Requirements requirements;
    public Member leader;
    public List<Member> coLeaders;
    public List<Member> members;
    public DateTime created_at;
    public DateTime updated_at;
    public List<string> bannedUsers;
    public string id;
    public int memberCount;
    public int capacity;


    //---Failed---
    [JsonConverter(typeof(StringEnumConverter))]
    public ResponseCode code = ResponseCode.SUCCESS;
    public string message;
    //---End---

    public int GetTeamScore()
    {
        return GetListMembers().Sum(x => x.level);
    }

    public List<Member> GetListMembers()
    {
        var concatList = new List<Member>();
        if (coLeaders != null)
        {
            concatList.AddRange(coLeaders);
        }

        if (members != null)
        {
            concatList.AddRange(members);
        }

        concatList.Insert(0, leader);
        return concatList;
    }
}

public class Member
{
    public string uid;
    public string name;
    public string id;
    public string avatar;
    public int level;
    public int helpPoint;
}

public class GetListTeamsResponse
{
    [JsonConverter(typeof(StringEnumConverter))]
    public ResponseCode code;
    public PageInfo pageInfo;
    public List<TeamResponse> data;
}

public class PageInfo
{
    public string nextCursor;
    public string previousCursor;
}

[Serializable]
public class GetListTeamsRequest : QueryConvertible
{
    public int? limit = 50;
    [JsonConverter(typeof(StringEnumConverter))]
    public SortOrder? order = SortOrder.asc;
    [JsonConverter(typeof(StringEnumConverter))]
    public TeamType? type;
    public string search;
    public string cursor;
}

public class TeamUserModifyRequest
{
    public string userId;
}

public class TeamUserModifyResponse
{
    public string message;
    [JsonConverter(typeof(StringEnumConverter))]
    public ResponseCode code;
    public string id;
}

public class TeamProcessJoinRequest
{
    [JsonConverter(typeof(StringEnumConverter))]
    public RequestAcceptType status;
}

public class TeamProcessJoinResponse
{
    [JsonConverter(typeof(StringEnumConverter))]
    public ResponseCode code;

    public string message;
}

public class TeamPendingRequestsResponse
{
    [JsonConverter(typeof(StringEnumConverter))]
    public ResponseCode code;
    public List<JoinRequest> Requests;
    public string message;
}

public class JoinRequest
{
    public Member user;
    public string team;
    public string status;
    public DateTime created_at;
    public DateTime updated_at;
    public string id;
}

public class UserPendingRequestResponse
{
    [JsonConverter(typeof(StringEnumConverter))]
    public ResponseCode code;
    public string message;
    public List<PendingRequest> data;
}

public class PendingRequest
{
    public string team;
    public Member user;
    public RequestAcceptType status;
    public DateTime created_at;
    public DateTime updated_at;
    public string id;
}

public class TeamMessagesResponse
{
    [JsonConverter(typeof(StringEnumConverter))]
    public ResponseCode code;
    public PageInfo pageInfo;
    public List<TeamMessageData> data;
}

public class TeamMessageData
{
    [JsonConverter(typeof(StringEnumConverter))]
    public ResponseCode code = ResponseCode.SUCCESS;

    public string message;
    public DateTime? deleted_at;
    public string team;

    [JsonConverter(typeof(StringEnumConverter))]
    public TeamMessageType type;

    public string content;
    public string resource;
    public Member sender;
    public TeamHelpRequest helpRequest;
    public string created_at;
    public string updated_at;
    public string id;

    public TeamRewardInfo rewardsInfo;
}

public class TeamHelpRequest
{
    public RequestAcceptType status;
    public List<TeamHelper> helpers;
}

public class TeamHelper
{
    public DateTime timestamp;
    public string name;
    public string id;
    public string avatar;
}

public class BaseResponse
{
    [JsonConverter(typeof(StringEnumConverter))]
    public ResponseCode code;
    public string message;
}

public class TeamGetMessageRequest : QueryConvertible
{
    public int? limit;

    [JsonConverter(typeof(StringEnumConverter))]
    public TargetEnum? target = TargetEnum.chat;

    public string cursor;
    public DateTime? startUpdatedTime;
    public DateTime? startCreatedTime;
}

public class TeamSendMessageRequest
{
    [JsonConverter(typeof(StringEnumConverter))]
    public TeamMessageType type;
    public string content;
    public string resource;
    public TeamRewardInfo rewardsInfo;
}

public class TeamInfo
{
    public string id;
    public string name;
}

public class TeamRewardInfo
{
    public List<RewardDataJson> rewards;
    public string expiresAt;
    public List<string> receiverIds;
    public string source;
}

public class RewardDataJson
{
    public string resource;
    public int quantity;
    public int time;

    public RewardDataJson(string resource, int quantity, int time)
    {
        this.resource = resource;
        this.quantity = quantity;
        this.time = time;
    }

    public ResourceData ToResourceData()
    {
        if (EnumHelper.TryGetEnumByName(resource, out GameResource gameResource))
        {
            return new ResourceData(gameResource, quantity);
        }
        else
        {
            return null;
        }
    }
}

[JsonConverter(typeof(StringEnumConverter))]
public enum ResponseCode
{
    NOT_FOUND = 0,
    FORBIDDEN = 1,
    SUCCESS = 2,
    TEAM_CLOSED = 3,
    USER_HAS_TEAM = 4,
    TEAM_FULL = 5,
    USER_LOW_LEVEL = 6,
    TEAM_MAX_PENDING_REQUEST = 7,
    EXISTED = 8,
    HELP_REQUEST_COOL_DOWN = 9,
    USER_HELPED = 10,
    HELP_REQUEST_FULL = 11,
    MESSAGE_INVALID_TYPE = 12,
    USER_CANT_HELP_OWN_REQUEST = 13,
    USER_BANNED = 14,
    TEAM_MAX_CO_LEADER = 15,
    USER_DOES_NOT_HAVE_TEAM = 16,
    JOIN_REQUEST_NOT_PENDING = 17,
}

[JsonConverter(typeof(StringEnumConverter))]
public enum TeamType
{
    open = 0,
    closed = 1
}

[JsonConverter(typeof(StringEnumConverter))]
public enum SortOrder
{
    asc = 0,
    desc = 1
}

[JsonConverter(typeof(StringEnumConverter))]
public enum RequestAcceptType
{
    accepted = 0,
    rejected = 1,
    canceled = 2,
    pending = 3,
    completed = 4,
}

public enum TeamRole
{
    None,
    Leader,
    Co_Leader,
    Member
}

[JsonConverter(typeof(StringEnumConverter))]
public enum TargetEnum
{
    chat = 0,
    manage = 1,
    activity = 2
}

[JsonConverter(typeof(StringEnumConverter))]
public enum TeamMessageResourceType
{
    lives = 0,
    coin = 1
}

[JsonConverter(typeof(StringEnumConverter))]
public enum TeamMessageType
{
    text = 0,
    help_request = 1,
    join_request_pending = 2,

    [EnumMember(Value = "team.join.success")]
    join_success = 3,

    [EnumMember(Value = "team.member.removed")]
    member_removed = 4,

    [EnumMember(Value = "team.co-leader.demote")]
    co_leader_demote = 5,

    [EnumMember(Value = "team.co-leader.promote")]
    co_leader_promote = 6,

    [EnumMember(Value = "team.leave.success")]
    leave_success = 7,

    team_reward = 8
}

public enum TeamUpdateMessageType
{
    Update,
    Insert,
    Delete
}

