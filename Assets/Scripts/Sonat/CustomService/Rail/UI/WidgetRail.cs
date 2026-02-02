using Cysharp.Threading.Tasks;
using Sonat;
using SonatFramework.Scripts.UIModule;
using SonatFramework.Scripts.Utils;
using SonatFramework.Systems.EventBus;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace GrillSort.Rail
{
    public class WidgetRail : UIHomeWidget
    {
        [SerializeField] private Button btn;
        [SerializeField] private GameObject noti;

        private EventBinding<RailEventCompletedEvent> railEventCompletedBinding;

        public override void Setup()
        {
            // Lắng nghe event hoàn thành Rail
            railEventCompletedBinding = new EventBinding<RailEventCompletedEvent>(OnRailEventCompleted);

            // Kiểm tra xem event đã hoàn thành chưa hoặc chưa active
            if (!RailService.Instance.IsActiveEvent() || RailService.Instance.IsEventCompleted())
            {
                Hide();
                return;
            }

            SetupButton();
            UpdateNoti();

            // Subscribe vào OnTick để update noti real-time (cho free token cooldown)
            RailService.OnTick += UpdateNoti;
        }

        private void SetupButton()
        {
            btn.onClick.AddListener(() =>
            {
                PanelManager.Instance.OpenPanel<PopupRail>();
            });
        }

        private void OnRailEventCompleted(RailEventCompletedEvent evt)
        {
            Hide();
        }

        private void Hide()
        {
            active = false;
            gameObject.SetActive(false);
        }

        private void UpdateNoti()
        {
            // Check nếu event đã deactivate → hide widget
            if (!RailService.Instance.IsActiveEvent())
            {
                Hide();
                return;
            }

            if (noti == null) return;

            bool shouldShowNoti = false;

            // Case 1: Chưa join → luôn hiện noti
            if (!RailService.Instance.IsJoinEvent())
            {
                shouldShowNoti = true;
            }
            // Case 2: Đã join → check điều kiện
            else
            {
                // Hiện noti nếu pass milestone hoặc có thể claim free token
                bool canPassMilestone = RailService.Instance.IsPassCurrentMilestone();
                bool canClaimFree = RailService.Instance.CanClaimFreeToken();

                shouldShowNoti = canPassMilestone || canClaimFree;
            }

            noti.SetActive(shouldShowNoti);
        }

        public override void OnFocus()
        {
            // Update noti khi focus lại về home
            UpdateNoti();
        }

        public override void OnLoseFocus()
        {
            // Có thể cleanup khi lose focus
        }

        private void OnDestroy()
        {
            if (railEventCompletedBinding != null)
            {
                EventBus<RailEventCompletedEvent>.Deregister(railEventCompletedBinding);
            }

            // Unsubscribe từ OnTick
            RailService.OnTick -= UpdateNoti;
        }

        public override async UniTask<bool> ProcessTask()
        {
            // Kiểm tra xem Rail đã unlock và active chưa
            if (!RailService.Instance.IsUnlocked.BoolValue) return false;
            if (!RailService.Instance.IsActiveEvent()) return false;
            if (RailService.Instance.IsEventCompleted()) return false;

            // Case 1: User đã pass milestone hiện tại → auto mở popup để chạy animation
            if (RailService.Instance.IsJoinEvent() && RailService.Instance.IsPassCurrentMilestone())
            {
                PopupRail.ActivePopupCount++;
                PanelManager.Instance.OpenPanel<PopupRail>();

                // Tracking
                MySonatFramework.customTrackingService.OnShowPopup(
                    gameObject.name.ToLogString(),
                    "pop_up",
                    "non_iap","auto"
                );

                // Đợi cho đến khi tất cả PopupRail đã đóng (counter về 0)
                // Bao gồm cả popup ban đầu và popup thứ 2 (nếu complete stage)
                await UniTask.WaitUntil(() => PopupRail.ActivePopupCount == 0);

                await UniTask.Delay(750);
                return true;
            }

            // Case 2: Lần đầu unlock → show popup giới thiệu
            if (!RailService.Instance.HasShownFirstPopup.BoolValue)
            {
                // Đánh dấu đã show lần đầu
                RailService.Instance.HasShownFirstPopup.BoolValue = true;

                // Mở PopupRail
                var popup = PanelManager.Instance.OpenPanel<PopupRail>();

                // Tracking
                MySonatFramework.customTrackingService.OnShowPopup(
                    gameObject.name.ToLogString(),
                    "pop_up",
                    "non_iap",
                    "auto"
                );

                // Đợi popup đóng (không cần counter vì case này không reopen)
                await UniTask.WaitUntil(() => popup == null || !popup.gameObject.activeInHierarchy);
                await UniTask.Delay(750);

                return true;
            }

            return false;
        }
    }
}