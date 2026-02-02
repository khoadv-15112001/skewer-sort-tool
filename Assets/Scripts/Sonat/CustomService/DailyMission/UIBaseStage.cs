using DG.Tweening;
using Gameplay.Entities.ItemScripts;
using I2.Loc;
using MyGame.Modules.CardCollection;
using SonatFramework.Scripts.UIModule;
using SonatFramework.Scripts.UIModule.UIElements;
using SonatFramework.Systems.AudioManagement;
using SonatFramework.Systems.EventBus;
using SonatFramework.Systems.InventoryManagement;
using SonatFramework.Systems.InventoryManagement.GameResources;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace GrillSort.DailyMission
{
    public class UIBaseStage : MonoBehaviour
    {
        [SerializeField] private Button btn;
        [SerializeField] private GameObject claimingGO;
        [SerializeField] private GameObject claimedGO;

        [SerializeField] private Localize describeTxt;

        [SerializeField] private UIRewardItemDailyMission reward;
        [SerializeField] private TMP_Text rewardTxt;
        [SerializeField] private GameObject ribbon;
        [SerializeField] private Slider progressSlider;
        [SerializeField] private Image progressImg;
        [SerializeField] private TMP_Text progressTxt;

        [SerializeField] private Sprite progressGreenSprite;
        [SerializeField] private Sprite progressPinkSprite;

        private ResourceData _resourceData;

        private int _indexStage;
        private int _target;

        private int curItem => DailyMissionService.Instance.numItem.Value;

        public void Setup(int indexStage)
        {
            _indexStage = indexStage;
            _target = DailyMissionService.Instance.GetCurrentDailyMission().GetNumItemBaseStage(indexStage);

            SetupState();
            describeTxt.SetTerm(DailyMissionService.Instance.GetCurrentDailyMission().DescribeTerm);
        }

        private void SetupState()
        {
            if (DailyMissionService.Instance.listClaimedBaseStageIndex.Contains(_indexStage))
            {
                Set(EState.Claimed);
                return;
            }

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

            reward.gameObject.SetActive(true);
            ribbon.gameObject.SetActive(true);

            SetupReward();

            progressImg.gameObject.SetActive(true);

            progressImg.sprite = progressPinkSprite;

            int item = Mathf.Clamp(curItem, 0, _target);

            progressSlider.value = item * 1f / _target;
            progressTxt.text = $"{item}/{_target}";

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

            reward.gameObject.SetActive(true);
            ribbon.gameObject.SetActive(true);

            SetupReward();

            progressImg.gameObject.SetActive(true);

            progressImg.sprite = progressGreenSprite;

            int item = Mathf.Clamp(curItem, 0, _target);

            progressSlider.value = item * 1f / _target;
            progressTxt.text = $"{item}/{_target}";

            btn.onClick.RemoveAllListeners();
            btn.onClick.AddListener(Claim);
        }

        private void SetClaimed()
        {
            claimingGO.SetActive(false);
            claimedGO.SetActive(true);

            reward.gameObject.SetActive(false);
            ribbon.gameObject.SetActive(false);

            progressImg.gameObject.SetActive(false);
        }

        private void SetupReward()
        {
            _resourceData = DailyMissionService.Instance.GetRewardBaseStage(_indexStage).resourceDatas[0];

            reward.Init(_resourceData.resource, _resourceData.quantity);
        }

        private void Claim()
        {
            MySonatFramework.GetService<AudioService>().PlaySound(Sonat.Enums.AudioId.ButtonClick);

            btn.onClick.RemoveAllListeners();

            var bubbleItem = Instantiate(reward.gameObject, PanelManager.Instance.transform).GetComponent<RectTransform>();
            bubbleItem.transform.position = reward.transform.position;

            bubbleItem.transform.DOLocalMoveY(bubbleItem.transform.localPosition.y + 100f, 0.6f).SetEase(Ease.InOutSine);
            bubbleItem.GetComponent<Image>().DOFade(0f, 1f).OnComplete(() =>
            {
                Destroy(bubbleItem.gameObject);
            });

            //Debug.LogError("RewardData: " + _resourceData.resource);

            MySonatFramework.GetService<InventoryService>().AddResource(_resourceData.resource, _resourceData.quantity, new EarnResourceLogData
            {
                source = "non_iap",
                spendType = "feature",
                spendId = "dm"
            });

            EventBus<AddItemEvent>.Raise(new AddItemEvent()
            {
                resource = _resourceData.resource,
                quantity = _resourceData.quantity,
            });

            DailyMissionService.Instance.ClaimBaseStageIndex(_indexStage);

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