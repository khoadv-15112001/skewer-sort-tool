using Cysharp.Threading.Tasks;
using GrillSort.OnlineService;
using GrillSort.RealTime;
using Helper;
using MyGame.Modules.CardCollection.Animation;
using Newtonsoft.Json;
using Sirenix.OdinInspector;
using Sonat.Enums;
using SonatFramework.Scripts.Helper;
using SonatFramework.Scripts.UIModule;
using SonatFramework.Systems;
using SonatFramework.Systems.EventBus;
using SonatFramework.Systems.InventoryManagement;
using SonatFramework.Systems.InventoryManagement.GameResources;
using SonatFramework.Systems.TimeManagement;
using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Services.Core;
using UnityEngine;

namespace MyGame.Modules.CardCollection
{
    [CreateAssetMenu(fileName = "CardCollectionService", menuName = "Sonat Services/CardCollection/CardCollectionService")]
    public class CardCollectionService : SonatServiceSo, IServiceInitialize
    {
        [Header("CONFIGS")] public CardCollectionConfig config;
        public StarExchangeConfig starExchangeConfig;

        // [Header("Custom Data Service")]
        // public CardInventoryService cardInventory;

        public event Action OnChangeData;
        public event Action OnNewCardCountChanged;
        public event Action OnCompleteAlbum;
        public event Action OnCompleteCollection;

        public List<PendingResourceResponse> pendingCards;

        public int TotalCard => _collectedCardTypes.Count;
        public int CardStar => _cardStar.Value;

        public const string DATA_KEY = "CARD_COLLECTION";

        // quản lý các thẻ mới
        private HashSet<CardType> _collectedCardTypes = new();
        private HashSet<CardType> _newCardTypes = new(); // card mới nhưng tạm thời chưa được xem trong album
        private ListDataPref<int> _collectedCardsData;
        private ListDataPref<int> _newCardData;
        private ListDataPref<int> _completedAlbumTypes;
        private DictionaryDataPref<int, int> _duplicatedCards;
        private IntDataPref _cardStar;
        private IntDataPref _isUnlocked;
        private IntDataPref _cardStarExchangeIndex;
        private IntDataPref _isCompleteCardCollection;
        private LongDataPref _expireTime;
        private IntDataPref _numCompleteCardCollection;
        private IntDataPref _sentCardToday;
        private LongDataPref _timeResetRequestCard;

        private Queue<RewardData> _currentRewardQueue = new();
        private Queue<AlbumType> _completedAlbumTypesQueue = new();

        private static bool _isFetching = false;
        private bool _isRunQueueRewardCard = false;
        public static bool IsRunQueueRewardCard => SonatSystem.GetService<CardCollectionService>()._isRunQueueRewardCard;


        private readonly Service<RealTimeService> _timeService = new();

        public void Initialize()
        {
            LoadConfig();
            LoadData();

            new EventBinding<AddItemEvent>(OnAddItemEvent);

            if (IsUnlocked())
            {
                CheckExpire().Forget();
            }

            CheckDailyReset();

            _timeService.Instance.OnNextDay += CheckDailyReset;
        }

        private void CheckDailyReset()
        {
            if (IsUnlocked())
                _sentCardToday.Value = 0;
        }

        #region Data/ Config

        private void LoadConfig()

        {
        }

        private void LoadData()
        {
            pendingCards = new();

            _isUnlocked = new IntDataPref($"{DATA_KEY}_isUnlocked", 0);

            _duplicatedCards = new($"{DATA_KEY}_duplicatedCards", new());

            // collected cards
            _collectedCardsData = new ListDataPref<int>($"{DATA_KEY}_collectedCards");
            if (_collectedCardsData.Value.Count > 0)
            {
                _collectedCardTypes = new HashSet<CardType>(_collectedCardsData.Value.Select(e => (CardType)e));
            }

            // new cards
            _newCardData = new ListDataPref<int>($"{DATA_KEY}_newCards");
            if (_newCardData.Value.Count > 0)
            {
                _newCardTypes = new HashSet<CardType>(_newCardData.Value.Select(e => (CardType)e));
            }

            // card star
            _cardStar = new IntDataPref($"{DATA_KEY}_cardStar");

            // completed album types
            _completedAlbumTypes = new ListDataPref<int>($"{DATA_KEY}_completedAlbumTypes");

            // card star exchange index
            _cardStarExchangeIndex = new IntDataPref($"{DATA_KEY}_cardStarExchangeIndex", -1);

            // is complete card collection
            _isCompleteCardCollection = new IntDataPref($"{DATA_KEY}_isCompleteCardCollection", 0);

            _numCompleteCardCollection = new IntDataPref($"{DATA_KEY}_numCompleteCardCollection", _isCompleteCardCollection.BoolValue ? 1 : 0);

            // expire time
            _expireTime = new LongDataPref($"{DATA_KEY}_expireTime");

            _sentCardToday = new IntDataPref($"{DATA_KEY}_sentCardToday", 0);
            _timeResetRequestCard = new LongDataPref($"{DATA_KEY}_timeResetRequestCard");
        }

        public void SaveData()
        {
            _collectedCardsData.Value = _collectedCardTypes.ToList().Select(e => (int)e).ToList();
            _newCardData.Value = _newCardTypes.ToList().Select(e => (int)e).ToList();
            OnChangeData?.Invoke();
        }

        private void ResetData()
        {
            _collectedCardTypes = new();
            _newCardTypes = new(); // card mới nhưng tạm thời chưa được xem trong album
            _collectedCardsData.Clear();
            _newCardData.Clear();
            _duplicatedCards.Clear();

            _cardStar.Value = 0;
            _cardStarExchangeIndex.Value = -1;
            _isCompleteCardCollection.Value = 0;

            _currentRewardQueue = new();
            _completedAlbumTypesQueue = new();
            _completedAlbumTypes.Clear();

            _isRunQueueRewardCard = false;

            _sentCardToday.Value = 0;

            SetExpireTime();
            SaveData();
        }

        public bool IsUnlocked()
        {
            return _isUnlocked.Value == 1;
        }

        public bool CanUnlock()
        {
            var level = MySonatFramework.userDataService.GetLevel();
            return config.liveOpsPackData.CheckCondition() && level >= config.unlockLevel;
        }

        public void Unlock()
        {
            _isUnlocked.Value = 1;
            ResetData();
            CheckExpire().Forget();
        }

        #endregion

        private async UniTask CheckExpire()
        {
            if (_expireTime.Value > 0)
            {
                var remainTime = GetRemainTime();
                if (remainTime > 0)
                {
                    await UniTask.Delay(TimeSpan.FromSeconds(remainTime));
                }
            }

            ResetData();
            CheckExpire().Forget();
        }

        private void SetExpireTime()
        {
            // đến ngày cuối cùng của tháng thứ 3
            var date = _timeService.Instance.GetCurrentTime();
            var newDate = date.AddMonths(config.durationMonth - 1);

            var lastDayOfMonth = new DateTime(newDate.Year, newDate.Month, DateTime.DaysInMonth(newDate.Year, newDate.Month), 23, 59, 59);
            _expireTime.Value = ((DateTimeOffset)lastDayOfMonth).ToUnixTimeSeconds() + 1;
        }

        public long GetRemainTime()
        {
            var currentTime = _timeService.Instance.GetCurrentTimeUnix();
            return _expireTime.Value - currentTime;
        }


        private void OnAddItemEvent(AddItemEvent eventData)
        {
            // BUG: Cần thêm hàng đợi khi mở các gói liên tiếp
            if (GameResourceHelper.ResourceType(eventData.resource) == GameResourceType.Card)
            {
                for (int i = 0; i < eventData.quantity; i++) // mở từng gói card packs
                {
                    var rewardData = new RewardData();
                    rewardData.resourceDatas = new()
                    {
                        new ResourceData(eventData.resource, 1)
                    };

                    _currentRewardQueue.Enqueue(rewardData);
                    //Debug.Log("anhnt: Enqueue reward card pack " + JsonConvert.SerializeObject(_currentRewardQueue));
                    if (_isRunQueueRewardCard == false)
                    {
                        _isRunQueueRewardCard = true;
                        RunQueueRewardCard().Forget();
                    }
                }
            }
        }

        private async UniTask RunQueueRewardCard()
        {
            await UniTask.Delay(1000);
            while (_currentRewardQueue.Count > 0)
            {
                var rewardData = _currentRewardQueue.Dequeue();

                var popup = PanelManager.Instance.OpenPanelByName<PopupReceiveCardBase>("PopupReceiveCard_Immediately",
                    new UIData().Add(PopupReceiveCardBase.REWARD_DATA_KEY, rewardData));

                await UniTask.WaitUntil(() => (popup == null || popup.gameObject.activeInHierarchy == false));
            }

            _isRunQueueRewardCard = false;
        }

        // hard code
        public async UniTask<bool> RunQueueCompleteAlbum()
        {
            var open = false;
            await UniTask.Delay(1000);
            while (_completedAlbumTypesQueue.Count > 0)
            {
                await UniTask.Delay(300);
                var popupReceiveCard = PanelManager.Instance.GetPanel<PopupReceiveCard_Immediately>();
                var popupReward = PanelManager.Instance.GetPanel<PopupReward>();
                await UniTask.WaitUntil(() => (popupReceiveCard == null || popupReceiveCard.gameObject.activeInHierarchy == false)
                                              && (popupReward == null || popupReward.gameObject.activeInHierarchy == false));


                var albumType = _completedAlbumTypesQueue.Dequeue();

                var popup = PanelManager.Instance.OpenPanel<PopupCompleteAlbum>(new UIData().Add("AlbumType", albumType));
                open = true;
                await UniTask.WaitUntil(() => (popup == null || popup.gameObject.activeInHierarchy == false));
            }

            if (_isCompleteCardCollection.Value == 1)
            {
                var popupReward = PanelManager.Instance.GetPanel<PopupReward>();
                await UniTask.WaitUntil(() => ((popupReward == null || popupReward.gameObject.activeInHierarchy == false)));

                _isCompleteCardCollection.Value = 0;
                var reward = config.rewardInSeason;
                MySonatFramework.inventoryService.AddReward(reward, new EarnResourceLogData
                {
                    spendType = "card_collection",
                    spendId = "card_collection",
                    isFirstBuy = false,
                    source = "non_iap"
                });
                UIData uiData = new UIData();
                uiData.Add("Title", "REWARD!");
                uiData.Add("Reward", reward);
                uiData.Add("x2", false);
                PanelManager.Instance.OpenPanelByName<PopupReward>("PopupRewardCardCollection", uiData);
            }

            return open;
        }

        #region Get Data

        public AlbumConfig GetAlbumConfig(AlbumType albumType)
        {
            return config.albums.Find(album => album.type == albumType);
        }

        public CardConfig GetCardConfig(CardType cardType)
        {
            return config.cards.Find(card => card.type == cardType);
        }

        public int GetNumCardInAlbum(AlbumType albumType)
        {
            int numCard = 0;
            var albumConfig = GetAlbumConfig(albumType);
            foreach (var cardType in albumConfig.cards)
            {
                if (GetNumCard(cardType) > 0)
                {
                    numCard++;
                }
            }

            return numCard;
        }

        public int GetNumCard(CardType cardType)
        {
            if (_collectedCardTypes.Contains(cardType))
            {
                return 1;
            }

            return 0;
            // return cardInventory.GetCard(cardType);
        }

        #endregion

        #region Xử lý card mới

        public int GetNewCardCount(AlbumType albumType = AlbumType.None)
        {
            if (albumType == AlbumType.None)
            {
                return _newCardTypes.Count;
            }
            else
            {
                int numCard = 0;
                foreach (var cardType in GetAlbumConfig(albumType).cards)
                {
                    if (IsNewCardButNotSeen(cardType))
                    {
                        numCard++;
                    }
                }

                return numCard;
            }
        }

        public void RemoveNewCard(CardType cardType)
        {
            _newCardTypes.Remove(cardType);

            SaveData();
            OnNewCardCountChanged?.Invoke();
        }

        public bool IsNewCard(CardType cardType)
        {
            return _collectedCardTypes.Contains(cardType) == false;
        }

        public bool IsNewCardButNotSeen(CardType cardType)
        {
            return _newCardTypes.Contains(cardType);
        }

        #endregion


        public void AddCard(CardType cardType, int numCard)
        {
            // cardInventory.AddCard(cardType, numCard);

            var count = _collectedCardTypes.Count;
            _collectedCardTypes.Add(cardType);

            // check complete album
            var albumType = config.GetAlbumType(cardType);
            if (_completedAlbumTypes.Contains((int)albumType) == false && CheckCompleteAlbum(albumType))
            {
                Debug.Log("anhnt: complete album");
                CompleteAlbum(albumType);
            }

            // check new card
            if (count != _collectedCardTypes.Count)
            {
                _newCardTypes.Add(cardType);
                SaveData();
                OnNewCardCountChanged?.Invoke();

                if (numCard > 1)
                    _duplicatedCards.Add((int)cardType, numCard - 1);
            }
            else
            {
                _duplicatedCards[(int)cardType] = _duplicatedCards.Value.GetValueOrDefault((int)cardType, 0) + numCard;
            }
        }

        private void CompleteAlbum(AlbumType albumType)
        {
            if (_completedAlbumTypes.Contains((int)albumType) == false)
            {
                _completedAlbumTypes.Add((int)albumType);
            }

            if (_completedAlbumTypesQueue.Contains(albumType) == false)
            {
                _completedAlbumTypesQueue.Enqueue(albumType);
            }


            if (CheckCompleteCardCollection())
            {
                _isCompleteCardCollection.Value = 1;
                _numCompleteCardCollection.Value++;
                OnCompleteCollection?.Invoke();
            }

            OnCompleteAlbum?.Invoke();
            SaveData();
        }


        private bool CheckCompleteCardCollection()
        {
            foreach (var albumType in config.albums)
            {
                if (_completedAlbumTypes.Contains((int)albumType.type) == false)
                {
                    return false;
                }
            }

            return true;
        }

        public int GetCountCompleteAlbum()
        {
            return _completedAlbumTypes.Value.Count;
        }

        public int GetCountCompleteCollection()
        {
            return _numCompleteCardCollection.Value;
        }

        public void AddCardStar(int numStar)
        {
            _cardStar.Value += numStar;
            SaveData();
        }

        public bool ExchangeCardStarToReward(int index)
        {
            //if (_cardStarExchangeIndex.Value >= index)
            //{
            //    PopupToast.Cretate("You have already exchanged this reward");
            //    return false;
            //}
            var milestone = starExchangeConfig.milestones[index];
            var neededStar = milestone.star;
            var reward = milestone.reward;

            if (CardStar < neededStar)
            {
                PopupToast.Cretate("You don't have enough stars");
                return false;
            }

            _cardStar.Value -= neededStar;
            _cardStarExchangeIndex.Value = index;
            SaveData();

            var logData = new EarnResourceLogData
            {
                spendType = "card_collection",
                spendId = "card_collection",
                isFirstBuy = false,
                source = "non_iap"
            };
            MySonatFramework.inventoryService.AddReward(reward, logData);

            UIData uiData = new UIData();
            uiData.Add("Title", "REWARD!");
            uiData.Add("Reward", reward);
            uiData.Add("x2", false);
            PanelManager.Instance.OpenPanel<PopupReward>(uiData);

            return true;
        }

        public bool CheckCompleteAlbum(AlbumType albumType)
        {
            var albumConfig = GetAlbumConfig(albumType);
            foreach (var cardType in albumConfig.cards)
            {
                if (_collectedCardTypes.Contains(cardType) == false) return false;
            }

            return true;
        }

        internal bool CheckHasClaimChest(int idGift)
        {
            return _cardStar.Value >= starExchangeConfig.milestones[idGift].star;
        }

        public bool IsCollectedCard(CardType cardType)
        {
            return _collectedCardsData.Contains(((int)cardType));
        }

        public float GetPercentageAppearNewCard()
        {
            int A = GetCountCompleteAlbum();
            float Cavg;

            int cardCompleteCount = _collectedCardsData.Value.Count;
            int maxCard = 0;

            foreach (var album in config.albums)
            {
                maxCard += album.cards.Count;
            }

            Cavg = (cardCompleteCount * 1f / maxCard);

            var P = Mathf.Max(0.05f, 0.9f - 0.7f * Cavg - 0.05f * A);

            //Debug.LogError("A: " + A);
            //Debug.LogError($"numCardCollected={cardCompleteCount}, maxCard={maxCard}, Cavg={Cavg}");
            //Debug.LogError("P: " + P);

            return P;
        }

        public bool IsCardCollected(CardType cardType)
        {
            return _collectedCardTypes.Contains(cardType);
        }

        #region Send/Request Cards

        public bool IsSendableCard(CardType cardType)
        {
            return CardPackHelper.GetNumStarReward(cardType) < 3;
        }

        public int GetSendCardDailyLimit()
        {
            return config.sendLimit;
        }

        public int GetSentCardToday()
        {
            return _sentCardToday.Value;
        }

        public bool CanSendCardToday()
        {
            return GetSentCardToday() < GetSendCardDailyLimit();
        }

        public bool CanRequestCard()
        {
            return _timeResetRequestCard.Value == 0 ||
                _timeService.Instance.GetCurrentTimeUnix() > _timeResetRequestCard.Value;
        }

        public void HandleSendCardSuccess(CardType cardType)
        {
            RemoveDuplicaledCard(cardType);
            _sentCardToday.Value++;
        }

        public int GetDuplicatedNumCard(CardType cardType)
        {
            return _duplicatedCards.Value.GetValueOrDefault((int)cardType);
        }

        public void RemoveDuplicaledCard(CardType cardType)
        {
            if (_duplicatedCards.ContainsKey((int)cardType))
            {
                if (_duplicatedCards[(int)cardType] > 1)
                    _duplicatedCards[(int)cardType]--;
                else
                    _duplicatedCards.Remove((int)cardType);
            }
        }

        public void HandleRequestCardSuccess()
        {
            _timeResetRequestCard.Value = _timeService.Instance.GetCurrentTime().AddDays(1).Date.ToUnixTimeSeconds();
        }

        public long GetTimeUntilNextRequest()
        {
            return Math.Max(0, _timeResetRequestCard.Value - _timeService.Instance.GetCurrentTime().ToUnixTimeSeconds());
        }

        public async UniTask FetchPendingCards()
        {
            if (_isFetching) return;
            _isFetching = true;

            pendingCards = await MySonatFramework.GetService<HybridDataService>().GetPendingResourceResponses(OnlineResourceType.Card, MyStringComparison.Contain);

            _isFetching = false;
        }

        public void ResetTimeRequestCard()
        {
            _timeResetRequestCard.Value = 0;
        }

        public void ResetLimitSendCard()
        {
            _sentCardToday.Value = 0;
        }

        #endregion
    }
}