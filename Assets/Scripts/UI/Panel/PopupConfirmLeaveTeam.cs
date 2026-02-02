using Cysharp.Threading.Tasks;
using GrillSort.OnlineService;
using SonatFramework.Scripts.UIModule;
using SonatFramework.Systems;
using System;
using UnityEngine;

public class PopupConfirmLeaveTeam : Panel
{
    private readonly Service<TeamService> teamService = new();
    private readonly Service<OnlineService> onlineService = new();

    public void ClickYes()
    {
        TryLeaveTeam().Forget();
    }

    private async UniTaskVoid TryLeaveTeam()
    {
        bool isProcessing = true;

        UIData data = new();
        data.Add("condition", new Func<bool>(() => !isProcessing));
        PanelManager.Instance.OpenForget<PopupProcessing>(data);

        await UniTask.WaitUntil(() => !teamService.Instance.isUpdatingCache);

        var response = await teamService.Instance.LeaveTeam(new TeamUserModifyRequest()
        {
            userId = onlineService.Instance.UserID,
        });

        isProcessing = false;
        Close();

        if (response.code == ResponseCode.SUCCESS)
        {
            teamService.Instance.LeaveTeam();
        }
        else
        {
            PopupToast.Cretate(response.message);
        }
    }
}
