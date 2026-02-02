using System;
using System.Collections.Generic;
using Gameplay.LevelData;
using Sonat.Enums;
using UnityEngine;

namespace MyGame.SkewerJam.Level
{
    public class LevelData_SkewerJam : LevelData
    {
        // TODO: Thêm thuộc tính nào thì luôn cần thêm vào hàm clone
        public int numberOfWaitingGrill;
        public int numberOfOrder;

        public List<WaitingGrillData> ListWaitingGrillData;
        public List<OrderData_SkewerJam> ListOrderData;
        public int sequenceLogicOrderIndex;

        public List<DropColumnData> listDropColumnData;

        // // logic order basic
        // public List<LogicOrderConfig> logicOrderConfigs;

        public LevelData_SkewerJam CloneSkewerJam()
        {
            var levelData = base.Clone();

            var levelDataSkewerJam = new LevelData_SkewerJam()
            {
                gameMode = this.gameMode,
                level = this.level,
                category = this.category,

                time = levelData.time,
                levelType = levelData.levelType,
                difficulty = levelData.difficulty,
                difficultyValue = levelData.difficultyValue,

                grillData = levelData.grillData,
                conveyorData = levelData.conveyorData,
                orderData = levelData.orderData,
                obstacleData = levelData.obstacleData,
                isDropMode = levelData.isDropMode,

                numberOfWaitingGrill = this.numberOfWaitingGrill,
                numberOfOrder = this.numberOfOrder,

                // rescueCondition = this.rescueCondition,
                sequenceLogicOrderIndex = this.sequenceLogicOrderIndex,
                // logicOrderConfigs = this.logicOrderConfigs,

                listDropColumnData = this.listDropColumnData,

                ListWaitingGrillData = this.ListWaitingGrillData,
                ListOrderData = this.ListOrderData,
            };
            return levelDataSkewerJam;
        }
    }


    [Serializable]
    public class WaitingGrillData
    {
        public int id;
        public int active;
        public int itemId;
    }

    [Serializable]
    public class OrderData_SkewerJam
    {
        public int id;
        public int active;
        public int itemId;
        public int num;
        public int maxNum;
    }

    [Serializable]
    public class LogicOrderConfig
    {
        [Range(0, 1)]
        public float threshold;
        public int indexBO;
        public int indexSO;
    }

    [Serializable]
    public class DropColumnData
    {
        public int id;
        public List<int> grillIds;
    }
}