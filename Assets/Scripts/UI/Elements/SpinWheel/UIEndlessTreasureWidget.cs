using System;
using Cysharp.Threading.Tasks;
using SonatFramework.Scripts.UIModule;
using SonatFramework.Scripts.UIModule.UIElements;
using SonatFramework.Scripts.Utils;
using SonatFramework.Systems;
using SonatFramework.Systems.UserData;
using UnityEngine;

namespace GrillSort.EndlessTreasure
{
    public class UIEndlessTreasureWidget : UIHomeWidget
    {
        [SerializeField] private UITimeCounter timeCounter;

        private bool showPopup = false;
        private readonly Service<EndlessTreasureService> endlessTreasureService = new();
        public override void Setup()
        {
            base.Setup();
            active = active && endlessTreasureService.Instance.IsUnlocked();
            gameObject.SetActive(active);

            if(!active) return;
            if (endlessTreasureService.Instance.CanUnlock())
            {
                endlessTreasureService.Instance.Unlock();
            }


            showPopup = CheckShowPopup();

            timeCounter.SetData(endlessTreasureService.Instance.GetTimeToReset(), null);

            endlessTreasureService.Instance.OnChangeData += UpdateUI;
        }

        private void OnDisable()
        {
            endlessTreasureService.Instance.OnChangeData -= UpdateUI;
        }

        private void UpdateUI(bool obj)
        {
            timeCounter.SetData(endlessTreasureService.Instance.GetTimeToReset(), null);
        }

        private bool CheckShowPopup()
        {
            return false;
        }

        public override void OnFocus()
        {

        }

        public override void OnLoseFocus()
        {

        }

        // Chỉ hiện ở phiên đầu tiên trong ngày
        public override async UniTask<bool> ProcessTask()
        {
            if (showPopup)
            {
                var popup = PanelManager.Instance.OpenPanelByName<BasePanel>("EndlessTreasurePanel");
                MySonatFramework.customTrackingService.OnShowPopup(gameObject.name.ToLogString(), "widget", "non_iap", "auto");
                await UniTask.WaitUntil(() => popup == null || !popup.gameObject.activeInHierarchy);
                await UniTask.Delay(750);
                return true;
            }
            return false;
        }

        public void OnClick()
        {
            PanelManager.Instance.OpenPanelByName<BasePanel>("EndlessTreasurePanel");
            MySonatFramework.customTrackingService.OnShowPopup(gameObject.name.ToLogString(), "widget", "non_iap", "user");
        }
    }
}