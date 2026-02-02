using System.Collections.Generic;
using Gameplay.LevelData;
using UnityEngine;
using UnityEngine.Serialization;

namespace Tool
{
    public class ToolObstacleBase: MonoBehaviour
    {
        public byte id;
        public ObstacleType obstacleType;
        public List<ToolGrill> toolGrills;
        public ObstacleData obstacleData;

        public virtual void SetData(ObstacleData data)
        {
            this.id = data.id;
            this.obstacleData = data;
        }

        public virtual void SetGrills(List<ToolGrill> toolGrills)
        {
            this.toolGrills = toolGrills;
        }

        public virtual void ValidateObstacle()
        {
            
        }
    }
}