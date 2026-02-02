using Cysharp.Threading.Tasks;
using Sirenix.OdinInspector;
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

namespace GrillSort.DailyMission
{
    public class WidgetDailyMission : UIHomeWidget
    {
        [SerializeField] private Button btn;
        [SerializeField] private Image icon;
        [SerializeField] private GameObject noti;
        [SerializeField] private Button bubbleBtn;
        [SerializeField] private GameObject x2Tag;
        [SerializeField] private TMP_Text x2TimeTxt;

        public override void Setup()
        {
            base.Setup();

            if (!DailyMissionService.Instance.isUnlocked.BoolValue)
            {
                gameObject.SetActive(false);
                return;
            }

            SetupButton();
            CheckShowNoti();
            CheckIcon();
            CheckShowBubble();

            DailyMissionService.OnChangeMission += CheckShowNoti;
            DailyMissionService.OnClaimStage += CheckShowNoti;
            DailyMissionService.OnChangeItem += CheckShowNoti;
            DailyMissionService.OnShowTut += CheckShowNoti;

            DailyMissionService.OnChangeMission += CheckIcon;

            DailyMissionService.OnChangeMission += CheckShowBubble;
            DailyMissionService.OnBuyX2 += CheckShowBubble;
            DailyMissionService.OnExpiredX2 += CheckShowBubble;

            DailyMissionService.OnTick += Tick;
        }

        private void OnDestroy()
        {
            DailyMissionService.OnChangeMission -= CheckShowNoti;
            DailyMissionService.OnClaimStage -= CheckShowNoti;
            DailyMissionService.OnChangeItem -= CheckShowNoti;
            DailyMissionService.OnShowTut -= CheckShowNoti;

            DailyMissionService.OnChangeMission -= CheckIcon;

            DailyMissionService.OnChangeMission -= CheckShowBubble;
            DailyMissionService.OnBuyX2 -= CheckShowBubble;
            DailyMissionService.OnExpiredX2 -= CheckShowBubble;

            DailyMissionService.OnTick -= Tick;
        }

        private void SetupButton()
        {
            btn.onClick.AddListener(OnClick);
            bubbleBtn.onClick.AddListener(OnClickBubble);
        }

        private void CheckIcon()
        {
            icon.sprite = DailyMissionService.Instance.GetCurrentDailyMission().widgetSprite;
        }

        private void OnClick()
        {
            MySonatFramework.customTrackingService.OnShowPopup(gameObject.name.ToLogString(), "widget", "non_iap", "user");
            PanelManager.Instance.OpenPanelByName<PopupDailyMission>(DailyMissionService.Instance.GetCurrentDailyMission().NamePopup);
        }

        private void OnClickBubble()
        {
            PanelManager.Instance.OpenPanel<PopupDailyMissionX2>();
        }

        private void CheckShowNoti()
        {
            noti.SetActive(DailyMissionService.Instance.CanNoti());
        }

        private void CheckShowBubble()
        {
            bubbleBtn.gameObject.SetActive(DailyMissionService.Instance.CanShowBubbleX2());
            x2Tag.gameObject.SetActive(DailyMissionService.Instance.isX2.BoolValue);
            x2TimeTxt.text = SonatUtils.GetTimeByFormat(DailyMissionService.Instance.timeEndX2.Value - DailyMissionService.Instance.GetTimeUnixNow());
        }

        private void Tick()
        {
            if (!DailyMissionService.Instance.isX2.BoolValue) return;

            x2TimeTxt.text = SonatUtils.GetTimeByFormat(DailyMissionService.Instance.timeEndX2.Value - DailyMissionService.Instance.GetTimeUnixNow());
        }

        [Button]
        public void TestCompleteMission()
        {
            DailyMissionService.Instance.CompleteMission();
        }
    }
}

