using Cysharp.Threading.Tasks;
using SonatFramework.Scripts.UIModule;
using SonatFramework.Scripts.Utils;
using SonatFramework.Systems;
using SonatFramework.Systems.AudioManagement;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Services.Core;
using UnityEngine;
using UnityEngine.UI;

namespace GrillSort.LavaQuest
{
    public class WidgetLavaQuest : UIHomeWidget
    {
        [SerializeField] private Button btn;
        [SerializeField] private TMP_Text nameTxt;
        [SerializeField] private TMP_Text timeTxt;
        [SerializeField] private GameObject notiObj;

        public override void Setup()
        {
            base.Setup();

            SetupButton();
            UpdateVisual();
            OnTick();

            LavaQuestService.OnTick += OnTick;
            LavaQuestService.OnReset += UpdateVisual;
            LavaQuestService.OnJoinAction += UpdateVisual;

            notiObj.SetActive(!LavaQuestService.Instance.isJoined.BoolValue);
        }

        private void OnDestroy()
        {
            LavaQuestService.OnTick -= OnTick;
            LavaQuestService.OnReset -= UpdateVisual;
            LavaQuestService.OnJoinAction -= UpdateVisual;

            PopupLavaQuest.IsShowContinue = false;
        }

        public override async UniTask<bool> ProcessTask()
        {
            if (!LavaQuestService.Instance.isUnlocked.BoolValue)
                return false;

            if (!LavaQuestService.Instance.isShownPopup.BoolValue)
            {
                LavaQuestService.Instance.isShownPopup.BoolValue = true;

                MySonatFramework.GetService<AudioService>().PlaySound(Sonat.Enums.AudioId.ButtonClick);
                var popup = PanelManager.Instance.OpenPanel<PopupLavaQuestStart>();

                await UniTask.WaitUntil(() => popup == null);

                await UniTask.WaitForSeconds(0.75f);

                return true;
            }

            if (LavaQuestService.Instance.isExpired.BoolValue)
            {
                MySonatFramework.GetService<AudioService>().PlaySound(Sonat.Enums.AudioId.ButtonClick);
                var popup = PanelManager.Instance.OpenPanel<PopupLavaQuest>();

                await UniTask.WaitUntil(() => popup == null);

                await UniTask.WaitForSeconds(0.75f);

                return true;
            }

            return false;

        }

        private void SetupButton()
        {
            btn.onClick.AddListener(OnClick);
        }

        private void OnTick()
        {
            timeTxt.text = LavaQuestService.Instance.GetStringTimeCountdown();
        }

        private void OnClick()
        {
            PopupLavaQuest.IsShowContinue = true;

            MySonatFramework.customTrackingService.OnShowPopup(gameObject.name.ToLogString(), "widget", "non_iap", "user");

            if (!LavaQuestService.Instance.isJoined.BoolValue)
            {
                PanelManager.Instance.OpenPanel<PopupLavaQuestStart>();
                return;
            }

            PanelManager.Instance.OpenPanel<PopupLavaQuest>();
        }

        private void UpdateVisual()
        {
            if (!LavaQuestService.Instance.isUnlocked.BoolValue && MySonatFramework.userDataService.GetLevel() < LavaQuestService.Instance.GetLevelStart())
            {
                gameObject.SetActive(false);
                return;
            }

            UpdateName();
        }

        private void UpdateName()
        {
            var isJoined = LavaQuestService.Instance.isJoined.BoolValue;

            nameTxt.gameObject.SetActive(!isJoined);
            timeTxt.gameObject.SetActive(isJoined);
        }

    }
}

