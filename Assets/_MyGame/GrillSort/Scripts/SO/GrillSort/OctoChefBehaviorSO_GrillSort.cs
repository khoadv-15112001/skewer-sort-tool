using System.Collections.Generic;
using System.Linq;
using Gameplay.Entities;
using Gameplay.Entities.Obstacle;
using Gameplay.LevelData;
using UnityEngine;

namespace MyGame.SkewerJam.Scripts.SO.Behavior
{
    [CreateAssetMenu(fileName = "OctoChefBehaviorSO_GrillSort", menuName = "MyGame/GrillSort/OctoChefBehaviorSO_GrillSort")]
    public class OctoChefBehaviorSO_GrillSort : OctoChefBehaviorSO
    {

        public override LevelData GetLevelData()
        {
            return GameplayController.instance.levelGenerator.LevelData;
        }

        public override List<PrimaryGrill> GetPrimaryGrills()
        {
            return GameplayController.instance.levelGenerator.GetPrimaryGrills();
        }

        public override float GetItemThreshold()
        {
            return -1f;
        }

        public override float GetGrillThreshold()
        {
            return -1f;
        }

        public override void SetStartGrill(OctoChefObstacle octoChefObstacle, List<GrillBase> grills)
        {
            octoChefObstacle.SetCurrentGrill((PrimaryGrill)grills[0]);
        }

        public override PrimaryGrill GetRandomGrill(List<PrimaryGrill> validGrills, int slotCount, PrimaryGrill lastGrill, System.Random rng)
        {
            var tempList = validGrills.Where(e => (e.CheckCanOctoChef() && e.SlotCount == slotCount && e != lastGrill)).Select(e => e.id).ToList();
            //var tempList = _validGrillIds.Where(x => !_currentGrillIds.Contains(x)).ToList();
            var randId = rng.Next(0, tempList.Count);
            return validGrills.First(e => e.id == tempList[randId]);
        }
    }
}