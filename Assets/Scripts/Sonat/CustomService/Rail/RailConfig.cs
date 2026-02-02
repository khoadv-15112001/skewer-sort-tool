using Sirenix.OdinInspector;
using SonatFramework.Systems.InventoryManagement.GameResources;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "RailConfig", menuName = "My Services/Rail/Config")]
public class RailConfig : ScriptableObject
{
    public int LevelUnlock = 45;
    public List<Stage> Stages = new();

    [Serializable]
    public class Stage
    {
        [ValueDropdown(nameof(GetAllTerms))]
        public string Name;
        public BackgroundLocalize Background;
        public List<Milestone> Milestones = new();

        public Sprite GetBackgroundSprite()
        {
            return Background.GetBackgroundSprite();
        }

        public IEnumerable<string> GetAllTerms()
        {
            return LocalizationUtils.GetAllTerms();
        }

        [Serializable]
        public class BackgroundLocalize
        {
            public Sprite Global;
            public Sprite Japan;

            public Sprite GetBackgroundSprite()
            {
                return LocalizationUtils.IsJapanese() ? (Japan ? Japan : Global) : Global;
            }
        }

        [Serializable]
        public class Milestone
        {
            public int ItemRequire;
            public RewardData Rewards;
        }

    }

}
