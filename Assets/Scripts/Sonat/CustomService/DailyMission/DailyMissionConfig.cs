using GrillSort.Rail;
using SonatFramework.Systems.InventoryManagement.GameResources;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GrillSort.DailyMission
{
    [CreateAssetMenu(fileName = "DailyMissionConfig", menuName = "My Services/DailyMissionConfig")]
    public class DailyMissionConfig : ScriptableObject
    {
        public int levelStart;
        public List<Reward> BasicStageRewards = new();
        public Reward XLStageReward;
        public List<X2Data> ListX2Data = new();

        public const string NameOfPopupTut = "PopupDailyMissionTut";

        public enum EMission
        {
            Combo_Master,
            Booster_Feast,
            Veggie_Hunt,

            END
        }

        [Serializable]
        public class Reward
        {
            public List<RewardData> rewardDatas = new();
            public RewardData railRewards = new();

            public RewardData GetRewardData(int index)
            {
                if (RailService.Instance.IsJoinEvent() && railRewards.resourceDatas.Count != 0)
                    return railRewards;

                return rewardDatas[index];
            }
        }

        [Serializable]
        public class BasicStage
        {
            public List<TargetData> Targets = new();
        }

        [Serializable]
        public class XLStage
        {
            public TargetData Target = new();
        }

        [Serializable]
        public class TargetData
        {
            public int NumItem;
        }

        [Serializable]
        public class X2Data
        {
            public int TimeDuration_Second;
            public int CoinPrice;
        }
    }
}