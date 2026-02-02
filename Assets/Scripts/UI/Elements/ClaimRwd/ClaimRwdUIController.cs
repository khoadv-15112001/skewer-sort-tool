using UnityEngine;
using UnityEngine.UI;
using TMPro;
using GrillSort.RealTime;
using System;
using SonatFramework.Scripts.UIModule;
using System.Linq;

namespace GrillSort.ClaimRrdService
{
    public class ClaimRwdUIController : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private Button claimButton;
        [SerializeField] private Button countdownButton;
        [SerializeField] private TMP_Text levelText;
        [SerializeField] private TMP_Text cooldownText;
        [SerializeField] private TMP_Text rewardText;
        //[SerializeField] private RectTransform rectTransform;
        [SerializeField] private GameObject bannerObj;

        //private readonly float height = 192f;
        private ClaimRwdService _claimRwdService;
        private RealTimeService _realTimeService;
        private void Awake()
        {
            _claimRwdService = MySonatFramework.GetService<ClaimRwdService>();
            _realTimeService = MySonatFramework.GetService<RealTimeService>();

            if (claimButton != null)
                claimButton.onClick.AddListener(OnClaimButtonClicked);

            if (countdownButton != null)
                countdownButton.onClick.AddListener(OnCountdownButtonClicked);

            _realTimeService.OnNextDay += RefreshUI;
        }

        private void OnEnable()
        {
            TickService.Register(UpdateCooldownUI, TickService.TickPhase.TickRealtime);
            RefreshUI();
        }

        private void OnDisable()
        {
            TickService.UnRegister(UpdateCooldownUI, TickService.TickPhase.TickRealtime);
        }

        private void OnDestroy()
        {
            _realTimeService.OnNextDay -= RefreshUI;

            if (claimButton != null)
                claimButton.onClick.RemoveListener(OnClaimButtonClicked);
            if (countdownButton != null)
                countdownButton.onClick.RemoveListener(OnCountdownButtonClicked);
        }

        private void RefreshUI()
        {
            if (_claimRwdService == null || _realTimeService == null)
                return;
       
            if (!_claimRwdService.CanUnlock() || _claimRwdService.CurrentLevel == _claimRwdService.Config.datas.Count + 1)
            {
                bannerObj.SetActive(false);
               // rectTransform.sizeDelta = new Vector2(rectTransform.sizeDelta.x, 0);
                return;
            }

            if (!bannerObj.activeSelf)
            {
                bannerObj.SetActive(true);
                //rectTransform.sizeDelta = new Vector2(rectTransform.sizeDelta.x, height);
            }

            UpdateRewardText(_claimRwdService.CurrentLevel);
            UpdateCooldownUI();
        }

        private void UpdateCooldownUI()
        {
            if (_claimRwdService == null || _realTimeService == null)
                return;

            if (_claimRwdService.CanClaim())
            {
                claimButton.gameObject.SetActive(true);
                countdownButton.gameObject.SetActive(false);
                claimButton.interactable = true;
            }
            else
            {
                claimButton.gameObject.SetActive(false);
                countdownButton.gameObject.SetActive(true);

                long now = _realTimeService.GetCurrentTimeUnix();
                long remain = (long)Mathf.Max(0, _claimRwdService.NextTime - now);
                cooldownText.text = FormatTime(remain);
            }
        }

        private async void OnClaimButtonClicked()
        {
            claimButton.interactable = false;

            var (success, reward) = await _claimRwdService.ClaimAsync();
            if (!success || reward == null)
            {
                claimButton.interactable = true;
                return;
            }
            if (_claimRwdService.CurrentLevel == _claimRwdService.Config.datas.Count + 1)
            {
                bannerObj.SetActive(false);

                return;
            }
            UpdateRewardText(_claimRwdService.CurrentLevel);

            claimButton.gameObject.SetActive(false);
            countdownButton.gameObject.SetActive(true);
            UpdateCooldownUI();
        }


        private void UpdateRewardText(int currentLevel)
        {
            var config = _claimRwdService.Config;
            var data = config.datas.Find(d => d.level == currentLevel);
            if (data != null && data.rewardData != null && data.rewardData.resourceDatas.Count > 0)
            {
                var r = data.rewardData.resourceDatas[0];
                rewardText.text = $"{r.quantity}";
            }
            else
            {
                rewardText.text = $"{config.datas.Last().rewardData.resourceDatas[0].quantity}";
            }
            if (PanelManager.Instance.GetPanel<CheatPanel>() != null)
                levelText.text = $"Level {_claimRwdService.CurrentLevel}";
            else
                levelText.text = "";
        }
        private void OnCountdownButtonClicked()
        {
            PopupToast.Cretate("Comeback after", cooldownText.text);
        }

        private string FormatTime(long seconds)
        {
            TimeSpan t = TimeSpan.FromSeconds(seconds);
            if (t.Hours > 0)
                return $"{t.Hours:D2}:{t.Minutes:D2}:{t.Seconds:D2}";
            return $"{t.Minutes:D2}:{t.Seconds:D2}";
        }
    }
}
