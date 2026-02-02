using System.Collections.Generic;
using Sirenix.OdinInspector;
using Sonat.Enums;
using SonatFramework.Systems.ConfigManagement;
using UnityEngine;

namespace SonatFramework.Systems.LevelManagement
{
    [CreateAssetMenu(fileName = "LevelConfig", menuName = "Sonat Configs/LevelConfig", order = 1)]
    public class LevelConfig: ConfigSo
    {
       // [ShowInInspector] public List<GameModeLevel> totalLevels;
    }

    [System.Serializable]
    public class GameModeLevel
    {
        public GameMode mode;
        public int level;
    }
}