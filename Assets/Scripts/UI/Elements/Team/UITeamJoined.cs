using Cysharp.Threading.Tasks;
using GrillSort.OnlineService;
using GrillSort.RealTime;
using Manager;
using SonatFramework.Scripts.UIModule;
using SonatFramework.Systems;
using SonatFramework.Systems.EventBus;
using System;
using System.Linq;
using System.Threading;
using TMPro;
using Unity.Services.Core;
using UnityEngine;
using UnityEngine.UI;

public class UITeamJoined : MonoBehaviour
{
    [SerializeField] private GameObject content;
    [SerializeField] private GameObject cooldownRequestLivesBtn;
    [SerializeField] private GameObject requestLivesBtn;
    [SerializeField] private GameObject claimLivesBtn;

    [SerializeField] private Image logo;

    [SerializeField] private TextMeshProUGUI teamName;
    [SerializeField] private TextMeshProUGUI cooldownRequestLives;
    [SerializeField] private TextMeshProUGUI pendingLivesTxt;

    private EventBinding<TeamService.UpdateTeamEvent> updateTeamEvent;
    private EventBinding<TeamService.UpdateMessageEvent> updateMessagesEvent;

    private CancellationTokenSource cts;

    private bool isCountdown;
    private bool isFetchingLives;
    private long endCountdown;
    private int pendingLives;

    private readonly Service<TeamService> teamService = new();
    private readonly Service<OnlineService> onlineService = new();
    private readonly Service<HybridDataService> hybridDataService = new();

    private void OnEnable()
    {
        isCountdown = false;

        UpdateUI();
        //UpdateRequestBtn().Forget();

        updateTeamEvent = new EventBinding<TeamService.UpdateTeamEvent>(OnUpdateTeam);
        updateMessagesEvent = new EventBinding<TeamService.UpdateMessageEvent>(OnUpdateMessages);
    }

    private void OnDisable()
    {
        EventBus<TeamService.UpdateTeamEvent>.Deregister(updateTeamEvent);
        EventBus<TeamService.UpdateMessageEvent>.Deregister(updateMessagesEvent);

        if (cts != null)
        {
            cts.Cancel();
            cts.Dispose();
            cts = null;
        }
    }

    private void UpdateUI()
    {
        var teamData = teamService.Instance.CurrentTeamData;
        teamName.text = teamData.name;

        TryLoadLogo(teamData);
    }

    public async UniTaskVoid UpdateRequestBtn()
    {
        if (isFetchingLives) return;
        isFetchingLives = true;

        pendingLives = await hybridDataService.Instance.GetPendingResource(OnlineResourceType.Lives);

        if (pendingLives > 0)
        {
            claimLivesBtn.SetActive(true);
            requestLivesBtn.SetActive(false);
            cooldownRequestLivesBtn.SetActive(false);

            pendingLivesTxt.text = pendingLives.ToString();
        }
        else
        {
            claimLivesBtn.SetActive(false);

            bool canRequestLives = teamService.Instance.CanSendLiveRequest();
            requestLivesBtn.SetActive(canRequestLives);
            cooldownRequestLivesBtn.SetActive(!canRequestLives);

            if (!canRequestLives)
            {
                if (isCountdown)
                {
                    cts.Cancel();
                    cts.Dispose();
                    cts = null;
                }
                
                isCountdown = true;
                endCountdown = TeamService.LastHelpRequestCoolDown + BIGameConfigService.Instance.BIGameConfig.coolDownSecondsBetweenHelpRequest;

                cts = new CancellationTokenSource();
                CountdownRequest(cts.Token).Forget();
            }
        }

        isFetchingLives = false;
    }

    private void OnUpdateTeam(TeamService.UpdateTeamEvent eventData)
    {
        UpdateUI();
    }

    private void OnUpdateMessages(TeamService.UpdateMessageEvent eventData)
    {
        if (eventData.messageData == null) return;

        if (eventData.messageData.type == TeamMessageType.help_request &&
            eventData.messageData.resource == OnlineResourceType.Lives &&
            eventData.messageData.sender.id == onlineService.Instance.UserID)
        {
            UpdateRequestBtn().Forget();
        }
    }

    private void TryLoadLogo(TeamResponse data)
    {
        try
        {
            _ = logo.SetSpriteAsync(PathManager.TeamLogoSprite(int.Parse(data.logo)));
        }
        catch
        {
            _ = teamService.Instance.UpdateTeam(data.id, new()
            {
                logo = "1"
            });

            _ = logo.SetSpriteAsync(PathManager.TeamLogoSprite(1));
        }
    }

    public void ClickViewInfo()
    {
        PanelManager.Instance.OpenForget<PopupTeamInfo>();
    }

    public void ClickRequestLives()
    {
        SendMessageAsync().Forget();
    }

    public void ClickCooldownRequestLives()
    {
        PopupToast.Cretate($"Request is on cooldown. Please wait");
    }

    public void ClaimLives()
    {
        ClaimPendingLives().Forget();
    }

    private async UniTaskVoid SendMessageAsync()
    {
        bool isProcessing = true;

        UIData data = new();
        data.Add("condition", new Func<bool>(() => !isProcessing));
        PanelManager.Instance.OpenForget<PopupProcessing>(data);

        var response = await Service<TeamService>.Get().SendMessage("", TeamMessageType.help_request);

        isProcessing = false;

        if (response.code != ResponseCode.SUCCESS)
            PopupToast.Cretate(response.message);
        else
        {
            var find = teamService.Instance.ListMessage.FindAll(m => m.sender.id == onlineService.Instance.UserID &&
            m.type == TeamMessageType.help_request && m.resource == OnlineResourceType.Lives)
                .OrderByDescending(m => m.created_at).ToList();

            if (find != null && find.Count > 1)
            {
                for (int i = 1; i < find.Count; i++)
                {
                    teamService.Instance.ListMessage.Remove(find[i]);
                }

                EventBus<TeamService.UpdateMessageEvent>.Raise(new TeamService.UpdateMessageEvent()
                {
                    updateType = TeamUpdateMessageType.Delete,
                });
            }
        }

        UpdateRequestBtn().Forget();
    }

    private async UniTask CountdownRequest(CancellationToken ct)
    {
        long time = endCountdown - Service<RealTimeService>.Get().GetCurrentTimeUnix();
        if (time <= 0) return;

        while (!ct.IsCancellationRequested)
        {
            cooldownRequestLives.text = teamService.Instance.GetCountdownRequestStr(time);

            await UniTask.WaitForSeconds(1);

            time--;
            if (time <= 0) break;
        }

        isCountdown = false;

        UpdateRequestBtn().Forget();
    }

    private async UniTaskVoid ClaimPendingLives()
    {
        if (isFetchingLives) return;

        bool isProcessing = true;

        UIData data = new();
        data.Add("condition", new Func<bool>(() => !isProcessing));
        PanelManager.Instance.OpenForget<PopupProcessing>(data);

        var success = await hybridDataService.Instance.ClaimPendingLives();

        isProcessing = false;

        if (success)
            UpdateRequestBtn().Forget();
    }
}
