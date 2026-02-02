using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Sirenix.OdinInspector;
using Sonat;
using Sonat.Enums;
using Sonat.TrackingModule;
using SonatFramework.Scripts.SonatSDKAdapterModule;
using SonatFramework.Systems.LevelManagement;
using SonatFramework.Systems.LoadObject;
using SonatFramework.Systems.UserData;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.Serialization;

namespace SonatFramework.Systems.LevelManagement
{
    [CreateAssetMenu(fileName = "LevelRemoteService", menuName = "Sonat Services/Level Service/Level Remote Service")]
    public class LevelRemoteService : LevelService, IServiceInitialize, IServiceWaitingRemoteConfig
    {
        private LevelRemoteConfigData levelRemoteConfigData;
        private AssetBundle assetBundle;
        private static PlayerPrefString lastId;
        private bool outOfSegment;
        private PlayerPrefString cachedRemoteValue;
        public static string mapId = "default";
        private bool initialized = false;
        protected LevelData levelCache;

        [SerializeField] private LevelService levelServiceFallback;

        public void Initialize()
        {
            lastId = new PlayerPrefString("LAST_REMOTE_LEVEL_DATA_ID", "default");
            cachedRemoteValue = new PlayerPrefString("CACHED_REMOTE_VALUE", "");
            levelServiceFallback = Instantiate(levelServiceFallback);
        }

        public void OnRemoteConfigReady()
        {
            if (initialized) return;
            initialized = true;
            if (levelServiceFallback != null && levelServiceFallback is IServiceWaitingRemoteConfig fallback)
            {
                fallback.OnRemoteConfigReady();
            }

            GetRemoteData();
        }

        public override T GetLevelData<T>(int level, GameMode gameMode, bool force = false, bool loop = true, int category = 0)
        {
            if (!force && levelCache != null && levelCache.gameMode == gameMode && levelCache.level == level && levelCache.category == category)
                return levelCache as T;
            return GetLevel<T>(level, gameMode, category);
        }

        protected override T GetLevel<T>(int level, GameMode gameMode, int category)
        {
            if (!outOfSegment && assetBundle != null)
            {
                string assetBundleName = category == 0 ? level.ToString() : $"{level}.{category}";
                TextAsset textAsset = assetBundle.LoadAsset<TextAsset>(assetBundleName);
                if (textAsset != null && !string.IsNullOrEmpty(textAsset.text))
                {
                    var data = JsonConvert.DeserializeObject<T>(textAsset.text, Settings);
                    if (data != null && data.gameMode == gameMode)
                    {
                        data.category = category;
                        levelCache = data;
                        mapId = lastId.Value;
                        return data;
                    }
                }
            }

            mapId = "default";
            return levelServiceFallback.GetLevelData<T>(level, gameMode, category: category);
        }


        public override void SaveLevel<T>(T levelData)
        {
            levelServiceFallback.SaveLevel(levelData);
        }

        private void GetRemoteData(int currentLevel = -1)
        {
            string json = SonatSDKAdapter.GetRemoteString(RemoteConfigKey.remote_level_data.ToString(), null);

            if (string.IsNullOrEmpty(json) && !SonatSDKAdapter.IsInternetConnection())
            {
                json = cachedRemoteValue.Value;
            }

            if (!string.IsNullOrEmpty(json))
            {
                if (!cachedRemoteValue.Value.Equals(json))
                {
                    cachedRemoteValue.Value = json;
                    if (PlayerPrefs.HasKey("DOWN_LOAD_LEVEL_LOG"))
                    {
                        PlayerPrefs.DeleteKey("DOWN_LOAD_LEVEL_LOG");
                    }
                }

                levelRemoteConfigData = JsonConvert.DeserializeObject<LevelRemoteConfigData>(json);
                levelRemoteConfigData.ParseData();
                if (string.IsNullOrEmpty(lastId.Value) || levelRemoteConfigData.id != lastId.Value)
                {
                    if (levelRemoteConfigData.force)
                    {
                        if (currentLevel < 0) currentLevel = SonatSystem.GetService<UserDataService>().GetLevel();
                        if (currentLevel > levelRemoteConfigData.levelStart)
                        {
                            outOfSegment = true;
                        }
                    }
                }
            }
            else
            {
                outOfSegment = true;
            }

            if (!outOfSegment)
            {
                SonatSystem.Instance.StartCoroutine(DownloadAssetBundle());
            }
        }

        private IEnumerator DownloadAssetBundle()
        {
            Debug.Log("DownloadLevelManager: start load remote level data");
            Hash128 hash128 = new Hash128();
            hash128.Append(levelRemoteConfigData.id);
            CachedAssetBundle cached = new CachedAssetBundle(levelRemoteConfigData.id, hash128);
            LogDownLoadPhase("start", levelRemoteConfigData.id);
            using (UnityWebRequest www = UnityWebRequestAssetBundle.GetAssetBundle(levelRemoteConfigData.url, cached, 0))
            {
                yield return www.SendWebRequest();
                if (www.result != UnityWebRequest.Result.Success)
                {
                    Debug.Log("DownloadLevelManager: request fail!!");
                    LogDownLoadPhase("failed", levelRemoteConfigData.id);
                }
                else
                {
                    Debug.Log("DownloadLevelManager: download success!");
                    assetBundle = DownloadHandlerAssetBundle.GetContent(www);
                    if (assetBundle != null)
                        lastId.Value = levelRemoteConfigData.id;
                    yield return new WaitForSeconds(0.1f);
                    LogDownLoadPhase("success", levelRemoteConfigData.id);
                    PlayerPrefs.SetString("DOWN_LOAD_LEVEL_LOG_ID", levelRemoteConfigData.id);
                }
            }
        }


        private void LogDownLoadPhase(string phase, string map_id)
        {
            if (PlayerPrefs.GetString("DOWN_LOAD_LEVEL_LOG_ID", "unknow").Equals(map_id)) return;
            new SonatLogDownloadLevelData()
            {
                phase = phase,
                map_id = map_id
            }.Post();
        }
    }

    public class LevelRemoteConfigData
    {
        public string url;

        public string id;

        // public uint version;
        public int order;
        public int levelStart;
        public int levelEnd;
        public bool force = false;

        public void ParseData()
        {
            var temp = url.Split('/');
            id = temp[temp.Length - 1];
            //string pattern = @"a_d(?<date>.+)f(?<from>.+)t(?<to>.+)";
            //RegexOptions options = RegexOptions.Singleline | RegexOptions.IgnoreCase;

            //Match match = Regex.Match(id, pattern, options);
            //if (match.Success)
            //{
            //    levelStart = int.Parse(match.Groups["from"].Value);
            //    levelEnd = int.Parse(match.Groups["to"].Value);
            //}

            string[] data = id.Split('_');
            if (data[data.Length - 1].ToLower().Equals("alllevel"))
            {
                levelStart = 1;
                levelEnd = 999999;
            }
            else
            {
                levelStart = int.Parse(data[data.Length - 2]);
                levelEnd = int.Parse(data[data.Length - 1]);
            }
        }
    }
}