using System;
using System.Collections.Generic;
using Gameplay.LevelData;
using SonatFramework.Systems.ObjectPooling;
using UnityEngine;

namespace Gameplay.Entities.Obstacle
{
    public abstract class ObstacleBase : EntityBase, IPoolingObject
    {
        public override EntityType entityType => EntityType.Obstacle;
        protected ObstacleData obstacleData;
        public bool active = true;
        public abstract ObstacleType ObstacleType { get; }

        public virtual void SetData(ObstacleData data)
        {
            this.obstacleData = data;
        }

        public abstract void SetGrill(List<GrillBase> grills);
        public abstract void OnComplete();
        public abstract void Setup();

        public abstract void OnCreateObj(params object[] args);

        public abstract void OnReturnObj();

        public virtual void Highlight(bool highlight)
        {
        }

        public virtual ObstacleData GetObstacleData()
        {
            return obstacleData;
        }
    }
}