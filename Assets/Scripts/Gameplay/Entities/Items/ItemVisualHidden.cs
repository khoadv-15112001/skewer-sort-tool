using Cysharp.Threading.Tasks;
using DG.Tweening;
using Gameplay.Entities.ItemScripts;
using Gameplay.LevelData;
using Manager;
using UnityEngine;
using UnityEngine.Rendering;

namespace Gameplay.Entities.Items
{
    public class ItemVisualHidden : ItemVisual
    {
        [SerializeField] private SpriteRenderer hidden;

        public override void UpdateVisual()
        {
            spriteRenderer.SetSpriteAsync(PathManager.ItemSprite(id)).Forget();
            spriteRenderer.material = GameResourceReference.Instance.hiddenMaterial;
            hidden.gameObject.SetActive(true);
        }

        protected override void SetMaterial(Material material)
        {
        }

        public override void SetVisibleMaskState(bool state)
        {
            if (state == isConveyorState) return;
            isConveyorState = state;
            switch (state)
            {
                case true:
                    spriteRenderer.maskInteraction = SpriteMaskInteraction.VisibleOutsideMask;
                    hidden.maskInteraction = SpriteMaskInteraction.VisibleOutsideMask;
                    break;
                case false:
                    spriteRenderer.maskInteraction = SpriteMaskInteraction.None;
                    hidden.maskInteraction = SpriteMaskInteraction.None;
                    break;
            }
        }
    }
}