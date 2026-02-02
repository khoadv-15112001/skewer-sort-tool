using Cysharp.Threading.Tasks;
using DG.Tweening;
using SonatFramework.Scripts.UIModule;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace GrillSort.LavaQuest
{
    public class PopupLavaQuestFinding : Panel
    {
        [SerializeField] private Button continueBtn;
        [SerializeField] private TMP_Text continueTxt;

        [SerializeField] private AvatarSpawner avatarSpawner;

        [SerializeField] private float timeWaitBeforeFinding;
        [SerializeField] private float timeWaitAfterFinding;

        public override void OnSetup()
        {
            base.OnSetup();

            avatarSpawner.ResetText();
        }

        public override void Open(UIData uiData)
        {
            base.Open(uiData);

            Finding();
        }

        public override void OnOpenCompleted()
        {
            base.OnOpenCompleted();
        }

        public override void OnFocus()
        {

        }

        public override void OnFocusLost()
        {

        }

        public override void Close()
        {
            base.Close();
        }

        protected override void OnCloseCompleted()
        {
            base.OnCloseCompleted();
        }

        private void OnClickCloseButton()
        {
            PanelManager.Instance.OpenPanel<PopupLavaQuest>();
            Close();
        }

        private async UniTaskVoid Finding()
        {
            continueBtn.interactable = false;
            continueTxt.transform.localScale = Vector3.zero;

            await UniTask.WaitForSeconds(timeWaitBeforeFinding);

            avatarSpawner.Spawn();

            await UniTask.WaitForSeconds(timeWaitAfterFinding);

            continueBtn.interactable = true;
            continueBtn.onClick.AddListener(OnClickCloseButton);

            _ = continueTxt.transform.DOScale(1f, 0.3f).SetEase(Ease.OutBack);
        }
    }
}