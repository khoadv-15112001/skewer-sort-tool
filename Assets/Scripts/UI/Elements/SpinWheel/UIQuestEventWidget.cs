using System;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using I2.Loc;
using SonatFramework.Scripts.UIModule;
using SonatFramework.Scripts.UIModule.UIElements;
using SonatFramework.Scripts.Utils;
using SonatFramework.Systems;
using SonatFramework.Systems.TimeManagement;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace GrillSort.QuestEvent
{
    public class UIQuestEventWidget : UIHomeWidget
    {
        [SerializeField] private UIReceiveQuestItem uiReceiveQuestItem;
        [SerializeField] private UIQuestProgress uiQuestProgress;
        [SerializeField] private LocalizationParamsManager txtUnlockAtLevel;
        [SerializeField] private GameObject pTxtQuest;
        [SerializeField] private GameObject imgReward;
        [SerializeField] private GameObject imgLock;

        [SerializeField] private GameObject noti;
        private readonly Service<QuestEventService> questEventService = new();
        [SerializeField] private UITimeCounter uiTimeCounter;
        private bool isOpen = false;

        public override void Setup()
        {
            if (questEventService.Instance.IsUnlocked() == false)
            {
                if (questEventService.Instance.CheckLiveOpsCondition() == false)
                {
                    gameObject.SetActive(false);
                    return;
                }
                else if (questEventService.Instance.CanUnlock() == false)
                {
                    isOpen = false;
                    txtUnlockAtLevel.gameObject.SetActive(true);
                    uiTimeCounter.gameObject.SetActive(false);

                    noti.SetActive(false);
                    pTxtQuest.SetActive(false);
                    txtUnlockAtLevel.SetParameterValue("VALUE", questEventService.Instance.config.unlockedLevel.ToString());
                    imgLock.SetActive(true);
                    imgReward.SetActive(false);
                    return;
                }

                questEventService.Instance.Unlock();
            }


            // noti.SetActive(false);
            // txtUnlockAtLevel.gameObject.SetActive(false);
            // pTxtQuest.SetActive(true);
            // imgLock.SetActive(false);
            // imgReward.SetActive(true);

            questEventService.Instance.OnReceiveReward += HideNoti;
            questEventService.Instance.OnFinishQuestEvent += OnFinishEvent;
            forceOpen = true;

            isOpen = questEventService.Instance.CheckOpen();
            if (isOpen)
            {
                SetOpen();
            }
            else
            {
                SetClose();
            }
        }


        private void SetOpen()
        {
            isOpen = true;
            uiTimeCounter.gameObject.SetActive(false);

            noti.SetActive(false);
            txtUnlockAtLevel.gameObject.SetActive(false);
            pTxtQuest.SetActive(true);
            imgLock.SetActive(false);
            imgReward.SetActive(true);
        }

        private void SetClose()
        {
            long timeToOpen = GetTimeToNextWeekend();
            uiTimeCounter.gameObject.SetActive(true);
            uiTimeCounter.SetData(timeToOpen, SetOpen);

            txtUnlockAtLevel.gameObject.SetActive(false);
            noti.SetActive(false);
            pTxtQuest.SetActive(false);
            txtUnlockAtLevel.SetParameterValue("VALUE", questEventService.Instance.config.unlockedLevel.ToString());
            imgLock.SetActive(true);
            imgReward.SetActive(false);
        }

        private void OnFinishEvent()
        {
            isOpen = false;
            SetClose();
        }

        private void OnDestroy()
        {
            questEventService.Instance.OnReceiveReward -= HideNoti;
            questEventService.Instance.OnFinishQuestEvent -= OnFinishEvent;
        }

        private void HideNoti()
        {
            noti.SetActive(false);
        }

        public override void OnFocus()
        {
            UpdateUI();
        }

        public override void OnLoseFocus()
        {
        }

        public override async UniTask<bool> ProcessTask()
        {
            if (questEventService.Instance.IsUnlocked() == false) return false;

            // effect collect
            var numCollectedItem = questEventService.Instance.NumCollectedItemInGame;
            if (numCollectedItem > 0)
            {
                Collect(numCollectedItem);
            }

            var level = MySonatFramework.userDataService.GetLevel();
            var configLevel = questEventService.Instance.config.unlockedLevel;
            bool showPopup = false;
            if (level >= configLevel)
            {
                // Đến level thì hiện popup giới thiệu trước, sau đó lần tiếp theo hiện popup quest event
                var showPopup1 = await TryShowIntroPopup();

                // Nếu hoàn thành quest thì hiện popup để nhận
                var showPopup2 = await TryShowPopupWhenCompleteQuest(numCollectedItem > 0);
                showPopup = showPopup1 || showPopup2;
            }

            return showPopup;
        }

        private async UniTask<bool> TryShowPopupWhenCompleteQuest(bool waitCollect)
        {
            if (questEventService.Instance.CheckCanClaimReward())
            {
                noti.SetActive(true);
                if (waitCollect)
                    await UniTask.Delay(3000);
                var popup = PanelManager.Instance.OpenPanel<PopupQuestEvent>();
                MySonatFramework.customTrackingService.OnShowPopup(gameObject.name.ToLogString(), "pop_up", "non_iap", "auto");
                await UniTask.WaitUntil(() => popup == null || !popup.gameObject.activeInHierarchy);
                await UniTask.Delay(750);
                return true;
            }

            return false;
        }

        private async UniTask<bool> TryShowIntroPopup()
        {
            if (PlayerPrefs.HasKey("AppearPopupGoldenSlice") == false && isOpen)
            {
                PlayerPrefs.SetInt("AppearPopupGoldenSlice", 1);
                var uiData = new UIData();
                uiData.Add("hideButtonPlay", true);
                var popup = PanelManager.Instance.OpenPanel<PopupGoldenSlice>(uiData);
                await UniTask.WaitUntil(() => popup == null || !popup.gameObject.activeInHierarchy);
                await UniTask.Delay(100);
                var popup2 = PanelManager.Instance.GetPanelByName<BasePanel>("PopupTutQuestEvent");
                await UniTask.WaitUntil(() => popup2 == null || !popup2.gameObject.activeInHierarchy);
                await UniTask.Delay(750);
                return true;
            }

            return false;
        }

        public void OnClick()
        {
            var level = MySonatFramework.userDataService.GetLevel();
            var configLevel = questEventService.Instance.config.unlockedLevel;
            if (level >= configLevel)
            {
                if (!isOpen)
                {
                    PopupToast.Cretate($"Open on weekend!");
                    return;
                }

                PanelManager.Instance.OpenPanel<PopupQuestEvent>();
                MySonatFramework.customTrackingService.OnShowPopup(gameObject.name.ToLogString(), "widget", "non_iap", "user");
            }
            else
            {
                PopupToast.Cretate($"Unlock at level:", configLevel.ToString());
            }
        }

        private void UpdateUI(QuestEventData data = null)
        {
        }

        private void Collect(int numCollectedItem = 10)
        {
            uiReceiveQuestItem.Collect(numCollectedItem, () => { questEventService.Instance.AddItemCollectIngame(); });
            // SonatUtils.DelayCall(1.2f, () => uiQuestProgress.UpdateProgress(numCollectedItem), this);
        }

        private long GetTimeToNextWeekend()
        {
            var now = MySonatFramework.GetService<SonatTimeService>().GetCurrentTime();

            int daysUntilSaturday = ((int)DayOfWeek.Saturday - (int)now.DayOfWeek + 7) % 7;

            if (daysUntilSaturday == 0)
            {
                daysUntilSaturday = 7;
            }

            DateTime nextSaturday = now.Date.AddDays(daysUntilSaturday);

            //DateTime nextNextSaturdayMidnight = nextSaturday.AddDays(7);

            TimeSpan timeRemaining = nextSaturday - now;

            return (long)timeRemaining.TotalSeconds;
        }
    }
}