using System;
using System.Collections.Generic;
using SonatFramework.Systems;
using SonatFramework.Systems.ObjectPooling;
using UnityEngine;

namespace MyGame.Modules.CardCollection
{
    public class UIDetailAlbum : UIAlbum
    {
        [Space]
        [Header("List cards")]
        [SerializeField] private Transform container;

        [Space]
        [Header("Album Background")]
        [SerializeField] private AlbumBackground albumBackground;

        private readonly Service<PoolingContainerService> _poolingService = new();

        private List<UICard> cards = new();

        public override void Setup(AlbumType albumType)
        {
            base.Setup(albumType);

            _poolingService.Instance.CleanContainer(container);
            foreach (var cardType in albumConfig.cards)
            {
                var cardObj = _poolingService.Instance.CreateObject<UICard>(container);
                cardObj.Setup(cardType);
                cards.Add(cardObj);
            }

            albumBackground.Setup(albumConfig);
        }

        public override void SetData(int numCard, int totalCard)
        {
            base.SetData(numCard, totalCard);

            foreach (var cardObj in cards)
            {
                cardObj.UpdateData();
            }
        }

        public override void UpdateData()
        {
            base.UpdateData();
            foreach (var cardObj in cards)
            {
                cardObj.UpdateData();
            }
        }

        public void SeeNewCard()
        {
            foreach (var cardObj in cards)
            {
                var cardType = cardObj.CardType;
                cardCollectionService.Instance.RemoveNewCard(cardType);
            }
        }
    }
}
