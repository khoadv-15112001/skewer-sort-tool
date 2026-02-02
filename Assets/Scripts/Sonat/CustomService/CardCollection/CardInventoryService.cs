using System.Collections.Generic;
using SonatFramework.Systems;
using SonatFramework.Systems.GameDataManagement;
using UnityEngine;

namespace MyGame.Modules.CardCollection
{
    [CreateAssetMenu(fileName = "CardInventoryService", menuName = "Sonat Services/CardCollection/CardInventoryService")]
    public class CardInventoryService : ScriptableObject
    {
        private const string DATA_KEY = "CARD_INVENTORY";
        private readonly Service<DataService> dataService = new();

        private readonly Dictionary<CardType, int> cards = new();

        public int GetCard(CardType cardType)
        {
            if (cards.TryGetValue(cardType, out var value)) return value;
            value = dataService.Instance.GetInt($"{DATA_KEY}_{cardType}", 0);
            cards.Add(cardType, value);
            return value;
        }

        public int SetCard(CardType cardType, int value)
        {
            dataService.Instance.SetInt($"{DATA_KEY}_{cardType}", value);
            if (!cards.TryAdd(cardType, value))
            {
                cards[cardType] = value;
            }
            return value;
        }

        public void AddCard(CardType cardType, int value)
        {
            SetCard(cardType, GetCard(cardType) + value);
        }
    }
}