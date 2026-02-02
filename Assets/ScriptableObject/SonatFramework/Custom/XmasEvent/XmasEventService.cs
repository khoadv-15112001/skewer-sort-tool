using System;
using System.Linq;
using Cysharp.Threading.Tasks;
using SonatFramework.Systems;
using SonatFramework.Scripts.SonatSDKAdapterModule;
using SonatFramework.Systems.GameDataManagement;
using SonatFramework.Scripts.Feature.Shop;
using SonatFramework.Systems.InventoryManagement.GameResources;
using GrillSort.RealTime;
using UnityEngine;
using Sonat.IapModule;

namespace GrillSort.XmasEvent
{
    [CreateAssetMenu(fileName = "XmasEventService", menuName = "My Services/XmasEventService")]
    public class XmasEventService : SonatServiceSo, IServiceInitialize
    {
        public XmasEventConfig config;
        private readonly Service<DataService> _dataService = new();
        private readonly Service<RealTimeService> _realTime = new();
        private readonly Service<UserGroupService> _userGroupService = new();
        private readonly Service<ShopService> _shopService = new();
        public XmasEventData data;

        private const string DataKey = "XmasEventData";
        public Action OnStateChanged;
        public Action OnPackPurchased;
        
        // Cached user group cho event này - được init 1 lần duy nhất khi bắt đầu event
        private UserGroup? cachedUserGroup = null;

        public void Initialize()
        {
            // Validate config first
            if (config == null)
            {
                Debug.LogWarning("[XmasEvent] Config is null - event will not be initialized");
                return;
            }

            if (string.IsNullOrEmpty(config.startDate) || string.IsNullOrEmpty(config.endDate))
            {
                Debug.LogWarning("[XmasEvent] Start date or end date is not set in config");
                return;
            }

            LoadData();
            
            // Check reset if out of time range
            if (!IsEventActive())
            {
                CheckAndResetData();
            }

            // Load remote config if in time range
            var now = _realTime.Instance.GetCurrentTime();
            if (now >= DateTime.Parse(config.startDate) && now <= DateTime.Parse(config.endDate))
            {
                config = SonatSDKAdapter.GetRemoteConfig("xmas_event_config", config);
            }
        }

        private void LoadData()
        {
            data = _dataService.Instance.GetData<XmasEventData>(DataKey) ?? new XmasEventData();
        }

        private void SaveData()
        {
            _dataService.Instance.SetData(DataKey, data);
            OnStateChanged?.Invoke();
        }

        private void CheckAndResetData()
        {
            // Reset data nếu event đã hết hạn
            if (data.hasParticipated && !IsEventActive())
            {
                data = new XmasEventData();
                cachedUserGroup = null;
                SaveData();
                Debug.Log("[XmasEvent] Event expired - data reset");
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
                if (data.cachedUserGroup >= 0 && data.cachedUserGroup < System.Enum.GetValues(typeof(UserGroup)).Length)
                {
                    cachedUserGroup = (UserGroup)data.cachedUserGroup;
                    Debug.Log($"[XmasEvent] Loaded cached user group from save: {cachedUserGroup.Value}");
                }
                else
                {
                    // Chưa có trong save -> Calculate lần đầu tiên
                    cachedUserGroup = _userGroupService.Instance.CalculateUserGroup();
                    data.cachedUserGroup = (int)cachedUserGroup.Value;
                    SaveData();
                    Debug.Log($"[XmasEvent] Initialized user group for this event: {cachedUserGroup.Value}");
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
            data.cachedUserGroup = (int)userGroup;
            SaveData();
            Debug.Log($"[XmasEvent] User group changed to: {userGroup} and saved");
        }

        /// <summary>
        /// Recalculate user group cho event này (force refresh từ UserGroupService)
        /// </summary>
        public UserGroup RecalculateUserGroup()
        {
            cachedUserGroup = _userGroupService.Instance.CalculateUserGroup();
            data.cachedUserGroup = (int)cachedUserGroup.Value;
            SaveData();
            Debug.Log($"[XmasEvent] User group recalculated for this event: {cachedUserGroup.Value}");
            return cachedUserGroup.Value;
        }

        /// <summary>
        /// Lấy current pack index (0-based: 0, 1, 2)
        /// </summary>
        public int GetCurrentPackIndex()
        {
            return data.currentTierIndex;
        }

        /// <summary>
        /// Lấy segment data tương ứng với user group
        /// </summary>
        public XmasSegmentData GetCurrentSegment()
        {
            if (config == null || config.segmentData == null)
            {
                Debug.LogWarning("[XmasEvent] Config or segment data is null");
                return null;
            }

            var seg = GetUserGroup();
            return config.segmentData.FirstOrDefault(s => s.segmentType == seg);
        }

        /// <summary>
        /// Lấy current pack tier hiện tại
        /// </summary>
        public XmasPackTier GetCurrentPack()
        {
            var seg = GetCurrentSegment();
            if (seg == null || seg.packTiers == null || seg.packTiers.Count == 0) 
            {
                Debug.LogWarning($"[XmasEvent] No segment or pack tiers found for user group: {GetUserGroup()}");
                return null;
            }
            
            int idx = Mathf.Clamp(data.currentTierIndex, 0, seg.packTiers.Count - 1);
            return seg.packTiers[idx];
        }

        /// <summary>
        /// Lấy tất cả 3 pack tiers của segment hiện tại
        /// </summary>
        public XmasPackTier[] GetAllPackTiers()
        {
            var seg = GetCurrentSegment();
            if (seg == null || seg.packTiers == null) return new XmasPackTier[0];
            return seg.packTiers.ToArray();
        }

        /// <summary>
        /// Lấy rewards của một pack từ ShopConfig
        /// </summary>
        public RewardData GetPackRewards(int tierIndex)
        {
            var packTiers = GetAllPackTiers();
            if (tierIndex < 0 || tierIndex >= packTiers.Length)
            {
                Debug.LogWarning($"[XmasEvent] Invalid tier index: {tierIndex}");
                return null;
            }

            var packTier = packTiers[tierIndex];
            if (packTier == null)
            {
                Debug.LogWarning($"[XmasEvent] Pack tier {tierIndex} is null");
                return null;
            }

            return GetPackRewardsByShopKey(packTier.shopItemKey);
        }

        /// <summary>
        /// Lấy rewards của current pack từ ShopConfig
        /// </summary>
        public RewardData GetCurrentPackRewards()
        {
            var currentPack = GetCurrentPack();
            if (currentPack == null)
            {
                return null;
            }

            return GetPackRewardsByShopKey(currentPack.shopItemKey);
        }

        /// <summary>
        /// Lấy rewards từ ShopConfig theo ShopItemKey
        /// </summary>
        private RewardData GetPackRewardsByShopKey(Sonat.Enums.ShopItemKey shopItemKey)
        {
            var packData = _shopService.Instance.GetPackData(shopItemKey);
            if (packData == null)
            {
                Debug.LogWarning($"[XmasEvent] Pack data not found in ShopConfig for key: {shopItemKey}");
                return null;
            }

            return packData.rewardData;
        }

        /// <summary>
        /// Check xem có thể mua pack hiện tại không
        /// </summary>
        public bool CanBuyCurrentPack()
        {
            return IsEventActive() && data.currentTierIndex < 3;
        }

        /// <summary>
        /// Check xem đã mua hết 3 pack chưa
        /// </summary>
        public bool IsCompleted()
        {
            return data.currentTierIndex >= 3;
        }

        /// <summary>
        /// Callback sau khi mua pack thành công
        /// </summary>
        public void OnPackBought(int tierIndex)
        {
            if (tierIndex != data.currentTierIndex)
            {
                Debug.LogWarning($"[XmasEvent] Pack tier mismatch: expected {data.currentTierIndex}, got {tierIndex}");
                return;
            }

            data.currentTierIndex++; // Chuyển sang pack tiếp theo
            data.hasParticipated = true;
            SaveData();
            
            Debug.Log($"[XmasEvent] Pack {tierIndex} purchased. Next pack index: {data.currentTierIndex}");
            OnPackPurchased?.Invoke();
        }

        /// <summary>
        /// Check event có đang active không
        /// </summary>
        public bool IsEventActive()
        {
            if (config == null || string.IsNullOrEmpty(config.startDate) || string.IsNullOrEmpty(config.endDate))
            {
                return false;
            }

            try
            {
                var now = _realTime.Instance.GetCurrentTime();
                return now >= DateTime.Parse(config.startDate) && now <= DateTime.Parse(config.endDate);
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[XmasEvent] Error checking event active: {e.Message}");
                return false;
            }
        }

        /// <summary>
        /// Check event có nên show không (active + chưa complete)
        /// </summary>
        public bool ShouldShowEvent()
        {
            return IsEventActive() && !IsCompleted();
        }

        /// <summary>
        /// Get time remaining cho event (seconds)
        /// </summary>
        public long GetTimeRemaining()
        {
            if (config == null || string.IsNullOrEmpty(config.endDate))
            {
                return 0;
            }

            try
            {
                var now = _realTime.Instance.GetCurrentTime();
                var end = DateTime.Parse(config.endDate);
                var remaining = (long)(end - now).TotalSeconds;
                return (long)Mathf.Max(0, remaining);
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[XmasEvent] Error getting time remaining: {e.Message}");
                return 0;
            }
        }

        /// <summary>
        /// Cheat: Reset toàn bộ event data
        /// </summary>
        public void CheatResetEventData()
        {
            data = new XmasEventData();
            cachedUserGroup = null;
            SaveData();
            Debug.Log("[XmasEvent] Cheat: Event data reset");
        }

        internal bool CanShow()
        {
            return !IsCompleted() && IsEventActive();
        }

        internal void SetHasParticipated(bool v)
        {
            data.hasParticipated = v;
            SaveData();
        }
    }

    [Serializable]
    public class XmasEventData
    {
        public int currentTierIndex = 0; // 0,1,2: current pack index; >=3 => completed
        public int cachedUserGroup = -1; // User group đã được cache cho event này (-1 = chưa init)
        public bool hasParticipated = false; // Đã tham gia event chưa (để reset khi event mới)
    }
}
