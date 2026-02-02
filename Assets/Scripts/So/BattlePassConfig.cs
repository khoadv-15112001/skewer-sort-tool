using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using SonatFramework.Systems.InventoryManagement.GameResources;
using UnityEngine;

namespace GrillSort.BattlePass
{
    [CreateAssetMenu(fileName = "BattlePassConfig", menuName = "Sonat Configs Custom/BattlePass/BattlePassConfig")]
    public class BattlePassConfig : ScriptableObject
    {
        public List<MileStoneData> milestoneDatas;


        public int GetMileStoneCount()
        {
            return milestoneDatas.Count;
        }

        public RewardData GetReward(int currentMilestoneIdx)
        {
            if (currentMilestoneIdx < 0 || currentMilestoneIdx >= milestoneDatas.Count) return new RewardData() { resourceDatas = new List<ResourceData>() };
            return milestoneDatas[currentMilestoneIdx].reward;
        }

#if UNITY_EDITOR
        public void OnValidate()
        {
            for (int i = 0; i < milestoneDatas.Count; i++)
            {
                milestoneDatas[i].stt = i + 1;
            }
        }
#endif
    }

    [Serializable]
    public class MileStoneData
    {
        [GUIColor(0, 1, 0, 1)]
        [ReadOnly]
        public int stt;
        public RewardData reward;
    }
}
