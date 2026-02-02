using Newtonsoft.Json;
using Sonat.Enums;

namespace SonatFramework.Systems.LevelManagement
{
    public abstract class LevelService : SonatServiceSo
    {
        public static readonly JsonSerializerSettings Settings = new() { TypeNameHandling = TypeNameHandling.Auto };
        public abstract T GetLevelData<T>(int level, GameMode gameMode, bool force = false,
            bool loop = true, int category = 0) where T : LevelData;

        protected abstract T GetLevel<T>(int level, GameMode gameMode, int category) where T : LevelData;
        public abstract void SaveLevel<T>(T levelData) where T : LevelData;
    }
}