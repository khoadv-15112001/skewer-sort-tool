using Cysharp.Threading.Tasks;
using Helper;
using SonatFramework.Scripts.Helper;
using SonatFramework.Scripts.SonatSDKAdapterModule;
using SonatFramework.Systems;
using SonatFramework.Systems.EventBus;
using SonatFramework.Systems.InventoryManagement;
using SonatFramework.Systems.InventoryManagement.GameResources;
using SonatFramework.Systems.TimeManagement;
using SonatFramework.Systems.UserData;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GrillSort.Rail
{
    [CreateAssetMenu(fileName = "RailService", menuName = "My Services/Rail/Service")]
    public class RailService : SonatServiceSo, IServiceInitializeAsync
    {
        public static RailService Instance => SonatSystem.GetService<RailService>();

        [SerializeField] private RailConfig _config;
        public RailConfig Config => _config;

        public IntDataPref IsUnlocked;
        public IntDataPref IsActive;
        public IntDataPref IsJoin;
        public IntDataPref HasEverJoined;
        public IntDataPref IsCompleted;
        public IntDataPref HasShownFirstPopup;
        public IntDataPref CurrentStage;
        public IntDataPref CurrentMilestone;
        public IntDataPref LastToken;
        public LongDataPref LastFreeTokenClaimTime;
        public LongDataPref CurrentEventStartTime; // Lưu thời gian bắt đầu của event cycle hiện tại
        public LongDataPref EventCooldownEndTime; // Lưu thời gian thứ 6 tiếp theo để bắt đầu event mới

        // Lưu tạm rewards khi mở quà, sẽ được hiển thị khi đóng popup
        public RewardData AccumulatedRewards { get; private set; } = new RewardData();

        // Thời gian hồi free token (2 giờ = 7200 giây)
        private const long FREE_TOKEN_COOLDOWN = 2 * 60 * 60;
        private const int FREE_TOKEN_AMOUNT = 5;

        // Event duration: 10 ngày (Friday → Sunday tuần sau)
        private const int EVENT_DURATION_DAYS = 10;

        public static Action OnTick;

        private InventoryService inventoryService => SonatSystem.GetService<InventoryService>();
        private TimeService timeService => MySonatFramework.GetService<TimeService>();

        public async UniTaskVoid InitializeAsync()
        {
            IsUnlocked = new IntDataPref("rail_is_unlocked");
            IsActive = new IntDataPref("rail_is_active");
            IsJoin = new IntDataPref("rail_is_join");
            HasEverJoined = new IntDataPref("rail_has_ever_joined");
            IsCompleted = new IntDataPref("rail_is_completed");
            HasShownFirstPopup = new IntDataPref("rail_has_shown_first_popup");
            CurrentStage = new IntDataPref("rail_current_stage", 1);
            CurrentMilestone = new IntDataPref("rail_current_milestone", 1);
            LastToken = new IntDataPref("rail_last_token");
            LastFreeTokenClaimTime = new LongDataPref("rail_last_free_token_claim_time");
            CurrentEventStartTime = new LongDataPref("rail_current_event_start_time");
            EventCooldownEndTime = new LongDataPref("rail_event_cooldown_end_time");

            await UniTask.Yield();

            TryUnlock();
            CheckAndResetEventIfNeeded(); // Check xem có phải event mới không
            //
            new EventBinding<LevelEndedEvent>(OnLevelEnd);
            
            TickService.Register(Tick, TickService.TickPhase.TickRealtime);
        }

        private void Tick()
        {
            if (!IsUnlocked.BoolValue) return;

            DateTime now = GetTimeNow();

            // Case 1: Đang trong event → check xem đã hết 10 ngày chưa
            if (IsActive.BoolValue)
            {
                bool isInTime = IsInTimeEvent();
                
                // Hết thời gian event → deactivate (nếu không có popup đang mở)
                if (!isInTime && PopupRail.ActivePopupCount == 0)
                {
                    DeactivateEvent();
                }
            }
            // Case 2: Đang không trong event → check xem đã đến cooldown chưa
            else
            {
                // Nếu có cooldown → check xem đã đến thời gian chưa
                if (EventCooldownEndTime.Value > 0)
                {
                    long nowUnix = now.ToUnixTimeSeconds();
                    if (nowUnix >= EventCooldownEndTime.Value)
                    {
                        ActivateEventFromNearestFriday();
                    }
                }
                // Nếu chưa có cooldown → activate luôn từ thứ 6 gần nhất
                else
                {
                    ActivateEventFromNearestFriday();
                }
            }

            OnTick?.Invoke();
        }

        /// <summary>
        /// Activate event từ thứ 6 gần nhất
        /// </summary>
        private void ActivateEventFromNearestFriday()
        {
            DateTime now = GetTimeNow();
            DateTime nearestFriday = GetNearestFriday(now);

            // Reset data (vì đã qua cooldown nên chắc chắn là event mới)
            if (CurrentEventStartTime.Value > 0)
            {
                Debug.Log($"[RailService] New event cycle, resetting data...");
                ResetEventData();
            }

            IsActive.BoolValue = true;

            // Lưu thời gian bắt đầu event = thứ 6 gần nhất
            CurrentEventStartTime.Value = nearestFriday.ToUnixTimeSeconds();
            
            // Set cooldown = event start + 14 ngày (thứ 6 tuần sau nữa)
            DateTime cooldownEnd = nearestFriday.AddDays(14);
            EventCooldownEndTime.Value = cooldownEnd.ToUnixTimeSeconds();

            Debug.Log($"[RailService] Event activated! Start: {nearestFriday}, Next event: {cooldownEnd}");
        }

        private void DeactivateEvent()
        {
            IsActive.BoolValue = false;

            // Cooldown đã được set từ lúc ActivateEventFromNearestFriday()
            DateTime cooldownEnd = EventCooldownEndTime.Value.ToLocalDateTime();
            Debug.Log($"[RailService] Event deactivated! Next event: {cooldownEnd}");
        }

        /// <summary>
        /// Lấy thứ 6 tiếp theo (00:00)
        /// </summary>
        private DateTime GetNextFriday(DateTime from)
        {
            DateTime current = from.Date; // Reset về 00:00
            
            // Tìm thứ 6 tiếp theo
            while (current.DayOfWeek != DayOfWeek.Friday || current <= from)
            {
                current = current.AddDays(1);
            }

            return current;
        }

        /// <summary>
        /// Lấy thứ 6 gần nhất (có thể là tuần này hoặc tuần trước)
        /// </summary>
        private DateTime GetNearestFriday(DateTime from)
        {
            DateTime now = from.Date; // Reset về 00:00

            // Tính số ngày từ Monday (0 = Monday, 6 = Sunday)
            int daysSinceMonday = ((int)now.DayOfWeek + 6) % 7;
            
            // Tính Monday đầu tuần
            DateTime monday = now.AddDays(-daysSinceMonday);
            
            // Thứ 6 tuần này
            DateTime fridayThis = monday.AddDays(4);

            // Nếu hôm nay >= thứ 6 tuần này → return thứ 6 tuần này
            if (now >= fridayThis)
            {
                return fridayThis;
            }
            
            // Nếu hôm nay < thứ 6 tuần này → return thứ 6 tuần trước
            return fridayThis.AddDays(-7);
        }

        /// <summary>
        /// Check xem event cũ đã hết chưa khi mới vào game
        /// </summary>
        private void CheckAndResetEventIfNeeded()
        {
            // Logic này đã được xử lý trong Tick() và ActivateEventFromNearestFriday()
            // Không cần làm gì ở đây nữa
        }

        /// <summary>
        /// Reset tất cả data của event (khi event cycle mới bắt đầu)
        /// </summary>
        private void ResetEventData()
        {
            IsJoin.BoolValue = false;
            IsCompleted.BoolValue = false;
            CurrentStage.Value = 1;
            CurrentMilestone.Value = 1;
            SetToken(0);
            LastToken.Value = 0;
            LastFreeTokenClaimTime.Value = 0;

            Debug.Log("[RailService] Event data reset!");
        }

        public int GetLevelUnlock()
        {
            return SonatSDKAdapter.GetRemoteInt("rail_level_unlock", Config.LevelUnlock);
        }

        public bool IsJoinEvent()
        {
            return IsJoin.BoolValue;
        }

        public bool IsActiveEvent()
        {
            return IsActive.BoolValue;
        }

        private void TryUnlock()
        {
            if (IsUnlocked.BoolValue) return;

            if (SonatSystem.GetService<UserDataService>().GetLevel() < GetLevelUnlock()) return;

            IsUnlocked.BoolValue = true;

            // Nếu đang trong event (đã có event start time và chưa hết 10 ngày) → activate
            if (IsInTimeEvent())
            {
                IsActive.BoolValue = true;
                Debug.Log("[RailService] Unlocked and event is still active!");
            }
            // Nếu không trong event → activate từ thứ 6 gần nhất
            else
            {
                ActivateEventFromNearestFriday();
            }
        }

        public void JoinEvent()
        {
            if (IsJoin.BoolValue) return; // Already joined

            IsJoin.BoolValue = true;
            HasEverJoined.BoolValue = true;
            // Không reset CurrentStage - giữ nguyên stage hiện tại
            CurrentMilestone.Value = 1;

            SetToken(0);
            LastToken.Value = 0;
            
            // Reset thời gian free token khi join event
            LastFreeTokenClaimTime.Value = 0;
        }

        public void CompleteStage()
        {
            // Tăng stage lên
            CurrentStage.Value++;

            // Reset lại các giá trị (luôn reset ngay cả khi complete event)
            IsJoin.BoolValue = false;
            CurrentMilestone.Value = 1;
            SetToken(0);
            LastToken.Value = 0;

            // Kiểm tra xem có phải đã hoàn thành hết tất cả stage không
            if (CurrentStage.Value > Config.Stages.Count)
            {
                CompleteEvent();
            }
        }

        public void CompleteEvent()
        {
            IsCompleted.BoolValue = true;
            
            // Bắn event thông báo đã hoàn thành event
            EventBus<RailEventCompletedEvent>.Raise(new RailEventCompletedEvent());
        }

        public bool IsEventCompleted()
        {
            return IsCompleted.BoolValue;
        }

        public void AddAccumulatedReward(RewardData rewardData)
        {
            if (rewardData == null || rewardData.resourceDatas == null) return;

            foreach (var reward in rewardData.resourceDatas)
            {
                AccumulatedRewards.AddReward(reward);
            }
        }

        public void ClearAccumulatedRewards()
        {
            AccumulatedRewards = new RewardData();
        }

        public bool HasAccumulatedRewards()
        {
            return AccumulatedRewards != null && AccumulatedRewards.resourceDatas != null && AccumulatedRewards.resourceDatas.Count > 0;
        }

        private void OnLevelEnd(LevelEndedEvent @event)
        {
            if (@event.success) OnWin();
            else OnLose();
        }

        private void OnWin()
        {
            if (!IsUnlocked.BoolValue)
            {
                TryUnlock();
                return;
            }
        }

        private void OnLose()
        {

        }

        public int GetToken()
        {
            return inventoryService.GetResource(Sonat.Enums.GameResource.Token_Rail);
        }

        public void AddToken(int value)
        {
            if (value == 0) return;

            var abs = Mathf.Abs(value);

            if (value > 0)
                inventoryService.AddResource(Sonat.Enums.GameResource.Token_Rail, abs);
            else
                inventoryService.ReduceResource(Sonat.Enums.GameResource.Token_Rail, abs);
        }

        public void SetToken(int value)
        {
            inventoryService.SetResource(Sonat.Enums.GameResource.Token_Rail, value);
        }

        public void SyncToken()
        {
            LastToken.Value = GetToken();
        }

        public bool IsSyncToken()
        {
            return LastToken.Value == GetToken();
        }

        public RailConfig.Stage GetStage(int stageId)
        {
            var index = stageId - 1;

            if (index < 0 || index >= Config.Stages.Count)
            {
                Debug.LogError($"[RailService] Stage {stageId} is not available");
                return Config.Stages[Mathf.Clamp(index, 0, Config.Stages.Count - 1)];
            }

            return Config.Stages[index];
        }

        public RailConfig.Stage GetCurrentStage()
        {
            return GetStage(CurrentStage.Value);
        }

        public RailConfig.Stage.Milestone GetMilestone(int milestoneId)
        {
            var index = milestoneId - 1;
            var milestones = GetCurrentStage().Milestones;

            if (index < 0 || index >= milestones.Count)
            {
                Debug.LogError($"[RailService] Milestone {milestoneId} is not available");
                return milestones[Mathf.Clamp(index, 0, milestones.Count - 1)];
            }

            return milestones[index];
        }

        public RailConfig.Stage.Milestone GetCurrentMilestone()
        {
            return GetMilestone(CurrentMilestone.Value);
        }

        public bool IsLastMilestone()
        {
            return CurrentMilestone.Value == GetCurrentStage().Milestones.Count;
        }

        public bool IsPassCurrentMilestone()
        {
            var token = GetToken();
            var targetToken = GetCurrentMilestone().ItemRequire;

            return token >= targetToken;
        }

        public long GetTimeUnixNow()
        {
            return GetTimeNow().ToUnixTimeSeconds();
        }

        public DateTime GetTimeNow()
        {
#if UNITY_EDITOR
            return DateTime.Now;
#else
            return timeService.GetCurrentTime();
#endif
        }

        // ============ TIME-BASED EVENT LOGIC ============

        /// <summary>
        /// Kiểm tra xem hiện tại có trong thời gian event không
        /// Event: 10 ngày kể từ khi activate
        /// </summary>
        public bool IsInTimeEvent()
        {
            // Nếu chưa có event start time → chưa trong event
            if (CurrentEventStartTime == null || CurrentEventStartTime.Value == 0)
                return false;

            DateTime now = GetTimeNow();
            DateTime eventStart = CurrentEventStartTime.Value.ToLocalDateTime();
            DateTime eventEnd = eventStart.AddDays(EVENT_DURATION_DAYS);

            return now >= eventStart && now < eventEnd;
        }

        /// <summary>
        /// Lấy thời gian bắt đầu của event hiện tại
        /// </summary>
        public DateTime GetCurrentEventStart()
        {
            if (CurrentEventStartTime == null || CurrentEventStartTime.Value == 0)
                return DateTime.MinValue;

            return CurrentEventStartTime.Value.ToLocalDateTime();
        }

        /// <summary>
        /// Lấy thời gian bắt đầu event tiếp theo (thứ 6 tiếp theo hoặc từ cooldown)
        /// </summary>
        public DateTime GetNextEventStart()
        {
            // Nếu có cooldown → return cooldown time
            if (EventCooldownEndTime != null && EventCooldownEndTime.Value > 0)
            {
                return EventCooldownEndTime.Value.ToLocalDateTime();
            }

            // Nếu không có cooldown → tính thứ 6 tiếp theo
            DateTime now = GetTimeNow();
            return GetNextFriday(now);
        }

        /// <summary>
        /// Lấy thời gian kết thúc event hiện tại
        /// </summary>
        public DateTime GetCurrentEventEnd()
        {
            if (!IsInTimeEvent())
            {
                // Không trong event → return next event start
                return GetNextEventStart();
            }

            // Đang trong event → event start + 10 days
            DateTime eventStart = GetCurrentEventStart();
            return eventStart.AddDays(EVENT_DURATION_DAYS);
        }

        /// <summary>
        /// Lấy thời gian còn lại của event (giây)
        /// </summary>
        public long GetRemainingEventTime()
        {
            if (!IsInTimeEvent())
                return 0;

            DateTime now = GetTimeNow();
            DateTime eventEnd = GetCurrentEventEnd();
            TimeSpan remaining = eventEnd - now;

            return (long)remaining.TotalSeconds;
        }

        // ============ FREE TOKEN LOGIC ============

        /// <summary>
        /// Kiểm tra xem có thể claim free token không
        /// </summary>
        public bool CanClaimFreeToken()
        {
            // Nếu chưa từng claim thì có thể claim
            if (LastFreeTokenClaimTime.Value == 0)
                return true;

            // Tính thời gian đã trôi qua
            long now = GetTimeUnixNow();
            long elapsed = now - LastFreeTokenClaimTime.Value;

            // Nếu đã qua thời gian cooldown thì có thể claim
            return elapsed >= FREE_TOKEN_COOLDOWN;
        }

        /// <summary>
        /// Lấy thời gian còn lại để claim free token (giây)
        /// </summary>
        public long GetRemainingCooldownTime()
        {
            if (CanClaimFreeToken())
                return 0;

            long now = GetTimeUnixNow();
            long elapsed = now - LastFreeTokenClaimTime.Value;
            long remaining = FREE_TOKEN_COOLDOWN - elapsed;

            return Mathf.Max(0, (int)remaining);
        }

        /// <summary>
        /// Claim free token
        /// </summary>
        public bool ClaimFreeToken()
        {
            if (!CanClaimFreeToken())
            {
                Debug.LogWarning("[RailService] Cannot claim free token - still in cooldown");
                return false;
            }

            // Add token
            AddToken(FREE_TOKEN_AMOUNT);

            // Lưu thời gian claim
            LastFreeTokenClaimTime.Value = GetTimeUnixNow();

            Debug.Log($"[RailService] Claimed {FREE_TOKEN_AMOUNT} free tokens");
            return true;
        }
    }
}