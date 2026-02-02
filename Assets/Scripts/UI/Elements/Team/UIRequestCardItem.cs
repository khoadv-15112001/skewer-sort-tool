using Cysharp.Threading.Tasks;
using GrillSort.RealTime;
using Helper;
using I2.Loc;
using MyGame.Modules.CardCollection;
using SonatFramework.Scripts.UIModule;
using SonatFramework.Scripts.UIModule.UIElements;
using SonatFramework.Scripts.Utils;
using SonatFramework.Systems;
using TMPro;
using UnityEngine;

public class UIRequestCardItem : MonoBehaviour
{
    [SerializeField] private UIAvatarBase avatar;

    [SerializeField] private UICard card;

    [SerializeField] private TextMeshProUGUI username;
    [SerializeField] private TextMeshProUGUI contentTxt;
    [SerializeField] private TextMeshProUGUI expiredTime;
    [SerializeField] private TextMeshProUGUI limitTxt;

    [SerializeField] private GameObject sendBtn;
    [SerializeField] private GameObject limitSendBtn;
    [SerializeField] private GameObject loading;

    [SerializeField] private RectTransform usernameRect;

    [SerializeField] private TMPMarquee marquee;

    [SerializeField] private float usernameMaxWidth = 357f;

    [SerializeField] private bool isSelf;

    private TeamMessageData data;
    private CardType cardType;
    private string requestId;

    private readonly Service<CardCollectionService> cardService = new();
    private readonly Service<TeamService> teamService = new();
    private readonly Service<RealTimeService> timeService = new();

    private void OnEnable()
    {
        TickService.Register(OnTick, TickService.TickPhase.TickRealtime);
    }

    private void OnDisable()
    {
        TickService.UnRegister(OnTick, TickService.TickPhase.TickRealtime);
    }

    public void Init()
    {
        
    }

    public void Bind(TeamMessageData data, string requestId, CardType cardType)
    {
        this.requestId = requestId;
        this.data = data;
        this.cardType = cardType;

        card.Setup(cardType);
        card.SetData(1, false);

        username.text = data.sender.name;

        if (isSelf)
        {
            avatar.InitSelf();
        }
        else
        {
            loading.SetActive(false);

            avatar.Init(data.sender.avatar);

            int sentCards = cardService.Instance.GetSentCardToday();
            int limitSend = cardService.Instance.GetSendCardDailyLimit();
            limitTxt.text = LocalizationManager.GetTranslation("Daily Limit:") + $"{limitSend - sentCards}/{limitSend}";

            limitSendBtn.SetActive(sentCards >= limitSend);
            sendBtn.SetActive(sentCards < limitSend);
        }

        float width = Mathf.Min(username.preferredWidth, usernameMaxWidth);

        usernameRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, width);
        contentTxt.margin = new Vector4(width + 15f, contentTxt.margin.y, contentTxt.margin.z, contentTxt.margin.w);

        if (username.preferredWidth > usernameMaxWidth)
            marquee.StartMarquee();

        OnTick();
    }

    private void OnTick()
    {
        if (expiredTime != null)
        {
            var expireDuration = BIGameConfigService.Instance.GetResourceRequestConfig(OnlineResourceType.Card).expireSecondsOfHelpRequest;
            var now = timeService.Instance.GetCurrentTimeUnix();
            var timeEnd = TimeHelper.GetUtcTimeFromISO(data.created_at).AddSeconds(expireDuration).ToUnixTimeSeconds();
            if (timeEnd - now > 0)
                expiredTime.text = SonatUtils.FormatTimeSmartFull(timeEnd - now);
            else
                teamService.Instance.DeleteMessage(requestId);
        }
    }

    public void OnClickSend()
    {
        if (cardService.Instance.GetDuplicatedNumCard(cardType) > 0)
        {
            TrySendCard().Forget();
        }
        else
        {
            PopupToast.Cretate("You need a duplicate to send this card!");
        }
    }

    public void OnClickLimitSend()
    {
        PopupToast.Cretate("Reach limit today!");
    }

    public void OnClickViewProfile()
    {
        UIData data = new();
        data.Add("userId", this.data.sender.id);
        PanelManager.Instance.OpenForget<PopupUserProfile>(data);
    }

    private async UniTask TrySendCard()
    {
        try
        {
            loading.SetActive(true);
            sendBtn.SetActive(false);

            var response = await teamService.Instance.SendHelpRequest(data.team, requestId);

            loading.SetActive(false);

            if (response.code == ResponseCode.SUCCESS)
            {
                PopupToast.Cretate("Card sent!");

                cardService.Instance.HandleSendCardSuccess(cardType);

                teamService.Instance.DeleteMessage(requestId);
            }
            else
            {
                if (response.code == ResponseCode.HELP_REQUEST_FULL)
                {
                    PopupToast.Cretate("This request has already been handled");
                    teamService.Instance.DeleteMessage(requestId);
                }
                else
                {
                    PopupToast.Cretate(response.message);
                    sendBtn.SetActive(true);
                }
            }
        }
        catch
        {
            loading.SetActive(false);
            sendBtn.SetActive(true);

            PopupToast.Cretate($"Failed!");
        }
    }
}
