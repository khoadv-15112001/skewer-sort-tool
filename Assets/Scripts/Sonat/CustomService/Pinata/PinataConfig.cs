using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GrillSort.Pinata
{
    [CreateAssetMenu(fileName = "PinataConfig", menuName = "My Services/PinataConfig")]
    public class PinataConfig : ScriptableObject
    {
        public int LevelUnlock;
        public DayOfWeek dayStart;
        public DayOfWeek dayEnd;
        public List<Stage> Stages = new();

        public int MaxStage => Stages.Count;

        [Serializable]
        public class Stage
        {
            public int MaxMilestone;
            public int NumChance;
            public float BonusCardRate;
            public UpgradeStarRate[] UpgradeStarRates;

            [Serializable]
            public class UpgradeStarRate
            {
                public float Rate;
                public bool IsUpgrade;
            }
        }
    }
}