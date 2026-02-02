
using Manager;
using UnityEngine;
using Cysharp.Threading.Tasks;

namespace MyGame.SkewerJam.Scripts.SO.Behavior
{
    [CreateAssetMenu(fileName = "ItemVisualSO_SkewerJam", menuName = "MyGame/SkewerJam/ItemVisualSO_SkewerJam")]
    public class ItemVisualSO_SkewerJam : ItemVisualSO
    {
        public override void SetVisual(SpriteRenderer spriteRenderer, int id, bool isPrimary)
        {
            spriteRenderer.SetSpriteAsync(PathManager.ItemHLWSprite((ItemId)id)).Forget();
        }
    }
}