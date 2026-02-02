using System.Collections.Generic;
using UnityEngine;
using System;
using Sirenix.OdinInspector;
using SonatFramework.Systems.InventoryManagement.GameResources;

namespace GrillSort.BattlePass
{
    [CreateAssetMenu(fileName = "BattlePassGeneralConfig", menuName = "Sonat Configs Custom/BattlePass/BattlePassGeneralConfig")]
    public class BattlePassGeneralConfig : ScriptableObject
    {
        [Header("Config active")]
        public bool active;
        public LiveOpsPackData liveOpsPackData;

        [Space(10)]
        [Header("Config reward")]
        public List<MileStoneData> milestoneDatas;
        public RewardData winReward;
        public Sprite KeySprite;

        [Space(10)]
        [Header("Config visual")]
        public List<ThemeData> themeDatas;

        public enum Theme
        {
            Default,
            Thanksgiving,
            Xmas
        }

        [Serializable]
        public class ThemeData
        {
            public Theme theme;
            public Sprite icon;
            public RewardData activateRewards;
            public string namePopupTut;
            public string namePopupBoard;
            public string namePopupActivate;
        }

        public bool CheckActive()
        {
            return active && liveOpsPackData.CheckCondition();
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

        public int GetTotalExp(int milestoneIdx)
        {
            if (milestoneIdx < 0 || milestoneIdx >= milestoneDatas.Count)
                return 0;
            var totalExp = 0;
            for (int i = 0; i <= milestoneIdx; i++)
            {
                totalExp += milestoneDatas[i].exp;
            }
            return totalExp;
        }

        public int GetExpInMilestone(int milestoneIdx)
        {
            if (milestoneIdx < 0 || milestoneIdx >= milestoneDatas.Count)
                return 0;
            return milestoneDatas[milestoneIdx].exp;
        }

        public int GetMileStoneCount()
        {
            return milestoneDatas.Count;
        }


        [Serializable]
        public class MileStoneData
        {
            [GUIColor(0, 1, 0, 1)]
            [ReadOnly]
            public int stt;
            public int exp;
        }

    }
}
