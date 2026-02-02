using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace GrillSort.Story
{
    public abstract class UIBackground : MonoBehaviour
    {
        [SerializeField] protected GameObject bg_global;
        [SerializeField] protected GameObject bg_jp;
        [SerializeField] protected Image bgStory;

        protected StoryBase _story;
        protected GameObject bg;

        protected virtual void Start()
        {
            Setup();
        }

        private void OnEnable()
        {
            StoryService.OnChangeStory += Setup;
        }

        private void OnDisable()
        {
            StoryService.OnChangeStory -= Setup;
        }

        protected virtual void Setup()
        {
            _story = StoryService.Instance.GetCurrentStory();

            bg_global.SetActive(false);
            bg_jp.SetActive(false);

            bg = LocalizationUtils.IsJapanese() ? bg_jp : bg_global;

            SetBackground();
        }

        protected void SetBackground()
        {
            if (_story == null)
            {
                bg.SetActive(true);
                bgStory.gameObject.SetActive(false);
            }
            else
            {
                bg.SetActive(false);
                bgStory.gameObject.SetActive(true);

                bgStory.sprite = GetSpriteStory();
            }
        }

        protected abstract Sprite GetSpriteStory();
    }
}