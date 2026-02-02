using Manager;
using SonatFramework.Systems;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UILeaderboardItemView : MonoBehaviour
{
    [SerializeField] private Image medal;
    [SerializeField] private Image bg;

    [SerializeField] private UIAvatarBase avatar;

    [SerializeField] private TextMeshProUGUI rankTxt;
    [SerializeField] private TextMeshProUGUI username;
    [SerializeField] private TextMeshProUGUI teamName;
    [SerializeField] private TextMeshProUGUI scoreTxt;
    [SerializeField] private TextMeshProUGUI scoreLabel;

    [SerializeField] private TMPMarquee usernameMarquee;
    [SerializeField] private TMPMarquee teamNameMarquee;

    [SerializeField] private RectTransform usernameRect;
    [SerializeField] private RectTransform usernameMask;
    [SerializeField] private RectTransform teamNameRect;
    [SerializeField] private RectTransform teamNameMask;

    private bool isSelf;

    private float usernameMaxWidth;
    private float teamNameMaxWidth;

    private readonly Service<ProfileService> profileService = new();
    private readonly Service<LeaderboardService> leaderboardService = new();
    private readonly Service<TeamService> teamService = new();

    public void Init()
    {
        usernameMaxWidth = usernameMask.sizeDelta.x;
        teamNameMaxWidth = teamNameMask.sizeDelta.x;
    }

    public void Bind(LeaderboardEntryData data, int rank)
    {
        isSelf = false;

        username.text = data.name;
        scoreTxt.text = data.score.ToString();

        //if (ColorUtility.TryParseHtmlString("#8A5E52", out Color color))
        //    scoreLabel.color = color;

        avatar.Init(data.avatar);
        UpdateBadge(avatar.BadgeId);

        UpdateRankVisual(rank);

        CheckUsernameWidth();

        if (string.IsNullOrEmpty(data.teamName))
        {
            teamNameMask.gameObject.SetActive(false);
        }
        else
        {
            teamNameMask.gameObject.SetActive(true);
            teamName.text = data.teamName;
            CheckTeamNameWidth();
        }
    }

    public void BindSelf(int score, int rank)
    {
        isSelf = true;

        username.text = profileService.Instance.Name;
        scoreTxt.text = score.ToString();

        //if (ColorUtility.TryParseHtmlString("#256E3B", out Color color))
        //    scoreLabel.color = color;


        UpdateBadge(profileService.Instance.BadgeId);
        UpdateRankVisual(rank);

        avatar.InitSelf();

        CheckUsernameWidth();

        if (!teamService.Instance.IsJoinedTeam())
        {
            teamNameMask.gameObject.SetActive(false);
        }
        else
        {
            teamNameMask.gameObject.SetActive(true);
            teamName.text = teamService.Instance.CurrentTeamData.name;
            CheckTeamNameWidth();
        }
    }

    private void UpdateRankVisual(int rank)
    {
        rankTxt.text = rank < 1000 ? rank.ToString() : "999+";
        rankTxt.gameObject.SetActive(rank > 3);

        if (rank <= 3)
            medal.sprite = leaderboardService.Instance.config.GetMedalSprite(rank, isSelf);
        //bg.sprite = leaderboardService.Instance.config.GetBgItemSprite(isSelf);
    }

    private void CheckUsernameWidth()
    {
        float width = Mathf.Min(username.preferredWidth, usernameMaxWidth);
        usernameRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, width);

        if (username.preferredWidth > usernameMaxWidth)
            usernameMarquee.StartMarquee();
    }

    private void CheckTeamNameWidth()
    {
        float width = Mathf.Min(teamName.preferredWidth, teamNameMaxWidth);
        teamNameRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, width);

        if (teamName.preferredWidth > teamNameMaxWidth)
            teamNameMarquee.StartMarquee();
    }

    private void UpdateBadge(int badgeId)
    {
        ProfileConfig.Badge badgeData = profileService.Instance.GetBadgeData(badgeId);

        if (badgeId == 0 && isSelf)
        {
            badgeData = profileService.Instance.ProfileConfig.selfBadgeDefault;
        }

        bg.sprite = badgeData.sprite;
        medal.sprite = badgeData.icoMedal;

        username.fontSharedMaterial = badgeData.nameMaterial;
        rankTxt.fontSharedMaterial = badgeData.rankMaterial;
        scoreTxt.fontSharedMaterial = badgeData.rankMaterial;

        username.color = badgeData.colorName;

        teamName.color = badgeData.colorLevel;
        scoreLabel.color = badgeData.colorLevel;

    }
}
