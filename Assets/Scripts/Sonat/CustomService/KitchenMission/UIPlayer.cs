using Cysharp.Threading.Tasks;
using DG.Tweening;
using Manager;
using SonatFramework.Scripts.UIModule;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace GrillSort.KitchenMission
{
    public class UIPlayer : MonoBehaviour
    {
        [SerializeField] private Slider slider;
        [SerializeField] private Image frameImg;
        [SerializeField] private Image avtImg;
        [SerializeField] private TMPMarqueeSmart nameTxt;

        [SerializeField] private GameObject boardYourself;
        [SerializeField] private GameObject boardOther;

        [SerializeField] private CanvasGroup rewardCanvasGroup;
        [SerializeField] private Image closePlateImg;
        [SerializeField] private GameObject tick;
        [SerializeField] private GameObject claimTxt;
        [SerializeField] private Button claimBtn;
        [SerializeField] private Image medal;

        [SerializeField] private List<Sprite> medalSprites;

        private KitchenMissionConfig.Players.Player _player;
        private KitchenMissionConfig.Stages.Stage _stage;

        public void Init(KitchenMissionConfig.Players.Player playerData)
        {
            _player = playerData;
            _stage = KitchenMissionService.Instance.GetCurrentStageData();

            frameImg.SetSpriteAsync(PathManager.FrameSprite(playerData.fID)).Forget();
            avtImg.SetSpriteAsync(PathManager.AvatarSprite(playerData.aID)).Forget();
            nameTxt.SetText(playerData.name);

            boardYourself.SetActive(playerData.isYourself);
            boardOther.SetActive(!playerData.isYourself);

            medal.DOFade(0, 0);

            slider.value = playerData.lastStep * 1f / _stage.maxStep;
            TryStep().Forget();

            rewardCanvasGroup.alpha = 0;
        }

        private async UniTaskVoid TryStep()
        {
            await UniTask.WaitForSeconds(0.5f);

            await slider.DOValue(_player.step * 1f / _stage.maxStep, 0.75f).SetEase(Ease.InOutSine);

            CheckRewards();
        }

        private void CheckRewards()
        {
            if (_player.step != KitchenMissionService.Instance.GetCurrentStageData().maxStep) return;

            rewardCanvasGroup.DOFade(1f, 0.4f).SetEase(Ease.InOutSine);

            closePlateImg.sprite = _stage.chestCloseSprite;

            tick.SetActive(!_player.isYourself);
            claimTxt.SetActive(_player.isYourself);
            claimBtn.interactable = _player.isYourself;

            claimBtn.onClick.AddListener(ClaimRewards);

            UIPlayerController.OnSetMedal?.Invoke();
        }

        public void SetMedal(int index)
        {
            if (index >= medalSprites.Count) return;

            if (medal != null && medal.gameObject.activeInHierarchy)
            {
                medal.sprite = medalSprites[index];
                medal.DOFade(1, 0.4f).SetEase(Ease.InOutSine);
            }
        }

        private void ClaimRewards()
        {
            PopupKitchenMission.OnClaimReward?.Invoke();
        }
    }
}