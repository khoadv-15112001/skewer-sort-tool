using Sonat.Data;
using SonatFramework.Scripts.SonatSDKAdapterModule;

public class RwdGlobalHelper
{
    public static bool RwdGlobalEnable => SonatSDKAdapter.GetRemoteBool($"camp_{UserData.UserCampaignSegment.Value}_rwd_global_enable", true);
}
