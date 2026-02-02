using SonatFramework.Systems.ObjectPooling;
using UnityEngine;

namespace Gameplay.Entities.ItemScripts
{
    public class ItemPlaceHolder: MonoBehaviour, IPoolingObject
    {
        [SerializeField] private SpriteRenderer spriteRenderer;

        public void SetSprite(Sprite sprite)
        {
            spriteRenderer.sprite = sprite;
        }

        public void Setup()
        {
            
        }

        public void OnCreateObj(params object[] args)
        {
            
        }

        public void OnReturnObj()
        {
            
        }
    }
}