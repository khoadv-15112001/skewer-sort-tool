
using System;
using I2.Loc;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;

namespace MyGame.Modules.CardCollection
{

    [RequireComponent(typeof(TMP_Text))]
    public class ChangeTextMaterial : MonoBehaviour
    {
        public TextMaterialSO materialSO;
        public bool useFillColor;
        private TMP_Text text;
        private Localize localize;

        private void Setup()
        {
            text = GetComponent<TMP_Text>();
            localize = GetComponent<Localize>();
        }

        public void SetMaterial(TextColorType index)
        {
            Setup();
            if (useFillColor)
            {
                text.color = materialSO.colors[(int)index];
            }
            else
            {
                if (localize != null)
                {
                    localize.SecondaryTerm = materialSO.materialTerms[(int)index];
                    localize.OnLocalize(true);
                }
                else
                {
                    text.fontMaterial = materialSO.materials[(int)index];
                }
            }
        }

#if UNITY_EDITOR
        [Header("Test Editor")]
        [SerializeField] private TextColorType index;
        [Button("Test")]
        public void Test()
        {
            SetMaterial(index);
        }
#endif
    }

    public static class TMP_TextExtensions
    {
        public static void SetMaterial(this TMP_Text text, TextColorType index)
        {
            if (text.transform.TryGetComponent<ChangeTextMaterial>(out var changeTextMaterial))
            {
                changeTextMaterial.SetMaterial(index);
            }
        }
    }
}