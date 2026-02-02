using SonatFramework.Scripts.UIModule;
using SonatFramework.Systems;
using SonatFramework.Systems.EventBus;
using UnityEngine;

public class PopupChooseTeamLogo : Panel
{
    [SerializeField] private Transform container;

    private EventBinding<UITeamLogoItem.ChooseTeamLogoEvent> chooseTeamLogoEvent;

    private readonly Service<TeamService> teamService = new();

    public override void Open(UIData uiData)
    {
        base.Open(uiData);

        chooseTeamLogoEvent = new EventBinding<UITeamLogoItem.ChooseTeamLogoEvent>(OnChooseTeamLogo);

        MySonatFramework.poolingContainer.CleanContainer(container);

        for (int i = 1; i <= teamService.Instance.config.numLogo; i++)
        {
            var item = MySonatFramework.poolingContainer.CreateObject<UITeamLogoItem>(container);
            item.Bind(i);
        }
    }

    public override void Close()
    {
        base.Close();
    }

    protected override void OnCloseCompleted()
    {
        base.OnCloseCompleted();

        EventBus<UITeamLogoItem.ChooseTeamLogoEvent>.Deregister(chooseTeamLogoEvent);
    }

    private void OnChooseTeamLogo(UITeamLogoItem.ChooseTeamLogoEvent data)
    {
        Close();
    }
}
