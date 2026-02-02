using MyGame.Modules.CardCollection;
using SonatFramework.Scripts.UIModule;
using UnityEngine;

public class PopupSendCard : Panel
{
    [SerializeField] private GameObject notJoinObj;

    [SerializeField] private UITeamMemberSendCard scroll;

    public override void Open(UIData uiData)
    {
        base.Open(uiData);

        if (MySonatFramework.GetService<TeamService>().IsJoinedTeam())
        {
            notJoinObj.SetActive(false);
            scroll.gameObject.SetActive(true);
            scroll.Init(uiData.Get<CardType>("cardType"));
        }
        else
        {
            notJoinObj.SetActive(true);
            scroll.gameObject.SetActive(false);
        }
    }
}
