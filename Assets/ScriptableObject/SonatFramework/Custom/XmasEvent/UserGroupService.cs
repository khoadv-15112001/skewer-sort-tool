using System;
using Cysharp.Threading.Tasks;
using SonatFramework.Systems;
using SonatFramework.Systems.GameDataManagement;
using Sonat.IapModule;
using UnityEngine;

public enum UserGroup
{
    NonPayer,
    Minnow,
    Dolphin,
    GrandDolphin,
    Whale
}

namespace GrillSort
{
    /// <summary>
    /// Service chung để tính toán User Group/Segment dựa trên LTV, Level, và Purchase Status
    /// Mỗi event nên cache riêng user group của mình, service này chỉ cung cấp logic tính toán
    /// </summary>
    [CreateAssetMenu(fileName = "UserGroupService", menuName = "My Services/UserGroupService")]
    public class UserGroupService : SonatServiceSo, IServiceInitializeAsync
    {
        private const string CHEAT_LTV_KEY = "USER_GROUP_CHEAT_LTV";
        
        // Cheat LTV - override LTV value for testing (default = 0 nghĩa là dùng real LTV)
        public static float CheatLTV = 0f;

        public async UniTaskVoid InitializeAsync()
        {
            Debug.Log("[UserGroupService] Initialize");
            
            // Load CheatLTV từ PlayerPrefs nếu có
            LoadCheatLTV();
            
            await UniTask.CompletedTask;
        }

        /// <summary>
        /// Calculate user group based on game data (không cache, mỗi event tự cache)
        /// </summary>
        public UserGroup CalculateUserGroup()
        {
            // Use cheat LTV if > 0, otherwise use real LTV
            float ltv = CheatLTV > 0 ? CheatLTV : SonatIap.sn_ltv_iap;
            int level = MySonatFramework.userDataService.GetLevel();
            bool hasPurchased = SonatIap.PaidUser();
            
            // Nếu CheatLTV > 0 thì auto set hasPurchased = true để test paid user logic
            if (CheatLTV > 0)
            {
                hasPurchased = true;
            }

            Debug.Log($"[UserGroupService] Calculating user group - LTV: {ltv} (CheatLTV: {CheatLTV}, Real: {SonatIap.sn_ltv_iap}), Level: {level}, HasPurchased: {hasPurchased}");

            if (!hasPurchased && level >= 100)
                return UserGroup.NonPayer;
            if (!hasPurchased && level < 100)
                return UserGroup.Minnow;
            if (ltv <= 15) return UserGroup.Minnow;
            if (ltv <= 50) return UserGroup.Dolphin;
            if (ltv <= 100) return UserGroup.GrandDolphin;
            return UserGroup.Whale;
        }

        #region Cheat Methods

        /// <summary>
        /// Load CheatLTV từ PlayerPrefs (default = 0 nghĩa là dùng real LTV)
        /// </summary>
        private void LoadCheatLTV()
        {
            CheatLTV = PlayerPrefs.GetFloat(CHEAT_LTV_KEY, 0f);
            
            if (CheatLTV > 0)
                Debug.Log($"[UserGroupService] Loaded CheatLTV from PlayerPrefs: {CheatLTV}");
        }
        
        /// <summary>
        /// Save CheatLTV vào PlayerPrefs (0 = dùng real LTV)
        /// </summary>
        private void SaveCheatLTV(float ltv)
        {
            PlayerPrefs.SetFloat(CHEAT_LTV_KEY, ltv);
            PlayerPrefs.Save();
        }

        /// <summary>
        /// Cheat: Set LTV để test với LTV cao (chỉ dùng cho debug/testing)
        /// Pass 0 để reset về real LTV
        /// NOTE: Mỗi event service cần tự recalculate user group sau khi set cheat LTV
        /// </summary>
        public void SetCheatLTV(float ltv)
        {
            CheatLTV = ltv;
            
            // Lưu vào PlayerPrefs để persist qua các lần mở game
            SaveCheatLTV(ltv);
            
            if (ltv > 0)
                Debug.Log($"[UserGroupService] Cheat: LTV set to {ltv} (saved to PlayerPrefs). Each event should recalculate its cached user group.");
            else
                Debug.Log($"[UserGroupService] Cheat: LTV reset to real value (cleared). Each event should recalculate its cached user group.");
        }

        /// <summary>
        /// Cheat: Reset CheatLTV về 0 (dùng real LTV) (chỉ dùng cho debug/testing)
        /// </summary>
        public void CheatClearLTV()
        {
            SetCheatLTV(0f);
        }

        #endregion
    }
}

