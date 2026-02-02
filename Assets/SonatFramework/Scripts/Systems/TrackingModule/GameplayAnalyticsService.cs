using System.Collections.Generic;
using Sonat.Enums;
using Sonat.FirebaseModule.RemoteConfig;
using SonatFramework.Scripts.Helper;
using SonatFramework.Systems.EventBus;
using SonatFramework.Systems.UserData;
using UnityEngine;

namespace SonatFramework.Systems.TrackingModule
{
    [CreateAssetMenu(fileName = "GameplayAnalyticsService", menuName = "Sonat Services/Gameplay Analytics")]
    public class GameplayAnalyticsService : SonatServiceSo, IServiceInitialize
    {
        protected readonly Service<UserDataService> userDataService = new();

        protected readonly IntDataPref lastLevelPlay = new("LastLevelPlay");
        protected readonly IntDataPref startCount = new("StartCount");
        public LevelPlayData levelPlayData = new();
        public int levelStartCount => startCount.Value;

        public virtual void Initialize()
        {
            new EventBinding<LevelStartedEvent>(OnStartLevel);
            new EventBinding<LevelEndedEvent>(OnEndLevel);
            new EventBinding<LevelStuckEvent>(OnLevelStuck);
            new EventBinding<LevelContinueEvent>(OnPlayContinue);
            new EventBinding<PhaseStartedEvent>(OnStartPhase);
            new EventBinding<UseBoosterEvent>(OnUseBooster);
            new EventBinding<LevelQuitEvent>(OnQuitLevel);

            SetLevelPlayData();
        }

        protected virtual void SetLevelPlayData()
        {
            levelPlayData = new LevelPlayData()
            {
                level = userDataService.Instance.GetLevel(),
                gameMode = GameMode.Classic,
            };
        }

        public virtual void OnStartLevel(LevelStartedEvent eventData)
        {
            levelPlayData = new LevelPlayData();
            levelPlayData.isFirstPlay = eventData.level != lastLevelPlay.Value;
            lastLevelPlay.Value = eventData.level;
            levelPlayData.timeStartLevel = Time.time;
            levelPlayData.level = eventData.level;
            levelPlayData.gameMode = eventData.gameMode;
            levelPlayData.moveCount = 0;
            if (levelPlayData.isFirstPlay)
            {
                startCount.Value = 1;
            }
            else
            {
                startCount.Value += 1;
            }

            levelPlayData.continueTimes = 0;
            levelPlayData.startCount = startCount.Value;
        }

        public virtual void OnStartPhase(PhaseStartedEvent eventData)
        {
            levelPlayData.phase = eventData.phase;
        }

        public virtual void OnEndLevel(LevelEndedEvent eventData)
        {
            if (eventData.success)
            {
                levelPlayData.timeEndLevel = Time.time;
                levelPlayData.isWin = true;
                int level = eventData.level;
            }
        }

        public virtual void OnLevelStuck(LevelStuckEvent eventData)
        {
            levelPlayData.loseCause = eventData.cause;
            levelPlayData.stuckCount++;
            levelPlayData.timeEndLevel = Time.time;
            levelPlayData.isWin = false;
        }

        public virtual void OnPlayContinue(LevelContinueEvent eventData)
        {
            levelPlayData.continueWith = eventData.by;
            levelPlayData.continueTimes++;
        }

        public virtual void OnUseBooster(UseBoosterEvent eventData)
        {
            levelPlayData.useBoosterCount++;
        }

        public virtual void OnQuitLevel(LevelQuitEvent eventData)
        {
            levelPlayData.loseCause = eventData.cause;
        }

        public int CheckStartCount(int level)
        {
            if (level != lastLevelPlay.Value)
            {
                return 1;
            }
            else
            {
                return startCount.Value + 1;
            }
        }
    }

    public class LevelPlayData
    {
        public GameMode gameMode;
        public int level;
        public int phase;
        public bool isFirstPlay;
        public float timeStartLevel;
        public float timeEndLevel;
        public int useBoosterCount;
        public int moveCount;
        public string loseCause;
        public bool isWin;
        public string continueWith;
        public int stuckCount;
        public int startCount;
        public int reviveByRwd;
        public int buyBoosterByRwd;
        public int continueTimes;
        public Dictionary<string, object> otherData = new();

        public bool TryGet(string key, out object value)
        {
            return otherData.TryGetValue(key, out value);
        }

        public void TrySetData(string key, object value)
        {
            if (otherData.ContainsKey(key))
            {
                otherData[key] = value;
            }
            else
            {
                otherData.Add(key, value);
            }
        }

        public float FullTimePlayLevel
        {
            get
            {
                if (timeEndLevel > timeStartLevel)
                {
                    return timeEndLevel - timeStartLevel;
                }
                else
                {
                    return Time.time - timeStartLevel;
                }
            }
        }
    }
}