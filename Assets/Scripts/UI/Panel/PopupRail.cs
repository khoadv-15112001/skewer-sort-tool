using Cysharp.Threading.Tasks;
using DG.Tweening;
using SonatFramework.Scripts.UIModule;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace GrillSort.Rail
{
    public class PopupRail : Panel
    {
        [SerializeField] private Button tutBtn;
        [SerializeField] private Button closeBtn;

        [SerializeField] private UIRailController railController;
        [SerializeField] private RailMusicManager musicManager;

        public static int ActivePopupCount = 0;

        public override void OnSetup()
        {
            base.OnSetup();

            // Tăng counter (để tracking và tránh deactivate event khi user đang nhận quà)
            //ActivePopupCount++;

            SetupButton();
            SetupMusic();
            SetupRail();
        }

        private void SetupMusic()
        {
            if (musicManager != null)
            {
                bool isJoined = RailService.Instance.IsJoinEvent();
                musicManager.Setup(isJoined);
            }
        }

        private void SetupButton()
        {
            tutBtn.onClick.AddListener(OnClickTut);
            closeBtn.onClick.AddListener(OnClickClose);
        }

        private void OnClickTut()
        {
            PanelManager.Instance.OpenPanelByName<BasePanel>("PopupRailTut");
        }

        private void OnClickClose()
        {
            OnClickCloseAsync().Forget();
        }

        private async UniTask OnClickCloseAsync()
        {
            // Kiểm tra và mở PopupRailReward nếu có rewards (TRƯỚC khi đóng popup)
            await ShowRewardPopupIfNeeded();

            //// Delay một chút
            //await UniTask.Delay(300);

            // Đóng popup
            Close();
        }

        private void SetupRail()
        {
            railController.Setup(closeBtn, musicManager, OnStageComplete);
        }

        public override void Close()
        {
            // Stop music trước khi đóng popup
            if (musicManager != null)
            {
                musicManager.Stop();
            }

            // Giảm counter khi popup đóng
            ActivePopupCount--;
            if (ActivePopupCount < 0) ActivePopupCount = 0; // Safety check

            base.Close();
        }

        private void OnStageComplete()
        {
            OnStageCompleteAsync().Forget();
        }

        private async UniTask OnStageCompleteAsync()
        {
            await ShowRewardPopupIfNeeded();

            await UniTask.Delay(300);

            bool hasNextStage = !RailService.Instance.IsEventCompleted();

            if (hasNextStage)
            {
                ActivePopupCount++; // Reserve cho popup tiếp theo
            }

            Close();

            // Delay một chút nữa
            await UniTask.Delay(500);

            if (!hasNextStage)
            {
                // Đã hoàn thành hết event - không mở lại popup
                Debug.Log("[PopupRail] Event completed! All stages finished.");
                return;
            }

            PanelManager.Instance.OpenPanel<PopupRail>();
        }

        private async UniTask<bool> ShowRewardPopupIfNeeded()
        {
            if (!RailService.Instance.HasAccumulatedRewards())
                return false;

            // Lấy rewards đã tích lũy
            var rewardData = RailService.Instance.AccumulatedRewards;

            // Clear accumulated rewards
            RailService.Instance.ClearAccumulatedRewards();

            // Tạo UIData cho PopupRailReward
            var uiData = new UIData();
            uiData.Add("Reward", rewardData);
            //uiData.Add("Title", "Rail Reward");

            // Tạo TaskCompletionSource để đợi popup đóng
            //var tcs = new UniTaskCompletionSource();
            //uiData.Add("onClaimComplete", new System.Action(() =>
            //{
            //    tcs.TrySetResult();
            //}));

            // Mở PopupRailReward
            var popup = PanelManager.Instance.OpenPanelByName<PopupReward>("PopupRailReward", uiData);

            // Đợi popup đóng
            await UniTask.WaitUntil(() => popup == null);

            return true;
        }
    }
}