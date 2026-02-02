using System.Collections.Generic;
using UnityEngine;

namespace MyGame.Modules.CardCollection
{
    [CreateAssetMenu(fileName = "TextMaterialSO", menuName = "Gameplay/UI/TextMaterialSO")]
    public class TextMaterialSO : ScriptableObject
    {
        public List<Material> materials;
        public List<Color> colors;
        public List<string> materialTerms;
    }
}