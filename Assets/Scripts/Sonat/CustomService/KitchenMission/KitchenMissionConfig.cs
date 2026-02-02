using Cysharp.Threading.Tasks;
using GrillSort.LavaQuest;
using Sirenix.OdinInspector;
using SonatFramework.Systems;
using SonatFramework.Systems.InventoryManagement.GameResources;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace GrillSort.KitchenMission
{
    [CreateAssetMenu(fileName = "KitchenMissionConfig", menuName = "My Services/KitchenMissionConfig")]
    public class KitchenMissionConfig : ScriptableObject
    {
        public int levelStart;
        public Stages stages = new();
        public BotConfig botConfig = new();
        public RewardData joinRewards;
        public TextAsset nameBotSource;

        public const string NameOfPopup_StartReward = "PopupKitchenMissionStartReward";
        public const string NameOfPopup_Tut = "PopupKitchenMissionTut";

        public List<string> GetRandomBotNames(int num)
        {
            if (num <= 0) return new List<string>();

            List<string> allNames = new();

            if (nameBotSource != null)
            {
                allNames = nameBotSource.text
                    .Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries)
                    .Select(name => name.Trim())
                    .Where(name => !string.IsNullOrEmpty(name))
                    .Distinct()
                    .ToList();
            }

            allNames = allNames.OrderBy(_ => UnityEngine.Random.value).ToList();

            string mainGeneratedName = GenerateGuidPlayerName();

            while (allNames.Count < num - 1)
            {
                string newName = GenerateGuidPlayerName();
                if (!allNames.Contains(newName))
                    allNames.Add(newName);
            }

            List<string> randomNames = allNames.Take(num - 1).ToList();

            int randomIndex = UnityEngine.Random.Range(0, randomNames.Count + 1);
            randomNames.Insert(randomIndex, mainGeneratedName);

            return randomNames;
        }

        private string GenerateGuidPlayerName()
        {
            uint randomNumber = BitConverter.ToUInt32(Guid.NewGuid().ToByteArray(), 0);
            string digits = randomNumber.ToString("D10");
            return $"Player#{digits}";
        }

        [Button("Test Generate Name")]
        public void Test()
        {
            foreach (var name in GetRandomBotNames(4))
            {
                Debug.LogError(name);
            }
        }

        [Serializable]
        public class Players
        {
            public List<Player> datas = new();

            [Serializable]
            public class Player
            {
                public string name;
                public int fID;
                public int aID;
                public int step;
                public int lastStep;
                public bool isYourself;

                public Player(string name, int fID, int aID, bool isYourself = false)
                {
                    this.name = name;
                    this.fID = fID;
                    this.aID = aID;
                    step = 0;
                    lastStep = 0;
                    this.isYourself = isYourself;
                }

                public void UpdateProfile(string name, int fID, int aID)
                {
                    this.name = name;
                    this.fID = fID;
                    this.aID = aID;
                }

                public string GetName()
                {
                    return name;
                }

                public void LoseBot()
                {
                    if (KitchenMissionService.Instance.isOver.BoolValue) return;

                    step = 0;
                }

                public void WinBot()
                {
                    if (KitchenMissionService.Instance.isOver.BoolValue) return;

                    step = Mathf.Min(step + 1, KitchenMissionService.Instance.GetCurrentStageData().maxStep);

                    if (step == KitchenMissionService.Instance.GetCurrentStageData().maxStep)
                    {
                        if (!KitchenMissionService.Instance.IsPassTimeSafe())
                        {
                            var random = UnityEngine.Random.Range(0, 2);
                            if (random == 1) LoseBot();
                            else step--;
                        }
                        else
                        {
                            KitchenMissionService.Instance.isOver.BoolValue = true;
                        }
                    }
                }

                public bool WinUser()
                {
                    if (KitchenMissionService.Instance.isOver.BoolValue) return false;

                    step = Mathf.Min(step + 1, KitchenMissionService.Instance.GetCurrentStageData().maxStep);

                    if (step == KitchenMissionService.Instance.GetCurrentStageData().maxStep)
                    {
                        KitchenMissionService.Instance.isOver.BoolValue = true;
                    }

                    return true;
                }

                public void LoseUser()
                {
                    step = 0;
                }

                public bool IsCompleted()
                {
                    return step == KitchenMissionService.Instance.GetCurrentStageData().maxStep;
                }
            }
        }

        [Serializable]
        public class Stages
        {
            public List<Stage> datas = new();

            [Serializable]
            public class Stage
            {
                [ValueDropdown(nameof(GetAllTerms))]
                public string nameTerm;
                public int maxStep;
                public BackgroundLocalize backgroundLocalize;
                public Sprite chestCloseSprite;
                public string nameSkin;
                public ModifyBotConfig modifyBotConfig;
                public RewardData rewardData;
                public RewardData railReward;

                public RewardData GetRewardData()
                {
#if CUSTOM_ENUM
                    // Nếu user join Rail event và có railReward → return railReward
                    if (Rail.RailService.Instance != null && 
                        Rail.RailService.Instance.IsJoinEvent() && 
                        railReward != null && 
                        railReward.resourceDatas != null && 
                        railReward.resourceDatas.Count > 0)
                    {
                        return railReward;
                    }
#endif
                    return rewardData;
                }

                public Sprite GetBackgroundSprite()
                {
                    return backgroundLocalize.GetBackgroundSprite();
                }

                private IEnumerable<string> GetAllTerms()
                {
                    return LocalizationUtils.GetAllTerms();
                }

                [Serializable]
                public class BackgroundLocalize
                {
                    public Sprite Global;
                    public Sprite Japan;

                    public Sprite GetBackgroundSprite()
                    {
                        return LocalizationUtils.IsJapanese() ? (Japan ? Japan : Global) : Global;
                    }
                }

                [Serializable]
                public class ModifyBotConfig
                {
                    public long timeRange;
                    public int numUseBooster;
                    public int numUseRevive;
                    public Config winConfig;
                    public Config loseConfig;

                    [Serializable]
                    public struct Config
                    {
                        public float LoseRate;
                        public float Speed;
                    }
                }
            }
        }

        [Serializable]
        public struct BotConfig
        {
            public long TimeCycle;
            public float Speed;
            public float LoseRate;
            public float Variance;

            public float GetSpeedAfterCalculateVariance()
            {
                float randomVariance = UnityEngine.Random.Range(-Variance, Variance);

                return GetSpeed() + randomVariance;
            }

            public float GetLoseRate()
            {
                if (KitchenMissionService.Instance.IsPassTimeSafe())
                {
                    var curStageData = KitchenMissionService.Instance.GetCurrentStageData();

                    bool isIncreaseBot = true;

                    if (KitchenMissionService.Instance.numUseBooster.Value >= curStageData.modifyBotConfig.numUseBooster)
                    {
                        isIncreaseBot = false;
                    }

                    if (KitchenMissionService.Instance.numUseRevive.Value >= curStageData.modifyBotConfig.numUseRevive)
                    {
                        isIncreaseBot = false;
                    }

                    if (isIncreaseBot)
                        return LoseRate + curStageData.modifyBotConfig.loseConfig.LoseRate;
                    else
                        return LoseRate + curStageData.modifyBotConfig.winConfig.LoseRate;
                }

                return LoseRate;
            }

            public float GetSpeed()
            {
                if (KitchenMissionService.Instance.IsPassTimeSafe())
                {
                    var curStageData = KitchenMissionService.Instance.GetCurrentStageData();

                    bool isIncreaseBot = true;

                    if (KitchenMissionService.Instance.numUseBooster.Value >= curStageData.modifyBotConfig.numUseBooster)
                    {
                        isIncreaseBot = false;
                    }

                    if (KitchenMissionService.Instance.numUseRevive.Value >= curStageData.modifyBotConfig.numUseRevive)
                    {
                        isIncreaseBot = false;
                    }

                    if (isIncreaseBot)
                        return Speed + curStageData.modifyBotConfig.loseConfig.Speed;
                    else
                        return Speed + curStageData.modifyBotConfig.winConfig.Speed;
                }

                return Speed;
            }
        }
    }
}