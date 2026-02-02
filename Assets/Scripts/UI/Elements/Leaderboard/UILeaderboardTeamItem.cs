using Manager;
using SonatFramework.Systems;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UILeaderboardTeamItem : MonoBehaviour
{
    [SerializeField] private Image logo;
    [SerializeField] private Image medal;

    [SerializeField] private TextMeshProUGUI capacityTxt;
    [SerializeField] private TextMeshProUGUI teamName;
    [SerializeField] private TextMeshProUGUI rankTxt;
    [SerializeField] private TextMeshProUGUI scoreTxt;

    [SerializeField] private TMPMarquee marquee;

    [SerializeField] private RectTransform teamNameRect;
    [SerializeField] private RectTransform mask;

    private float teamNameMaxWidth;

    private readonly Service<LeaderboardService> leaderboardService = new();

    public void Init()
    {
        teamNameMaxWidth = mask.sizeDelta.x;
    }

    public void Bind(LeaderboardTeamData data, int rank)
    {
        teamName.text = data.name;
        capacityTxt.text = $"{data.memberCount}/{data.capacity}";
        scoreTxt.text = data.score.ToString();

        _ = logo.SetSpriteAsync(PathManager.TeamLogoSprite(int.Parse(data.logo)));

        UpdateRankVisual(rank);

        float width = Mathf.Min(teamName.preferredWidth, teamNameMaxWidth);
        teamNameRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, width);

        if (teamName.preferredWidth > teamNameMaxWidth)
            marquee.StartMarquee();
    }

    private void UpdateRankVisual(int rank)
    {
        rankTxt.text = rank.ToString();
        rankTxt.gameObject.SetActive(rank > 3);

        medal.sprite = leaderboardService.Instance.config.GetMedalSprite(rank, false);
    }
}
