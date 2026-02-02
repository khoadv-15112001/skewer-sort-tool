using System;
using System.Collections.Generic;
using DG.Tweening;
using Sonat.Enums;
using SonatFramework.Scripts.Helper;
using SonatFramework.Scripts.SonatSDKAdapterModule;
using SonatFramework.Scripts.UIModule;
using SonatFramework.Scripts.UIModule.UIElements;
using SonatFramework.Scripts.Utils;
using SonatFramework.Systems;
using SonatFramework.Systems.InventoryManagement;
using SonatFramework.Systems.InventoryManagement.GameResources;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace GrillSort.LuckySpin
{
    public class UILuckySpin : MonoBehaviour
    {
        [Header("UI")][SerializeField] private UISpinWheel spinWheel;
        [SerializeField] private Image progressBar;
        [SerializeField] private TMP_Text txtSpinCount;
        [SerializeField] private TMP_Text txtSpinCount1;
        [SerializeField] private GameObject btnSpin;
        [SerializeField] private GameObject btnSpinAds;


        private readonly Service<InventoryService> inventoryService = new();
        private readonly Service<LuckySpinService> luckySpinService = new();
        private BlockPanel blockPanel;

        void Start()
        {
            spinWheel.onSpinEnd += OnSpinWheelEnd;
            luckySpinService.Instance.onDataChanged += UpdateUI;
        }

        private void OnEnable()
        {
            // config
            var luckySpinConfig = luckySpinService.Instance.luckySpinConfig;
            spinWheel.SetData(luckySpinConfig.rewards);

            // data
            var data = luckySpinService.Instance.GetData();

            UpdateUI();
            progressBar.fillAmount = GetProgressValue();
        }

        private void OnDisable()
        {
        }

        private void OnDestroy()
        {
            spinWheel.onSpinEnd -= OnSpinWheelEnd;
            luckySpinService.Instance.onDataChanged -= UpdateUI;
        }

        // #if UNITY_EDITOR
        //         [Button("Set Data")]
        //         private void SetData()
        //         {
        //             spinWheel.SetData(spinRewardConfig.rewards);
        //         }
        // #endif


        public void OnSpinAdsClick()
        {
            if(luckySpinService.Instance.Cooldown > 0)
            {
                Debug.Log($"anhnt: cooldown = true");
                PopupToast.Cretate("No more spins! Come back tomorrow!");

                return;
            }    

            if (!CanSpinBecauseOfSpinCount())
            {
                //var panelSpin = PanelManager.Instance.GetPanel<SpinPanel>();
                //panelSpin.ShowLog("No more spins! Come back tomorrow!!!");
                PopupToast.Cretate("No more spins! Come back tomorrow!");
                return;
            }

            // if (SonatSDKAdapter.IsRewardAdsReady())
            // {
            //     luckySpinService.Instance.AddSpin();
            // }
            MySonatFramework.ShowRewardAds(OnSpinClick, "add_spin", "add_spin");
        }

        public void OnSpinClick()
        {
            luckySpinService.Instance.AddSpin();
            StartSpin();
        }

        private bool CanSpinBecauseOfSpinCount()
        {
            var data = luckySpinService.Instance.GetData();
            var luckySpinConfig = luckySpinService.Instance.luckySpinConfig;
            return data.currentSpinCount < luckySpinConfig.maxSpinCount;
        }

        private void StartSpin()
        {
            BlockUI(true);
            // update data

            spinWheel.StartSpin();
        }

        private float GetProgressValue()
        {
            var data = luckySpinService.Instance.GetData();
            if (data.currentSpinCount == 1)
            {
                return 0.328f;
            }
            else if (data.currentSpinCount == 2)
            {
                return 0.665f;
            }
            else if (data.currentSpinCount == 3)
            {
                return 1;
            }

            return 0;
        }

        public void OnSpinWheelEnd(ResourceData r)
        {
            var reward = spinWheel.GetReward();

            var data = luckySpinService.Instance.GetData();
            var luckySpinConfig = luckySpinService.Instance.luckySpinConfig;

            DOVirtual.DelayedCall(0.5f, () =>
            {
                RewardData rewardData = new RewardData() { resourceDatas = new List<ResourceData>() };
                rewardData.resourceDatas.Add(new ResourceData() { resource = reward.resource, quantity = reward.quantity });
                var logData = new EarnResourceLogData
                {
                    spendType = "lucky_spin",
                    spendId = "lucky_spin",
                    isFirstBuy = false,
                    source = "non_iap"
                };
                inventoryService.Instance.AddReward(rewardData, logData);
                UIData uiData = new UIData();
                uiData.Add("Title", "REWARD!");
                uiData.Add("Reward", rewardData);
                uiData.Add("x2", false);


                // kiểm tra nhận phần thưởng tích lũy
                if (data.currentSpinCount == luckySpinConfig.maxSpinCount)
                {
                    var nextUiData = GetAccumulatedRewardUIData();
                    uiData.Add("onClaimComplete", (Action)(() =>
                    {
                        SonatUtils.DelayCall(0.5f, () =>
                        {
                            PanelManager.Instance.OpenPanel<PopupRewardLuckySpin>(nextUiData);
                        });
                    }));
                }

                BlockUI(false);
                PanelManager.Instance.OpenPanel<PopupReward>(uiData);

                MySonatFramework.audioService.PlaySound(AudioId.Spin_Win_prize_Grill_sort);
            });

            // var progressValue = GetProgressValue();
            // progressBar.DOFillAmount(progressValue, 0.3f).SetDelay(0.2f);
        }

        private UIData GetAccumulatedRewardUIData()
        {
            var luckySpinConfig = luckySpinService.Instance.luckySpinConfig;
            RewardData rewardData = luckySpinConfig.accumulatedReward;
            var logData = new EarnResourceLogData
            {
                spendType = "lucky_spin_accumulatedRewards",
                spendId = "lucky_spin_accumulatedRewards",
                isFirstBuy = false,
                source = "non_iap"
            };
            inventoryService.Instance.AddReward(rewardData, logData);
            UIData uiData = new UIData();
            uiData.Add("Title", "SPECIAL GIFT!");
            uiData.Add("Reward", rewardData);
            uiData.Add("x2", true);
            return uiData;
        }

        public bool CheckSpinning()
        {
            return spinWheel.IsSpinning;
        }

        private void Update()
        {
// #if UNITY_EDITOR
//             if (Input.GetKeyDown(KeyCode.Space))
//             {
//                 Debug.Log("ResetSpin");
//                 luckySpinService.Instance.ResetData();
//             }
// #endif
        }

        private void UpdateUI(LuckySpinData d = null)
        {
            var data = luckySpinService.Instance.GetData();
            var luckySpinConfig = luckySpinService.Instance.luckySpinConfig;

            txtSpinCount.SetLocalizeParam("VALUE", $"{data.currentSpinCount}/{luckySpinConfig.maxSpinCount}");
            txtSpinCount1.text = $"{data.currentSpinCount}/{luckySpinConfig.maxSpinCount}";
            var progressValue = GetProgressValue();
            progressBar.DOFillAmount(progressValue, 0.3f).SetDelay(0.2f);

            if (luckySpinService.Instance.CheckFreeSpin())
            {
                btnSpin.SetActive(true);
                btnSpinAds.SetActive(false);
            }
            else
            {
                btnSpin.SetActive(false);
                btnSpinAds.SetActive(true);
            }
        }

        private void BlockUI(bool state)
        {
            if (state)
            {
                if (blockPanel == null)
                {
                    blockPanel = PanelManager.Instance.OpenPanel<BlockPanel>();
                }
            }
            else
            {
                if (blockPanel != null)
                {
                    blockPanel.Close();
                    blockPanel = null;
                }
            }
        }
    }
}