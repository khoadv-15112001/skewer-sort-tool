using Sirenix.OdinInspector;
using SonatFramework.Scripts.Helper;
using SonatFramework.Systems;
using SonatFramework.Systems.InventoryManagement.GameResources;
using SonatFramework.Systems.UserData;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GrillSort.Story
{
    public abstract class StoryBase : ScriptableObject, IStory
    {
        [SerializeField] protected Data data;

        public Data GetData() { return data; }

        protected IntDataPref LevelUnlock;

        protected static bool HaveNewStory;

        public virtual void Init()
        {
            LevelUnlock = new IntDataPref($"story_level_unlock_{(int)GetData().Id}", InitLevelUnlock());
        }

        private int InitLevelUnlock()
        {
            var curLevel = SonatSystem.GetService<UserDataService>().GetLevel();

            if (curLevel == 1) curLevel = 0;

            if (curLevel < data.LevelReached && !HaveNewStory)
                return data.LevelReached;
            else
            {
                HaveNewStory = true;
                return curLevel + (data.LevelReached - StoryService.Instance.MaxLevelStory.Value);
            }
        }

        public virtual string GetFileNameVideo()
        {
            return GetData().FileNameVideo;
        }

        public int GetLevelUnlock()
        {
            return LevelUnlock.Value;
        }

        [Serializable]
        public class Data
        {
            public EStory Id;
            public bool IsComingSoon;
            [ValueDropdown(nameof(GetAllTerms))]
            public string Name;
            public int LevelReached;
            public string FileNameVideo;
            public AudioClip BackgroundHomeMusic;
            public RewardData RewardData;

            public ConfigImage ImageConfig;

            public IEnumerable<string> GetAllTerms()
            {
                return LocalizationUtils.GetAllTerms();
            }

            [Serializable]
            public class ConfigImage
            {
                public Sprite BackgroundHomeSprite;
                public Sprite BackgroundIngameSprite;

                public Thumbnail ThumbnailConfig;
                [Range(-85f, 50f)]
                public float OffsetYBackgroundIngame;

                [Serializable]
                public class Thumbnail
                {
                    public Sprite ThumbnailGraySprite;

                    public Vector2 Position;
                    public float Scale;
                }
            }
        }

        public enum EStory
        {
            None,
            Story_1,
            Story_2,
            Story_3,
            Story_4,
            Story_5,
            MAX
        }
    }
}

public interface IStory
{

}