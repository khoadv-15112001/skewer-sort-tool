using Cysharp.Threading.Tasks;
using I2.Loc;
using SonatFramework.Scripts.UIModule;
using SonatFramework.Systems;
using System;
using UnityEngine;

public class PopupConfirmKickOut : Panel
{
    [SerializeField] private LocalizationParamsManager description;

    private Member member;

    private readonly Service<TeamService> teamService = new();

    public override void Open(UIData uiData)
    {
        base.Open(uiData);

        member = uiData.Get<Member>("member");

        description.SetParameterValue("VALUE", $"<color=#C50045>{member.name}</color>");
    }

    public void ClickYes()
    {
        TryKickMember().Forget();
    }

    private async UniTaskVoid TryKickMember()
    {
        bool isProcessing = true;

        UIData data = new();
        data.Add("condition", new Func<bool>(() => !isProcessing));
        PanelManager.Instance.OpenForget<PopupProcessing>(data);

        var response = await teamService.Instance.RemoveUser(teamService.Instance.CurrentTeamData.id, new TeamUserModifyRequest()
        {
            userId = member.id,
        });

        isProcessing = false;
        Close();

        if (response.code != ResponseCode.SUCCESS)
            PopupToast.Cretate(response.message);
        else
            PanelManager.Instance.GetPanel<PopupTeamInfo>()?.ForceReload();
    }
}
