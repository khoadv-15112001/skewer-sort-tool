using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Sonat.Enums;
using SonatFramework.Scripts.UIModule;
using SonatFramework.Systems.InventoryManagement.GameResources;
using UnityEngine;
namespace MyGame.Modules.CardCollection
{
    public static class CardPackHelper
    {
        public static int GetNumCardInPack(GameResource packType)
        {
            int result = (int)packType - (int)GameResource.Card_Randomx1 + 1;
            Debug.Log($"[CardPackHelper] packType={packType} ({(int)packType}) → base={(int)GameResource.Card_Randomx1} → result={result}");
            return result;
        }

        public static CardType GetRandomCardType()
        {
            var cardCollectionService = MySonatFramework.GetService<CardCollectionService>();
            var config = cardCollectionService.config;

            float newCardRate = cardCollectionService.GetPercentageAppearNewCard();
            bool rollNewCard = UnityEngine.Random.value <= newCardRate;

            int star = GetRandomStar();

            List<CardConfig> candidateCards = GetCandidateCardsByStar(star, rollNewCard, cardCollectionService, config);

            if (candidateCards.Count == 0)
            {
                var availableStars = config.cards
                    .Select(c => c.star)
                    .Distinct()
                    .Where(s => GetCandidateCardsByStar(s, rollNewCard, cardCollectionService, config).Count > 0)
                    .ToList();

                if (availableStars.Count > 0)
                {
                    star = GetWeightedRandomStar(availableStars);
                    candidateCards = GetCandidateCardsByStar(star, rollNewCard, cardCollectionService, config);
                }
                else
                {
                    if (rollNewCard)
                    {
                        rollNewCard = false;
                        star = GetWeightedRandomStar(config.cards.Select(c => c.star).Distinct().ToList());
                        candidateCards = GetCandidateCardsByStar(star, rollNewCard, cardCollectionService, config);
                    }
                }
            }

            if (candidateCards == null || candidateCards.Count == 0)
            {
                Debug.LogWarning("[CardCollection] No candidate cards found at any star level!");
                return (CardType)UnityEngine.Random.Range(0, (int)CardType.MAX);
            }

            int idx = UnityEngine.Random.Range(0, candidateCards.Count);
            return candidateCards[idx].type;
        }

        private static List<CardConfig> GetCandidateCardsByStar(int star, bool rollNewCard, CardCollectionService cardService, CardCollectionConfig config)
        {
            if (rollNewCard)
                return config.cards.Where(c => c.star == star && !cardService.IsCollectedCard(c.type)).ToList();
            else
                return config.cards.Where(c => c.star == star && cardService.IsCollectedCard(c.type)).ToList();
        }

        private static int GetRandomStar()
        {
            int random = UnityEngine.Random.Range(0, 100);
            if (random < 70) return 1;
            if (random < 90) return 2;
            return 3;
        }

        private static int GetWeightedRandomStar(List<int> availableStars)
        {
            Dictionary<int, float> baseWeights = new Dictionary<int, float>
            {
                { 1, 70f },
                { 2, 20f },
                { 3, 10f }
            };

            var validWeights = baseWeights
                .Where(kvp => availableStars.Contains(kvp.Key))
                .ToDictionary(kvp => kvp.Key, kvp => kvp.Value);

            float total = validWeights.Values.Sum();
            float roll = UnityEngine.Random.Range(0f, total);
            float cumulative = 0f;

            foreach (var kvp in validWeights)
            {
                cumulative += kvp.Value;
                if (roll <= cumulative)
                    return kvp.Key;
            }

            return validWeights.Keys.Last();
        }

        public static List<CardType> GetCardListByStars(List<int> availableStars)
        {
            var result = new List<CardType>();

            var cardCollectionService = MySonatFramework.GetService<CardCollectionService>();
            var config = cardCollectionService.config;

            if (availableStars == null || availableStars.Count == 0)
            {
                Debug.LogWarning("[CardCollection] Empty star list provided!");
                return result;
            }

            foreach (var star in availableStars)
            {
                float newCardRate = cardCollectionService.GetPercentageAppearNewCard();
                bool rollNewCard = UnityEngine.Random.value <= newCardRate;

                List<CardConfig> candidateCards = GetCandidateCardsByStar(star, rollNewCard, cardCollectionService, config);

                if (candidateCards.Count == 0 && rollNewCard)
                {
                    candidateCards = GetCandidateCardsByStar(star, false, cardCollectionService, config);
                }

                if (candidateCards.Count == 0)
                {
                    Debug.LogWarning($"[CardCollection] No cards available for star {star}");
                    continue;
                }

                int idx = UnityEngine.Random.Range(0, candidateCards.Count);
                result.Add(candidateCards[idx].type);
            }

            return result;
        }

        //public static CardType GetRandomCardType()
        //{
        //    // 70% card 1x
        //    // 20% card 2x
        //    // 10% card 3x
        //    var random = UnityEngine.Random.Range(0, 100);
        //    var star = 0;

        //    if (random < 70)
        //    {
        //        star = 1;
        //    }
        //    else if (random < 90)
        //    {
        //        star = 2;
        //    }
        //    else
        //    {
        //        star = 3;
        //    }

        //    // if (star == 0)
        //    // {
        //    //     var randomType = UnityEngine.Random.Range(0, (int)CardType.MAX);
        //    //     return (CardType)randomType;
        //    // }

        //    var cardCollectionService = MySonatFramework.GetService<CardCollectionService>();
        //    var cards = cardCollectionService.config.cards.Where(card => card.star == star).ToList();
        //    var idx = UnityEngine.Random.Range(0, cards.Count);
        //    return cards[idx].type;
        //}


        public static List<CardType> GetCardReward(RewardData rewardData)
        {
            List<CardType> cardList = new();

            foreach (var reward in rewardData.resourceDatas)
            {
                var packType = reward.resource;

                if (GameResourceHelper.ResourceType(reward.resource) == GameResourceType.Card)
                {
                    int quantity = GetNumCardInPack(packType); // ví dụ 6
                    for (int i = 0; i < quantity; i++)
                    {
                        var randomCardType = GetRandomCardType();
                        cardList.Add(randomCardType);
                    }
                }
            }

            return cardList;
        }

        public static int GetNumStarReward(CardType cardType)
        {
            var config = MySonatFramework.GetService<CardCollectionService>().config;
            var cardConfig = config.cards.FirstOrDefault(e => e.type == cardType);
            return cardConfig.star;

        }

        public static int GetPackIndex(int count)
        {
            return (count - 1) / 2;
        }
    }
}