using UnityEngine;
using SonatFramework.Systems;
using Cysharp.Threading.Tasks;
using System.Threading.Tasks;
using SonatFramework.Systems.GameDataManagement;
using System;
using SonatFramework.Systems.InventoryManagement;
using SonatFramework.Systems.EventBus;
using Sonat.Enums;
using SonatFramework.Scripts.Helper;

namespace GrillSort.PiggyBank
{

    [CreateAssetMenu(fileName = "PiggyBankService", menuName = "My Services/Piggy Bank Service")]
    public class PiggyBankService : SonatServiceSo, IServiceInitializeAsync
    {
        [SerializeField] private PiggyBankConfig _piggyBankConfig;



        public PiggyBankConfig PiggyBankConfig => _piggyBankConfig;
        public PiggyBankData PiggyBankData => _piggyBankData;
        public Action OnDataChanged;

        private readonly Service<DataService> _dataService = new();
        private readonly Service<InventoryService> _inventoryService = new();
        private PiggyBankData _piggyBankData;

        public int Point => _point;
        private int _point;

        private const string PIGGY_BANK_DATA_KEY = "PIGGY_BANK_DATA";

        private IntDataPref _isUnlocked;
        
        public async UniTaskVoid InitializeAsync()
        {
            await LoadData();
            await LoadConfig();
            
            new EventBinding<AddItemEvent>(OnAddCurrency);
        }

        private void OnAddCurrency(AddItemEvent @event)
        {
            if (@event.resource != GameResource.PiggyPoint) return;
            AddPiggyPoints(@event.quantity);
        }

        #region Data/ Config
        private async Task LoadConfig()
        {
            // throw new NotImplementedException();
        }

        private async Task LoadData()
        {
            _piggyBankData = _dataService.Instance.GetData<PiggyBankData>(PIGGY_BANK_DATA_KEY);
            if (_piggyBankData == null)
            {
                _piggyBankData = new PiggyBankData();
                _dataService.Instance.SetData(PIGGY_BANK_DATA_KEY, _piggyBankData);
            }

            _point = _inventoryService.Instance.GetResource(GameResource.PiggyPoint);

            _isUnlocked = new IntDataPref(PIGGY_BANK_DATA_KEY + "_isUnlocked", 0);
        }

        private void SaveData()
        {
            _dataService.Instance.SetData(PIGGY_BANK_DATA_KEY, _piggyBankData);

            OnDataChanged?.Invoke();
        }
        #endregion

        #region Change point Data
        public void AddPiggyPoints(int point, EarnResourceLogData logData = null)
        {
            var tierConfig = _piggyBankConfig.GetPiggyTierConfig(_piggyBankData.piggyTier);

            // points
            var newValue = _point + point;
            SetPoint(newValue);

            // reward idx
            _piggyBankData.piggyRewardIdx = tierConfig.GetRewardIdxByPoints(_point);

            SaveData();
        }

        public bool CanUpdatePiggyTier()
        {
            return _point >= _piggyBankConfig.GetPiggyTierConfig(_piggyBankData.piggyTier).GetMaxPoint();
        }

        public void UpdatePiggyTier()
        {
            var newValue = _piggyBankData.piggyTier + 1;
            var maxValue = _piggyBankConfig.piggyTiers.Count - 1;
            _piggyBankData.piggyTier = Mathf.Clamp(newValue, 0, maxValue);

            ResetPoint();
        }

        private void SetPoint(int point)
        {
            var maxValue = _piggyBankConfig.GetPiggyTierConfig(_piggyBankData.piggyTier).GetMaxPoint();
            _point = Mathf.Clamp(point, 0, maxValue);
            _inventoryService.Instance.SetResource(GameResource.PiggyPoint, _point);
            _inventoryService.Instance.NotiUpdateResource(GameResource.PiggyPoint);
        }

        private void ResetPoint()
        {

            SetPoint(0);
            _piggyBankData.piggyRewardIdx = -1;
            _piggyBankData.piggyClaimedRewardIdx = -1;

            SaveData();
        }

        public void ResetData()
        {
            _piggyBankData = new PiggyBankData();
            SaveData();
        }
        #endregion

        public int GetCurrentRewardQuantity()
        {
            var config = _piggyBankConfig.GetPiggyTierConfig(_piggyBankData.piggyTier);
            return config.GetRewardCoins(_piggyBankData.piggyRewardIdx);
        }

        public bool CanClaimReward()
        {
            var currentRewardIdx = _piggyBankData.piggyRewardIdx;
            var claimedRewardIdx = _piggyBankData.piggyClaimedRewardIdx;
            return currentRewardIdx != claimedRewardIdx;
        }

        public void ClaimReward()
        {
            _piggyBankData.piggyClaimedRewardIdx = _piggyBankData.piggyRewardIdx;
            var reward = GetCurrentRewardQuantity();
            var logData = new EarnResourceLogData()
            {
                spendId = "piggy_bank",
                spendType = "piggy_bank"
            };
            _inventoryService.Instance.AddResource(GameResource.Coin, reward, logData, false);
            EventBus<AddItemEvent>.Raise(new AddItemEvent()
            {
                resource = GameResource.Coin,
                quantity = reward,
                position = Vector3.zero,
                collectEffect = new CollectEffectMultiple()
            });
            SaveData();
            // ResetPoint();
        }

        public int GetAddedReward()
        {
            var newValue = _point + _piggyBankConfig.reward;
            var maxValue = _piggyBankConfig.GetPiggyTierConfig(_piggyBankData.piggyTier).GetMaxPoint();
            return maxValue > newValue ? _piggyBankConfig.reward : maxValue - _point;
        }

        public ShopItemKey GetCurrentRewardShopKey()
        {
            var config = _piggyBankConfig.GetPiggyTierConfig(_piggyBankData.piggyTier);
            return config.GetRewardShopKey(_piggyBankData.piggyRewardIdx);
        }


        public bool CanUnlock()
        {
            var userDay = MySonatFramework.userDataService.UserDay;
            var session = MySonatFramework.userDataService.SessionToday;
            // Debug.Log("CanUnlock: " + userDay + " " + session);
            return _piggyBankConfig.liveOpsPackData.CheckCondition();
        }

        public bool IsUnlocked()
        {
            return _isUnlocked.Value == 1;
        }

        public void Unlock()
        {
            _isUnlocked.Value = 1;
        }
    }

    public class PiggyBankData
    {
        public int piggyTier = 0; // mốc hiện tại
        public int piggyRewardIdx = -1; // mốc phần thưởng hiện tại
        public int piggyClaimedRewardIdx = -1; // mốc phần thưởng đã nhận
    }
}