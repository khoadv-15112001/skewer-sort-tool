using System;
using System.Collections.Generic;
using Gameplay.LevelData;
using MyGame.SkewerJam.Gameplay;
using UnityEngine;

namespace MyGame.SkewerJam.Objects
{
    public class ConveyorManager : MonoBehaviour
    {
        private List<ConveyorController> conveyors = new List<ConveyorController>();

        public void Init()
        {

        }

        public void Clear()
        {
            foreach (var conveyor in conveyors)
            {
                GameFactory.Instance.ReturnEntity(conveyor);
            }
            conveyors.Clear();
        }

        public void AddConveyor(ConveyorController conveyor)
        {
            conveyors.Add(conveyor);
        }

        public List<ConveyorData> GetConveyorData()
        {
            return GameController.Instance.LevelGenerator.LevelData.conveyorData;
        }
    }
}