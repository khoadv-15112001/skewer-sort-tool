using Sonat.Enums;
using UnityEngine;


namespace MyGame.Modules.CardCollection
{
    [CreateAssetMenu(fileName = "CardConfig", menuName = "Sonat Configs/CardCollection/CardConfig")]
    public class CardConfig : ScriptableObject
    {
        public CardType type;
        public string cardName;
        public int star;
        //public Sprite sprite;
        public TextColorType textColorIndex;
//#if UNITY_EDITOR
//        private void OnValidate()
//        {
//            cardName = type.ToString();
//        }
//#endif
    }

    public enum TextColorType
    {
        Red,
        Blue,
        Cyan,
        Mallard,
        Magenta,
        Green,
        Purple,
        Pink,
        Yellow,
        Orange
    }

    public enum CardType
    {
        None = -1,

        Card_0_0 = 0,
        Card_0_1,
        Card_0_2,
        Card_0_3,
        Card_0_4,
        Card_0_5,
        Card_0_6,
        Card_0_7,
        Card_0_8,
        Card_1_0,
        Card_1_1,
        Card_1_2,
        Card_1_3,
        Card_1_4,
        Card_1_5,
        Card_1_6,
        Card_1_7,
        Card_1_8,
        Card_2_0,
        Card_2_1,
        Card_2_2,
        Card_2_3,
        Card_2_4,
        Card_2_5,
        Card_2_6,
        Card_2_7,
        Card_2_8,
        Card_3_0,
        Card_3_1,
        Card_3_2,
        Card_3_3,
        Card_3_4,
        Card_3_5,
        Card_3_6,
        Card_3_7,
        Card_3_8,
        Card_4_0,
        Card_4_1,
        Card_4_2,
        Card_4_3,
        Card_4_4,
        Card_4_5,
        Card_4_6,
        Card_4_7,
        Card_4_8,
        Card_5_0,
        Card_5_1,
        Card_5_2,
        Card_5_3,
        Card_5_4,
        Card_5_5,
        Card_5_6,
        Card_5_7,
        Card_5_8,
        Card_6_0,
        Card_6_1,
        Card_6_2,
        Card_6_3,
        Card_6_4,
        Card_6_5,
        Card_6_6,
        Card_6_7,
        Card_6_8,
        Card_7_0,
        Card_7_1,
        Card_7_2,
        Card_7_3,
        Card_7_4,
        Card_7_5,
        Card_7_6,
        Card_7_7,
        Card_7_8,
        Card_8_0,
        Card_8_1,
        Card_8_2,
        Card_8_3,
        Card_8_4,
        Card_8_5,
        Card_8_6,
        Card_8_7,
        Card_8_8,
        Card_9_0,
        Card_9_1,
        Card_9_2,
        Card_9_3,
        Card_9_4,
        Card_9_5,
        Card_9_6,
        Card_9_7,
        Card_9_8,
        MAX,
    }
}