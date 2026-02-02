using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using Newtonsoft.Json;
using Sirenix.OdinInspector;
using Sonat.Enums;
using SonatFramework.Systems.LoadObject;
using UnityEngine;

namespace SonatFramework.Systems.LevelManagement
{
    [CreateAssetMenu(fileName = "SonatLevelServiceAsync",
        menuName = "Sonat Services/Level Service/Sonat Level Service Async")]
    public class SonatLevelServiceAsync : LevelServiceAsync
    {
        protected static readonly JsonSerializerSettings Settings = new() { TypeNameHandling = TypeNameHandling.Auto };

        [BoxGroup("SERVICES")] [Required] [SerializeField]
        private Service<LoadObjectServiceAsync> loadObjectServiceAsync = new();

        [BoxGroup("SERVICES")] [Required] [SerializeField]
        private Service<SaveObjectServiceAsync> saveObjectServiceAsync = new();

        [SerializeField] protected List<GameModeLevel> gameModeLevels = new();

        //[BoxGroup("CONFIGS")] [SerializeField] [Required]
        //protected LevelConfig config;

        protected LevelData levelCache;


        public override async UniTask<T> GetLevelData<T>(int level, GameMode gameMode, bool force = false,
            bool loop = true, int category = 0)
        {
            if (!force && levelCache != null && levelCache.gameMode == gameMode && levelCache.level == level && levelCache.category == category)
                return levelCache as T;
            if (loop)
            {
                var totalLevel = gameModeLevels.FirstOrDefault(e => e.mode == gameMode)!.level;
                if (level > totalLevel)
                {
                    var levelData = await GetLevel<T>(totalLevel / 2 + level % (totalLevel / 2), gameMode);
                    levelData.level = level;
                    return levelData;
                }
            }

            return await GetLevel<T>(level, gameMode);
        }

        protected override async UniTask<T> GetLevel<T>(int level, GameMode gameMode, int category = 0)
        {
            string fileName = category == 0 ? $"{level}.json" : $"{level}.{category}.json";
            var levelData = await loadObjectServiceAsync.Instance.LoadAsync<T>(fileName);
            return levelData;
        }

        public override async UniTask SaveLevel<T>(T levelData)
        {
            await saveObjectServiceAsync.Instance.SaveObject(levelData, $"{levelData.level}.json");
        }
    }
}