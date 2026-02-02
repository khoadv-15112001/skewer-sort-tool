
using SonatFramework.Scripts.UIModule;

public class PopupAdsOffer : Panel
{
    public override void Open(UIData uiData)
    {
        base.Open(uiData);
    }

    public void OnClickNoAds()
    {
        PanelManager.Instance.OpenPanelByName<PopupPack>("PopupRemoveAds");
        Close();
    }
}
