using DG.Tweening;
using I2.Loc;
using Spine.Unity;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace GrillSort.KitchenMission
{
    public class UIVisualController : MonoBehaviour
    {
        [SerializeField] private Localize nameLocalize;
        [SerializeField] private LocalizationParamsManager describeParams;
        [SerializeField] private GameObject failedTxt;
        [SerializeField] private Image backgroundImg;
        [SerializeField] private SkeletonGraphic plateAnim;
        [SerializeField] private GameObject plateEff;
        [SerializeField] private GameObject rewards;
        [SerializeField] private UIBubbleReward bubbleRewards;
        [SerializeField] private Image closePlateImg;

        [SerializeField] private AudioClip moveAudio;
        [SerializeField] private float timeDelayMoveAudio;
        [SerializeField] private float timeDelayPlateEff;

        private KitchenMissionConfig.Stages.Stage data;

        public void Setup()
        {
            data = KitchenMissionService.Instance.GetCurrentStageData();

            SetupName();
            SetupDescribe();
            SetupBackground();
            SetupPlate();
        }

        private void SetupName()
        {
            nameLocalize.SetTerm(data.nameTerm);
        }

        private void SetupDescribe()
        {
            bool isUserFailed = KitchenMissionService.Instance.IsUserFailed();

            describeParams.gameObject.SetActive(!isUserFailed);
            failedTxt.SetActive(isUserFailed);

            describeParams.SetParameterValue("value", data.maxStep.ToString());
        }

        private void SetupBackground()
        {
            backgroundImg.sprite = data.GetBackgroundSprite();
        }

        private void SetupPlate()
        {
            plateAnim.initialSkinName = data.nameSkin;
            plateAnim.Initialize(true);

            plateEff.transform.localScale = Vector3.zero;

            plateAnim.AnimationState.SetAnimation(0, "In", false);
            plateAnim.AnimationState.AddAnimation(0, "Idle", true, 0);

            DOVirtual.DelayedCall(timeDelayPlateEff, () =>
            {
                plateEff.transform.DOScale(1f, 0.3f);
            });

            DOVirtual.DelayedCall(timeDelayMoveAudio, () =>
            {
                MySonatFramework.audioService.PlayAudio("", moveAudio);
            });
        }

        public void ShowRewards(bool state)
        {
            rewards.gameObject.SetActive(state);

            closePlateImg.sprite = data.chestCloseSprite;
            bubbleRewards.SetReward(data.GetRewardData());
            bubbleRewards.SetBlockUpdate();
            bubbleRewards.Select();
        }
    }
}