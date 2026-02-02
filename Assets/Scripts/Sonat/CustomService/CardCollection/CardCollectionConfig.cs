using System;
using System.Collections.Generic;
using SonatFramework.Systems.ConfigManagement;
using SonatFramework.Systems.InventoryManagement.GameResources;
using UnityEngine;

namespace MyGame.Modules.CardCollection
{
    [CreateAssetMenu(fileName = "CardCollectionConfig", menuName = "Sonat Configs/CardCollection/CardCollectionConfig")]
    public class CardCollectionConfig : ConfigSo
    {
        public int unlockLevel;
        public LiveOpsPackData liveOpsPackData;
        public int sendLimit = 2;

        [Header("Config expire")]
        public int durationMonth = 3;

        [Space(10)]
        [Header("Config reward")]
        public RewardData rewardInSeason;

        [Space(10)]
        public List<AlbumConfig> albums;
        public List<CardConfig> cards;
        [Space(10)]
        public RewardData RewardUnlock;

        public int GetNumCard()
        {
            return cards.Count;
        }

        public AlbumType GetAlbumType(CardType cardType)
        {
            return albums.Find(e => e.cards.Contains(cardType))?.type ?? AlbumType.None;
        }
    }
}