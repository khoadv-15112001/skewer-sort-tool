using Gameplay.Entities;
using Gameplay.LevelData;
using MyGame.SkewerJam.Gameplay;
using UnityEngine;

namespace MyGame.SkewerJam.Scripts.SO.Behavior
{
    [CreateAssetMenu(fileName = "ConveyorControllerSO_SkewerJam", menuName = "MyGame/SkewerJam/ConveyorControllerSO_SkewerJam")]
    public class ConveyorControllerSO_SkewerJam : ConveyorControllerSO
    {
        public override PrimaryGrill GetGrill(int grillId)
        {
            var gameLogicHandler = MyGame.SkewerJam.Gameplay.GameController.Instance.GameLogicHandler;
            var grill = gameLogicHandler.GrillManager.GetGrill(grillId);
            return grill;
        }

        public override void GetStartEndPositions(ConveyorData conveyData, MoveType moveType, float speed, ref Vector3 startPosition, ref Vector3 endPosition)
        {
            if (moveType == MoveType.Horizontal)
            {
                if (conveyData.speed > 0)
                {
                    startPosition.x = GameController.Instance.GameViewport.MinX - 3f;
                    endPosition.x = GameController.Instance.GameViewport.MaxX + 3f;
                }
                else
                {
                    startPosition.x = GameController.Instance.GameViewport.MaxX + 3f;
                    endPosition.x = GameController.Instance.GameViewport.MinX - 3f;
                }
            }
            else if (moveType == MoveType.Vertical)
            {
                if (conveyData.speed > 0)
                {
                    startPosition.y = GameController.Instance.GameViewport.MinY - 3f;
                    endPosition.y = GameController.Instance.GameViewport.MaxY + 3f;
                }
                else
                {
                    startPosition.y = GameController.Instance.GameViewport.MaxY + 3f;
                    endPosition.y = GameController.Instance.GameViewport.MinY - 3f;
                }
            }
        }
    }
}