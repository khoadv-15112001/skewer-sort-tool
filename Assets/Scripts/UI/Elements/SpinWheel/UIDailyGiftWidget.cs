using Cysharp.Threading.Tasks;
using GrillSort.RealTime;
using Sonat;
using SonatFramework.Scripts.UIModule;
using SonatFramework.Scripts.UIModule.UIElements;
using SonatFramework.Scripts.Utils;
using SonatFramework.Systems;
using UnityEngine;

namespace GrillSort.DailyGift
{
    public class UIDailyGiftWidget : UIHomeWidget
    {
        [Header("UI Can Claim")]
        [SerializeField]
        private GameObject pCanClaim;

        [Header("UI time")][SerializeField] private GameObject pTime;
        [SerializeField] private UITimeCounter timeCounter;

        [SerializeField] private GameObject noti;

        private bool isAppearDailyGift;

        private readonly Service<DailyGiftService7> dailyGiftService7 = new();
        private readonly Service<DailyGiftService30> dailyGiftService30 = new();
        private readonly Service<RealTimeService> realTimeService = new();

        // xuất hiện từ d1 session 2 > 1
        public override void Setup()
        {
            if (dailyGiftService7.Instance.IsUnlocked() && dailyGiftService30.Instance.IsUnlocked())
            {

            }
            else
            {
                if (dailyGiftService7.Instance.CanUnlock() && dailyGiftService30.Instance.CanUnlock())
                {
                    dailyGiftService7.Instance.Unlock();
                    dailyGiftService30.Instance.Unlock();
                }
                else
                {
                    gameObject.SetActive(false);
                    active = false;
                    isAppearDailyGift = false;
                    return;
                }
            }

            UpdateUI();
            dailyGiftService7.Instance.onDataChanged += UpdateUI;
            
            bool canClaim = dailyGiftService7.Instance.CanClaim();
            isAppearDailyGift = canClaim;
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
            dailyGiftService7.Instance.onDataChanged -= UpdateUI;
        }

        public override async UniTask<bool> ProcessTask()
        {
            if (isAppearDailyGift)
            {
                isAppearDailyGift = false;
                // noti.SetActive(true);
                // var popup = PanelManager.Instance.OpenPanel<PopupDailyGift>();
                // MySonatFramework.customTrackingService.OnShowPopup(gameObject.name.ToLogString(), "pop_up", "non_iap", "auto");
                // await UniTask.WaitUntil(() => popup == null || !popup.gameObject.activeInHierarchy);
                // await UniTask.Delay(750);
            }

            return false;
        }

        public void OnClick()
        {
            PanelManager.Instance.OpenPanel<PopupDailyGift>();
            MySonatFramework.customTrackingService.OnShowPopup(gameObject.name.ToLogString(), "widget", "non_iap", "user");
        }

        private void UpdateUI()
        {
            pTime.SetActive(false);
            pCanClaim.SetActive(false);
            if (dailyGiftService7.Instance.CanClaim() || dailyGiftService30.Instance.CanClaim())
            {
                pCanClaim.SetActive(true);
                noti.SetActive(true);
            }
            else
            {
                pTime.SetActive(true);
                int timeRemaining = realTimeService.Instance.GetRemainingTimeInDay();
                timeCounter.SetData(timeRemaining, null);
                noti.SetActive(false);
            }
        }
    }
}