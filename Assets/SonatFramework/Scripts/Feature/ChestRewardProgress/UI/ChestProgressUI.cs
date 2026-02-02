using System;
using System.Collections;
using DG.Tweening;
using Sirenix.OdinInspector;
using Sonat.Enums;
using SonatFramework.Scripts.UIModule;
using SonatFramework.Systems;
using UnityEngine;
using UnityEngine.UI;

namespace SonatFramework.Scripts.Feature.ChestRewardProgress
{
    public class ChestProgressUI : MonoBehaviour
    {
        [Header("Progress Bar")] [SerializeField]
        private UIProgressBar uIProgressBar;

        [SerializeField] private ChestDisplayUIController chestDisplayUIController;
        [SerializeField] private Button btnClaim;

        [SerializeField] private bool playOnEnable = true;
        [SerializeField] private float delayToSlide = 0.5f;
        [SerializeField] private float slideDuration = 0.15f;

        [Header("Block")] [SerializeField] private GameObject objBlock;

        private readonly Service<ChestRewardService> chestRewardService = new();
        private ChestConfig chestClaimed;
        private ChestConfig chestConfig;
        private ChestRewardProgressData data;

        private void OnEnable()
        {
            LoadData();
            objBlock.gameObject.SetActive(false);
            if (playOnEnable) Play();
        }

        private void LoadData()
        {
            chestConfig = chestRewardService.Instance.GetChestConfig();
            chestClaimed = chestRewardService.Instance.GetChestClaimed();
            data = chestRewardService.Instance.data;
        }

        public void Play()
        {
            SetProgressValue();

            if (!chestRewardService.Instance.CanStartChest()) return;
            if (uIProgressBar.CheckPreFull())
            {
                objBlock.gameObject.SetActive(true);
            }

            DOVirtual.DelayedCall(delayToSlide, UpdateProgress);
        }

        private void SetProgressValue()
        {
            if (data.currentProgress == 0)
            {
                if (data.currentChestIndex == 0)
                {
                    uIProgressBar.SetData(0, chestConfig.levelRequired);
                }
                else
                {
                    uIProgressBar.SetData(chestClaimed.levelRequired - 1, chestClaimed.levelRequired);
                }
            }
            else
                uIProgressBar.SetData(data.currentProgress - 1, chestConfig.levelRequired);
        }

        private void UpdateProgress()
        {
            uIProgressBar.AddValue(1);
            if (uIProgressBar.IsFull)
            {
                objBlock.gameObject.SetActive(true);
                OpenChest();
            }
        }

        private void OpenChest()
        {
            StartCoroutine(IOpenChest());
        }

        private IEnumerator IOpenChest()
        {
            yield return new WaitForSeconds(0.5f);
            //chestDisplayUIController.UpdateChestState(ChestState.Opening);
            yield return new WaitForSeconds(1f);

            var popup = PanelManager.Instance.GetPanel<PopupReward>();
            yield return new WaitUntil(() => popup == null || !popup.gameObject.activeInHierarchy);
            PanelManager.Instance.OpenPanel<PopupRewardReceive>(new UIData().Add("RewardData", chestClaimed.reward)
                .Add(UIDataKey.CallBackOnClose, (Action)OnChestClaimed));
            yield return new WaitForSeconds(0.5f);
            objBlock.gameObject.SetActive(false);
        }

        private void OnChestClaimed()
        {
            chestDisplayUIController.UpdateChestState(ChestState.Opened);
            if (!PlayerPrefs.HasKey("ShowRatePopup"))
            {
                PlayerPrefs.SetInt("ShowRatePopup", 1);
                PanelManager.Instance.OpenForget<PopupRate>();
            }
        }

#if UNITY_EDITOR
        [Button("Add Chest")]
        private void AddChest()
        {
            var chestRewardService = SonatSystem.GetService<ChestRewardService>();
            chestRewardService.UpdateProgress();
            LoadData();
            UpdateProgress();
        }
#endif
    }
}