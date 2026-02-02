using DG.Tweening;
using MyGame.UI.ScrollView;
using Sirenix.OdinInspector;
using SkewerJam.Features.Leaderboard.Service;
using Sonat.Enums;
using SonatFramework.Scripts.UIModule.UIElements;
using SonatFramework.Systems;
using SonatFramework.Systems.InventoryManagement.GameResources;
using UnityEngine;
using UnityEngine.UI;

namespace MyGame.Leaderboard.UI
{
    public class LeaderboardItemView : ItemViewBase
    {
        [Header("Background Score")]
        [SerializeField] private Image bgScore;

        [Space(10)]
        [Header("UI Bubble Reward")]
        [SerializeField] private bool isShowBubbleReward = false;
        [SerializeField, ShowIf("isShowBubbleReward")] private FixedImageRatio imgChest;
        [SerializeField, ShowIf("isShowBubbleReward")] private UIBubbleReward uiBubbleReward;

        private Service<LeaderboardHLWService> leaderboardHLWService = new();
        protected override void UpdateRankVisual(int rank)
        {
            base.UpdateRankVisual(rank);

            if (bgScore != null)
                bgScore.sprite = leaderboardConfigSO.GetBgScoreSprite(isSelf);

            if (isShowBubbleReward)
            {
                if (rank >= 1 && rank <= 3)
                {
                    imgChest.gameObject.SetActive(true);
                    imgChest.SetSprite(leaderboardConfigSO.GetChestSprite(rank));

                    var reward = leaderboardHLWService.Instance.config.GetRewardTop(rank);
                    uiBubbleReward.SetReward(reward);
                    userName.SetMaxWidth(300);
                }
                else
                {
                    imgChest.gameObject.SetActive(false);
                    userName.SetMaxWidth(9999);
                }
            }
        }

        public void UpdateScore(int newScore)
        {
            scoreTxt.DOText(newScore.ToString(), 0.75f).SetEase(Ease.OutSine);
        }
    }
}