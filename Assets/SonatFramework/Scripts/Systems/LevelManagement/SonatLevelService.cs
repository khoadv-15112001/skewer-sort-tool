using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using Sirenix.OdinInspector;
using Sonat.Enums;
using SonatFramework.Scripts.SonatSDKAdapterModule;
using SonatFramework.Systems.LoadObject;
using UnityEngine;

namespace SonatFramework.Systems.LevelManagement
{
    [CreateAssetMenu(fileName = "SonatLevelService", menuName = "Sonat Services/Level Service/Sonat Level Service")]
    public class SonatLevelService : LevelService, IServiceInitialize
    {
        [SerializeField] protected string path = "Level";

        [BoxGroup("SERVICES")]
        [Required]
        [SerializeField]
        protected Service<LoadObjectService> loadObjectService = new();

        [BoxGroup("SERVICES")]
        [Required]
        [SerializeField]
        private Service<SaveObjectService> saveObjectService = new();


        [BoxGroup("CONFIGS")][SerializeField] protected LevelConfig config;

        protected LevelData levelCache;
        [SerializeField] private List<GameModeLevel> gameModeLevels = new();

        public void Initialize()
        {
            gameModeLevels = SonatSDKAdapter.GetRemoteConfig("game_level", gameModeLevels);
            Debug.Log($"Game Level: {JsonConvert.SerializeObject(gameModeLevels)}");
        }

        public override T GetLevelData<T>(int level, GameMode gameMode, bool force = false, bool loop = true, int category = 0)
        {
            if (!force && levelCache != null && levelCache.gameMode == gameMode && levelCache.level == level && levelCache.category == category)
                return levelCache as T;
            if (loop)
            {
                if (LocalizationUtils.IsJapanese() && gameModeLevels.Find(x => x.mode == GameMode.ClassicJapan) != null)
                {
                    gameMode = GameMode.ClassicJapan;
                }
                var totalLevel = gameModeLevels.FirstOrDefault(e => e.mode == gameMode)!.level;
                if (level > totalLevel)
                {
                    var levelData = GetLevel<T>(totalLevel / 2 + level % (totalLevel / 2), gameMode, category);
                    levelData.level = level;
                    return levelData;
                }
            }

            return GetLevel<T>(level, gameMode, category);
        }

        public void SetFolder(string folder)
        {
            path = folder;
        }

        protected override T GetLevel<T>(int level, GameMode gameMode, int category)
        {
            string mergePath = category == 0 ? string.IsNullOrEmpty(path) ? $"{level}.json" : $"{path}/{level}.json" :
                string.IsNullOrEmpty(path) ? $"{level}.{category}.json" : $"{path}/{level}.{category}.json";
            var levelData = loadObjectService.Instance.LoadObject<T>(mergePath);
            if (levelData == null && category > 0)
            {
                levelData = GetLevel<T>(level, gameMode, category - 1);
            }

            return levelData;
        }

        public override void SaveLevel<T>(T levelData)
        {
            if (Directory.Exists(saveObjectService.Instance.path) == false) Directory.CreateDirectory(saveObjectService.Instance.path);
            if (Directory.Exists(Path.Combine(saveObjectService.Instance.path, path)) == false)
                Directory.CreateDirectory(Path.Combine(saveObjectService.Instance.path, path));

            var fileName = levelData.category == 0 ? $"{path}/{levelData.level}.json" : $"{path}/{levelData.level}.{levelData.category}.json";
            saveObjectService.Instance.SaveObject(levelData, fileName);
        }
    }
}