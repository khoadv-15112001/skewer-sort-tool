using Sirenix.OdinInspector;
using Sonat.Enums;
using SonatFramework.Systems.LoadObject;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SonatFramework.Systems.LevelManagement
{
    [CreateAssetMenu(fileName = "SonatLevelLocalizeService", menuName = "Sonat Services/Level Service/Sonat Level Service Localize")]
    public class SonatLevelLocalizeService : SonatLevelService
    {
        [BoxGroup("SERVICES")]
        [Required]
        [SerializeField]
        private Service<LoadObjectService> loadObjectJapanService = new();

        protected override T GetLevel<T>(int level, GameMode gameMode, int category)
        {
            string mergePath = category == 0 ? string.IsNullOrEmpty(path) ? $"{level}.json" : $"{path}/{level}.json" :
                string.IsNullOrEmpty(path) ? $"{level}.{category}.json" : $"{path}/{level}.{category}.json";
            var levelData = GetLoadObjectService().Instance.LoadObject<T>(mergePath);
            if (levelData == null && category > 0)
            {
                levelData = GetLevel<T>(level, gameMode, category - 1);
            }

            return levelData;
        }

        private Service<LoadObjectService> GetLoadObjectService()
        {
            return LocalizationUtils.IsJapanese() ? (loadObjectJapanService != null ? loadObjectJapanService : loadObjectService) : loadObjectService;
        }
    }
}