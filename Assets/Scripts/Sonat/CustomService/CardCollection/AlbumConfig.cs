using System.Collections.Generic;
using Sonat.Enums;
using SonatFramework.Systems.InventoryManagement.GameResources;
using UnityEngine;

namespace MyGame.Modules.CardCollection
{
    [CreateAssetMenu(fileName = "AlbumConfig", menuName = "Sonat Configs/CardCollection/AlbumConfig")]
    public class AlbumConfig : ScriptableObject
    {
        public AlbumType type;
        public string albumName;
        public RewardData reward;
        public List<CardType> cards;

        [Space]
        [Header("Album Background")]
        //public Sprite albumIcon;
        public Sprite albumBorder;
        public Sprite albumBackground;
        public Sprite albumExit;

        [Space]
        [Header("Text Color")]
        public TextColorType color;

//#if UNITY_EDITOR
//        private void OnValidate()
//        {
//            albumName = type.ToString();
//        }
//#endif
    }

    [System.Serializable]
    public enum AlbumType
    {
        None = -1,
        Album_0 = 0,
        Album_1 = 1,
        Album_2 = 2,
        Album_3 = 3,
        Album_4 = 4,
        Album_5 = 5,
        Album_6 = 6,
        Album_7 = 7,
        Album_8 = 8,
        Album_9 = 9
    }

}