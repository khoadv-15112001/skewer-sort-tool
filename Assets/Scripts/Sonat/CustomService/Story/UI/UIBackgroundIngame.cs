using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace GrillSort.Story
{
    public class UIBackgroundIngame : UIBackground
    {
        protected override void Setup()
        {
            base.Setup();

            SetupOffset();
        }

        private void SetupOffset()
        {
            if (_story == null)
            {
                //SetupDefault();
                return;
            }

            if (bgStory.TryGetComponent<AspectRatioFitter>(out var aspectRatio))
            {
                aspectRatio.enabled = false;
            }

            var rectTransform = bgStory.GetComponent<RectTransform>();

            rectTransform.anchoredPosition = new Vector2(0, Mathf.Clamp(_story.GetData().ImageConfig.OffsetYBackgroundIngame, -85f, 50f));
        }

        //private void SetupDefault()
        //{
        //    if (bgDefault.TryGetComponent<AspectRatioFitter>(out var aspectRatio))
        //    {
        //        aspectRatio.enabled = false;
        //    }

        //    var rectTransform = bgDefault.GetComponent<RectTransform>();

        //    rectTransform.anchoredPosition = new Vector2(0, -85f);
        //}

        protected override Sprite GetSpriteStory()
        {
            var story = StoryService.Instance.GetCurrentStory();

            return story.GetData().ImageConfig.BackgroundIngameSprite;
        }
    }
}