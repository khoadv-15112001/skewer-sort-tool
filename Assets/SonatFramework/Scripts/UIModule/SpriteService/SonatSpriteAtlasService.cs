using Cysharp.Threading.Tasks;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.U2D;

namespace SonatFramework.Scripts.UIModule.SpriteService
{
    [CreateAssetMenu(fileName = "SonatSpriteAtlasService", menuName = "Sonat Services/Atlas Service")]
    public class SonatSpriteAtlasService : SpriteAtlasService
    {
        [Required] [SerializeField] private SpriteAtlas spriteAtlas;

        public override Sprite GetSprite(string spriteName)
        {
            if (spriteAtlas == null) return null;
            return spriteAtlas.GetSprite(spriteName);
        }
    }
}