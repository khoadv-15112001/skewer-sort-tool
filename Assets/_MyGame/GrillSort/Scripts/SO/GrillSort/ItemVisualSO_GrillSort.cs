using Gameplay;
using Manager;
using UnityEngine;  
using Cysharp.Threading.Tasks;

namespace MyGame.SkewerJam.Scripts.SO.Behavior
{
    [CreateAssetMenu(fileName = "ItemVisualSO_GrillSort", menuName = "MyGame/GrillSort/ItemVisualSO_GrillSort")]
    public class ItemVisualSO_GrillSort : ItemVisualSO
    {
        public override void SetVisual(SpriteRenderer spriteRenderer, int id, bool isPrimary)
        {
            spriteRenderer.SetSpriteAsync(PathManager.ItemSprite(id)).Forget();
            spriteRenderer.material = isPrimary ? GameResourceReference.Instance.itemMaterials[0] : GameResourceReference.Instance.subMaterial;
        }
    }
}