using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using SonatFramework.Systems.InventoryManagement.GameResources;
using UnityEngine;

namespace GrillSort.QuestEvent
{
    [CreateAssetMenu(fileName = "QuestEventConfig", menuName = "Sonat Configs Custom/QuestEventConfig")]
    public class QuestEventConfig : ScriptableObject
    {
        public bool Active = true;
        public int unlockedLevel = 5;
        //public int duration = 14; // số ngày
        public List<QuestData> questDatas;
        public LiveOpsPackData liveOpsPackData;

        public int GetMaxNum(int questIdx)
        {
            if (questIdx < 0 || questIdx >= questDatas.Count) return int.MaxValue;
            return questDatas[questIdx].numItem;
        }

        public int GetQuestCount()
        {
            return questDatas.Count;
        }

        public RewardData GetReward(int currentQuestIdx)
        {
            if (currentQuestIdx < 0 || currentQuestIdx >= questDatas.Count) return new RewardData(){resourceDatas = new List<ResourceData>()};
            return questDatas[currentQuestIdx].GetRewardData();
        }

#if UNITY_EDITOR
        public void OnValidate()
        {
            for (int i = 0; i < questDatas.Count; i++)
            {
                questDatas[i].stt = i + 1;
            }
        }
#endif
    }

    [Serializable]
    public class QuestData
    {
        [GUIColor(0, 1, 0, 1)]
        [ReadOnly]
        public int stt;
        public int numItem;
        public int itemId;
        public RewardData reward;
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
            return reward;
        }
    }
}
