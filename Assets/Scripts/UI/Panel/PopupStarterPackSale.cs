using SonatFramework.Scripts.UIModule.UIElements;
using SonatFramework.Systems.TimeManagement;
using SonatFramework.Systems;
using Sonat.AppsFlyerModule;
using SonatFramework.Systems.UserData;

public class PopupStarterPackSale : PopupPack
{
    public UITimeCounter timeCounter;
    public int exprireInHours = 48;
    public override void OnSetup()
    {
        base.OnSetup();
        var currentTime = SonatSystem.GetService<TimeService>().GetUnixTimeSeconds();

        var exp = MySonatFramework.userDataService.FirstTimeOpen + exprireInHours * 3600;

        if (exp <= currentTime)
        {
            gameObject.SetActive(false);
            return;
        }

        timeCounter.SetData(exp - currentTime, () => { Close(); });
    }
}
