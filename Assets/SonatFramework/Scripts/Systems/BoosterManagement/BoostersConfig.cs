using System;
using System.Collections.Generic;
using Sonat.Enums;
using SonatFramework.Systems.ConfigManagement;
using UnityEngine;

namespace SonatFramework.Systems.BoosterManagement
{
    [CreateAssetMenu(fileName = "BoostersConfig", menuName = "Sonat Configs/BoostersConfig", order = 1)]
    public class BoostersConfig : ConfigSo
    {
        public List<BoosterConfig> configs;
    }

    [Serializable]
    public class BoosterConfig
    {
        public GameResource booster;
        public int levelUnlock;
        public GameResource priceCurrency;
        public int price;
        public int defaultValue;
        public int value;
    }
}