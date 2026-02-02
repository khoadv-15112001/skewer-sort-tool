using SonatFramework.Scripts.UIModule;
using SonatFramework.Systems;
using UnityEngine;

public class UITeamOfferWidget : UIHomeWidget
{
    private readonly Service<TeamOfferService> teamOfferService = new();

    public override void Setup()
    {
        base.Setup();

        if (!teamOfferService.Instance.IsActive())
        {
            gameObject.SetActive(false);
            return;
        }

        TeamOfferService.OnFeatureEnd += Deactive;
    }

    private void OnDestroy()
    {
        TeamOfferService.OnFeatureEnd -= Deactive;
    }

    private void Deactive()
    {
        gameObject.SetActive(false);
    }

    public void OnClick()
    {
        PanelManager.Instance.OpenForget<PopupTeamBonusOffer>();
    }
}
