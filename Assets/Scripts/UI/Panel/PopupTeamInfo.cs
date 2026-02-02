using Cysharp.Threading.Tasks;
using I2.Loc;
using Manager;
using SonatFramework.Scripts.UIModule;
using SonatFramework.Systems;
using SonatFramework.Systems.EventBus;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PopupTeamInfo : Panel
{
    [SerializeField] private GameObject content;
    [SerializeField] private GameObject joinBtn;
    [SerializeField] private GameObject leaveBtn;
    [SerializeField] private GameObject editTeamBtn;

    [SerializeField] private Image logo;

    [SerializeField] private TextMeshProUGUI teamName;
    [SerializeField] private TextMeshProUGUI description;
    [SerializeField] private TextMeshProUGUI capacityTxt;
    [SerializeField] private TextMeshProUGUI teamScoreTxt;
    [SerializeField] private Localize teamTypeTxt;
    [SerializeField] private TextMeshProUGUI requiredLevelTxt;

    [SerializeField] private UITeamMemberScrollView scroll;

    private EventBinding<TeamService.LeaveTeamEvent> leaveTeamEvent;
    private EventBinding<TeamService.UpdateTeamEvent> updateTeamEvent;

    private TeamResponse data;
    private bool isProcessing;

    private readonly Service<TeamService> teamService = new();

    public override void Open(UIData uiData)
    {
        base.Open(uiData);

        leaveTeamEvent = new EventBinding<TeamService.LeaveTeamEvent>(OnLeaveTeam);
        updateTeamEvent = new EventBinding<TeamService.UpdateTeamEvent>(OnUpdateTeam);

        bool isJoined = teamService.Instance.IsJoinedTeam();
        joinBtn.SetActive(!isJoined);
        leaveBtn.SetActive(isJoined);

        TeamRole myRole = teamService.Instance.GetMyRole();
        editTeamBtn.SetActive(myRole == TeamRole.Leader || myRole == TeamRole.Co_Leader);

        InitAsync().Forget();
    }

    protected override void OnCloseCompleted()
    {
        base.OnCloseCompleted();

        EventBus<TeamService.LeaveTeamEvent>.Deregister(leaveTeamEvent);
        EventBus<TeamService.UpdateTeamEvent>.Deregister(updateTeamEvent);
    }

    private async UniTaskVoid InitAsync()
    {
        bool isMyTeam = false;
        if (uiData == null || !uiData.TryGet("teamId", out string teamId))
        {
            teamId = teamService.Instance.CurrentTeamData.id;
            isMyTeam = true;
        }

        await LoadTeamData(teamId, isMyTeam);

        UpdateUI();
    }

    private async UniTask LoadTeamData(string teamId, bool isMyTeam)
    {
        bool isLoading = true;

        UIData popupData = new();
        popupData.Add("condition", new Func<bool>(() => !isLoading));
        PanelManager.Instance.OpenForget<PopupProcessing>(popupData);

        content.SetActive(false);

        data = await teamService.Instance.GetTeamInfo(teamId);

        if (isMyTeam)
            teamService.Instance.UpdateTeam(data);

        content.SetActive(true);
        isLoading = false;
    }

    private void UpdateUI()
    {
        scroll.Init(data);

        _ = logo.SetSpriteAsync(PathManager.TeamLogoSprite(int.Parse(data.logo)));
        teamName.text = data.name;
        description.text = data.description;
        capacityTxt.text = $"{data.GetListMembers().Count}/{data.capacity}";
        teamScoreTxt.text = data.GetTeamScore().ToString();
        teamTypeTxt.SetTerm(data.type == TeamType.closed ? "Closed" : "Open");
        requiredLevelTxt.text = data.requirements.minLevel.ToString();
    }

    public void ForceReload()
    {
        LoadTeamData(teamService.Instance.CurrentTeamData.id, true).Forget();
    }

    public void ClickJoin()
    {
        if (isProcessing) return;
        isProcessing = true;

        TryJoinTeam().Forget();
    }

    public void ClickLeave()
    {
        PanelManager.Instance.OpenForget<PopupConfirmLeaveTeam>();
    }

    public void ClickEdit()
    {
        PanelManager.Instance.OpenForget<PopupEditTeam>();
    }

    private async UniTaskVoid TryJoinTeam()
    {
        await UniTask.WaitUntil(() => !teamService.Instance.isUpdatingCache);

        var response = await teamService.Instance.JoinTeam(data.id);

        if (response.code == ResponseCode.SUCCESS)
        {
            var data = await teamService.Instance.GetCurrentTeamInfo();
            Close();
            teamService.Instance.JoinTeam(data);
        }
        else if (response.code == ResponseCode.TEAM_CLOSED)
        {
            teamService.Instance.RequestJoinTeam(data.id);
            PopupToast.Cretate(response.message);

            EventBus<TeamService.RequestJoinTeamEvent>.Raise(new() { teamId = data.id });

            Close();
        }
        else
        {
            PopupToast.Cretate(response.message);
        }

        isProcessing = false;
    }

    private void OnLeaveTeam(TeamService.LeaveTeamEvent eventData)
    {
        Close();
    }

    private void OnUpdateTeam(TeamService.UpdateTeamEvent eventData)
    {
        data = teamService.Instance.CurrentTeamData;
        UpdateUI();
    }
}
