using I2.Loc;
using SonatFramework.Scripts.UIModule;
using SonatFramework.Systems;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UITeamMemberItemView : MonoBehaviour
{
    [SerializeField] private Image medal;
    [SerializeField] private Image bg;

    [SerializeField] private UIAvatarBase avatar;

    [SerializeField] private TextMeshProUGUI rankTxt;
    [SerializeField] private TextMeshProUGUI username;
    [SerializeField] private TextMeshProUGUI scoreTxt;
    [SerializeField] private TextMeshProUGUI scoreLabel;
    [SerializeField] private TextMeshProUGUI roleTxt;
    [SerializeField] private Localize roleLocalize;

    [SerializeField] private TMPMarquee marquee;

    [SerializeField] private RectTransform usernameRect;
    [SerializeField] private RectTransform mask;

    private bool isSelf;

    private Color otherColor;
    private Color selfColor;

    private Member memberData;
    private TeamRole role;
    private string teamId;
    private float usernameMaxWidth;

    private RectTransform rect;

    private readonly Service<ProfileService> profileService = new();
    private readonly Service<LeaderboardService> leaderboardService = new();

    public void Init()
    {
        ColorUtility.TryParseHtmlString("#8A5E52", out otherColor);
        ColorUtility.TryParseHtmlString("#256E3B", out selfColor);

        usernameMaxWidth = mask.sizeDelta.x;

        rect = GetComponent<RectTransform>(); 
    }

    public void Bind(Member data, int rank, TeamRole role, string teamId)
    {
        memberData = data;
        this.teamId = teamId;
        this.role = role;

        isSelf = false;

        username.text = data.name;
        scoreTxt.text = data.level.ToString();

        roleLocalize.SetTerm(TeamService.GetRoleString(role));
        roleTxt.gameObject.SetActive(role == TeamRole.Leader || role == TeamRole.Co_Leader);

        scoreLabel.color = otherColor;
        roleTxt.color = otherColor;

        UpdateRankVisual(rank);

        avatar.Init(data.avatar);

        float width = Mathf.Min(username.preferredWidth, usernameMaxWidth);
        usernameRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, width);

        if (username.preferredWidth > usernameMaxWidth)
            marquee.StartMarquee();
    }

    public void BindSelf(Member data, int rank, TeamRole role, string teamId)
    {
        memberData = data;
        this.teamId = teamId;
        this.role = role;

        isSelf = true;

        roleLocalize.SetTerm(TeamService.GetRoleString(role));
        roleTxt.gameObject.SetActive(role == TeamRole.Leader || role == TeamRole.Co_Leader);

        username.text = profileService.Instance.Name;
        scoreTxt.text = data.level.ToString();

        scoreLabel.color = selfColor;
        roleTxt.color = selfColor;

        UpdateRankVisual(rank);

        avatar.InitSelf();
    }

    private void UpdateRankVisual(int rank)
    {
        rankTxt.text = rank < 1000 ? rank.ToString() : "999+";
        rankTxt.gameObject.SetActive(rank > 3);

        medal.sprite = leaderboardService.Instance.config.GetMedalSprite(rank, isSelf);
        bg.sprite = leaderboardService.Instance.config.GetBgItemSprite(isSelf);
    }

    public void OnClick()
    {
        if (isSelf) return;

        UIData popupData = new();
        popupData.Add("member", memberData);
        popupData.Add("teamId", teamId);
        popupData.Add("role", role);
        popupData.Add("position", rect.TransformPoint(rect.rect.center));
        PanelManager.Instance.OpenForget<PopupTeamMemberAction>(popupData);
    }
}
