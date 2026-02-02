using SonatFramework.Scripts.UIModule;
using UnityEngine;

public class PopupTeamBonusOffer : Panel
{
    public override void Open(UIData uiData)
    {
        base.Open(uiData);

        TeamOfferService.OnFeatureEnd += OnFeatureEnd;
    }

    protected override void OnCloseCompleted()
    {
        base.OnCloseCompleted();

        TeamOfferService.OnFeatureEnd -= OnFeatureEnd;
    }

    private void OnFeatureEnd()
    {
        gameObject.SetActive(false);
    }
}
