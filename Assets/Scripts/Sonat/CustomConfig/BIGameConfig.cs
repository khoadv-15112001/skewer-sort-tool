using UnityEngine;

public class BIGameConfig
{
    public int maxPendingJoinRequest;
    public int maxTeamMembers;
    public int maxTeamCoLeaders;
    public int maxInactivityDaysOfLeader;
    public bool allowRejoinTeam = false;

    public int maxItemsPerHelpRequestQuantity;
    public int expireSecondsOfHelpRequest;
    public int coolDownSecondsBetweenHelpRequest;
    public bool enableHelperResourceDeduction;
    public bool hideOldHelpRequestAfterCoolDownReset;

    public BIRequestResourceConfig helpRequestConfigByResource;
}

public class BIRequestConfig
{
    public int maxItemsPerHelpRequestQuantity;
    public int expireSecondsOfHelpRequest;
    public int coolDownSecondsBetweenHelpRequest;
    public bool enableHelperResourceDeduction;
    public bool hideOldHelpRequestAfterCoolDownReset;

    public BIRequestConfig(int maxItemsPerHelpRequestQuantity, int expireSecondsOfHelpRequest, int coolDownSecondsBetweenHelpRequest, bool enableHelperResourceDeduction, bool hideOldHelpRequestAfterCoolDownReset)
    {
        this.maxItemsPerHelpRequestQuantity = maxItemsPerHelpRequestQuantity;
        this.expireSecondsOfHelpRequest = expireSecondsOfHelpRequest;
        this.coolDownSecondsBetweenHelpRequest = coolDownSecondsBetweenHelpRequest;
        this.enableHelperResourceDeduction = enableHelperResourceDeduction;
        this.hideOldHelpRequestAfterCoolDownReset = hideOldHelpRequestAfterCoolDownReset;
    }
}

public class BIRequestResourceConfig
{
    public BIRequestConfig lives;
    public BIRequestConfig card;
}
