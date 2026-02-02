using Manager;
using MyGame.Leaderboard.SO;
using SonatFramework.Systems;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static ProfileConfig;

namespace MyGame.UI.ScrollView
{
    public class ItemViewBase : MonoBehaviour
    {
        [SerializeField] private Image medal;
        [SerializeField] private Image avatar;
        [SerializeField] private Image frame;
        [SerializeField] private Image bg;

        [SerializeField] private TextMeshProUGUI rankTxt;
        //[SerializeField] private TextMeshProUGUI username;
        [SerializeField] protected TextMeshProUGUI scoreTxt;
        [SerializeField] private TextMeshProUGUI scoreLabel;

        [SerializeField] protected TMPMarquee userName;

        [SerializeField] private bool autoSetScoreColor = true;

        [SerializeField] private bool isSetFollowProfile;

        [Space(10)]
        [Header("Config SO")]
        [SerializeField]
        protected LeaderboardConfigSO leaderboardConfigSO;

        protected bool isSelf;

        private readonly Service<ProfileService> profileService = new();

        public virtual void Bind(LeaderboardEntryData data, int rank)
        {
            isSelf = false;

            //if (autoSetScoreColor && ColorUtility.TryParseHtmlString("#8A5E52", out Color color))
            //    scoreLabel.color = color;
            userName.SetText(data.name);
            scoreTxt.text = data.score.ToString();

            TryParseAvatar(data.avatar);
            UpdateRankVisual(rank);
        }

        public void BindSelf(int score, int rank)
        {
            isSelf = true;
            //if (autoSetScoreColor && ColorUtility.TryParseHtmlString("#256E3B", out Color color))
            //    scoreLabel.color = color;

            UpdateBadge(profileService.Instance.BadgeId);
            UpdateRankVisual(rank);
            userName.SetText(profileService.Instance.Name);
            scoreTxt.text = score.ToString();

            _ = avatar.SetSpriteAsync(PathManager.AvatarSprite(profileService.Instance.AvatarId));
            _ = frame.SetSpriteAsync(PathManager.FrameSprite(profileService.Instance.FrameId));
        }

        protected virtual void UpdateRankVisual(int rank)
        {
            rank = rank <= 0 ? 1000 : rank;
            if (rank >= 1000)
            {
                rankTxt.text = "999+";
            }
            else if (rank > 100)
            {
                rankTxt.text = $"{rank / 100 * 100}+";
            }
            // else if (rank > 20)
            // {
            //     rankTxt.text = $"{rank / 10 * 10}+";
            // }
            else
            {
                rankTxt.text = rank.ToString();
            }

            rankTxt.gameObject.SetActive(rank > 3);

            if (medal != null)
            {
                if (rank <= 3)
                    medal.sprite = leaderboardConfigSO.GetMedalSprite(rank, isSelf);
            }
            //if (bg != null)
            //    bg.sprite = leaderboardConfigSO.GetBgItemSprite(isSelf);
        }

        private void UpdateBadge(int badgeId)
        {
            ProfileConfig.Badge badgeData = profileService.Instance.GetBadgeData(badgeId);

            if (isSetFollowProfile)
            {
                if (badgeId == 0 && isSelf)
                {
                    badgeData = profileService.Instance.ProfileConfig.selfBadgeDefault;
                }
            }
            else
            {
                if (isSelf)
                {
                    badgeData = profileService.Instance.ProfileConfig.selfBadgeDefault;
                }
                else
                {
                    badgeData = profileService.Instance.GetBadgeData(0);
                }
            }

            if (isSetFollowProfile)
            {
                bg.sprite = badgeData.sprite;
            }
            else
            {
                bg.sprite = leaderboardConfigSO.GetBgItemSprite(isSelf);
            }

            medal.sprite = badgeData.icoMedal;

            userName.GetComponent<TMP_Text>().fontSharedMaterial = badgeData.nameMaterial;
            rankTxt.fontSharedMaterial = badgeData.rankMaterial;
            scoreTxt.fontSharedMaterial = badgeData.rankMaterial;

            userName.GetComponent<TMP_Text>().color = badgeData.colorName;

            //teamName.color = badgeData.colorLevel;
            //scoreLabel.color = badgeData.colorLevel;

        }

        private void TryParseAvatar(string s)
        {
            int avatarId = 0;
            int frameId = 0;
            int badgeId = 0;

            if (IsAvatarFormatAscii(s))
            {
                var list = s.Split(',');

                avatarId = int.Parse(list[0]);
                frameId = int.Parse(list[1]);

                if (list.Length >= 3)
                {
                    int.TryParse(list[2], out badgeId);
                }
            }

            _ = avatar.SetSpriteAsync(PathManager.AvatarSprite(avatarId));
            _ = frame.SetSpriteAsync(PathManager.FrameSprite(frameId));

            UpdateBadge(badgeId);
        }

        private bool IsAvatarFormatAscii(string s)
        {
            if (string.IsNullOrEmpty(s)) return false;

            int comma = s.IndexOf(',');
            if (comma <= 0 || comma == s.Length - 1) return false;

            ReadOnlySpan<char> a = s.AsSpan(0, comma);
            ReadOnlySpan<char> b = s.AsSpan(comma + 1);

            return IsDigitsAscii(a) && IsDigitsAscii(b);
        }

        private bool IsDigitsAscii(ReadOnlySpan<char> span)
        {
            if (span.Length == 0) return false;
            foreach (var ch in span)
                if (ch < '0' || ch > '9')
                    return false;
            return true;
        }
    }
}