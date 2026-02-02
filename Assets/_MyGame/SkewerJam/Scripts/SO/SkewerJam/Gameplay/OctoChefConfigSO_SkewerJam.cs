using System;
using System.Collections.Generic;
using System.Linq;
using Sonat.Enums;
using UnityEngine;

namespace MyGame.SkewerJam.Scripts.SO.SkewerJam.Gameplay
{
    [CreateAssetMenu(fileName = "OctoChefConfigSO_SkewerJam", menuName = "MyGame/SkewerJam/OctoChefConfigSO_SkewerJam")]
    public class OctoChefConfigSO_SkewerJam : ScriptableObject
    {
        public List<DifficultyConfig> difficultyConfigs;

        public float GetGrillThreshold(LevelDifficulty difficulty)
        {
            return difficultyConfigs.First(e => e.difficulty == difficulty).grillThreshold;
        }

        public float GetItemThreshold(LevelDifficulty difficulty)
        {
            return difficultyConfigs.First(e => e.difficulty == difficulty).itemThreshold;
        }

        public (float rate0, float rate1, float rate2) GetRate(LevelDifficulty difficulty)
        {
            var config = difficultyConfigs.First(e => e.difficulty == difficulty);
            return (config.rate0, config.rate1, config.rate2);
        }

#if UNITY_EDITOR
        public void OnValidate()
        {
            foreach (var config in difficultyConfigs)
            {
                
                config.rate2 = 1 - config.rate0 - config.rate1;
            }
        }
#endif
    }

    [Serializable]
    public class DifficultyConfig
    {
        public LevelDifficulty difficulty;
        public float itemThreshold;
        public float grillThreshold;

        [Range(0, 1)]
        public float rate0 = 0;
        [Range(0, 1)]
        public float rate1;
        [Range(0, 1)]
        public float rate2;
    }
}