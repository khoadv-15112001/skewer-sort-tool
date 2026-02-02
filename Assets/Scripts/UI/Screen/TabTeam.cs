using Cysharp.Threading.Tasks;
using SonatFramework.Systems;
using SonatFramework.Systems.EventBus;
using System.Threading;
using UnityEngine;

public class TabTeam : UITabBase
{
    [SerializeField] private GameObject notJoinObj;
    [SerializeField] private GameObject joinedObj;
    [SerializeField] private GameObject lockObj;
    [SerializeField] private GameObject loadingObj;

    private GameObject currentObj;
    private GameObject currentPrefab;

    private CancellationTokenSource cts;

    private EventBinding<TeamService.JoinTeamEvent> createTeamEvent;
    private EventBinding<TeamService.LeaveTeamEvent> leaveTeamEvent;

    private readonly Service<TeamService> teamService = new();

    public override void OnShow()
    {
        base.OnShow();

        if (!isActive) return;

        if (BIGameConfigService.Instance.BIGameConfig == null)
        {
            PopupToast.Cretate("Loading failed! Please retry later.");
            return;
        }

        RefreshUI();

        createTeamEvent = new EventBinding<TeamService.JoinTeamEvent>(OnCreateTeam);
        leaveTeamEvent = new EventBinding<TeamService.LeaveTeamEvent>(OnLeaveTeam);
    }

    public override void OnHide()
    {
        base.OnHide();

        if (isActive) return;

        if (cts != null)
        {
            cts.Cancel();
            cts.Dispose();
            cts = null;
        }

        EventBus<TeamService.JoinTeamEvent>.Deregister(createTeamEvent);
        EventBus<TeamService.LeaveTeamEvent>.Deregister(leaveTeamEvent);
    }

    private void OnCreateTeam(TeamService.JoinTeamEvent eventData)
    {
        RefreshUI();
    }

    private void OnLeaveTeam(TeamService.LeaveTeamEvent eventData)
    {
        RefreshUI();
    }

    private void RefreshUI()
    {
        cts = new();
        LoadUIAsync(cts.Token).Forget();
    }

    private async UniTaskVoid LoadUIAsync(CancellationToken ct)
    {
        loadingObj.SetActive(true);
        await UniTask.WaitUntil(() => teamService.Instance.IsTeamInfoLoaded(), cancellationToken: ct);
        loadingObj.SetActive(false);

        bool isUnlocked = teamService.Instance.IsUnlockFeature();

        if (!isUnlocked)
        {
            if (currentPrefab == lockObj) return;

            if (currentObj != null)
                Destroy(currentObj);

            currentObj = Instantiate(lockObj, transform);
            currentPrefab = lockObj;
        }
        else
        {
            if (teamService.Instance.IsJoinedTeam())
            {
                if (currentPrefab != joinedObj)
                {
                    Destroy(currentObj);

                    currentObj = Instantiate(joinedObj, transform);
                    currentPrefab = joinedObj;
                }
            }
            else
            {
                if (currentPrefab != notJoinObj)
                {
                    Destroy(currentObj);

                    currentObj = Instantiate(notJoinObj, transform);
                    currentPrefab = notJoinObj;
                }
            }
        }
    }
}
