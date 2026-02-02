using I2.Loc;
using SonatFramework.Systems.AudioManagement;
using SonatFramework.Systems.EventBus;
using SonatFramework.Systems.InventoryManagement.GameResources;
using SonatFramework.Systems.InventoryManagement;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using SonatFramework.Scripts.UIModule;

namespace GrillSort.DailyMission
{
    public class UIXLStage : MonoBehaviour
    {
        [SerializeField] private Button btn;
        [SerializeField] private GameObject claimingGO;
        [SerializeField] private GameObject claimedGO;

        [SerializeField] private Localize describeTxt;

        [SerializeField] private GameObject chest;
        [SerializeField] private UIBubbleRewardSmart bubbleReward;
        [SerializeField] private Slider progressSlider;
        [SerializeField] private Image progressImg;
        [SerializeField] private TMP_Text progressTxt;

        [SerializeField] private Sprite progressGreenSprite;
        [SerializeField] private Sprite progressPinkSprite;

        private RewardData _rewardData;

        private int _target;

        private int curItem => DailyMissionService.Instance.numItem.Value;

        public void Setup()
        {
            _target = DailyMissionService.Instance.GetCurrentDailyMission().GetNumItemXLStage();

            SetupState();

            describeTxt.SetTerm(DailyMissionService.Instance.GetCurrentDailyMission().DescribeTerm);

            SetupReward();
        }

        private void SetupState()
        {
            Set(curItem >= _target ? EState.Claiming : EState.Progress);
        }

        private void Set(EState state)
        {
            switch (state)
            {
                case EState.Progress:
                    SetProgress();
                    break;

                case EState.Claiming:
                    SetClaiming();
                    break;

                case EState.Claimed:
                    SetClaimed();
                    break;

                default:
                    SetProgress();
                    break;
            }
        }

        private void SetProgress()
        {
            claimingGO.SetActive(false);
            claimedGO.SetActive(false);

            chest.gameObject.SetActive(true);

            progressImg.gameObject.SetActive(true);

            progressImg.sprite = progressPinkSprite;

            progressSlider.value = curItem * 1f / _target;
            progressTxt.text = $"{curItem}/{_target}";

            btn.onClick.RemoveAllListeners();
            btn.onClick.AddListener(() =>
            {
                MySonatFramework.GetService<AudioService>().PlaySound(Sonat.Enums.AudioId.ButtonClick);
                PopupToast.Cretate("Complete your task to earn rewards");
            });
        }

        private void SetClaiming()
        {
            claimingGO.SetActive(true);
            claimedGO.SetActive(false);

            chest.gameObject.SetActive(true);

            progressImg.gameObject.SetActive(true);

            progressImg.sprite = progressGreenSprite;

            progressSlider.value = curItem * 1f / _target;
            progressTxt.text = $"{curItem}/{_target}";

            btn.onClick.RemoveAllListeners();
            btn.onClick.AddListener(Claim);
        }

        private void SetClaimed()
        {
            claimingGO.SetActive(false);
            claimedGO.SetActive(true);

            chest.gameObject.SetActive(false);

            progressImg.gameObject.SetActive(false);
        }

        private void SetupReward()
        {
            _rewardData = DailyMissionService.Instance.GetRewardXLStage();

            bubbleReward.SetReward(_rewardData);
            bubbleReward.Select();
        }

        private void Claim()
        {
            MySonatFramework.GetService<AudioService>().PlaySound(Sonat.Enums.AudioId.ButtonClick);
            btn.onClick.RemoveAllListeners();

            MySonatFramework.GetService<InventoryService>().AddReward(_rewardData, new EarnResourceLogData
            {
                source = "non_iap",
                spendType = "feature",
                spendId = "dm"
            });

            UIData uiData = new UIData();
            uiData.Add("Reward", _rewardData);
            PanelManager.Instance.OpenPanel<PopupReward>(uiData);

            DailyMissionService.Instance.ClaimXLStage();

            DailyMissionService.Instance.CompleteMission();

            SetClaimed();
        }

        public enum EState
        {
            Progress,
            Claiming,
            Claimed
        }
    }
}