using System;
using System.Collections.Generic;
using Sonat.Enums;
using SonatFramework.Systems.ConfigManagement;
using SonatFramework.Systems.InventoryManagement.GameResources;
using UnityEngine;

namespace SonatFramework.Scripts.Gameplay
{
    //[CreateAssetMenu(menuName = "Sonat Configs/Game Play Config", fileName = "GamePlayConfig")]
    public class GamePlayConfig : ConfigSo
    {
        public ResourceData playOnPrice;
        public List<RewardByLevelDifficulty> winRewards = new List<RewardByLevelDifficulty>();

        public virtual ResourceData GetWinReward(LevelDifficulty levelDifficulty)
        {
            var data = winRewards.Find(x => x.levelDifficulty == levelDifficulty) ?? winRewards[0];
            return data.reward;
        }

        [Serializable]
        public class RewardByLevelDifficulty
        {
            public LevelDifficulty levelDifficulty;
            public ResourceData reward;
        }
    }
}