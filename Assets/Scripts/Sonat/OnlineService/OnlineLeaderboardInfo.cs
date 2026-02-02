using Newtonsoft.Json.Converters;
using Newtonsoft.Json;
using System.Collections.Generic;

public class LeaderboardRequestData
{
    public LeaderboardGroup grouping;
    public LeaderboardFilterType filterType;
    public int limit;
}

public class LeaderboardResponseData
{
    public List<LeaderboardEntryData> list;
    public int position;
    public int score;
    public long startDate;
    public long endDate;
}

public class LeaderboardContestRequest
{
    public bool previous;
}

public class LeaderboardTrackingData
{
    public bool success;
    public string message;
}

public class LeaderboardEntryData
{
    public string id;
    public string uid;
    public string name;
    public string geo;
    public string role;
    public string lastLoginAt;
    public int score;
    public string avatar;
    public string teamName;
}

public class LeaderboardTeamResponse
{
    [JsonConverter(typeof(StringEnumConverter))]
    public ResponseCode code;
    public List<LeaderboardTeamData> list;
    public int position;
}

public class LeaderboardTeamData
{
    public string id;
    public string name;
    public string logo;
    public string description;
    public string appId;
    public int memberCount;
    public int capacity;
    public int score;
}

[JsonConverter(typeof(StringEnumConverter))]
public enum LeaderboardGroup
{
    default_group = 0,
    team = 1
}

[JsonConverter(typeof(StringEnumConverter))]
public enum LeaderboardFilterType
{
    all = 0,
    geo
}
