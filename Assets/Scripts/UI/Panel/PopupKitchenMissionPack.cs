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
    public class PopupKitchenMissionPack : Panel
    {
        [SerializeField] private Button closeBtn;
        [SerializeField] private Button buyBtn;

        [SerializeField] private TMP_Text timeTxt;

        [SerializeField] private SkeletonGraphic charAnim;
        [SerializeField] private AudioClip startAudio;

        [SerializeField] private GameObject packSpecial;
        [SerializeField] private GameObject packNormal;

        [SerializeField] private List<UIPlate> uiPlates;

        bool hasAvatar = false;

        public override void OnSetup()
        {
            base.OnSetup();

            SetupButton();
            SetupVisual();

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
            buyBtn.onClick.AddListener(OnClickBuy);
        }

        private void SetupVisual()
        {
            //MySonatFramework.audioService.PlayAudio("", startAudio);
            charAnim.AnimationState.SetAnimation(0, "Appear", false);
            charAnim.AnimationState.AddAnimation(0, "Idle", true, 0);

            int stage = KitchenMissionService.Instance.stage.Value;

            for (int i = 0; i < uiPlates.Count; i++)
            {
                uiPlates[i].SetOpen(i < stage);
            }

            hasAvatar = MySonatFramework.inventoryService.GetResource(Sonat.Enums.GameResource.Avatar_Kitchen_Mission) >= 1;

            packNormal.SetActive(hasAvatar);
            packSpecial.SetActive(!hasAvatar);
        }

        private void OnClickClose()
        {
            Close();
        }

        private void OnClickBuy()
        {

        }

        public void OnBuySuccess()
        {
            KitchenMissionService.Instance.BoughtPack();
            Close();
        }
    }
}