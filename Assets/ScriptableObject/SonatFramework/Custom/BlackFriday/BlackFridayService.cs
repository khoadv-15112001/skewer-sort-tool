using System;
using System.Collections.Generic;
using GrillSort.RealTime;
using GrillSort.UI;
using SonatFramework.Systems;
using SonatFramework.Systems.GameDataManagement;
using Sonat.Enums;
using UnityEngine;
using Random = UnityEngine.Random;
using Sonat.IapModule;
using SonatFramework.Systems.InventoryManagement.GameResources;
using Cysharp.Threading.Tasks;

namespace GrillSort.BlackFriday
{
    [CreateAssetMenu(fileName = "BlackFridayService", menuName = "My Services/BlackFridayService")]
    public class BlackFridayService : SonatServiceSo, IServiceInitializeAsync
    {
        public BlackFridayConfig config;
        private readonly Service<RealTimeService> realTimeService = new();
        private readonly Service<DataService> dataService = new();
        private readonly Service<UserGroupService> userGroupService = new();

        private BlackFridayData _data;

        public Action OnSpinChanged;
        public Action OnPurchaseCountChanged;

        public BlackFridayData Data { get => _data;}

        private const int MAX_LIFETIME_PURCHASES = 2;
        
        // Cached user group cho event này - được init 1 lần duy nhất khi bắt đầu event
        private UserGroup? cachedUserGroup = null;

        public async UniTaskVoid InitializeAsync()
        {
            Debug.Log("anhnt: BlackFridayService Initialize");
            
            await LoadData();
            
            // Check và reset nếu sang năm mới
            CheckAndResetForNewYear();
            
            if (IsExpired()) ResetData();
        }

        private async UniTask LoadData()
        {
            _data = dataService.Instance.GetData<BlackFridayData>("BlackFridayData") ?? new BlackFridayData();
        }

        private void SaveData()
        {
            dataService.Instance.SetData("BlackFridayData", _data);
        }

        /// <summary>
        /// Check if event is active (within time range AND user has available purchases)
        /// </summary>
        public bool IsInEvent()
        {
            return IsInTimeRange() && CanPurchase();
        }

        /// <summary>
        /// Check if event is within time range only
        /// </summary>
        public bool IsInTimeRange()
        {
            var now = realTimeService.Instance.GetCurrentTime();
            DateTime start = DateTime.Parse(config.startDate);
            DateTime end = DateTime.Parse(config.endDate);
            return now >= start && now <= end;
        }

        /// <summary>
        /// Check if event time has expired OR user has no more purchases available
        /// </summary>
        public bool IsExpired()
        {
            return !IsInTimeRange();
        }

        public void ResetData()
        {
            _data = new BlackFridayData();
            cachedUserGroup = null; // Reset cache để tính lại cho event mới
            SaveData();
            Debug.Log("[BlackFriday] Data and user group cache reset");
        }

        /// <summary>
        /// Check xem có sang năm mới không, nếu có thì reset lifetime purchase count và cached user type
        /// </summary>
        private void CheckAndResetForNewYear()
        {
            int currentYear = realTimeService.Instance.GetCurrentTime().Year;

            if (_data.lastYear == 0)
            {
                // Lần đầu tiên khởi tạo - lưu năm hiện tại
                _data.lastYear = currentYear;
                SaveData();
                Debug.Log($"[BlackFriday] Initialized year tracking: {currentYear}");
            }
            else if (currentYear > _data.lastYear)
            {
                Debug.Log($"[BlackFriday] New year detected! {_data.lastYear} -> {currentYear}. Resetting all data...");
                
                // Reset toàn bộ BlackFridayData
                _data = new BlackFridayData
                {
                    lastYear = currentYear
                };
                cachedUserGroup = null; // Reset cache để tính lại cho năm mới
                SaveData();
                
                Debug.Log("[BlackFriday] All Black Friday data reset for new year");
                OnPurchaseCountChanged?.Invoke();
            }
        }

        /// <summary>
        /// Init cached user group nếu chưa có (cache riêng cho event này)
        /// </summary>
        private void InitUserGroupIfNeeded()
        {
            if (!cachedUserGroup.HasValue)
            {
                // Load từ saved data trước
                if (_data.cachedUserGroup >= 0 && _data.cachedUserGroup < System.Enum.GetValues(typeof(UserGroup)).Length)
                {
                    cachedUserGroup = (UserGroup)_data.cachedUserGroup;
                    Debug.Log($"[BlackFriday] Loaded cached user group from save: {cachedUserGroup.Value}");
                }
                else
                {
                    // Chưa có trong save -> Calculate lần đầu tiên
                    cachedUserGroup = userGroupService.Instance.CalculateUserGroup();
                    _data.cachedUserGroup = (int)cachedUserGroup.Value;
                    SaveData();
                    Debug.Log($"[BlackFriday] Initialized user group for this event: {cachedUserGroup.Value}");
                }
            }
        }

        public UserGroup GetUserGroup()
        {
            InitUserGroupIfNeeded();
            return cachedUserGroup.Value;
        }

        /// <summary>
        /// Set user group manually (dùng cho cheat hoặc force override)
        /// </summary>
        public void SetUserGroup(UserGroup userGroup)
        {
            cachedUserGroup = userGroup;
            _data.cachedUserGroup = (int)userGroup;
            SaveData();
            Debug.Log($"[BlackFriday] User group changed to: {userGroup} and saved");
        }

        /// <summary>
        /// Recalculate user group cho event này (force refresh từ UserGroupService)
        /// </summary>
        public UserGroup RecalculateUserGroup()
        {
            cachedUserGroup = userGroupService.Instance.CalculateUserGroup();
            _data.cachedUserGroup = (int)cachedUserGroup.Value;
            SaveData();
            Debug.Log($"[BlackFriday] User group recalculated for this event: {cachedUserGroup.Value}");
            return cachedUserGroup.Value;
        }

        public int GetSpinPrice()
        {
            var userType = GetUserGroup();
            var spinData = config.spinPrices.Find(x => x.userType == userType);
            if (spinData == null) return 100;
            return _data.spinCount == 0 ? spinData.firstSpinPrice : spinData.spinPrice;
        }

        public (int discountPercent, ShopItemKey packKey) SpinAndGetPack()
        {
            var userType = GetUserGroup();
            var discountTable = config.discountTables.Find(x => x.userType == userType);
            if (discountTable == null) return (20, ShopItemKey.None);

            int phase = _data.spinCount switch
            {
                0 => 1,
                1 => 2,
                _ => 3
            };

            var chosenDiscount = RollDiscount(discountTable.discountProbabilities, phase);
            var packKey = GetPackKey(userType, chosenDiscount);

            _data.spinCount++;
            _data.lastDiscount = chosenDiscount;
            _data.lastPackKey = packKey;
            Debug.Log($"anhnt: SpinAndGetPack userType={userType}, phase={phase}, chosenDiscount={chosenDiscount}, packKey={packKey}, spinCount={_data.spinCount}");
            SaveData();

            OnSpinChanged?.Invoke();
            return (chosenDiscount, packKey);
        }

        private int RollDiscount(List<DiscountProbability> list, int phase)
        {
            int total = 0;
            foreach (var item in list)
            {
                total += phase switch
                {
                    1 => item.weightFirst,
                    2 => item.weightSecond,
                    _ => item.weightLater
                };
            }

            int roll = Random.Range(0, total);
            int acc = 0;
            foreach (var item in list)
            {
                int weight = phase switch
                {
                    1 => item.weightFirst,
                    2 => item.weightSecond,
                    _ => item.weightLater
                };

                acc += weight;
                if (roll < acc)
                    return item.discountPercent;
            }

            return 20;
        }

        public ShopItemKey GetPackKey(UserGroup userGroup, int discountPercent)
        {
            return userGroup switch
            {
                UserGroup.NonPayer => discountPercent switch
                {
                    60 => ShopItemKey.Black_Friday_Non_Player_60,
                    40 => ShopItemKey.Black_Friday_Non_Player_40,
                    30 => ShopItemKey.Black_Friday_Non_Player_30,
                    20 => ShopItemKey.Black_Friday_Non_Player_20,
                    _ => ShopItemKey.Black_Friday_Non_Player
                },
                UserGroup.Minnow => discountPercent switch
                {
                    60 => ShopItemKey.Black_Friday_Minnow_60,
                    40 => ShopItemKey.Black_Friday_Minnow_40,
                    30 => ShopItemKey.Black_Friday_Minnow_30,
                    20 => ShopItemKey.Black_Friday_Minnow_20,
                    _ => ShopItemKey.Black_Friday_Minnow
                },
                UserGroup.Dolphin => discountPercent switch
                {
                    60 => ShopItemKey.Black_Friday_Dolphin_60,
                    40 => ShopItemKey.Black_Friday_Dolphin_40,
                    30 => ShopItemKey.Black_Friday_Dolphin_30,
                    20 => ShopItemKey.Black_Friday_Dolphin_20,
                    _ => ShopItemKey.Black_Friday_Dolphin
                },
                UserGroup.GrandDolphin => discountPercent switch
                {
                    60 => ShopItemKey.Black_Friday_Grand_Dolphin_60,
                    40 => ShopItemKey.Black_Friday_Grand_Dolphin_40,
                    30 => ShopItemKey.Black_Friday_Grand_Dolphin_30,
                    20 => ShopItemKey.Black_Friday_Grand_Dolphin_20,
                    _ => ShopItemKey.Black_Friday_Grand_Dolphin
                },
                UserGroup.Whale => discountPercent switch
                {
                    60 => ShopItemKey.Black_Friday_Whale_60,
                    40 => ShopItemKey.Black_Friday_Whale_40,
                    30 => ShopItemKey.Black_Friday_Whale_30,
                    20 => ShopItemKey.Black_Friday_Whale_20,
                    _ => ShopItemKey.Black_Friday_Whale
                },
                _ => ShopItemKey.None,
            };
        }

        internal ResourceData GetCoinReward()
        {
            var userType = GetUserGroup();
            var rewardData = config.rewardsPerSpin.Find(x => x.userType == userType);
            if (rewardData == null)
            {
                // Fallback default reward
                return new ResourceData() { resource = GameResource.Coin, quantity = 2500 };
            }
            return rewardData.reward;
        }

        internal long GetTimeExp()
        {
            return DateTimeOffset.Parse(config.endDate).ToUnixTimeSeconds();
        }

        internal ShopItemKey GetBlackFridayPackKey(int discount)
        {
            return GetPackKey(GetUserGroup(), discount);
        }

        /// <summary>
        /// Lấy discount hiện tại đang hiển thị (0 nếu chưa spin lần nào)
        /// </summary>
        public int GetCurrentDiscount()
        {
            return _data.currentDiscount;
        }

        /// <summary>
        /// Set discount hiện tại (gọi sau khi spin completed)
        /// </summary>
        public void SetCurrentDiscount(int discount)
        {
            _data.currentDiscount = discount;
            SaveData();
            Debug.Log($"[BlackFriday] Current discount set to {discount}%");
        }

        /// <summary>
        /// Lấy số lần đã mua pack trong lifetime (permanent, không reset)
        /// </summary>
        public int GetLifetimePurchaseCount()
        {
            return _data.lifetimePurchaseCount;
        }

        /// <summary>
        /// Tăng số lần mua pack (gọi sau khi mua thành công)
        /// </summary>
        public void IncrementPurchaseCount()
        {
            _data.lifetimePurchaseCount++;
            SaveData();
            
            Debug.Log($"[BlackFriday] Purchase count increased to {_data.lifetimePurchaseCount}/{MAX_LIFETIME_PURCHASES}");
            OnPurchaseCountChanged?.Invoke();
        }

        /// <summary>
        /// Kiểm tra xem user còn có thể mua pack không
        /// </summary>
        public bool CanPurchase()
        {
            return _data.lifetimePurchaseCount < MAX_LIFETIME_PURCHASES;
        }

        /// <summary>
        /// Lấy số lần mua còn lại
        /// </summary>
        public int GetRemainingPurchases()
        {
            return Mathf.Max(0, _data.lifetimePurchaseCount);
        }

        /// <summary>
        /// Cheat: Reset toàn bộ event data (chỉ dùng cho debug/testing)
        /// NOTE: CheatLTV sẽ KHÔNG bị reset để có thể test với LTV cao trước khi event init
        /// </summary>
        public void CheatResetAllEventData()
        {
            int currentYear = _data.lastYear; // Giữ lại lastYear
            if (currentYear == 0)
            {
                currentYear = realTimeService.Instance.GetCurrentTime().Year;
            }
            
            _data = new BlackFridayData
            {
                lastYear = currentYear
            };
            cachedUserGroup = null; // Reset cache để tính lại cho event mới
            
            SaveData();
            
            float cheatLTV = UserGroupService.CheatLTV;
            if (cheatLTV > 0)
                Debug.Log($"[BlackFriday] Cheat: All event data reset (user group will be recalculated with CheatLTV={cheatLTV})");
            else
                Debug.Log("[BlackFriday] Cheat: All event data reset (user group will be recalculated with real LTV)");
            
            OnSpinChanged?.Invoke();
            OnPurchaseCountChanged?.Invoke();
        }

        /// <summary>
        /// Cheat: Simulate next year để test auto reset (chỉ dùng cho debug/testing)
        /// </summary>
        public void CheatSimulateNextYear()
        {
            _data.lastYear = _data.lastYear - 1; // Set về năm trước để trigger reset khi restart
            SaveData();
            
            Debug.Log($"[BlackFriday] Cheat: Simulated next year. Restart game to see reset. (Set year to {_data.lastYear})");
        }
    }

    [Serializable]
    public class BlackFridayData
    {
        public int spinCount;
        public int lastDiscount; // Discount của lần spin cuối
        public ShopItemKey lastPackKey;
        public int currentDiscount; // Discount hiện tại đang hiển thị (0 nếu chưa spin)
        public int cachedUserGroup = -1; // User group đã được cache cho event này (-1 = chưa init)
        
        // Lifetime data - reset mỗi năm
        public int lifetimePurchaseCount; // Số lần đã mua trong năm
        public int lastYear; // Năm cuối cùng user mở game
    }
}
