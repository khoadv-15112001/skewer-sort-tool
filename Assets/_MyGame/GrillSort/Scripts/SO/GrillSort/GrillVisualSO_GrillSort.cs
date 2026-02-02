using Gameplay.Entities;
using Gameplay.Entities.GrillScripts;
using Manager;
using Sonat.Enums;
using UnityEngine;

namespace MyGame.SkewerJam.Scripts.SO.Behavior
{
    [CreateAssetMenu(fileName = "GrillVisualSO_GrillSort", menuName = "MyGame/GrillSort/GrillVisualSO_GrillSort")]
    public class GrillVisualSO_GrillSort : GrillVisualSO
    {
        public override void SetGrillBaseVisual(GrillVisual grillVisual, SpriteRenderer stove, SpriteRenderer lid, string stoveType, string lidType)
        {
            if (GameplayController.instance == null) return;

            if (GameplayController.instance.levelType is LevelType.Cake or LevelType.Fruit)
            {
                string stoveName = string.IsNullOrEmpty(stoveType) ? "Fruit" : $"Fruit_{stoveType}";
                stove.SetSpriteAsync(PathManager.TraySprite(stoveName));
                string lidName = string.IsNullOrEmpty(lidType) ? "Fruit" : $"Fruit_{lidType}";
                lid.SetSpriteAsync(PathManager.LidSprite(lidName));
            }
            else
            {
                string stoveName = string.IsNullOrEmpty(stoveType) ? "Normal" : $"Normal_{stoveType}";
                stove.SetSpriteAsync(PathManager.TraySprite(stoveName));
                string lidName = string.IsNullOrEmpty(lidType) ? "Normal" : $"Normal_{lidType}";
                lid.SetSpriteAsync(PathManager.LidSprite(lidName));
            }
        }

        public override void SetSubGrillVisual(SubGrill subGrill, SpriteRenderer visual, string grillVisualName)
        {
            if (GameplayController.instance.levelType is LevelType.Cake or LevelType.Fruit)
            {
                string visualName = string.IsNullOrEmpty(grillVisualName) ? "Fruit" : $"Fruit_{grillVisualName}";
                visual.SetSpriteAsync(PathManager.SubTraySprite(visualName));
            }
            else
            {
                string visualName = string.IsNullOrEmpty(grillVisualName) ? "Normal" : $"Normal_{grillVisualName}";
                visual.SetSpriteAsync(PathManager.SubTraySprite(visualName));
            }
        }
    }
}