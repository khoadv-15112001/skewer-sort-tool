using GrillSort.BattlePass;
using Manager;
using Sonat.Enums;
using SonatFramework.Systems;
using SonatFramework.Systems.InventoryManagement.GameResources;
using SonatFramework.Systems.TrackingModule;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GrillSort.LavaQuest
{
    [CreateAssetMenu(fileName = "LavaQuestConfig", menuName = "Sonat Configs Custom/LavaQuest/LavaQuestConfig")]
    public class LavaQuestConfig : ScriptableObject
    {
        public int levelStart;
        public int maxStep = 7;
        public int coin = 5000;

        public List<PlayerFailedDataByLevel> playerFailedDataByLevels = new();

        [Serializable]
        public class PlayerFailedDataByLevel
        {
            public int levelStart;
            public List<PlayerFailedDataByStep> playerFailedDataBySteps = new();
        }

        [Serializable]
        public class PlayerFailedDataByStep
        {
            public List<PlayerFailedData> playerFailedDataByBooster = new();
            public List<PlayerFailedData> playerFailedDataByRevive = new();
        }

        [Serializable]
        public class PlayerFailedData
        {
            public int min;
            public int max;
        }

        private PlayerFailedDataByLevel GetPlayerFailedDataByLevel()
        {
            var level = MySonatFramework.userDataService.GetLevel();

            PlayerFailedDataByLevel ans = new();

            foreach (var data in playerFailedDataByLevels)
            {
                if (level >= data.levelStart)
                {
                    ans = data;
                }
            }

            return ans;
        }

        public int GetCountPlayerKilled(int step)
        {
            var data = GetPlayerFailedDataByLevel().playerFailedDataBySteps[step - 1];

            var analytics = SonatSystem.GetService<GameplayAnalyticsService>();

            //var killByBoosterData = data.playerFailedDataByBooster[Mathf.Clamp(analytics.levelPlayData.useBoosterCount, 0, data.playerFailedDataByBooster.Count - 1)];
            //var killByReviveData = data.playerFailedDataByRevive[Mathf.Clamp(analytics.levelPlayData.stuckCount, 0, data.playerFailedDataByRevive.Count - 1)];
            
            var killByBoosterData = data.playerFailedDataByBooster[0];
            var killByReviveData = data.playerFailedDataByRevive[0];

            PlayerFailedData playerFailedData = null;

            if (killByBoosterData.max >= killByReviveData.max)
                playerFailedData = killByBoosterData;
            else
                playerFailedData = killByReviveData;

            return UnityEngine.Random.Range(playerFailedData.min, playerFailedData.max);
        }

        public int GetRandomFrameID()
        {
            return MySonatFramework.GetService<ProfileService>().GetRandomFrameSpriteID();
        }

        public int GetRandomAvatarID()
        {
            return MySonatFramework.GetService<ProfileService>().GetRandomAvatarSpriteID();
        }

        [Serializable]
        public class Players
        {
            public List<Player> datas = new();

            [Serializable]
            public class Player
            {
                public int fID;
                public int aID;
                public EState state;

                public Player(int f_ID, int a_ID)
                {
                    this.fID = f_ID;
                    this.aID = a_ID;
                    this.state = EState.Live;
                }

                public void UpdateProfile(int f_ID, int a_ID)
                {
                    this.fID = f_ID;
                    this.aID = a_ID;
                }

                public int GetFrameID()
                {
                    return Mathf.Clamp(fID, 0, MySonatFramework.GetService<ProfileService>().GetNumFrame() - 1);
                }

                public int GetAvatarID()
                {
                    return Mathf.Clamp(aID, 0, MySonatFramework.GetService<ProfileService>().GetNumAvatar() - 1);
                }

                public enum EState
                {
                    Live,
                    Die
                }

            }
        }

#if UNITY_EDITOR
        public TextAsset data;

        [ContextMenu("Assign Data")]
        public void AssignData()
        {
            if (data == null)
            {
                Debug.LogError("CSV data file is not assigned.");
                return;
            }

            string[] lines = data.text.Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);
            if (lines.Length < 2)
            {
                Debug.LogWarning("CSV has no data rows.");
                return;
            }
            PlayerFailedDataByLevel newData = new();
            newData.levelStart = 0;

            for (int i = 1; i < lines.Length; i++) // Skip header
            {
                string line = lines[i].Trim();
                if (string.IsNullOrWhiteSpace(line)) continue;
                string[] values = line.Split(',');

                PlayerFailedDataByStep playerFailedDataByStep = new PlayerFailedDataByStep();

                for (int j = 1; j <= 4; j++)
                {
                    string val = values[j];
                    string[] split = val.Split("-");

                    playerFailedDataByStep.playerFailedDataByBooster.Add(new PlayerFailedData
                    {
                        min = int.Parse(split[0]),
                        max = int.Parse(split[1])
                    });
                }

                playerFailedDataByStep.playerFailedDataByRevive.Add(new() { min = 0, max = 0 });

                for (int j = 5; j <= 7; j++)
                {
                    string val = values[j];
                    string[] split = val.Split("-");

                    playerFailedDataByStep.playerFailedDataByRevive.Add(new PlayerFailedData
                    {
                        min = int.Parse(split[0]),
                        max = int.Parse(split[1])
                    });
                }

                newData.playerFailedDataBySteps.Add(playerFailedDataByStep);
            }

            playerFailedDataByLevels.Add(newData);

            UnityEditor.EditorUtility.SetDirty(this);
            UnityEditor.AssetDatabase.SaveAssets();
            UnityEditor.AssetDatabase.Refresh();
        }

#endif
    }
}