using Cysharp.Threading.Tasks;
using SonatFramework.Scripts.UIModule;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace GrillSort.KitchenMission
{
    public class PopupKitchenMission : Panel
    {
        [SerializeField] private Button tutBtn;
        [SerializeField] private Button closeBtn;

        [SerializeField] private UIPlayerController playerController;
        [SerializeField] private UIVisualController visualController;

        [SerializeField] private TMP_Text timeTxt;

        public float delayAction = 2.5f;

        [SerializeField] private AudioClip winAudio;
        [SerializeField] private ParticleSystem winEff;

        public static Action OnClose;
        public static Action OnClaimReward;
        public static bool IsShowReward;

        public override void OnSetup()
        {
            base.OnSetup();

            SetupButton();
            playerController.Setup();
            visualController.Setup();

            visualController.ShowRewards(IsShowReward);

            UpdateTime();

            OnClaimReward += ClaimRewards;
            KitchenMissionService.OnTick += UpdateTime;
        }

        private void OnDestroy()
        {
            OnClaimReward -= ClaimRewards;
            KitchenMissionService.OnTick -= UpdateTime;

            IsShowReward = false;
        }

        private void UpdateTime()
        {
            timeTxt.text = KitchenMissionService.Instance.GetStringTimeCountdown();
        }

        public override void Open(UIData uiData)
        {
            base.Open(uiData);

            TryAction();
        }

        private void SetupButton()
        {
            tutBtn.onClick.AddListener(OnClickTut);

            if (KitchenMissionService.Instance.IsUserWin())
            {
                closeBtn.onClick.AddListener(ClaimRewards);
            }
            else
            {
                closeBtn.onClick.AddListener(OnClickClose);
            }

        }

        private void OnClickTut()
        {
            PanelManager.Instance.OpenPanelByName<BasePanel>(KitchenMissionConfig.NameOfPopup_Tut);
        }

        private void OnClickClose()
        {
            Close();

            OnClose?.Invoke();
            OnClose = null;
        }

        private async UniTaskVoid TryAction()
        {
            if (KitchenMissionService.Instance.isOver.BoolValue)
            {
                BlockPanel.Set(true);

                await UniTask.WaitForSeconds(delayAction);

                BlockPanel.Set(false);

                if (KitchenMissionService.Instance.IsUserFailed())
                {
                    KitchenMissionService.Instance.CompleteEvent(false);
                }
                else if (KitchenMissionService.Instance.IsUserWin())
                {
                    MySonatFramework.audioService.PlayAudio("", winAudio);
                    winEff.Play();
                }
            }
        }

        private void ClaimRewards()
        {
            ClaimRewardsAsync().Forget();
        }

        private async UniTask ClaimRewardsAsync()
        {
            var stage = KitchenMissionService.Instance.GetCurrentStageData();

            MySonatFramework.inventoryService.AddReward(stage.GetRewardData(), new SonatFramework.Systems.InventoryManagement.EarnResourceLogData
            {
                spendType = "feature",
                spendId = "km",
                source = "non_iap"
            });

            PopupRewardChest_Kitchen.Stage = KitchenMissionService.Instance.stage.Value;

            UIData uiData = new UIData();
            uiData.Add("Reward", stage.GetRewardData());
            var popup = PanelManager.Instance.OpenPanel<PopupRewardChest_Kitchen>(uiData);


            KitchenMissionService.Instance.NextStage();

            await UniTask.WaitUntil(() => popup == null);

            OnClickClose();
        }

        private async UniTask ClaimRewardsAsyncTest()
        {
            var stage = KitchenMissionService.Instance.GetCurrentStageData();

            MySonatFramework.inventoryService.AddReward(stage.GetRewardData(), new SonatFramework.Systems.InventoryManagement.EarnResourceLogData
            {
                spendType = "feature",
                spendId = "kitchen_mission",
                source = "non_iap"
            });

            UIData uiData = new UIData();
            uiData.Add("Reward", stage.GetRewardData());
            var popup = PanelManager.Instance.OpenPanel<PopupRewardChest_Kitchen>(uiData);

            await UniTask.WaitUntil(() => popup == null);

            OnClickClose();
        }
    }
}