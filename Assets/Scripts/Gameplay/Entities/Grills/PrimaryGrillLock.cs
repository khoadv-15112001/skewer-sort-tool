using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Gameplay.BoosteeManagement;
using Gameplay.Entities.Obstacle;
using Gameplay.LevelData;
using UnityEngine;

namespace Gameplay.Entities.Grills
{
    public class PrimaryGrillLock : PrimaryGrill
    {
        [SerializeField] private LockObstacle lockObstacle;

        public override async UniTask SetData(GrillData grillData)
        {
            lockState = 1;
            base.SetData(grillData);
            int priority = 0;
            if (grillData is GrillLockData grillLockData)
            {
                priority = grillLockData.priority;
            }
            lockObstacle.SetData(this, priority);
        }

        public override void Unlock()
        {
            base.Unlock();
            lockObstacle.ForceUnlock();
        }

        public override ShuffleLayerData GetShuffleLayerData()
        {
            if (IsLock)
                return null;
            return base.GetShuffleLayerData();
        }

        public override List<ShuffleLayerData> GetSubsShuffleLayerData()
        {
            if (IsLock)
                return null;
            return base.GetSubsShuffleLayerData();
        }

        public override void SetMaskVisible(bool state)
        {
            base.SetMaskVisible(state);
            lockObstacle.SetVisibility(state);
        }

        public void SetInteractable(bool interactable)
        {
            lockObstacle.SetBlockClick(!interactable);
        }
    }
}