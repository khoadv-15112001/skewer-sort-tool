using Cysharp.Threading.Tasks;
using SonatFramework.Scripts.UIModule;
using SonatFramework.Systems.AudioManagement;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

namespace GrillSort.LavaQuest
{
    public class PopupLavaQuestStart : Panel
    {
        [SerializeField] private Button closeBtn;
        [SerializeField] private Button tutBtn;
        [SerializeField] private Button continueBtn;
        [SerializeField] private Button continueGreyBtn;

        [SerializeField] private GameObject unlockObj;
        [SerializeField] private GameObject lockObj;

        [SerializeField] private GameObject winTxt;
        [SerializeField] private GameObject failedTxt;

        [SerializeField] private TMP_Text timeTxt;

        public override void OnSetup()
        {
            base.OnSetup();

            SetupButton();
            SetupState();

            OnTick();
            LavaQuestService.OnTick += OnTick;
        }

        public override void Close()
        {
            base.Close();
        }

        private void OnClickClose()
        {
            Close();
        }

        private void OnDestroy()
        {
            LavaQuestService.OnTick -= OnTick;
            StopAllCoroutines();
        }

        private void OnTick()
        {
            timeTxt.text = LavaQuestService.Instance.GetStringTimeGapCountdown();

            if (unlockObj.activeSelf) return;

            if (LavaQuestService.Instance.CanShowByTimeGap())
            {
                unlockObj.SetActive(true);
                lockObj.SetActive(false);

                continueBtn.gameObject.SetActive(true);
                continueGreyBtn.gameObject.SetActive(false);
            }
        }

        private void OnClickTut()
        {
            PanelManager.Instance.OpenPanelByName<BasePanel>(LavaQuestService.NamePopupTut);
        }

        private void SetupButton()
        {
            closeBtn.onClick.AddListener(OnClickClose);
            tutBtn.onClick.AddListener(OnClickTut);
            continueBtn.onClick.AddListener(OnClickContinue);
            continueGreyBtn.onClick.AddListener(OnClickContinueGray);
        }

        private void SetupState()
        {
            var canJoin = LavaQuestService.Instance.CanShowByTimeGap();

            unlockObj.SetActive(canJoin);
            lockObj.SetActive(!canJoin);

            if (!canJoin)
            {
                winTxt.SetActive(LavaQuestService.Instance.state.Value == (int)LavaQuestService.EState.Win);
                failedTxt.SetActive(LavaQuestService.Instance.state.Value == (int)LavaQuestService.EState.Lose);
            }

            continueBtn.gameObject.SetActive(canJoin);
            continueGreyBtn.gameObject.SetActive(!canJoin);
        }

        private void OnClickContinue()
        {
            if (!LavaQuestService.Instance.CanShowByTimeGap())
            {
                Close();
                return;
            }

            if (!LavaQuestService.Instance.notiTut.BoolValue)
            {
                LavaQuestService.Instance.notiTut.BoolValue = true;

                var uiData = new UIData();

                uiData.Add(UIDataKey.CallBackOnClose, new System.Action(() =>
                {
                    PanelManager.Instance.OpenPanel<PopupLavaQuestFinding>();
                }));

                PanelManager.Instance.OpenPanelByName<BasePanel>(LavaQuestService.NamePopupTut, uiData);
            }
            else
            {
                PanelManager.Instance.OpenPanel<PopupLavaQuestFinding>();
            }

            LavaQuestService.Instance.OnJoin();

            PopupLavaQuest.OnClose += Close;
        }

        private void OnClickContinueGray()
        {
            PopupToast.Cretate("Comeback after", $" {LavaQuestService.Instance.GetStringTimeGapCountdown()}");
        }
    }
}