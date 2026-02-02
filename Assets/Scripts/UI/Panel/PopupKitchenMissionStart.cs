using Sonat.Data;
using SonatFramework.Scripts.SonatSDKAdapterModule;
using SonatFramework.Scripts.UIModule;
using SonatFramework.Systems.InventoryManagement.GameResources;
using SonatFramework.Systems.InventoryManagement;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using TMPro;
using Spine.Unity;
using I2.Loc;

namespace GrillSort.KitchenMission
{
    public class PopupKitchenMissionStart : Panel
    {
        [SerializeField] private Button closeBtn;
        [SerializeField] private Button tutBtn;
        [SerializeField] private Button playBtn;
        [SerializeField] private Button continueBtn;

        [SerializeField] private GameObject failedObj;
        [SerializeField] private GameObject normalObj;

        [SerializeField] private LocalizationParamsManager stageParams;

        [SerializeField] private TMP_Text timeTxt;

        [SerializeField] private SkeletonGraphic charAnim;
        [SerializeField] private AudioClip startAudio;

        [SerializeField] private List<UIPlate> uiPlates;

        public override void OnSetup()
        {
            base.OnSetup();

            SetupButton();
            SetupVisual();

            KitchenMissionService.Instance.UpdateTimeEnd();

            UpdateTime();
            KitchenMissionService.OnTick += UpdateTime;
        }

        private void OnDestroy()
        {
            KitchenMissionService.OnTick -= UpdateTime;
        }

        private void UpdateTime()
        {
            timeTxt.text = KitchenMissionService.Instance.GetStringTimeCountdown();
        }

        public override void Open(UIData uiData)
        {
            base.Open(uiData);
        }

        private void SetupButton()
        {
            closeBtn.onClick.AddListener(OnClickClose);
            tutBtn.onClick.AddListener(OnClickTut);
            playBtn.onClick.AddListener(OnClickPlay);
            continueBtn.onClick.AddListener(OnClickContinue);
        }

        private void SetupVisual()
        {
            bool isExpired = false;

            failedObj.SetActive(isExpired);
            normalObj.SetActive(!isExpired);

            playBtn.gameObject.SetActive(!isExpired);
            continueBtn.gameObject.SetActive(isExpired);

            int stage = KitchenMissionService.Instance.stage.Value;

            stageParams.SetParameterValue("value", (stage + 1).ToString());

            MySonatFramework.audioService.PlayAudio("", startAudio);
            charAnim.AnimationState.SetAnimation(0, "Appear", false);
            charAnim.AnimationState.AddAnimation(0, "Idle", true, 0);

            for (int i = 0; i < uiPlates.Count; i++)
            {
                uiPlates[i].SetOpen(i < stage);
            }
        }

        private void OnClickClose()
        {
            Close();
        }

        private void OnClickTut()
        {
            PanelManager.Instance.OpenPanelByName<BasePanel>(KitchenMissionConfig.NameOfPopup_Tut);
        }

        private void OnClickPlay()
        {
            if (!KitchenMissionService.Instance.isShownTut.BoolValue)
            {
                KitchenMissionService.Instance.isShownTut.BoolValue = true;

                var uiData = new UIData();
                uiData.Add(UIDataKey.CallBackOnClose, (Action)OnJoinEvent);
                PanelManager.Instance.OpenPanelByName<BasePanel>(KitchenMissionConfig.NameOfPopup_Tut, uiData);

                return;
            }

            OnJoinEvent();
        }

        private void OnJoinEvent()
        {
            var logData = new EarnResourceLogData
            {
                spendType = "feature",
                spendId = "kitchen_mission",
            };

            var joinRewards = KitchenMissionService.Instance.Config.joinRewards;
            MySonatFramework.inventoryService.AddReward(joinRewards, logData);

            UIData uiData = new UIData();
            uiData.Add("Reward", joinRewards);
            uiData.Add(UIDataKey.CallBackOnClose, (Action)OpenPopupKitchenMission);
            PanelManager.Instance.OpenPanelByName<PopupReward>(KitchenMissionConfig.NameOfPopup_StartReward, uiData);

            KitchenMissionService.Instance.OnJoin();
        }

        private void OpenPopupKitchenMission()
        {
            PopupKitchenMission.IsShowReward = true;
            PanelManager.Instance.OpenPanel<PopupKitchenMission>();
            PopupKitchenMission.OnClose += Close;
        }

        private void OnClickContinue()
        {
            Close();
        }

        public override string GetPlacement()
        {
            return $"{base.GetPlacement()}_{KitchenMissionService.Instance.stage.Value + 1}";
        }
    }
}