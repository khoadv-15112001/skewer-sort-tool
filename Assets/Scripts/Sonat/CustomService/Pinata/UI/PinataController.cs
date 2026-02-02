using Cysharp.Threading.Tasks;
using DG.Tweening;
using MyGame.Modules.CardCollection.Animation;
using SonatFramework.Scripts.UIModule;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace GrillSort.Pinata
{
    public class PinataController : MonoBehaviour
    {
        [SerializeField] private PinataAnimHandle pinataAnimHandle;
        [SerializeField] private UIPinataMilestoneChanceController chanceController;
        [SerializeField] private UIPinataCardBubbleSpawner cardBubbleSpawner;
        [SerializeField] private Button fullscreenBtn;
        [SerializeField] private GameObject starParent;
        [SerializeField] private GameObject[] stars;
        [SerializeField] private GameObject tapTxt;
        [SerializeField] private GameObject chanceObj;
        //[SerializeField] private TMP_Text tapToClaimTxt;
        [SerializeField] private TMP_Text rewardTxt;

        [SerializeField] private Image backgroundImg;
        [SerializeField] private Image lightImg;

        [SerializeField] private Sprite[] backgroundSprites;
        [SerializeField] private Sprite[] lightSprites;

        public static int CurrentPigIndex;
        public static int CurrentStar;
        public static PinataConfig.Stage StageData;

        public static List<int> StarCollected = new();

        public static Action OnTapPinata;
        public static Action OnUpdateStar;
        public static Action OnFullMilestone;

        public void Setup()
        {
            CurrentPigIndex = PinataService.Instance.CurrentStage.Value;
            CurrentStar = 1;
            StageData = PinataService.Instance.GetCurrentStage();
            StarCollected.Clear();
            //tapToClaimTxt.transform.localScale = Vector3.zero;
            rewardTxt.transform.localScale = Vector3.zero;

            SetupButton(TapPinata);
            UpdateStar();

            pinataAnimHandle.Appear();
            chanceController.Setup();

            starParent.transform.localScale = Vector3.zero;
            starParent.transform.DOScale(1f, 0.4f).SetEase(Ease.OutBack).SetDelay(1f);

            OnFullMilestone += OnFullMilestoneAction;
            OnUpdateStar += UpdateStar;
            PinataAnimHandle.OnExplosion += OnExplosion;
            PinataAnimHandle.OnTapPinataSuccessful += OnTapSuccessful;
        }

        private void OnDestroy()
        {
            OnFullMilestone -= OnFullMilestoneAction;
            OnUpdateStar -= UpdateStar;
            PinataAnimHandle.OnExplosion -= OnExplosion;
            PinataAnimHandle.OnTapPinataSuccessful -= OnTapSuccessful;
        }

        private void SetupButton(Action act)
        {
            fullscreenBtn.onClick.RemoveAllListeners();
            fullscreenBtn.onClick.AddListener(() =>
            {
                act?.Invoke();
            });
        }

        private void TapPinata()
        {
            if (!pinataAnimHandle.IsReady) return;

            if (tapTxt.gameObject.activeSelf)
                tapTxt.gameObject.SetActive(false);

            OnTapPinata?.Invoke();
        }

        private void UpdateStar()
        {
            for (int i = 0; i < stars.Length; i++)
            {
                stars[i].gameObject.SetActive(i + 1 <= CurrentStar);
            }

            UpdateBackground();
        }

        private void UpdateBackground()
        {
            backgroundImg.sprite = CurrentStar == 3 ? backgroundSprites[1] : backgroundSprites[0];
            lightImg.sprite = CurrentStar == 3 ? lightSprites[1] : lightSprites[0];
        }

        private void OnTapSuccessful()
        {
            cardBubbleSpawner.Spawn(CurrentStar);
        }

        private void OnFullMilestoneAction()
        {
            SetupButton(null);
        }

        private void OnExplosion()
        {
            starParent.SetActive(false);
            chanceObj.SetActive(false);

            //tapToClaimTxt.DOScale(1f, 0.3f).SetEase(Ease.OutBack).SetDelay(0.5f);
            rewardTxt.DOScale(1f, 0.3f).SetEase(Ease.OutBack);

            OpenReceiveCard().Forget();

            PinataService.Instance.CompleteStage();
        }

        private async UniTaskVoid OpenReceiveCard()
        {
            var panel = PanelManager.Instance.OpenPanel<PopupReceiveCard_Pinata>();

            await UniTask.WaitUntil(() => panel == null);

            PanelManager.Instance.GetPanel<PopupPinata>().Close();
        }
    }
}