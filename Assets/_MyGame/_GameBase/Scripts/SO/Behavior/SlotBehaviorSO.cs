using Gameplay.Entities;
using MyGame.SkewerJam.Scripts.SO.Behavior;
using UnityEngine;

namespace MyGame.SkewerJam.Scripts.SO
{
    public abstract class SlotBaseBehaviorSO : ScriptableObject
    {
        [SerializeField] public GameFactorySO gameFactorySO;

        public abstract void OnMouseDown(PrimarySlot slot);
    }
}