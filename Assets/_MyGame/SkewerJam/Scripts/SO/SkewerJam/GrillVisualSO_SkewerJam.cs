using Gameplay.Entities;
using Gameplay.Entities.GrillScripts;
using UnityEngine;

namespace MyGame.SkewerJam.Scripts.SO.Behavior
{
    [CreateAssetMenu(fileName = "GrillVisualSO_SkewerJam", menuName = "MyGame/SkewerJam/GrillVisualSO_SkewerJam")]
    public class GrillVisualSO_SkewerJam : GrillVisualSO
    {
        public override void SetGrillBaseVisual(GrillVisual grillVisual, SpriteRenderer stove, SpriteRenderer lid, string stoveType, string lidType)
        {

        }

        public override void SetSubGrillVisual(SubGrill subGrill, SpriteRenderer visual, string grillVisualName)
        {

        }
    }
}