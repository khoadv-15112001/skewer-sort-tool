using UnityEngine;
using Cysharp.Threading.Tasks;
using SonatFramework.Systems;
using SonatFramework.Scripts.Helper;
using System;
using SonatFramework.Systems.TimeManagement;
using SonatFramework.Scripts.Feature.Lives;
using SonatFramework.Systems.EventBus;
using SonatFramework.Systems.InventoryManagement;
using Sonat.Enums;
using SonatFramework.Scripts.UIModule;
using SonatFramework.Scripts.Utils;
using SonatFramework.Scripts.SonatSDKAdapterModule;
using SonatFramework.Scripts.Feature.Shop;

namespace GrillSort.BattlePass
{
    [CreateAssetMenu(fileName = "BattlePassService", menuName = "My Services/BattlePassService")]
    public class BattlePassService : SonatServiceSo, IServiceInitialize
    {
        [SerializeField] private float delayShowReward = 8f;
        [SerializeField] public BattlePassGeneralConfig generalConfig;
        [SerializeField] private BattlePassBase[] battlePassBases;

        private Service<InventoryService> _inventoryService = new();

        public const string DATA_KEY = "BATTLEPASS_DATA_GENERAL";
        public const string LOG_KEY = "[BATTLEPASS]";
        private const string MIGRATION_XMAS_2025_KEY = "BATTLEPASS_MIGRATION_XMAS_2025";

        public int battlePassKey = 0;

        public int CurrentExp => _inventoryService.Instance.GetResource(GameResource.BattlePass_Key);
        public int CurrentMilestoneIdx => _currentMilestoneIdx.Value;

        public Action<int> OnUpdateUI;
        private IntDataPref _currentMilestoneIdx;
        private LongDataPref _expireTime;
        private IntDataPref _isUnlocked;
        private IntDataPref _theme;
        private readonly Service<TimeService> _timeService = new();

        public void Initialize()
        {
            LoadConfig();
            LoadData();

            if (!generalConfig.active)
            {
                Reset(); // reset lives
                return;
            }

            if (IsUnlocked())
            {
                CheckExpire().Forget();
            }
            else
            {
                if (CanUnlock())
                {
                    Unlock();
                }
                else
                {
                    Reset(); // reset lives
                }
            }

            if (CheckActivate())
            {
                ActivateBattlePass(true);
            }

            new EventBinding<LevelEndedEvent>(OnEndLevel);
            new EventBinding<EarnResourceEvent>(OnAddResource);
        }

        private void OnAddResource(EarnResourceEvent @event)
        {
            if (!generalConfig.active) return;
            if (@event.resource == GameResource.BattlePass_Key)
            {
                AddProgress(@event.value);
            }
        }

        private void OnEndLevel(LevelEndedEvent @event)
        {
            if (!generalConfig.active) return;
            if (@event.success && IsUnlocked())
            {
                // nếu đã full thì không nhận
                if (CheckFull()) return;

                //inventory
                var logData = new EarnResourceLogData
                {
                    spendType = "battle_pass",
                    spendId = "battle_pass",
                    isFirstBuy = false,
                    source = "non_iap"
                };
                _inventoryService.Instance.AddReward(generalConfig.winReward, logData);

                // uidata
                PanelManager.Instance.ClosePanel<PopupQuestEvent>();

                battlePassKey = 1;

                //UIData uiData = new UIData();
                //uiData.Add("Title", "REWARD!");
                //uiData.Add("Reward", generalConfig.winReward);
                //uiData.Add("x2", false);

                //SonatUtils.DelayCall(delayShowReward, () =>
                //{
                //    PanelManager.Instance.OpenPanel<PopupReward>(uiData);
                //});
            }
        }

        protected void LoadConfig()
        {
            generalConfig.liveOpsPackData = SonatSDKAdapter.GetRemoteConfig<LiveOpsPackData>($"{DATA_KEY}_liveOpsPackData", generalConfig.liveOpsPackData);
        }

        protected void LoadData()
        {
            _currentMilestoneIdx = new IntDataPref(DATA_KEY + "_currentMilestoneIdx", -1);
            _expireTime = new LongDataPref(DATA_KEY + "_expireTime");
            _isUnlocked = new IntDataPref(DATA_KEY + "_isUnlocked", 0);
            _theme = new IntDataPref(DATA_KEY + "_theme");
            foreach (var battlePassBase in battlePassBases)
            {
                battlePassBase.Initialize();
            }

            CheckTheme();
        }

        private void CheckTheme()
        {
            var now = _timeService.Instance.GetCurrentTime();
            var previousTheme = (BattlePassGeneralConfig.Theme)_theme.Value;
            BattlePassGeneralConfig.Theme newTheme;

            if (now.Month == 11)
            {
                newTheme = BattlePassGeneralConfig.Theme.Thanksgiving;
            }
            else if (now.Month == 12)
            {
                // Migration: Nếu user đã mua pack với theme Default trong tháng 12 (bản cũ chưa có Xmas)
                // thì chỉ đổi theme sang Xmas, không reset data
                if (previousTheme == BattlePassGeneralConfig.Theme.Default && CheckActivate())
                {
                    // Check nếu chưa migration
                    if (PlayerPrefs.GetInt(MIGRATION_XMAS_2025_KEY, 0) == 0)
                    {
                        Debug.Log($"{LOG_KEY} Migration: User đã mua pack với theme Default trong tháng 12. Chuyển sang Xmas theme mà không reset data.");
                        
                        // Chỉ đổi theme, không reset
                        _theme.Value = (int)BattlePassGeneralConfig.Theme.Xmas;
                        
                        // Đánh dấu đã migration
                        PlayerPrefs.SetInt(MIGRATION_XMAS_2025_KEY, 1);
                        PlayerPrefs.Save();
                        
                        // Update avatar/badge cho theme mới (nếu cần)
                        MigrateAvatarBadgeToXmas();
                        
                        return; // Không tiếp tục set theme bình thường
                    }
                }
                
                newTheme = BattlePassGeneralConfig.Theme.Xmas;
            }
            else
            {
                newTheme = BattlePassGeneralConfig.Theme.Default;
            }

            // Reset avatar/badge của theme cũ khi chuyển sang theme mới
            if (previousTheme != newTheme)
            {
                ResetPreviousThemeAvatarBadge(previousTheme);
            }

            _theme.Value = (int)newTheme;
        }

        /// <summary>
        /// Reset avatar/badge của theme cũ khi chuyển sang theme mới
        /// </summary>
        private void ResetPreviousThemeAvatarBadge(BattlePassGeneralConfig.Theme previousTheme)
        {
            var inventoryService = MySonatFramework.GetService<InventoryService>();

            switch (previousTheme)
            {
                case BattlePassGeneralConfig.Theme.Default:
                    inventoryService.SetResource(Sonat.Enums.GameResource.Avatar_Battle_Pass, 0);
                    inventoryService.SetResource(Sonat.Enums.GameResource.Badge_Battle_Pass, 0);
                    Debug.Log($"{LOG_KEY} Reset Default theme avatar/badge");
                    break;

                case BattlePassGeneralConfig.Theme.Thanksgiving:
                    inventoryService.SetResource(Sonat.Enums.GameResource.Avatar_Battle_Pass_Thanksgiving, 0);
                    inventoryService.SetResource(Sonat.Enums.GameResource.Badge_Battle_Pass_Thanksgiving, 0);
                    Debug.Log($"{LOG_KEY} Reset Thanksgiving theme avatar/badge");
                    break;

                case BattlePassGeneralConfig.Theme.Xmas:
                    inventoryService.SetResource(Sonat.Enums.GameResource.Avatar_Battle_Pass_Xmas, 0);
                    inventoryService.SetResource(Sonat.Enums.GameResource.Badge_Battle_Pass_Xmas, 0);
                    // NOTE: Avatar_Pack_Xmas và Badge_Pack_Xmas được giữ vĩnh viễn (IAP purchase)
                    Debug.Log($"{LOG_KEY} Reset Xmas theme avatar/badge (Avatar_Pack_Xmas, Badge_Pack_Xmas kept)");
                    break;
            }

            // Update profile để reset về default nếu đang dùng avatar/badge bị reset
            MySonatFramework.GetService<ProfileService>().CheckProfile();
        }

        /// <summary>
        /// Migration avatar/badge từ Default sang Xmas theme
        /// Chỉ gọi 1 lần khi migrate từ bản cũ (Default trong tháng 12) sang bản mới (Xmas)
        /// </summary>
        private void MigrateAvatarBadgeToXmas()
        {
            var inventoryService = MySonatFramework.GetService<InventoryService>();
            
            // Check nếu user có avatar/badge Default (từ việc mua pack trong tháng 12 với bản cũ)
            bool hasDefaultAvatar = inventoryService.GetResource(Sonat.Enums.GameResource.Avatar_Battle_Pass) > 0;
            bool hasDefaultBadge = inventoryService.GetResource(Sonat.Enums.GameResource.Badge_Battle_Pass) > 0;
            
            if (hasDefaultAvatar || hasDefaultBadge)
            {
                Debug.Log($"{LOG_KEY} Migration: Chuyển avatar/badge từ Default sang Xmas theme");
                
                // Clear Default avatar/badge
                inventoryService.SetResource(Sonat.Enums.GameResource.Avatar_Battle_Pass, 0);
                inventoryService.SetResource(Sonat.Enums.GameResource.Badge_Battle_Pass, 0);   
                
                // Clear Default avatar/badge
                inventoryService.SetResource(Sonat.Enums.GameResource.Avatar_Battle_Pass_Thanksgiving, 0);
                inventoryService.SetResource(Sonat.Enums.GameResource.Badge_Battle_Pass_Thanksgiving, 0);
                
                // Add Xmas avatar/badge thay thế
                inventoryService.SetResource(Sonat.Enums.GameResource.Avatar_Battle_Pass_Xmas, 1);
                inventoryService.SetResource(Sonat.Enums.GameResource.Badge_Battle_Pass_Xmas, 1);
                
                // Update profile
                MySonatFramework.GetService<ProfileService>().CheckProfile();
            }
        }


        private void Reset()
        {
            _inventoryService.Instance.SetResource(GameResource.BattlePass_Key, 0);
            _currentMilestoneIdx.Value = -1;

            var date = _timeService.Instance.GetCurrentTime();
            var lastDayOfMonth = new DateTime(date.Year, date.Month, DateTime.DaysInMonth(date.Year, date.Month), 23, 59, 59);
            _expireTime.Value = ((DateTimeOffset)lastDayOfMonth).ToUnixTimeSeconds();

            foreach (var battlePassBase in battlePassBases)
            {
                battlePassBase.Reset();
            }

            // reset reward (premium)
            MySonatFramework.GetService<LivesService>().ResetLimitLives();

            CheckTheme();

            OnUpdateUI?.Invoke(-1);
        }

        public void ResetAvatarBadge(bool isCheckProfile = true)
        {
            var inventoryService = MySonatFramework.GetService<InventoryService>();

            // Reset avatar/badge của BattlePass (tất cả theme)
            inventoryService.SetResource(Sonat.Enums.GameResource.Avatar_Battle_Pass, 0);
            inventoryService.SetResource(Sonat.Enums.GameResource.Badge_Battle_Pass, 0);

            inventoryService.SetResource(Sonat.Enums.GameResource.Avatar_Battle_Pass_Thanksgiving, 0);
            inventoryService.SetResource(Sonat.Enums.GameResource.Badge_Battle_Pass_Thanksgiving, 0);

            inventoryService.SetResource(Sonat.Enums.GameResource.Avatar_Battle_Pass_Xmas, 0);
            inventoryService.SetResource(Sonat.Enums.GameResource.Badge_Battle_Pass_Xmas, 0);

            // NOTE: Avatar_Pack_Xmas và Badge_Pack_Xmas thuộc về XmasEvent pack (IAP), không reset ở đây

            if (isCheckProfile)
                MySonatFramework.GetService<ProfileService>().CheckProfile();
        }

        public void Unlock()
        {
            _isUnlocked.Value = 1;
            Reset();
            CheckExpire().Forget();
        }

        public bool IsUnlocked()
        {
            if (!generalConfig.active) return false;
            return _isUnlocked.Value == 1;
        }

        public bool CanUnlock()
        {
            if (!generalConfig.active) return false;
            return _isUnlocked.Value == 0 && generalConfig.liveOpsPackData.CheckCondition();
        }

        private async UniTaskVoid CheckExpire()
        {
            if (_expireTime.Value > 0)
            {
                var timeToReset = GetTimeToReset();
                if (timeToReset > 0)
                {
                    await UniTask.Delay(TimeSpan.FromSeconds(timeToReset));
                }
            }

            Reset();
            CheckExpire().Forget();
        }

        public long GetTimeToReset()
        {
            return _expireTime.Value - _timeService.Instance.GetUnixTimeSeconds();
        }

        public void AddProgress(int amount)
        {
            Debug.Log($"{LOG_KEY} AddProgress: {amount} => {_inventoryService.Instance.GetResource(GameResource.BattlePass_Key)}");
            for (int i = generalConfig.GetMileStoneCount() - 1; i >= 0; i--)
            {
                if (_inventoryService.Instance.GetResource(GameResource.BattlePass_Key) >= generalConfig.GetTotalExp(i))
                {
                    _currentMilestoneIdx.Value = i;
                    OnUpdateUI?.Invoke(i);
                    break;
                }
            }
        }

        public void ActivateBattlePass(bool isInit = false)
        {
            if (!generalConfig.active) return;
            foreach (var battlePassBase in battlePassBases)
            {
                battlePassBase.Unlock();
            }

            // tăng giới hạn lives
            MySonatFramework.GetService<LivesService>().SetLimitLives(8);

            ResetAvatarBadge(false);

            foreach (var reward in GetCurrentThemeData().activateRewards.resourceDatas)
            {
                _inventoryService.Instance.AddResource(reward.resource, 1);
            }

            MySonatFramework.GetService<ProfileService>().CheckProfile();

            OnUpdateUI?.Invoke(-1);
        }

        public bool CheckActivate()
        {
            if (!generalConfig.active) return false;
            foreach (var battlePassBase in battlePassBases)
            {
                if (!battlePassBase.IsUnlocked())
                {
                    return false;
                }
            }
            return true;
        }

        public bool CanClaim()
        {
            foreach (var battlePassBase in battlePassBases)
            {
                if (battlePassBase.CanClaim(CurrentMilestoneIdx))
                {
                    return true;
                }
            }
            return false;
        }


        public bool CheckFull()
        {
            return CurrentMilestoneIdx == generalConfig.GetMileStoneCount() - 1;
        }

        public BattlePassGeneralConfig.ThemeData GetThemeData(BattlePassGeneralConfig.Theme theme)
        {
            return generalConfig.themeDatas.Find(x => x.theme == theme);
        }

        public BattlePassGeneralConfig.ThemeData GetCurrentThemeData()
        {
            return generalConfig.themeDatas.Find(x => x.theme == (BattlePassGeneralConfig.Theme)_theme.Value);
        }

        public PopupTutBattlePass OpenBattlePassTut(UIData uiData = null)
        {
            return PanelManager.Instance.OpenPanelByName<PopupTutBattlePass>(GetCurrentThemeData().namePopupTut, uiData);
        }

        public PopupBattlePass OpenBattlePass(UIData uiData = null)
        {
            return PanelManager.Instance.OpenPanelByName<PopupBattlePass>(GetCurrentThemeData().namePopupBoard, uiData);
        }

        public PopupBattlePass GetBattlePassPanel()
        {
            return PanelManager.Instance.GetPanelByName<PopupBattlePass>(GetCurrentThemeData().namePopupBoard);
        }

        public PopupActivateBattlePass OpenBattlePassActivate(UIData uiData = null)
        {
            return PanelManager.Instance.OpenPanelByName<PopupActivateBattlePass>(GetCurrentThemeData().namePopupActivate, uiData);
        }
    }
}