using System.Collections.Generic;
using System.Linq;

namespace GrillSort.OnlineService
{
    public enum MessageType
    {
        Unknown = 0,
        New = 1,
        Update = 2,
        TeamJoinSuccess = 3,
        TeamJoinPending = 4,
        TeamJoinAccepted = 5,
        TeamJoinRejected = 6,
        TeamLeaveSuccess = 7,
        TeamCoLeaderPromote = 8,
        TeamCoLeaderDemote = 9,
        TeamMemberRemoved = 10,
    }

    public class OnlineMessage
    {
        public static readonly Dictionary<MessageType, string> FBMessageType = new Dictionary<MessageType, string>
    {
        { MessageType.New,   "message.new" },
        { MessageType.Update,  "message.update" },
        { MessageType.TeamJoinSuccess, "team.join.success" },
        { MessageType.TeamJoinPending, "team.join.pending" },
        { MessageType.TeamJoinAccepted, "team.join.accepted" },
        { MessageType.TeamJoinRejected, "team.join.rejected" },
        { MessageType.TeamLeaveSuccess, "team.leave.success" },
        { MessageType.TeamCoLeaderPromote, "team.co-leader.promote" },
        { MessageType.TeamCoLeaderDemote, "team.co-leader.demote" },
        { MessageType.TeamMemberRemoved, "team.member.removed" },
    };

        private static readonly Dictionary<string, MessageType> Reverse =
            FBMessageType.ToDictionary(kvp => kvp.Value, kvp => kvp.Key);

        public static string ToStringValue(MessageType type)
        {
            return FBMessageType.TryGetValue(type, out var str) ? str : "Unknown";
        }

        public static MessageType FromString(string value)
        {
            return Reverse.TryGetValue(value, out var type) ? type : MessageType.Unknown;
        }
    }
}
