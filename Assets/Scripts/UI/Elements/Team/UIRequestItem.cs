using Cysharp.Threading.Tasks;
using GrillSort.OnlineService;
using I2.Loc;
using Manager;
using SonatFramework.Scripts.UIModule;
using SonatFramework.Systems;
using SonatFramework.Systems.EventBus;
using System;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIRequestItem : MonoBehaviour
{
    [SerializeField] private UIAvatarBase avatar;

    [SerializeField] private TextMeshProUGUI username;
    [SerializeField] private TextMeshProUGUI progressTxt;
    [SerializeField] private TextMeshProUGUI contentTxt;

    [SerializeField] private Localize contentLocalize;

    [SerializeField] private GameObject requestLives;
    [SerializeField] private GameObject requestJoin;
    [SerializeField] private UIHelpButton helpBtn;

    [SerializeField] private Slider progress;

    [SerializeField] private RectTransform usernameRect;

    [SerializeField] private TMPMarquee marquee;

    [SerializeField] private float usernameMaxWidth = 357f;

    [SerializeField] private bool isSelf;

    private bool canClick;
    private bool canSendLives;

    private TeamMessageData data;
    private string requestId;

    private readonly Service<ProfileService> profileService = new();
    private readonly Service<TeamService> teamService = new();
    private readonly Service<OnlineService> onlineService = new();

    public void Init()
    {
        canClick = true;
        progress.maxValue = BIGameConfigService.Instance.BIGameConfig.maxItemsPerHelpRequestQuantity;
    }

    public void Bind(TeamMessageData data, string requestId)
    {
        this.requestId = requestId;
        this.data = data;

        username.text = data.sender.name;

        if (data.type == TeamMessageType.join_request_pending)
            contentLocalize.SetTerm("want to join the team!");
        else if (data.type == TeamMessageType.help_request)
            contentLocalize.SetTerm("asking for help!");

        requestLives.SetActive(data.type == TeamMessageType.help_request);

        if (requestJoin != null)
            requestJoin.SetActive(data.type == TeamMessageType.join_request_pending);

        if (isSelf)
        {
            avatar.InitSelf();

            canSendLives = false;
        }
        else
        {
            avatar.Init(data.sender.avatar);

            if (data.type == TeamMessageType.help_request)
                canSendLives = !data.helpRequest.helpers.Any(h => h.id == onlineService.Instance.UserID);
            else
                canSendLives = false;
        }

        float width = Mathf.Min(username.preferredWidth, usernameMaxWidth);

        usernameRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, width);
        contentTxt.margin = new Vector4(width + 15f, contentTxt.margin.y, contentTxt.margin.z, contentTxt.margin.w);

        if (username.preferredWidth > usernameMaxWidth)
            marquee.StartMarquee();

        if (data.type == TeamMessageType.help_request)
        {
            progress.value = data.helpRequest == null ? 0 : (data.helpRequest.helpers == null ? 0 : data.helpRequest.helpers.Count);
            progressTxt.text = $"{progress.value}/{progress.maxValue}";
        }

        if (helpBtn != null)
            helpBtn.Bind(canSendLives, requestId);
    }

    public void ClickRejectJoin()
    {
        if (!canClick) return;
        canClick = false;

        TryHandleJoinRequest(RequestAcceptType.rejected).Forget();
    }

    public void ClickAcceptJoin()
    {
        if (!canClick) return;
        canClick = false;

        TryHandleJoinRequest(RequestAcceptType.accepted).Forget();
    }

    public void ClickViewProfile()
    {
        UIData data = new();
        data.Add("userId", this.data.sender.id);
        PanelManager.Instance.OpenForget<PopupUserProfile>(data);
    }  

    private async UniTaskVoid TryHandleJoinRequest(RequestAcceptType requestType)
    {
        TeamProcessJoinRequest request = new() { status = requestType };
        var response = await teamService.Instance.ProcessJoinRequest(request, teamService.Instance.CurrentTeamData.id, requestId);

        if (response.code != ResponseCode.SUCCESS)
        {
            PopupToast.Cretate(response.message);

            if (response.code == ResponseCode.JOIN_REQUEST_NOT_PENDING)
            {
                teamService.Instance.DeleteMessage(requestId);
            }
        }
        else
        {
            for (int i = 0; i < teamService.Instance.ListMessage.Count; i++)
            {
                var messages = teamService.Instance.ListMessage[i];
                if (messages.id == requestId)
                {
                    teamService.Instance.ListMessage.RemoveAt(i);

                    EventBus<TeamService.UpdateMessageEvent>.Raise(new TeamService.UpdateMessageEvent()
                    {
                        updateType = TeamUpdateMessageType.Delete,
                        messageData = messages
                    });

                    break;
                }
            }
        }

        canClick = true;
    }
}
