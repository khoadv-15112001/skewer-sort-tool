using Cysharp.Threading.Tasks;
using GrillSort.RealTime;
using SonatFramework.Scripts.UIModule;
using SonatFramework.Scripts.UIModule.UIElements;
using SonatFramework.Scripts.Utils;
using SonatFramework.Systems;
using UnityEngine;

namespace GrillSort.DailyReward
{
    public class UIDailyRewardWidget : UIHomeWidget
    {
        [Header("UI Can Claim")]
        [SerializeField]
        private GameObject pCanClaim;

        private static bool isAppearDailyReward = true;

        private readonly Service<DailyRewardService> dailyRewardService = new();
        private readonly Service<RealTimeService> realTimeService = new();

        // xuất hiện từ d1 session 2 > 1
        public override void Setup()
        {
            if (!dailyRewardService.Instance.active)
            {
                gameObject.SetActive(false);
                isAppearDailyReward = false;
                return;
            }
            int currentDay = MySonatFramework.userDataService.UserDay;
            int session = MySonatFramework.userDataService.SessionToday;

            if (currentDay == 0 && session == 1)
            {
                gameObject.SetActive(false);
                isAppearDailyReward = false;
                return;
            }

            UpdateUI();
            dailyRewardService.Instance.onDataChanged += UpdateUI;
            bool canClaim = dailyRewardService.Instance.CanClaimReward();

            if (currentDay == 0)
            {
                isAppearDailyReward = session > 1 && canClaim;
            }
            else
            {
                isAppearDailyReward = canClaim;
            }
        }

        public override void OnFocus()
        {
            UpdateUI();
        }

        public override void OnLoseFocus()
        {
        }

        private void OnDestroy()
        {
            dailyRewardService.Instance.onDataChanged -= UpdateUI;
        }

        public override async UniTask<bool> ProcessTask()
        {
            if (isAppearDailyReward)
            {
                isAppearDailyReward = false;
                var popup = PanelManager.Instance.OpenPanel<PopupDailyReward>();
                MySonatFramework.customTrackingService.OnShowPopup(gameObject.name.ToLogString(), "pop_up", "non_iap", "auto");
                await UniTask.WaitUntil(() => popup == null || !popup.gameObject.activeInHierarchy);
                await UniTask.Delay(750);
                return true;
            }
            return false;
        }

        public void OnClick()
        {
            PanelManager.Instance.OpenPanel<PopupDailyReward>();
            MySonatFramework.customTrackingService.OnShowPopup(gameObject.name.ToLogString(), "widget", "non_iap", "user");
        }

        private void UpdateUI(DailyRewardData data = null)
        {
            pCanClaim.SetActive(false);
            if (dailyRewardService.Instance.CanClaimReward())
            {
                pCanClaim.SetActive(true);
            }
        }
    }
}