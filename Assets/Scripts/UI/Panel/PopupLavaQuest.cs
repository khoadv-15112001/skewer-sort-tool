using Cysharp.Threading.Tasks;
using DG.Tweening;
using GrillSort.ConsecutiveWin;
using Sonat.Enums;
using SonatFramework.Scripts.UIModule;
using SonatFramework.Scripts.Utils;
using SonatFramework.Systems;
using SonatFramework.Systems.AudioManagement;
using SonatFramework.Systems.SceneManagement;
using SonatFramework.Systems.UserData;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace GrillSort.LavaQuest
{
    public class PopupLavaQuest : Panel
    {
        [SerializeField] private Button closeBtn;
        [SerializeField] private Button tutBtn;
        [SerializeField] private Button tapToContinueBtn;
        [SerializeField] private TMP_Text tapToContinueTxt;
        [SerializeField] private TMP_Text timeTxt;

        [SerializeField] private Button continueBtn;

        [SerializeField] private TMP_Text levelTxt;
        [SerializeField] private TMP_Text playersTxt;

        [SerializeField] private AvatarController avatarController;

        [SerializeField] private CanvasGroup normalCanvasGroup;
        [SerializeField] private CanvasGroup failedCanvasGroup;

        [SerializeField] private AudioClip failedAudio;

        public float TimeDelayBeforeJump = 2f;
        public float TimeDelayAfterJump = 2f;
        public float TimeDelayAfterSink = 2f;

        private EState eState;

        public static Action OnClose;
        public static Action OnCloseWin;
        public static Action OnCloseFailed;

        public static bool IsShowContinue;

        public override void OnSetup()
        {
            base.OnSetup();

            SetupButton();
            SetupAction();
        }

        public override void Open(UIData uiData)
        {
            base.Open(uiData);

            eState = EState.Normal;

            if (uiData != null)
            {
                uiData.TryGet("eState", out eState);
            }

            SetupVisual();

            SetupAvatar();

            TryAction();

            if (IsShowContinue)
            {
                ShowContinueButton();
                IsShowContinue = false;
            }
        }

        private void OnDestroy()
        {
            LavaQuestService.OnTick -= OnTick;
        }

        private void SetupButton()
        {
            continueBtn.transform.localScale = Vector3.zero;

            closeBtn.onClick.AddListener(OnClickClose);
            tutBtn.onClick.AddListener(OnClickTut);

            tapToContinueBtn.onClick.AddListener(OnClickClose);
            tapToContinueBtn.gameObject.SetActive(false);
            tapToContinueTxt.transform.localScale = Vector3.zero;

            continueBtn.onClick.AddListener(OnClickContinue);
        }

        private void SetupAction()
        {
            OnTick();
            LavaQuestService.OnTick += OnTick;
        }

        private void OnTick()
        {
            timeTxt.text = LavaQuestService.Instance.GetStringTimeCountdown();
        }

        private void OnClickClose()
        {
            // Lưu callbacks trước khi set null để tránh mất reference
            Action onCloseCallback = OnClose;
            Action onCloseWinCallback = OnCloseWin;
            Action onCloseFailedCallback = OnCloseFailed;
            
            // Clear callbacks trước để tránh duplicate calls
            OnClose = null;
            OnCloseWin = null;
            OnCloseFailed = null;

            if (eState == EState.Normal)
            {
                Close();
                onCloseCallback?.Invoke();
            }

            if (eState == EState.Win)
            {
                Close(); // ✅ Phải close popup trước khi invoke callback
                onCloseWinCallback?.Invoke();
            }

            if (eState == EState.Failed)
            {
                Close();
                onCloseFailedCallback?.Invoke();
            }
        }

        private void OnClickTut()
        {
            PanelManager.Instance.OpenPanelByName<BasePanel>(LavaQuestService.NamePopupTut);
        }

        private void CheckLives()
        {
            if (MySonatFramework.livesService.CanPlay())
            {
                var consecutiveWinService = MySonatFramework.GetService<ConsecutiveWinService>();
                var level = MySonatFramework.GetService<UserDataService>().GetLevel();
                if (consecutiveWinService.CheckStart(level))
                {
                    UIData uiData = new UIData();
                    uiData.Add("OnPlay", (Action)(() => SonatUtils.DelayCall(0.1f, PlayGame)));
                    uiData.Add("OnClose", (Action)(() =>
                    {
                        OnClickClose();
                    }));
                    PanelManager.Instance.OpenPanel<PopupPrePlay>(uiData);
                }
                else
                {
                    PlayGame();
                }
            }
            else
            {
                PanelManager.Instance.OpenPanel<PopupRefillLives>();
                PopupToast.Cretate("No more lives left!");
            }
        }

        private void PlayGame()
        {
            LoadingScreenInstance.Instance.Show(3.5f);
            SonatUtils.DelayCall(0.5f, () => { MySonatFramework.GetService<SceneService>().SwitchScene(GamePlacement.Gameplay); });
        }

        private void OnClickContinue()
        {
            CheckLives();
        }

        public void ShowContinueButton()
        {
            bool isExpired = LavaQuestService.Instance.isExpired.BoolValue;
            if (isExpired) return;

            DOVirtual.DelayedCall(0.25f, () =>
            {
                continueBtn.transform.DOScale(1f, 0.4f).SetEase(Ease.OutBack);
            });
        }

        private void SetupVisual()
        {
            normalCanvasGroup.alpha = 1;
            failedCanvasGroup.alpha = 0;

            switch (eState)
            {
                case EState.Win:
                    levelTxt.text = $"{LavaQuestService.Instance.step.Value - 1}/{LavaQuestService.Instance.Config.maxStep}";
                    playersTxt.text = $"{LavaQuestService.CacheCountLastPlayer}/{LavaQuestService.Instance.players.Value.datas.Count}";

                    break;

                case EState.Failed:
                    levelTxt.text = $"{LavaQuestService.Instance.step.Value}/{LavaQuestService.Instance.Config.maxStep}";
                    var player = LavaQuestService.Instance.GetListPlayersAtState(LavaQuestConfig.Players.Player.EState.Live).Count;
                    playersTxt.text = $"{player}/{LavaQuestService.Instance.players.Value.datas.Count}";
                    break;

                default:
                    SetupNormal();
                    break;
            }
        }

        private void SetupNormal()
        {
            levelTxt.text = $"{LavaQuestService.Instance.step.Value}/{LavaQuestService.Instance.Config.maxStep}";

            var player = LavaQuestService.Instance.GetListPlayersAtState(LavaQuestConfig.Players.Player.EState.Live).Count;
            playersTxt.text = $"{player}/{LavaQuestService.Instance.players.Value.datas.Count}";

            bool isExpired = LavaQuestService.Instance.isExpired.BoolValue;

            normalCanvasGroup.alpha = 1;
            failedCanvasGroup.alpha = 0;

            if (isExpired)
            {
                FadeToFailed("feature_ended");

                return;
            }

            //CheckTut();
        }

        private async UniTaskVoid CheckTut()
        {
            if (!LavaQuestService.Instance.notiTut.BoolValue)
            {
                LavaQuestService.Instance.notiTut.BoolValue = true;

                BlockPanel.Set(true);

                await UniTask.WaitForSeconds(0.5f);

                BlockPanel.Set(false);

                PanelManager.Instance.OpenPanelByName<BasePanel>(LavaQuestService.NamePopupTut);
            }
        }

        private void SetupAvatar()
        {
            avatarController.Hide();

            if (eState != EState.Win)
            {
                avatarController.Init(LavaQuestService.Instance.step.Value);
                avatarController.HideBox(LavaQuestService.Instance.step.Value);
            }
            else
            {
                avatarController.HideBox(LavaQuestService.Instance.step.Value - 1);
            }
        }

        private void TryAction()
        {
            if (eState == EState.Win)
            {
                TryActionWin();
            }

            if (eState == EState.Failed)
            {
                FadeToFailed();
            }
        }

        private async UniTaskVoid TryActionWin()
        {
            BlockPanel.Set(true);

            await UniTask.WaitForSeconds(0.5f);

            avatarController.Init(LavaQuestService.Instance.step.Value - 1);

            await UniTask.WaitForSeconds(0.5f);

            levelTxt.text = $"{LavaQuestService.Instance.step.Value}/{LavaQuestService.Instance.Config.maxStep}";
            //SoundManager.Instance.PlaySound(lastCountAudio);
            CountPlayer();

            await UniTask.WaitForSeconds(TimeDelayBeforeJump);

            avatarController.Jump(LavaQuestService.Instance.step.Value);

            await UniTask.WaitForSeconds(TimeDelayAfterJump);

            avatarController.SinkBox();

            if (LavaQuestService.Instance.step.Value > 1)
                await UniTask.WaitForSeconds(TimeDelayAfterSink);

            BlockPanel.Set(false);

            if (LavaQuestService.Instance.step.Value == LavaQuestService.Instance.Config.maxStep)
            {

                PanelManager.Instance.OpenForget<PopupLavaQuestWin>();
            }
            else
            {
                if (LavaQuestService.Instance.isExpired.BoolValue)
                {
                    FadeToFailed("feature_ended");
                }
                else
                {
                    tapToContinueBtn.gameObject.SetActive(true);
                    tapToContinueTxt.transform.DOScale(1, 0.4f).SetEase(Ease.OutBack);
                }
            }
        }

        private async UniTaskVoid FadeToFailed(string end_cause = null)
        {
            BlockPanel.Set(true);

            LavaQuestService.Instance.ResetEvent(false, end_cause);

            if (eState != EState.Win)
                await UniTask.WaitForSeconds(1f);

            await normalCanvasGroup.DOFade(0f, 0.4f);

            SonatSystem.GetService<SonatAudioService>().PlayAudio("", failedAudio);

            await failedCanvasGroup.DOFade(1f, 0.4f);

            BlockPanel.Set(false);

            tapToContinueBtn.gameObject.SetActive(true);
            tapToContinueTxt.transform.DOScale(1, 0.4f).SetEase(Ease.OutBack);
        }

        private void CountPlayer()
        {
            int displayValue = LavaQuestService.CacheCountLastPlayer;

            DOTween.To(
                () => displayValue,
                x =>
                {
                    displayValue = x;
                    playersTxt.text = $"{displayValue}/{LavaQuestService.Instance.players.Value.datas.Count}";
                },
                LavaQuestService.CacheCountCurPlayer,
                1f
            ).SetEase(Ease.OutQuad).OnComplete(() =>
            {
                //SoundManager.Instance.PlaySound(lastCountAudio);
            });
        }

        public enum EState
        {
            Normal,
            Win,
            Failed
        }
    }
}