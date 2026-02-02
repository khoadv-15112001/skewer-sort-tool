using Cysharp.Threading.Tasks;
using GrillSort.BattlePass;
using GrillSort.LavaQuest;
using SonatFramework.Scripts.UIModule;
using SonatFramework.Scripts.Utils;
using SonatFramework.Systems.AudioManagement;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace GrillSort.KitchenMission
{
    public class WidgetKitchenMission : UIHomeWidget
    {
        [SerializeField] private Button btn;
        [SerializeField] private TMP_Text rankTxt;
        [SerializeField] private Sprite kitchenSprite;
        [SerializeField] private float sizeItemCollect;

        [SerializeField] private GameObject nameTxt;
        [SerializeField] private TMP_Text timeTxt;

        [SerializeField] private Button bubbleBtn;

        public override void Setup()
        {
            if (!KitchenMissionService.Instance.isUnlocked.BoolValue
                || KitchenMissionService.Instance.isCompleteEvent.BoolValue
                || !KitchenMissionService.Instance.IsInTimeEvent())
            {
                gameObject.SetActive(false);
                return;
            }

            gameObject.SetActive(true);

            SetupButton();
            SetupVisual();

            if (!KitchenMissionService.Instance.isLogStart.BoolValue)
            {
                KitchenMissionService.Instance.start_count.Value++;
                KitchenMissionService.Instance.LogStartFeature(0);
                KitchenMissionService.Instance.isLogStart.BoolValue = true;
            }

            KitchenMissionService.OnChangePlayerData += SetupRank;
            KitchenMissionService.OnCompleteEvent += OnCompleteEvent;
            KitchenMissionService.OnNextStage += SetupRank;
            KitchenMissionService.OnResetEvent += SetupRank;
            KitchenMissionService.OnResetEvent += OnResetEvent;

            KitchenMissionService.OnBoughtPack += SetupBubble;

            KitchenMissionService.OnTick += UpdateName;
        }

        private void OnDestroy()
        {
            KitchenMissionService.OnChangePlayerData -= SetupRank;
            KitchenMissionService.OnCompleteEvent -= OnCompleteEvent;
            KitchenMissionService.OnNextStage -= SetupRank;
            KitchenMissionService.OnResetEvent -= SetupRank;
            KitchenMissionService.OnResetEvent -= OnResetEvent;

            KitchenMissionService.OnBoughtPack -= SetupBubble;

            KitchenMissionService.OnTick -= UpdateName;
        }

        private void UpdateName()
        {
            bool isJoined = KitchenMissionService.Instance.isJoined.BoolValue;

            nameTxt.gameObject.SetActive(!isJoined);
            timeTxt.gameObject.SetActive(isJoined);

            timeTxt.text = KitchenMissionService.Instance.GetStringTimeCountdown();
        }

        private void SetupButton()
        {
            btn.onClick.AddListener(OnClick);
            bubbleBtn.onClick.AddListener(OnClickBubble);
        }

        private void SetupVisual()
        {
            SetupRank();

            UpdateName();
        }

        private void SetupRank()
        {
            SetupBubble();

            if (!KitchenMissionService.Instance.isJoined.BoolValue)
            {
                rankTxt.gameObject.SetActive(false);
                return;
            }

            rankTxt.gameObject.SetActive(true);
            rankTxt.text = KitchenMissionService.Instance.GetMyRank().ToString();
        }

        private void SetupBubble()
        {
            if (!KitchenMissionService.Instance.isBoughtPack.BoolValue)
                bubbleBtn.gameObject.SetActive(KitchenMissionService.Instance.isEventActive.BoolValue);
            else
            {
                bubbleBtn.gameObject.SetActive(false);
            }
        }

        private void OnCompleteEvent()
        {
            gameObject.SetActive(false);
        }

        private void OnResetEvent()
        {
            if (KitchenMissionService.Instance.isCompleteEvent.BoolValue || !KitchenMissionService.Instance.IsInTimeEvent())
            {
                gameObject.SetActive(false);
            }
        }

        public override void OnFocus()
        {
            if (KitchenMissionService.NumKey == 0) return;

            Collect(0, kitchenSprite, () =>
            {
            }, 1, new Vector2(sizeItemCollect, sizeItemCollect));

            KitchenMissionService.NumKey = 0;
        }

        private void OnClick()
        {
            MySonatFramework.customTrackingService.OnShowPopup(gameObject.name.ToLogString(), "widget", "non_iap", "user");

            if (KitchenMissionService.Instance.isJoined.BoolValue)
            {
                PanelManager.Instance.OpenPanel<PopupKitchenMission>();
                return;
            }

            PanelManager.Instance.OpenPanel<PopupKitchenMissionStart>();
        }

        private void OnClickBubble()
        {
            PanelManager.Instance.OpenPanel<PopupKitchenMissionPack>();
        }

        public override async UniTask<bool> ProcessTask()
        {
            if (KitchenMissionService.Instance.isOver.BoolValue && KitchenMissionService.Instance.isJoined.BoolValue)
            {
                MySonatFramework.GetService<AudioService>().PlaySound(Sonat.Enums.AudioId.ButtonClick);

                var popup = PanelManager.Instance.OpenPanel<PopupKitchenMission>();

                await UniTask.WaitUntil(() => popup == null);

                await UniTask.WaitForSeconds(0.75f);

                return true;
            }

            if (!KitchenMissionService.Instance.isUnlocked.BoolValue
                || KitchenMissionService.Instance.isCompleteEvent.BoolValue
                || !KitchenMissionService.Instance.IsInTimeEvent())
                return false;

            if (!KitchenMissionService.Instance.isShownPopup.BoolValue)
            {
                KitchenMissionService.Instance.isShownPopup.BoolValue = true;

                MySonatFramework.GetService<AudioService>().PlaySound(Sonat.Enums.AudioId.ButtonClick);
                var popup = PanelManager.Instance.OpenPanel<PopupKitchenMissionStart>();

                await UniTask.WaitUntil(() => popup == null);

                await UniTask.WaitForSeconds(0.75f);

                return true;
            }

            return false;
        }
    }
}