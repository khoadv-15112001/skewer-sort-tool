using Cysharp.Threading.Tasks;
using I2.Loc;
using SonatFramework.Scripts.UIModule;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace GrillSort.Story
{
    public class UIStoryStage : MonoBehaviour
    {
        [Header("Ribbon")]
        [SerializeField] private Image ribbonImg;
        [SerializeField] private Sprite ribbonUnlock;
        [SerializeField] private Sprite ribbonLock;

        [Space]

        [Header("Name")]
        [SerializeField] private Localize nameLocalize;

        [Space]

        [Header("Frame")]
        [SerializeField] private Image frameImg;
        [SerializeField] private Sprite frameUnlock;
        [SerializeField] private Sprite frameLock;

        [Space]

        [Header("Chapter")]
        [SerializeField] private Image chapterImg;
        [SerializeField] private Sprite chapterUnlock;
        [SerializeField] private Sprite chapterLock;
        [SerializeField] private LocalizationParamsManager chapterParam;

        [Space]

        [Header("Thumbnail")]
        [SerializeField] private Image thumbnailImg;
        [SerializeField] private RectTransform thumbnailTransform;
        [SerializeField] private Image thumbnailGrayImg;

        [Space]

        [Header("Lock")]
        [SerializeField] private GameObject lockObj;
        [SerializeField] private GameObject comingSoon;
        [SerializeField] private LocalizationParamsManager levelUnlockParam;

        [Header("Other")]
        [SerializeField] private Button viewBtn;
        [SerializeField] private GameObject lineObj;

        private StoryBase _storyData;
        private bool _isLast;
        private bool _isUnlocked;
        private bool _isComingSoon;

        public void BindData(StoryBase storyData, bool isLast)
        {
            _storyData = storyData;
            _isLast = isLast;
            _isComingSoon = storyData.GetData().IsComingSoon;
            _isUnlocked = StoryService.Instance.IsUnlockedStory(storyData) && !_storyData.GetData().IsComingSoon;

            Setup();
        }

        private void Setup()
        {
            SetupRibbon();
            SetupName();
            SetupFrame();
            SetupChapter();
            SetupThumbnail();
            SetupLock();
            SetupButton();
            SetupLine();
        }

        private void SetupRibbon()
        {
            ribbonImg.sprite = _isUnlocked ? ribbonUnlock : ribbonLock;
        }

        private void SetupName()
        {
            nameLocalize.SetTerm(_storyData.GetData().Name);
        }

        private void SetupFrame()
        {
            frameImg.sprite = _isUnlocked ? frameUnlock : frameLock;
        }

        private void SetupChapter()
        {
            chapterImg.sprite = _isUnlocked ? chapterUnlock : chapterLock;
            chapterParam.SetParameterValue("value", ((int)_storyData.GetData().Id).ToString());
        }

        private void SetupThumbnail()
        {
            thumbnailImg.gameObject.SetActive(_isUnlocked);
            thumbnailGrayImg.gameObject.SetActive(!_isUnlocked);

            StoryBase.Data data = _storyData.GetData();

            if (_isUnlocked)
            {
                thumbnailImg.sprite = data.ImageConfig.BackgroundHomeSprite;
                thumbnailTransform.localPosition = data.ImageConfig.ThumbnailConfig.Position;
                thumbnailTransform.localScale = Vector3.one * data.ImageConfig.ThumbnailConfig.Scale;
            }
            else
            {
                thumbnailGrayImg.sprite = data.ImageConfig.ThumbnailConfig.ThumbnailGraySprite;
            }
        }

        private void SetupLock()
        {
            lockObj.SetActive(!_isUnlocked);
            levelUnlockParam.gameObject.SetActive(!_isUnlocked);
            comingSoon.SetActive(_isComingSoon);

            if (!_isUnlocked)
            {
                if (!_isComingSoon)
                    levelUnlockParam.SetParameterValue("value", StoryService.Instance.GetLevelUnlockStory(_storyData).ToString());

                levelUnlockParam.gameObject.SetActive(!_isComingSoon);
            }
        }

        private void SetupButton()
        {
            viewBtn.gameObject.SetActive(_isUnlocked);

            if (_isUnlocked)
            {
                viewBtn.onClick.RemoveAllListeners();
                viewBtn.onClick.AddListener(async () => PlayStory());

                async UniTaskVoid PlayStory()
                {
                    try
                    {
                        var localPath = await StoryService.Instance.GetLocalPathVideoByID(_storyData.GetData().Id);

                        UIData uiData = new UIData();

                        uiData.Add(PopupVideoStory.UI_DATA_LOCAL_PATH, localPath);
                        uiData.Add(PopupVideoStory.UI_DATA_CAN_SKIP, true);

                        PanelManager.Instance.OpenPanel<PopupVideoStory>(uiData);
                    }
                    catch (System.Exception e)
                    {
                        Debug.LogError("ViewStory Exception: " + e.Message);
                    }

                }
            }
        }

        private void SetupLine()
        {
            lineObj.SetActive(!_isLast);
        }

    }
}