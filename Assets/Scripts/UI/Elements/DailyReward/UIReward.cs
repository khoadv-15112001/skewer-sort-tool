using System;
using Sirenix.OdinInspector;
using SonatFramework.Scripts.SonatSDKAdapterModule;
using SonatFramework.Scripts.UIModule;
using SonatFramework.Scripts.UIModule.UIElements;
using SonatFramework.Systems;
using SonatFramework.Systems.InventoryManagement;
using TMPro;
using UnityEngine;
using UnityEngine.U2D;
using UnityEngine.UI;

namespace GrillSort.DailyReward
{
    public class UIReward : MonoBehaviour
    {
        [SerializeField] private int dailyRewardIndex;
        [SerializeField] private DailyRewardConfig dailyRewardConfig;

        [Header("UI")]
        [SerializeField] private UIRewardItem uiRewardItem;

        [SerializeField] private GameObject btnClaim;
        [SerializeField] private GameObject btnNoClaim;
        [SerializeField] private GameObject btnClaimAds;
        [SerializeField] private GameObject btnClaimed;
        [SerializeField] private TMP_Text txtClaimAds;

        private readonly Service<DailyRewardService> _dailyRewardService = new Service<DailyRewardService>();
        private readonly Service<InventoryService> inventoryService = new();
        private RewardConfig _rewardConfig;
        [SerializeField] private PopupDailyReward popupDailyReward;

        public void Start()
        {
            _rewardConfig = dailyRewardConfig.rewards[dailyRewardIndex];
            _dailyRewardService.Instance.onDataChanged += UpdateUI;
            UpdateUI(_dailyRewardService.Instance.GetData());
        }

        private void OnDestroy()
        {
            _dailyRewardService.Instance.onDataChanged -= UpdateUI;
        }

        private void OnValidate()
        {
            if (dailyRewardConfig != null)
            {
                _rewardConfig = dailyRewardConfig.rewards[dailyRewardIndex];
                uiRewardItem.Init(_rewardConfig.reward.resourceDatas[0].resource, _rewardConfig.reward.resourceDatas[0].quantity);
            }
        }

        private void UpdateUI(DailyRewardData data)
        {
            var currentNumAds = data.currentNumAds;

            btnNoClaim.SetActive(false);
            btnClaim.SetActive(false);
            btnClaimAds.SetActive(false);
            btnClaimed.SetActive(false);

            if (dailyRewardIndex == 0) // free
            {
                if (data.canClaimFreeReward)
                {
                    btnClaim.SetActive(true);
                }
                else
                {
                    btnClaimed.SetActive(true);
                }
            }
            else
            {
                if (data.currentRewardIndex == dailyRewardIndex)
                {
                    if (currentNumAds < _rewardConfig.numAds)
                    {
                        btnClaimAds.SetActive(true);
                        txtClaimAds.text = $"{currentNumAds}/{_rewardConfig.numAds}";
                    }
                    else
                    {
                        btnClaim.SetActive(true);
                    }
                }
                else if (dailyRewardIndex < data.currentRewardIndex)
                {
                    btnClaimed.SetActive(true);
                }
                else
                {
                    btnNoClaim.SetActive(true);
                }
            }

        }

        public void OnClickButtonClaim()
        {
            _dailyRewardService.Instance.ClaimReward(dailyRewardIndex);

            var reward = _rewardConfig.reward;
            var logData = new EarnResourceLogData
            {
                spendType = "daily_reward",
                spendId = "daily_reward",
                isFirstBuy = false,
                source = "non_iap"
            };
            inventoryService.Instance.AddReward(reward, logData);

            // uidata
            UIData uiData = new UIData();
            uiData.Add("Title", "REWARD!");
            uiData.Add("Reward", reward);
            uiData.Add("x2", false);
            uiData.Add(UIDataKey.CallBackOnClose, (Action)(popupDailyReward.Close));
            PanelManager.Instance.OpenPanel<PopupReward>(uiData);
        }

        public void OnClickButtonClaimAds()
        {
            MySonatFramework.ShowRewardAds(() =>
            {
                _dailyRewardService.Instance.IncreaseAds();
                
                if (_dailyRewardService.Instance.CanClaimReward(dailyRewardIndex))
                {
                    OnClickButtonClaim();
                }
            }, "daily_reward", "daily_reward");

        }
    }

}
