using System;
using Gameplay.Entities;
using Gameplay.Entities.GrillScripts;
using UnityEngine;

namespace MyGame.SkewerJam.Scripts.SO.Behavior
{
    public abstract class GrillVisualSO : ScriptableObject
    {
        public abstract void SetGrillBaseVisual(GrillVisual grillVisual, SpriteRenderer stove, SpriteRenderer lid, string stoveType, string lidType);

        public abstract void SetSubGrillVisual(SubGrill subGrill, SpriteRenderer visual, string grillVisualName);
    }
}