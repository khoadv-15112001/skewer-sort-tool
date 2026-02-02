using SonatFramework.Systems;
using TMPro;
using UnityEngine;

namespace SonatFramework.Scripts.Feature.DailyReward.UI
{
    public class DailyRewardItem : MonoBehaviour
    {
        [SerializeField] private TMP_Text dayText;
        [SerializeField] private GameObject lockLOjb, collectedObj;

        private readonly Service<DailyRewardManager> dailyRewardManager = new();
        private bool canClaim;
        private bool claimed;

        private DailyReward dailyReward;


        public virtual void SetData(DailyReward dailyReward, int currDay)
        {
            this.dailyReward = dailyReward;
            dayText.text = dailyReward.day.ToString();
            canClaim = !dailyRewardManager.Instance.CheckDayClaimed(dailyReward.day);
        }

        public virtual void ClickClaim()
        {
            if (!canClaim) return;
            dailyRewardManager.Instance.ClaimReward(dailyReward.day);
            UpdateUI();
            canClaim = false;
        }

        protected virtual void UpdateUI()
        {
        }
    }
}