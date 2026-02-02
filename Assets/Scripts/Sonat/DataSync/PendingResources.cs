using GrillSort.OnlineService;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;

public class PendingResourceResponse
{
    public DateTime? deleted_at;

    public string key; // resource

    [JsonConverter(typeof(StringEnumConverter))]
    public VariableEventType eventType;

    public int amountChange;

    [JsonConverter(typeof(StringEnumConverter))]
    public ResourceStatus status;

    public AdditionalInfo additionalInfo;

    public string user;
    public string id;
}

[JsonConverter(typeof(StringEnumConverter))]
public enum VariableEventType
{
    increase,
    decrease
}

[JsonConverter(typeof(StringEnumConverter))]
public enum ResourceStatus
{
    pending,
    done
}

public class AdditionalInfo
{
    [JsonConverter(typeof(StringEnumConverter))]
    public ResourceSourceType type;
    public UserProfile receiver;
    public UserProfile sender;
    public DateTime created_at;
    public DateTime updated_at;
}

[JsonConverter(typeof(StringEnumConverter))]
public enum ResourceSourceType
{
    help_request = 0,
    send_p2p = 1,
}

public class UpdatePendingResourceRequest
{
    public List<string> ids;
}

public class UpdatePendingResourceResponse
{
    public string message;
    public int updatedCount;
}

public class SendResourceRequest
{
    public string userId;
    public string resource;
    public int amount;
}

public class SendResourceResponse
{
    [JsonConverter(typeof(StringEnumConverter))]
    public ResponseCode code;
    public string message;
}

public static class OnlineResourceType
{
    public const string Lives = "lives";
    public const string Coin = "coin";
    public const string Card = "card";

    // Helper method to format card resource with specific card ID
    // Example: "card_Card_0_0"
    public static string FormatCardResource(string cardId)
    {
        return $"{Card}_{cardId}";
    }

    // Helper method to parse card resource
    // Returns card ID if it's a card resource, -1 otherwise
    public static int ParseCardId(string resource)
    {
        if (string.IsNullOrEmpty(resource)) return -1;

        var parts = resource.Split('_');
        if (parts.Length == 2 && parts[0] == Card)
        {
            if (int.TryParse(parts[1], out int result))
                return result;
            else return -1;
        }

        return -1;
    }
}