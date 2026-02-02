using System;
using Gameplay.Entities;
using Gameplay.LevelData;
using UnityEngine;

namespace MyGame.SkewerJam.Scripts.SO.Behavior
{
    public abstract class ConveyorControllerSO : ScriptableObject
    {
        public abstract PrimaryGrill GetGrill(int grillId);

        public abstract void GetStartEndPositions(ConveyorData conveyData, MoveType moveType, float speed, ref Vector3 startPosition, ref Vector3 endPosition);
    }
}