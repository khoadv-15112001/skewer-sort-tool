using Cysharp.Threading.Tasks;
using DG.Tweening;
using GrillSort.ConsecutiveWin;
using GrillSort.RealTime;
using Manager;
using Sonat.Data;
using SonatFramework.Scripts.SonatSDKAdapterModule;
using SonatFramework.Scripts.UIModule;
using SonatFramework.Scripts.UIModule.UIElements;
using SonatFramework.Scripts.Utils;
using SonatFramework.Systems;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace GrillSort.LuckySpin
{
    public class UISpinWidget : UIHomeWidget
    {
        [Header("UI progress")] [SerializeField]
        private GameObject pProgress;

        [SerializeField] private Image progressBar;
        [SerializeField] private TMP_Text txtProgress;

        [Header("UI time")] [SerializeField] private GameObject pTime;
        [SerializeField] private UITimeCounter timeCounter;

        private readonly Service<LuckySpinService> luckySpinService = new();
        private readonly Service<RealTimeService> realTimeService = new();
        private bool isAppearLuckySpin = false;

        private bool forceAppearLuckySpin = false;

        // xuất hiện từ d1 session 2 > 1 và đến unlock level
        public override void Setup()
        {
            // int currentDay = MySonatFramework.userDataService.UserDay;
            // int session = MySonatFramework.userDataService.SessionToday;
            // int level = MySonatFramework.userDataService.GetLevel();

            active = CheckScreenRatio() && SonatSDKAdapter.GetValueBySegment($"show_lucky_spin", UserData.UserCampaignSegment.Value, true);
            if (!active)
            {
                gameObject.SetActive(false);
                return;
            }

            if (luckySpinService.Instance.IsUnlocked())
            {
                isAppearLuckySpin = true;
            }
            else
            {
                if (luckySpinService.Instance.CanUnlock())
                {
                    luckySpinService.Instance.Unlock();
                    isAppearLuckySpin = true;
                }
                else
                {
                    gameObject.SetActive(false);
                    isAppearLuckySpin = false;
                    return;
                }
            }

            UpdateProgress();
            luckySpinService.Instance.onDataChanged += UpdateProgress;

            // win ở trận liên tiếp mở popup nếu vẫn còn lượt quay
            forceAppearLuckySpin = MySonatFramework.GetService<ConsecutiveWinService>().GetConsecutiveWins() == 3
                                   && luckySpinService.Instance.IsUnlocked()
                                   && luckySpinService.Instance.GetData().currentSpinCount < luckySpinService.Instance.luckySpinConfig.maxSpinCount;
        }

        private bool CheckScreenRatio()
        {
            return !(Screen.width * 1.0f / Screen.height > 9f / 16f);
        }

        public override void OnFocus()
        {
            UpdateProgress();
        }

        public override void OnLoseFocus()
        {
        }

        private void OnDestroy()
        {
            luckySpinService.Instance.onDataChanged -= UpdateProgress;
        }

        public void OnSpinClick()
        {
            PanelManager.Instance.OpenPanel<SpinPanel>();
            MySonatFramework.customTrackingService.OnShowPopup(gameObject.name.ToLogString(), "widget", "non_iap", "user");
        }

        public override async UniTask<bool> ProcessTask()
        {
            return false;
            // win 3 level liên tiếp mở lucky spin
            if (!forceAppearLuckySpin && (!isAppearLuckySpin || PlayerPrefs.HasKey("AppearLuckySpin"))) return false;

            PlayerPrefs.SetInt("AppearLuckySpin", 1);
            var popup = PanelManager.Instance.OpenPanel<SpinPanel>();
            MySonatFramework.customTrackingService.OnShowPopup(gameObject.name.ToLogString(), "pop_up", "non_iap", "auto");
            await UniTask.WaitUntil(() => popup == null || !popup.gameObject.activeInHierarchy);
            await UniTask.Delay(750);
            return true;
        }

        private void UpdateProgress(LuckySpinData d = null)
        {
            var data = luckySpinService.Instance.GetData();
            var luckySpinConfig = luckySpinService.Instance.luckySpinConfig;
            var progressValue = (float)data.currentSpinCount / luckySpinConfig.maxSpinCount;
            progressBar.DOFillAmount(progressValue, 0.3f);

            pTime.SetActive(false);
            pProgress.SetActive(false);
            if (data.currentSpinCount < luckySpinConfig.maxSpinCount)
            {
                pProgress.SetActive(true);
                txtProgress.text = $"{data.currentSpinCount}/{luckySpinConfig.maxSpinCount}";
            }
            else
            {
                pTime.SetActive(true);
                int timeRemaining = realTimeService.Instance.GetRemainingTimeInDay();
                timeCounter.SetData(timeRemaining, null);
            }
        }
    }
}