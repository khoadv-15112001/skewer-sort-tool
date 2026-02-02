using UnityEngine;
using Gameplay.Entities;
using Gameplay.LevelData;

namespace MyGame.SkewerJam.Scripts.SO.Behavior
{
    [CreateAssetMenu(fileName = "ConveyorControllerSO_GrillSort", menuName = "MyGame/GrillSort/ConveyorControllerSO_GrillSort")]
    public class ConveyorControllerSO_GrillSort : ConveyorControllerSO
    {
        public override PrimaryGrill GetGrill(int grillId)
        {
            return GameplayController.instance.levelGenerator.GetPrimaryGrill(grillId);
        }

        public override void GetStartEndPositions(ConveyorData conveyData, MoveType moveType, float speed, ref Vector3 startPosition, ref Vector3 endPosition)
        {
            if (moveType == MoveType.Horizontal)
            {
                if (conveyData.speed > 0)
                {
                    startPosition.x = GameplayController.instance.levelGenerator.gameViewport.minX - 3f;
                    endPosition.x = GameplayController.instance.levelGenerator.gameViewport.maxX + 3f;
                }
                else
                {
                    startPosition.x = GameplayController.instance.levelGenerator.gameViewport.maxX + 3f;
                    endPosition.x = GameplayController.instance.levelGenerator.gameViewport.minX - 3f;
                }
            }
            else if (moveType == MoveType.Vertical)
            {
                if (conveyData.speed > 0)
                {
                    startPosition.y = GameplayController.instance.levelGenerator.gameViewport.minY - 3f;
                    endPosition.y = GameplayController.instance.levelGenerator.gameViewport.maxY + 3f;
                }
                else
                {
                    startPosition.y = GameplayController.instance.levelGenerator.gameViewport.maxY + 3f;
                    endPosition.y = GameplayController.instance.levelGenerator.gameViewport.minY - 3f;
                }
            }
        }
    }
}