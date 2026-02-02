using GrillSort.ConsecutiveWin;
using I2.Loc;
using Sonat.Enums;
using SonatFramework.Scripts.UIModule;
using SonatFramework.Scripts.Utils;
using SonatFramework.Systems.SceneManagement;
using SonatFramework.Systems.UserData;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace GrillSort.Story
{
    public class PopupNewStory : Panel
    {
        [SerializeField] private Button closeBtn;
        [SerializeField] private Button continueBtn;

        [SerializeField] private Slider slider;
        [SerializeField] private TMP_Text sliderTxt;
        [SerializeField] private LocalizationParamsManager levelParam;

        public override void OnSetup()
        {
            base.OnSetup();

            closeBtn.onClick.AddListener(Close);
            continueBtn.onClick.AddListener(CheckLives);

            SetupVisual();
        }

        private void SetupVisual()
        {
            var curStory = StoryService.Instance.GetCurrentStory();
            bool hasStory = true;

            if (curStory == null)
            {
                curStory = StoryService.Instance.GetStoryBaseByID(StoryBase.EStory.Story_1);
                hasStory = false;
            }

            int curLevel = MySonatFramework.userDataService.GetLevel();
            int startLevel;
            int endLevel;

            if (!hasStory)
            {
                // Story đầu tiên
                startLevel = curStory.GetLevelUnlock() - curStory.GetData().LevelReached;
                endLevel = curStory.GetLevelUnlock();

                if (startLevel != 0) startLevel--;
            }
            else
            {
                var nextStory = StoryService.Instance.GetStoryBaseByID(curStory.GetData().Id + 1);
                if (nextStory != null)
                {
                    startLevel = curStory.GetLevelUnlock();
                    endLevel = nextStory.GetLevelUnlock();
                }
                else
                {
                    // Story cuối cùng
                    startLevel = curStory.GetLevelUnlock();
                    endLevel = startLevel + curStory.GetData().LevelReached;
                }
            }

            startLevel++;
            endLevel++;

            var total = endLevel - startLevel;
            var progress = curLevel - startLevel;

            //int total = endLevel - startLevel;
            //int progress = Mathf.Clamp(curLevel - startLevel, 0, total);

            //if (curLevel >= endLevel)
            //    progress = total;

            slider.value = total > 0 ? (progress * 1f / total) : 0f;
            sliderTxt.text = $"{progress}/{total}";
            levelParam.SetParameterValue("value", (endLevel - 1).ToString());

            //Debug.LogError($"Start: {startLevel}");
            //Debug.LogError($"End: {endLevel}");
            //Debug.LogError($"Cur: {curLevel}");
            //Debug.LogError($"Progress: {progress}/{total}");
        }

        private void CheckLives()
        {
            if (MySonatFramework.livesService.CanPlay())
            {
                var consecutiveWinService = MySonatFramework.GetService<ConsecutiveWinService>();
                var level = MySonatFramework.GetService<UserDataService>().GetLevel();
                if (consecutiveWinService.CheckStart(level))
                {
                    UIData uiData = new UIData();
                    uiData.Add("OnPlay", (Action)(() => SonatUtils.DelayCall(0.1f, PlayGame)));
                    PanelManager.Instance.OpenPanel<PopupPrePlay>(uiData);
                }
                else
                {
                    PlayGame();
                }
            }
            else
            {
                PanelManager.Instance.OpenPanel<PopupRefillLives>();
                PopupToast.Cretate("No more lives left!");
            }
        }

        private void PlayGame()
        {
            LoadingScreenInstance.Instance.Show(3.5f);
            SonatUtils.DelayCall(0.5f, () => { MySonatFramework.GetService<SceneService>().SwitchScene(GamePlacement.Gameplay); });
        }
    }
}