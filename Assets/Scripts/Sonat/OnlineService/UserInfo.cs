using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;

namespace GrillSort.OnlineService
{
    public class UserRequestData
    {
        public string name;
        public string geo;
        public string avatar;
        public List<Device> devices;
        public GeneralStats generalStats;
    }

    public class UserInfo
    {
        public string id;
        public string uid;
        public string name;
        public string avatar;
        public int level;
        public string created_at;
        public string updated_at;
        public string deleted_at;
        public string role;
        public string team;
        public string lastLoginAt;
        public DateTime? helpRequestCoolDown;
        public List<Device> devices;
        public AuthProvider authProvider;
        public string geo;
        public string appId;
        public GeneralStats generalStats;
    }

    public class Device
    {
        public string fb_analytics_instance_id;
        public string fb_instance_id;
        public string fcmToken;
    }

    public class AuthProvider
    {
        public string provider;
        public string id;
    }

    public class UserGameDataResponse
    {
        public Dictionary<string, JToken> data;
    }

    public class UserDataUpdateItem
    {
        public string path;
        public object value;
    }

    public class UserGameDataUpdateRequest
    {
        public List<UserDataUpdateItem> data;
    }

    public struct FetchUserResult
    {
        public bool Success;
        public int? StatusCode;
    }

    public class UserProfile
    {
        public string id;
        public string name;
        public string avatar;
        public int level;
        public TeamInfo team;
        public string created_at;
        public GeneralStats generalStats;
    }

    public class UserProfileResponse
    {
        [JsonConverter(typeof(StringEnumConverter))]
        public ResponseCode code;
        public UserProfile data;
    }

    public class GeneralStats
    {
        public int firstTryWin;
        public int longestStreak;
        public int treasureQuestWins;
        public int collectionsCompleted;
        public int setsCompleted;
    }
}
