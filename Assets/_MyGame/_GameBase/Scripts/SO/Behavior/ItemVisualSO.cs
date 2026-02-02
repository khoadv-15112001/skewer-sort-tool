using Gameplay.Entities.ItemScripts;
using UnityEngine;

namespace MyGame.SkewerJam.Scripts.SO.Behavior
{
    public abstract class ItemVisualSO : ScriptableObject
    {
        public abstract void SetVisual(SpriteRenderer spriteRenderer, int id, bool isPrimary);
    }
}