
using Cysharp.Threading.Tasks;
using DG.Tweening;
using SonatFramework.Scripts.Helper;
using SonatFramework.Scripts.UIModule;
using SonatFramework.Scripts.Utils;
using SonatFramework.Systems;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace GrillSort.PiggyBank
{
    public class UIPiggyWidget : UIHomeWidget
    {
        [Header("UI progress")]
        [SerializeField] private GameObject pProgress;

        [SerializeField] private Image progressBar;
        [SerializeField] private TMP_Text txtProgress;
        [SerializeField] private bool showPopup;

        private readonly Service<PiggyBankService> _piggyBankService = new();

        public override void Setup()
        {
            if (_piggyBankService.Instance.IsUnlocked() == false && _piggyBankService.Instance.CanUnlock() == false)
            {
                gameObject.SetActive(false);
                active = false;
                return;
            }

            _piggyBankService.Instance.Unlock();
            UpdateProgress();
            _piggyBankService.Instance.OnDataChanged += UpdateProgress;
        }

        private void OnDestroy()
        {
            _piggyBankService.Instance.OnDataChanged -= UpdateProgress;
        }

        public void OnClickPiggyBank()
        {
            PanelManager.Instance.OpenPanel<PopupPiggyBank>();
            MySonatFramework.customTrackingService.OnShowPopup(gameObject.name.ToLogString(), "widget", "non_iap", "user");
        }

        public override async UniTask<bool> ProcessTask()
        {
            if (!showPopup) return false;
            // Hiện khi đầy mốc và ưu tiên
            var tier = new IntDataPref("PIGGY_BANK_show_popup_condition_tier", -1);
            if (active && (_piggyBankService.Instance.PiggyBankData.piggyRewardIdx != tier.Value || PlayerPrefs.GetInt("PopupPiggyBank_Showed", 0) == 0))
            {
                PlayerPrefs.SetInt("PopupPiggyBank_Showed", 1);
                tier.Value = _piggyBankService.Instance.PiggyBankData.piggyRewardIdx;
                var popup = PanelManager.Instance.OpenPanel<PopupPiggyBank>();
                MySonatFramework.customTrackingService.OnShowPopup(gameObject.name.ToLogString(), "pop_up", "non_iap", "auto");
                await UniTask.WaitUntil(() => popup == null || !popup.gameObject.activeInHierarchy);
                await UniTask.Delay(750);
                return true;
            }
            return false;
        }
        private void UpdateProgress()
        {
            var data = _piggyBankService.Instance.PiggyBankData;
            var point = _piggyBankService.Instance.Point;
            var maxPoint = _piggyBankService.Instance.PiggyBankConfig.GetPiggyTierConfig(data.piggyTier).GetMaxPoint();

            var progressValue = (float)point / maxPoint;
            progressBar.DOFillAmount(progressValue, 0.3f);
            txtProgress.text = $"{point}/{maxPoint}";
        }
    }
}