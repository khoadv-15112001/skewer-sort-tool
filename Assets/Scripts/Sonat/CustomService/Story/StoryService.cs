using Cysharp.Threading.Tasks;
using Sirenix.OdinInspector;
using Sonat.Enums;
using SonatFramework.Scripts.Helper;
using SonatFramework.Systems;
using SonatFramework.Systems.UserData;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GrillSort.Story
{
    [CreateAssetMenu(fileName = "StoryService", menuName = "My Services/StoryService")]
    public class StoryService : SonatServiceSo, IServiceInitializeAsync
    {
        public static StoryService Instance => SonatSystem.GetService<StoryService>();

        [SerializeField] private StoryServer storyServer;
        [SerializeField] private List<StoryBase> stories;
        [SerializeField] private List<StoryBase> stories_jp;
        //[SerializeField] private AudioClip backgroundHomeMusicDefault;

        private List<StoryBase> storiesActive = new List<StoryBase>();
        public List<StoryBase> ActiveStories
        {
            get
            {
                // Nếu storyServer null thì trả về list rỗng để tránh lỗi null reference
                if (storyServer == null)
                {
                    storiesActive = new();
                }

                // Nếu danh sách chưa khởi tạo hoặc rỗng thì build lại
                if (storiesActive == null || storiesActive.Count == 0)
                {
                    var sourceList = LocalizationUtils.IsJapanese() ? stories_jp : stories;
                    storiesActive = sourceList.FindAll(story => !story.GetData().IsComingSoon);

                    Debug.Log($"[StoryService] ActiveStories count: {storiesActive.Count}");

                }

                return storiesActive;
            }
        }

        public List<StoryBase> AllStories
        {
            get
            {
                return LocalizationUtils.IsJapanese() ? stories_jp : stories;
            }
        }

        public IntDataPref CurrentStoryIndex;
        public IntDataPref MaxLevelStory;
        public IntDataPref IsNoti;
        public IntDataPref LevelInit;

        public static Action OnChangeStory;

        [ShowInInspector]
        [Button("Refresh ActiveStories")]
        private void RefreshActiveStories()
        {
            _ = ActiveStories;
        }

        public async UniTaskVoid InitializeAsync()
        {
            CurrentStoryIndex = new IntDataPref("story_current_story_index");

            if(CurrentStoryIndex.Value >= ((int)StoryBase.EStory.MAX))
                CurrentStoryIndex.Value = ((int)StoryBase.EStory.MAX) - 1;

            MaxLevelStory = new IntDataPref("story_max_level_story");
            IsNoti = new IntDataPref("story_is_noti", 1);
            LevelInit = new IntDataPref("story_level_init", SonatSystem.GetService<UserDataService>().GetLevel());

            Debug.Log($"[StoryService] Init story");

            await UniTask.Yield();

            RefreshActiveStories();

            int maxLevelStory = 0;
            
            foreach (var story in ActiveStories)
            {
                Debug.Log($"[StoryService] Init story {story.GetData().Id}");
                story.Init();
                maxLevelStory = story.GetData().LevelReached;
            }

            MaxLevelStory.Value = maxLevelStory;

            TryPreDownload();

            await UniTask.Yield();
        }

        //public int GetMaxLevelStory(int value)
        //{
        //    if (MaxLevelStory.Value < value)
        //    {
        //        MaxLevelStory.Value = value;
        //    }

        //    return MaxLevelStory.Value;
        //}

        //private int InitLevelStart()
        //{
        //    var curLevel = SonatSystem.GetService<UserDataService>().GetLevel();

        //    return curLevel == 1 ? 0 : curLevel;
        //}

        private void TryPreDownload()
        {
            List<int> indexs = new();

            for (int i = 1; i <= CurrentStoryIndex.Value + 1; i++)
            {
                indexs.Add(i);
                TryDownload((StoryBase.EStory)i);
            }

            foreach (var story in ActiveStories)
            {
                if (MySonatFramework.GetService<UserDataService>().GetLevel() > story.GetLevelUnlock())
                {
                    if (!indexs.Contains(((int)story.GetData().Id)))
                        TryDownload(story.GetData().Id);
                }
            }
        }

        private void TryDownloadNextStory()
        {
            TryDownload((StoryBase.EStory)CurrentStoryIndex.Value + 1);
        }

        public void TryDownload(StoryBase.EStory eStory)
        {
            if(eStory >= StoryBase.EStory.MAX) return;
            StoryBase story = GetStoryBaseByID(eStory);
            if (story == null) return;

            storyServer.DownloadVideoAsync(story.GetFileNameVideo()).Forget();
        }

        public StoryBase GetCurrentStory()
        {
            return GetStoryBaseByID((StoryBase.EStory)CurrentStoryIndex.Value);
        }

        public StoryBase GetStoryBaseByID(StoryBase.EStory eStory)
        {
            var story = ActiveStories.Find(x => x.GetData().Id == eStory);
            //if (story == null)
            //    Debug.LogError($"[StoryService] Không tìm thấy StoryBase có Id = {eStory}");
            return story;
        }

        public async UniTask PlayVideoByID(StoryBase.EStory eStory, Action onFailed = null)
        {
            var localPath = await GetLocalPathVideoByID(eStory);
            if (string.IsNullOrEmpty(localPath))
            {
                onFailed?.Invoke();
                Debug.LogError($"[StoryService] Không tìm thấy video cho story {eStory}");
                return;
            }

            PopupVideoStory.PlayVideo(localPath);
        }

        public async UniTask<string> GetLocalPathVideoByID(StoryBase.EStory eStory)
        {
            var story = GetStoryBaseByID(eStory);
            if (story == null) return null;

            return await storyServer.GetLocalVideoPath(story.GetFileNameVideo());
        }

        #region Cheating

        public async UniTask PlayVideoByID_Cheating(StoryBase.EStory eStory)
        {
            var localPath = await GetLocalPathVideoByID_Cheating(eStory);
            if (string.IsNullOrEmpty(localPath))
            {
                Debug.LogError($"[StoryService] Không tìm thấy video cho story {eStory}");
                return;
            }

            PopupVideoStory.PlayVideo(localPath);
        }

        private async UniTask<string> GetLocalPathVideoByID_Cheating(StoryBase.EStory eStory)
        {
            var story = GetStoryBaseByID(eStory);
            if (story == null) return null;

            return await storyServer.GetLocalVideoPath(story.GetFileNameVideo(), true);
        }

        #endregion

        public int GetLevelUnlockStory(StoryBase story)
        {
            if (story == null)
            {
                Debug.LogError("[StoryService] GetLevelUnlockStory: story is NULL!");
                return -1;
            }
            return story.GetLevelUnlock();
        }

        public bool IsUnlockedStory(StoryBase story)
        {
            if (story == null)
            {
                Debug.LogError("[StoryService] IsUnlockedStory: story is NULL!");
                return false;
            }

            if (story.GetData().IsComingSoon) return false;
            return MySonatFramework.userDataService.GetLevel() > GetLevelUnlockStory(story);
        }

        public StoryBase GetStoryAvailableUnlock()
        {
            foreach (var story in ActiveStories)
            {
                if (MySonatFramework.userDataService.GetLevel() == story.GetLevelUnlock() + 1 && CurrentStoryIndex.Value != ((int)story.GetData().Id))
                    return story;
            }

            return null;
        }

        public void MoveToStory(StoryBase story)
        {
            CurrentStoryIndex.Value = ((int)story.GetData().Id);

            //PlayBackgroundHomeMusic();

            TryDownloadNextStory();

            IsNoti.BoolValue = true;

            OnChangeStory?.Invoke();
        }

        //public AudioClip GetBackgroundHomeMusic()
        //{
        //    foreach (var story in ActiveStories)
        //    {
        //        if (((int)story.GetData().Id) == CurrentStoryIndex.Value)
        //        {
        //            return story.GetData().BackgroundHomeMusic;
        //        }
        //    }

        //    return backgroundHomeMusicDefault;
        //}

        //public void PlayBackgroundHomeMusic()
        //{
        //    MySonatFramework.audioService.PlayAudio("", GetBackgroundHomeMusic(), 0.5f, SonatFramework.Systems.AudioManagement.AudioTracks.Music);
        //}

        public bool IsAvailableStoryLocked()
        {
            foreach (var story in ActiveStories)
            {
                if (CurrentStoryIndex.Value < ((int)story.GetData().Id))
                    return true;
            }

            return false;
        }

        [Button]
        public void Test(StoryBase.EStory eStory)
        {
            PlayVideoByID(eStory);
        }
    }
}