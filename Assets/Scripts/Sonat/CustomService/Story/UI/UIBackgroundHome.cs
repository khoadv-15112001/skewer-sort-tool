using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace GrillSort.Story
{
    public class UIBackgroundHome : UIBackground
    {
        protected override Sprite GetSpriteStory()
        {
            return _story.GetData().ImageConfig.BackgroundHomeSprite;
        }
    }
}