using SonatFramework.Systems.TimeManagement;
using SonatFramework.Systems;
using SonatFramework.Scripts.UIModule.UIElements;
using SonatFramework.Scripts.Feature.Shop;

public class UIStarterPackSaleWidget : PackIapWidget
{
    private readonly Service<ShopService> shopService = new();

    public UITimeCounter timeCounter;
    private long currentTime;
    private long exp;

    public override void Setup()
    {
        base.Setup();

        exp = MySonatFramework.userDataService.FirstTimeOpen + 48 * 3600;

        gameObject.SetActive(CheckActive());

        if (gameObject.activeSelf)
        {
            timeCounter.SetData(exp - currentTime, () =>
            {
                gameObject.SetActive(false);
            });
        }
    }

    public override void OnFocus()
    {
        base.OnFocus();
        gameObject.SetActive(CheckActive());
    }
    public override void OnLoseFocus()
    {
        base.OnLoseFocus();
        gameObject.SetActive(CheckActive());
    }
    protected override bool CheckActive()
    {
        currentTime = SonatSystem.GetService<TimeService>().GetUnixTimeSeconds();

        if ( exp <= currentTime) return false;

        active = shopService.Instance.VerifyPack(shopItemKey) && 
            shopService.Instance.VerifyPack(Sonat.Enums.ShopItemKey.StarterBundle);
        
        if (!active)
            return false;

        return true;
    }
}