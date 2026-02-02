using System;
using System.Collections.Generic;
using System.Linq;
using Gameplay.Entities;
using Gameplay.Entities.Grills;
using Gameplay.Entities.Obstacle;
using Gameplay.LevelData;
using MyGame.SkewerJam.Gameplay;
using UnityEngine;

namespace MyGame.SkewerJam.Objects
{
    public class ObstacleManager : MonoBehaviour
    {
        private List<ObstacleBase> obstacles = new List<ObstacleBase>();

        public void Init()
        {

        }

        public void Clear()
        {
            foreach (var conveyor in obstacles)
            {
                GameFactory.Instance.ReturnEntity(conveyor);
            }
            obstacles.Clear();
            PrimaryGrillIce.primaryGrillIces = null;
            LockObstacle.lockObstacles = null;
        }

        public void AddObstacle(ObstacleBase obstacle)
        {
            obstacles.Add(obstacle);
        }

        public List<ObstacleData> GetObstacleData()
        {
            return GameController.Instance.LevelGenerator.LevelData.obstacleData;
        }

        public List<ObstacleStateData> GetObstacleStateData()
        {
            var states = new List<ObstacleStateData>();
            foreach (var obstacle in obstacles)
            {
                states.Add(new ObstacleStateData()
                {
                    id = obstacle.GetObstacleData().id,
                    active = obstacle.active,
                    grillId = obstacle as OctoChefObstacle != null ? (obstacle as OctoChefObstacle).GetCurrentGrill() : 0
                });
            }
            return states;
        }

        public void SetObstacleStateData(List<ObstacleStateData> obstacleStateData)
        {
            foreach (var state in obstacleStateData)
            {
                var obstacle = obstacles.FirstOrDefault(e => e.GetObstacleData().id == state.id);
                if (obstacle != null)
                {
                    obstacle.active = state.active;

                    if (state.active == false)
                    {
                        if (obstacle is OctoChefObstacle octoChefObstacle)
                        {
                            octoChefObstacle.UnlockCurrentGrill();
                            octoChefObstacle.OnComplete();
                        }
                    }
                    else
                    {
                        var grillId = state.grillId;
                        var grill = GameController.Instance.GameLogicHandler.GrillManager.GetGrill(grillId);
                        if (obstacle is OctoChefObstacle octoChefObstacle && grill != null)
                        {
                            octoChefObstacle.UnlockCurrentGrill();
                            octoChefObstacle.SetCurrentGrill(grill as PrimaryGrill);
                        }
                    }
                }
            }
        }
    }
}