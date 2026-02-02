using System;
using System.Collections.Generic;
using Gameplay.Entities;
using Gameplay.Entities.Obstacle;
using Gameplay.LevelData;
using UnityEngine;

namespace MyGame.SkewerJam.Scripts.SO.Behavior
{
    public abstract class OctoChefBehaviorSO : ScriptableObject
    {
        [SerializeField] public GameFactorySO gameFactorySO;
        [SerializeField] public EventSystemSO eventSystemSO;

        public abstract LevelData GetLevelData();

        public abstract List<PrimaryGrill> GetPrimaryGrills();

        public abstract float GetGrillThreshold();

        public abstract float GetItemThreshold();

        public abstract PrimaryGrill GetRandomGrill(List<PrimaryGrill> validGrills, int slotCount, PrimaryGrill lastGrill, System.Random rng);

        public abstract void SetStartGrill(OctoChefObstacle octoChefObstacle, List<GrillBase> grills);

        public virtual void RegisterEvents_OnCollectItem(Action<int> onCollectItem)
        {
            eventSystemSO.RegisterEvents_OnCollectItem(onCollectItem);
        }
        
        public virtual void UnregisterEvents_OnCollectItem(Action<int> onCollectItem)
        {
            eventSystemSO.UnregisterEvents_OnCollectItem(onCollectItem);
        }

        public virtual void CustomUpdate(OctoChefObstacle octoChefObstacle)
        {
        }
    }
}