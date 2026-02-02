using UnityEngine;

namespace Gameplay
{
    public class GameResourceReference : SingletonSimple<GameResourceReference>
    {
        public Material[] itemMaterials;
        public Material hiddenMaterial, subMaterial, grillHighlightMaterial;
        public Sprite subHidden;
        
        protected override void OnAwake()
        {
            base.OnAwake();
            DontDestroyOnLoad(gameObject);
        }
    }
}
