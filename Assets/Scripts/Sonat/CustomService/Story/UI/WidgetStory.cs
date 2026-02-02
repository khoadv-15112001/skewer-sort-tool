using Cysharp.Threading.Tasks;
using SonatFramework.Scripts.UIModule;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace GrillSort.Story
{
    public class WidgetStory : UIHomeWidget
    {
        [SerializeField] private Button btn;
        [SerializeField] private GameObject noti;

        public override void Setup()
        {
            SetupButton();
            CheckNoti();

            CheckAvailable();

            StoryService.OnChangeStory += CheckNoti;
            StoryService.OnChangeStory += CheckAvailable;
        }

        private void OnDestroy()
        {

            StoryService.OnChangeStory -= CheckNoti;
            StoryService.OnChangeStory -= CheckAvailable;
        }

        private void SetupButton()
        {
            btn.onClick.AddListener(() =>
            {
                StoryService.Instance.IsNoti.BoolValue = false;
                CheckNoti();
                PanelManager.Instance.OpenPanel<PopupNewStory>();
            });
        }

        private void CheckAvailable()
        {
            gameObject.SetActive(StoryService.Instance.IsAvailableStoryLocked());
        }

        private void CheckNoti()
        {
            if (noti == null) return;
            noti.SetActive(StoryService.Instance.IsNoti.BoolValue);
        }

        public override async UniTask<bool> ProcessTask()
        {
            var story = StoryService.Instance.GetStoryAvailableUnlock();

            if (story == null) return false;

            await UniTask.WaitForSeconds(0.75f);

            var uiData = new UIData();
            uiData.Add("story", story);
            var storyCard = PanelManager.Instance.OpenPanel<PopupStoryCard>(uiData);

            await UniTask.WaitUntil(() => storyCard == null);

            StoryService.Instance.MoveToStory(story);

            MySonatFramework.inventoryService.AddReward(story.GetData().RewardData, new() { spendType = "feature", spendId = "story" });

            var video = PanelManager.Instance.GetPanel<PopupVideoStory>();

            await UniTask.WaitUntil(() => video == null);

            TabHome.OnSlideIn?.Invoke();

            await UniTask.WaitForSeconds(0.75f);

            var uiDataReward = new UIData().Add("Reward", story.GetData().RewardData).Add("Title", story.GetData().Name);
            var popupReward = PanelManager.Instance.OpenPanel<PopupReward>(uiDataReward);

            await UniTask.WaitUntil(() => popupReward == null);
            
            //check show popup rate
            if (ShouldShowPopupRate())
            {
                const string AUTO_POPUP_RATE_SHOWED_KEY = "ShowRatePopup";

                var popupRate = PanelManager.Instance.OpenPanel<PopupRate>();
                await UniTask.WaitUntil(() => popupRate == null);
                PlayerPrefs.SetInt(AUTO_POPUP_RATE_SHOWED_KEY, 1);
                PlayerPrefs.Save();
            }

            return true;
        }

        private bool ShouldShowPopupRate()
        {
            const string AUTO_POPUP_RATE_SHOWED_KEY = "ShowRatePopup";

            return PlayerPrefs.GetInt(AUTO_POPUP_RATE_SHOWED_KEY, 0) == 0;
        }
    }
}