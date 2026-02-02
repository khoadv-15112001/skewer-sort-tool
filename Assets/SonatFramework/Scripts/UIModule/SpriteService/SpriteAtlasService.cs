using SonatFramework.Systems;
using UnityEngine;

namespace SonatFramework.Scripts.UIModule.SpriteService
{
    public abstract class SpriteAtlasService: SonatServiceSo
    {
        public abstract Sprite GetSprite(string spriteName);
    }
}