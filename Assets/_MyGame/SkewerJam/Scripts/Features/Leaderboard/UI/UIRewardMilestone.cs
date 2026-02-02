using SkewerJam.Features.Leaderboard.Service;
using SonatFramework.Systems;
using SonatFramework.Systems.InventoryManagement.GameResources;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace SkewerJam.Features.Leaderboard
{
    public class UIRewardMilestone : MonoBehaviour
    {
        [SerializeField] private int index;
        [SerializeField] private TMP_Text txtPumpkin;
        [SerializeField] private Image tickComplete;
        [SerializeField] private UIBubbleReward uiBubbleReward;

        private Service<LeaderboardHLWService> leaderboardService = new();

        private void OnEnable()
        {
            var config = leaderboardService.Instance.config;
            var data = config.rewardMilestone[index];

            SetData(data.pumpkin, data.reward);

            var isComplete = leaderboardService.Instance.CheckCompleteMilestone(index);
            SetComplete(isComplete);

        }

        private void SetData(int pumpkin, RewardData reward)
        {
            txtPumpkin.text = pumpkin.ToString();
            uiBubbleReward.SetReward(reward);
        }

        public void SetComplete(bool isComplete)
        {
            txtPumpkin.gameObject.SetActive(!isComplete);
            tickComplete.gameObject.SetActive(isComplete);
        }
    }
}