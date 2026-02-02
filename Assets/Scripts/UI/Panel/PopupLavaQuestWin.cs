using Cysharp.Threading.Tasks;
using DG.Tweening;
using I2.Loc;
using Sonat.Enums;
using SonatFramework.Scripts.UIModule;
using SonatFramework.Scripts.Utils;
using SonatFramework.Systems.EventBus;
using SonatFramework.Systems.InventoryManagement;
using SonatFramework.Systems;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using SonatFramework.Systems.AudioManagement;

namespace GrillSort.LavaQuest
{
    public class PopupLavaQuestWin : Panel
    {
        [SerializeField] private CanvasGroup coinCanvasGroup;
        [SerializeField] private Button closeBtn;
        [SerializeField] private TMP_Text closeTxt;
        [SerializeField] private TMP_Text coinTxt;
        [SerializeField] private Transform coinTransform;
        [SerializeField] private LocalizationParamsManager numPlayer;
        //[SerializeField] private TMP_Text numPlayer;

        [SerializeField] private AudioClip winAudio;

        public float timeGap;

        [SerializeField] private List<UIAvatarLavaQuest> avatars = new();

        public override void OnSetup()
        {
            base.OnSetup();

            coinTxt.text = LavaQuestService.Instance.GetCoinAfterSeparate().ToString();

            SetupButton();
            SetupPlayer();

            SonatSystem.GetService<AudioService>().PlayAudio("", winAudio);
            //SoundManager.Instance.PlaySound(winAudio);
        }

        public override void Open(UIData uiData)
        {
            base.Open(uiData);

            Show();
        }

        private void SetupButton()
        {
            closeBtn.onClick.AddListener(OnClickClose);
            closeBtn.interactable = false;
            closeTxt.transform.localScale = Vector3.zero;
        }

        private void SetupPlayer()
        {
            var otherLives = LavaQuestService.Instance.GetListPlayersAtState(LavaQuestConfig.Players.Player.EState.Live);

            int numOthers = Mathf.Min(5, otherLives.Count);

            for (int i = 0; i < avatars.Count; i++)
            {
                var otherAvatars = avatars[i];

                if (i >= numOthers)
                {
                    otherAvatars.gameObject.SetActive(false);
                    continue;
                }

                otherAvatars.Init(otherLives[i]);

                otherAvatars.transform.localScale = Vector3.zero;

            }

            //numPlayer.text = $"You are sharing the reward with {otherLives.Count} other winners!";
            numPlayer.SetParameterValue("value", (otherLives.Count - 1).ToString());

            coinTxt.text = "0";
        }

        private async UniTask Show()
        {
            await UniTask.WaitForSeconds(0.8f);

            _ = coinTxt.DOCounter(0, LavaQuestService.Instance.GetCoinAfterSeparate(), 0.8f).OnComplete(() =>
            {
                coinTransform.DOScale(0.9f, 0.2f).SetLoops(2, LoopType.Yoyo);
            });

            await UniTask.WaitForSeconds(0.8f);

            for (int i = 0; i < avatars.Count; i++)
            {
                var avatar = avatars[i];

                _ = avatar.transform.DOScale(1f, 0.4f).SetEase(Ease.OutBack).SetDelay(timeGap * i);
            }

            await UniTask.WaitForSeconds(1f);

            closeBtn.interactable = true;
            _ = closeTxt.transform.DOScale(1f, 0.4f).SetEase(Ease.OutBack);
        }

        private void OnClickClose()
        {
            WaitCoin();

            closeBtn.interactable = false;

            closeTxt.transform.DOKill();
            closeTxt.transform.DOScale(0f, 0.4f).SetEase(Ease.InBack);
        }

        private async UniTaskVoid WaitCoin()
        {
            _ = coinCanvasGroup.DOFade(1f, 0.3f);

            var log = new EarnResourceLogData()
            {
                spendId = "tq",
                spendType = "feature"
            };

            Service<InventoryService>.Get().AddResource(GameResource.Coin, LavaQuestService.Instance.GetCoinAfterSeparate(), log, false);
            EventBus<AddItemEvent>.Raise(new AddItemEvent()
            {
                resource = GameResource.Coin,
                quantity = LavaQuestService.Instance.GetCoinAfterSeparate(),
                position = closeBtn.transform.position,
                collectEffect = new CollectEffectMultiple()
            });

            SonatUtils.DelayCall(2, () =>
            {
                LavaQuestService.Instance.ResetEvent(true);
                PopupLavaQuest.OnCloseWin?.Invoke();
                PopupLavaQuest.OnCloseWin = null;
            });

            MySonatFramework.audioService.PlaySound(AudioId.Coin_Received);

        }
    }
}