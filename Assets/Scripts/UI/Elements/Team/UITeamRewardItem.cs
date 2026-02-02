using Cysharp.Threading.Tasks;
using GrillSort.OnlineService;
using Helper;
using I2.Loc;
using SonatFramework.Scripts.SonatSDKAdapterModule;
using SonatFramework.Scripts.UIModule;
using SonatFramework.Scripts.UIModule.UIElements;
using SonatFramework.Scripts.Utils;
using SonatFramework.Systems;
using SonatFramework.Systems.InventoryManagement;
using SonatFramework.Systems.InventoryManagement.GameResources;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UITeamRewardItem : MonoBehaviour
{
    [SerializeField] private UIAvatarBase avatar;

    [SerializeField] private TextMeshProUGUI username;
    [SerializeField] private TextMeshProUGUI contentTxt;
    [SerializeField] private TextMeshProUGUI expiredTime;

    [SerializeField] private Image giftIcon;
    [SerializeField] private Image bgImg;
    [SerializeField] private Image innerBgImg;

    [SerializeField] private UIRewardItem uiReward;

    [SerializeField] private LocalizationParamsManager localizedSourceParam;

    [SerializeField] private GameObject claimBtn;
    [SerializeField] private GameObject loading;

    [SerializeField] private RectTransform usernameRect;

    [SerializeField] private TMPMarquee marquee;

    [SerializeField] private float usernameMaxWidth = 357f;

    private string messageId;
    private string senderId;
    private string source;
    private long timeEnd;
    private RewardData rewards;

    private readonly Service<TeamOfferService> teamOfferService = new();
    private readonly Service<TeamService> teamService = new();
    private readonly Service<OnlineService> onlineService = new();

    private void OnEnable()
    {
        TickService.Register(Tick, TickService.TickPhase.Tick);
    }

    private void OnDisable()
    {
        TickService.UnRegister(Tick, TickService.TickPhase.Tick);
    }

    public void Init()
    {

    }

    public void Bind(TeamMessageData message)
    {
        messageId = message.id;
        senderId = message.sender.id;
        username.text = message.sender.name;
        source = message.rewardsInfo.source;
        timeEnd = TimeHelper.GetUnixTimeFromISO(message.rewardsInfo.expiresAt);

        avatar.Init(message.sender.avatar);

        contentTxt.GetComponent<Localize>().SetTerm(message.content);

        var offerData = teamOfferService.Instance.GetOfferData(SonatSDKAdapter.ConvertToProductID(source));

        contentTxt.GetComponent<TextMeshProUGUI>().color = offerData.contentColor;
        localizedSourceParam.SetParameterValue("VALUE", offerData.name);

        rewards = offerData.teamRewards;
        giftIcon.sprite = offerData.giftIcon;
        bgImg.sprite = offerData.bgChat;
        innerBgImg.sprite = offerData.innerBgChat;

        uiReward.Init(rewards.resourceDatas[0].resource, rewards.resourceDatas[0].quantity);

        float width = Mathf.Min(username.preferredWidth, usernameMaxWidth);

        usernameRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, width);
        contentTxt.margin = new Vector4(width + 15f, contentTxt.margin.y, contentTxt.margin.z, contentTxt.margin.w);

        if (username.preferredWidth > usernameMaxWidth)
            marquee.StartMarquee();

        claimBtn.SetActive(true);
        loading.SetActive(false);

        Tick();
    }

    private void Tick()
    {
        var now = teamOfferService.Instance.Config.TimeUnix();
        var remainTime = timeEnd - now;

        if (remainTime <= 0)
            teamService.Instance.DeleteMessage(messageId);
        else
            expiredTime.text = SonatUtils.GetTimeByFormat(remainTime);
    }

    public void ClickClaim()
    {
        claimBtn.SetActive(false);
        loading.SetActive(true);

        TryClaimReward().Forget();
    }

    public void ClickViewProfile()
    {
        UIData data = new();
        data.Add("userId", senderId);
        PanelManager.Instance.OpenForget<PopupUserProfile>(data);
    }

    private async UniTask TryClaimReward()
    {
        try
        {
            var response = await onlineService.Instance.Post<BaseResponse>
                ($"teams/{teamService.Instance.CurrentTeamData.id}/messages/{messageId}/claim", null);

            if (response.code == ResponseCode.SUCCESS)
            {
                var logData = new EarnResourceLogData
                {
                    spendType = "team_reward",
                    spendId = source,
                    isFirstBuy = false,
                    source = "non_iap"
                };
                MySonatFramework.GetService<InventoryService>().AddReward(rewards, logData);

                UIData data = new();
                data.Add("source", "team_offer");
                data.Add("Reward", rewards);
                PanelManager.Instance.OpenForget<PopupReward>(data);

                teamService.Instance.DeleteMessage(messageId);
            }
            else
            {
                PopupToast.Cretate("Failed!");
                claimBtn.SetActive(true);
                loading.SetActive(false);
            }
        }
        catch
        {
            PopupToast.Cretate("Failed!");
            claimBtn.SetActive(true);
            loading.SetActive(false);
        }
    }
}
