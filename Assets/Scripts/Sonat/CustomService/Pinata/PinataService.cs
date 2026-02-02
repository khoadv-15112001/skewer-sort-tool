using Cysharp.Threading.Tasks;
using Helper;
using SonatFramework.Scripts.Helper;
using SonatFramework.Scripts.SonatSDKAdapterModule;
using SonatFramework.Scripts.Utils;
using SonatFramework.Systems;
using SonatFramework.Systems.EventBus;
using SonatFramework.Systems.TimeManagement;
using SonatFramework.Systems.UserData;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GrillSort.Pinata
{
    [CreateAssetMenu(fileName = "PinataService", menuName = "My Services/PinataService")]
    public class PinataService : SonatServiceSo, IServiceInitializeAsync
    {
        public static PinataService Instance => SonatSystem.GetService<PinataService>();

        [SerializeField] private PinataConfig _config;
        public PinataConfig Config => _config;

        public IntDataPref IsUnlocked;
        public IntDataPref IsActive;
        public IntDataPref WinARow;
        public IntDataPref CurrentStage;
        public IntDataPref StartCount; // Track số lần start feature

        public LongDataPref LastCompleteCycleUnix;

        public static Action OnTick;
        public static Action OnCompleteStage;
        public static Action OnInactive;
        public static Action OnActive;

        public static bool CheatAutoGold = false;

        private TimeService timeService => MySonatFramework.GetService<TimeService>();

        public async UniTaskVoid InitializeAsync()
        {
            CheatAutoGold = false;

            IsUnlocked = new IntDataPref("pinata_is_unlocked");
            IsActive = new IntDataPref("pinata_is_active");
            WinARow = new IntDataPref("pinata_win_a_row");
            CurrentStage = new IntDataPref("pinata_current_stage", 1);
            StartCount = new IntDataPref("pinata_start_count");
            LastCompleteCycleUnix = new LongDataPref("pinata_last_complete_cycle_unix");

            await UniTask.Yield();

            TryUnlock();
            UpdateActiveState();

            new EventBinding<LevelEndedEvent>(OnLevelEnd);

            TickService.Register(Tick, TickService.TickPhase.TickRealtime);
        }

        private void Tick()
        {
            if (!IsUnlocked.BoolValue) return;
            UpdateActiveState();
            OnTick?.Invoke();
        }

        private void TryUnlock()
        {
            if (IsUnlocked.BoolValue) return;

            if (SonatSystem.GetService<UserDataService>().GetLevel() < GetLevelUnlock()) return;

            IsUnlocked.BoolValue = true;
        }

        private int GetLevelUnlock()
        {
            return SonatSDKAdapter.GetRemoteInt("pinata_level_unlock", Config.LevelUnlock);
        }

        private void OnLevelEnd(LevelEndedEvent @event)
        {
            if (!IsUnlocked.BoolValue)
            {
                TryUnlock();
                return;
            }

            if (!IsActive.BoolValue)
                return;

            var curStage = GetCurrentStage();
            if (curStage == null) return;

            if (@event.success)
            {
                WinARow.Value = Mathf.Clamp(WinARow.Value + 1, 0, curStage.MaxMilestone);
            }
            else
            {
                WinARow.Value = 0;
            }

            // Bắn level_progress_feature sau khi thay đổi WinARow
            LogProgressFeature();
        }

        public void Deactive()
        {
            IsActive.BoolValue = false;

            OnInactive?.Invoke();
        }

        public void CheatWinARow(int value)
        {
            if (!IsActive.BoolValue) return;
            WinARow.Value = Mathf.Clamp(value, 0, GetCurrentStage().MaxMilestone);
        }

        public PinataConfig.Stage GetCurrentStage()
        {
            return GetStage(CurrentStage.Value);
        }

        public PinataConfig.Stage GetStage(int stage)
        {
            if (stage < 1 || stage > Config.MaxStage)
            {
                Debug.LogError($"[PinataService] Pinata GetStage={stage} = null");
                return null;
            }

            return Config.Stages[stage - 1];
        }

        public bool IsAvailableCompleteStage()
        {
            var currentStage = GetCurrentStage();
            if (currentStage == null) return false;
            return WinARow.Value == currentStage.MaxMilestone;
        }

        public void CompleteStage()
        {
            var now = GetTimeNow();

            if (CurrentStage.Value == Config.MaxStage)
            {
                // Bắn level_end_feature khi hoàn thành stage cuối
                LogEndFeature(success: true, endCause: "user_win");

                CurrentStage.Value = 1;
                Deactive();
                LastCompleteCycleUnix.Value = GetCurrentCycleStartUnix(now);
            }
            else
            {
                // Nếu vừa hoàn thành stage 1 → bắn level_start_feature với continue_count = 1
                if (CurrentStage.Value == 1)
                {
                    LogStartFeature(continueCount: 1);
                }

                CurrentStage.Value++;
            }

            WinARow.Value = 0;
            OnCompleteStage?.Invoke();
        }

        private long _lastCycleChecked = -1;

        private void UpdateActiveState()
        {
            var now = GetTimeNow();

            if (!IsUnlocked.BoolValue)
            {
                Deactive();
                return;
            }

            // ⛔ Ngoài khung giờ hoạt động (Thứ 6 0h → Thứ 2 0h)
            if (!IsWithinPinataPeriod(now))
            {
                // Bắn level_end_feature khi hết thời gian
                if (IsActive.BoolValue)
                {
                    LogEndFeature(success: false, endCause: "feature_ended");
                }

                Deactive();
                _lastCycleChecked = -1;
                return;
            }

            long currentCycle = GetCurrentCycleStartUnix(now);

            // ⛔ Nếu đã hoàn thành stage cuối ở cycle này → chờ sang cycle tiếp theo
            if (LastCompleteCycleUnix.Value == currentCycle)
            {
                Deactive();
                return;
            }

            bool cycleChanged = (_lastCycleChecked != -1 && currentCycle != _lastCycleChecked);
            bool wasInactive = !IsActive.BoolValue;

            // Cập nhật lần kiểm tra gần nhất
            _lastCycleChecked = currentCycle;

            // ✅ Khi vừa sang cycle mới hoặc vừa active lại → reset 1 lần duy nhất
            if (cycleChanged || wasInactive)
            {
                IsActive.BoolValue = true;
                CurrentStage.Value = 1;
                WinARow.Value = 0;
                OnActive?.Invoke();

                // Bắn level_start_feature khi vừa active (continue_count = 0)
                LogStartFeature(continueCount: 0);

                if (cycleChanged)
                    Debug.Log($"[PinataService] Cycle changed → Reset data (Cycle={currentCycle})");
                else
                    Debug.Log("[PinataService] Pinata Activated & Data Reset");
            }
        }

        // 🗓 Kiểm tra trong khung giờ hoạt động (Thứ 6 0h → Thứ 2 0h)
        private bool IsWithinPinataPeriod(DateTime now)
        {
            // Tính ngày đầu tuần (thứ 2)
            int diffToMonday = ((int)now.DayOfWeek + 6) % 7; // Monday = 0
            DateTime monday = now.Date.AddDays(-diffToMonday);

            DateTime fridayStart = monday.AddDays(4); // Thứ 6 0h
            DateTime mondayNext = monday.AddDays(7); // Thứ 2 tuần sau 0h

            return now >= fridayStart && now < mondayNext;
        }

        // 🔁 Cycle 1 & 2 (mỗi cycle 36 tiếng)
        private long GetCurrentCycleStartUnix(DateTime now)
        {
            // Xác định mốc thứ 6 0h của tuần hiện tại
            int diffToMonday = ((int)now.DayOfWeek + 6) % 7;
            DateTime monday = now.Date.AddDays(-diffToMonday);
            DateTime fridayStart = monday.AddDays(4); // Thứ 6 0h

            var cycle1Start = fridayStart;
            var cycle2Start = fridayStart.AddHours(36); // Thứ 7 12h

            if (now < cycle2Start)
                return cycle1Start.ToUnixTimeSeconds();
            else
                return cycle2Start.ToUnixTimeSeconds(); ;
        }

        // 🕒 Trả về thời gian còn lại của cycle hiện tại hoặc tới khi event kết thúc
        public string GetRemainTime()
        {
            var now = GetTimeNow();

            // Nếu chưa tới thứ 6 0h → chưa mở
            if (!IsWithinPinataPeriod(now))
            {
                // Tính thời gian còn lại đến thứ 6 0h tiếp theo
                var nextFridayStart = GetNextFridayStart(now);
                var remain = nextFridayStart - now;
                return SonatUtils.GetTimeByFormat((long)remain.TotalSeconds, TxtTimeFormat.ShortDay_FullTime);
            }

            // Nếu đang trong khung Pinata hoạt động → xem còn bao lâu tới hết cycle hiện tại
            var currentCycleStart = GetCurrentCycleStartUnix(now).ToLocalDateTime();
            var currentCycleEnd = currentCycleStart.AddHours(36);

            // Nếu vượt qua cycle 2 (thứ 2 0h)
            var mondayEnd = GetMondayEnd(now);
            if (currentCycleEnd > mondayEnd)
                currentCycleEnd = mondayEnd;

            var remainCycle = currentCycleEnd - now;
            if (remainCycle.TotalSeconds < 0) remainCycle = TimeSpan.Zero;

            return SonatUtils.GetTimeByFormat((long)remainCycle.TotalSeconds, TxtTimeFormat.ShortDay_FullTime);
        }

        public long GetTimeEndUnix()
        {
            var now = GetTimeNow();

            // Nếu chưa tới thứ 6 0h → chưa mở
            if (!IsWithinPinataPeriod(now))
            {
                // Tính thời gian còn lại đến thứ 6 0h tiếp theo
                var nextFridayStart = GetNextFridayStart(now);
                return nextFridayStart.ToUnixTimeSeconds();
            }

            // Nếu đang trong khung Pinata hoạt động → xem còn bao lâu tới hết cycle hiện tại
            var currentCycleStart = GetCurrentCycleStartUnix(now).ToLocalDateTime();
            var currentCycleEnd = currentCycleStart.AddHours(36);

            // Nếu vượt qua cycle 2 (thứ 2 0h)
            var mondayEnd = GetMondayEnd(now);
            if (currentCycleEnd > mondayEnd)
                currentCycleEnd = mondayEnd;

            return currentCycleEnd.ToUnixTimeSeconds();
        }

        // 🔹 Helper: Lấy thời điểm thứ 6 0h tiếp theo
        private DateTime GetNextFridayStart(DateTime now)
        {
            int diffToMonday = ((int)now.DayOfWeek + 6) % 7; // Monday = 0
            DateTime monday = now.Date.AddDays(-diffToMonday);
            DateTime nextFriday = monday.AddDays(4); // Thứ 6 0h tuần này

            if (now >= nextFriday)
                nextFriday = nextFriday.AddDays(7);

            return nextFriday;
        }

        // 🔹 Helper: Lấy thời điểm thứ 2 0h của tuần kế tiếp
        private DateTime GetMondayEnd(DateTime now)
        {
            int diffToMonday = ((int)now.DayOfWeek + 6) % 7;
            DateTime monday = now.Date.AddDays(-diffToMonday);
            return monday.AddDays(7); // Thứ 2 tuần sau 0h
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

        // ============ TRACKING ============

        public void LogStartFeature(int continueCount)
        {
            // Mỗi khi active thì start_count +1
            if (continueCount == 0)
            {
                StartCount.Value++;
            }

            var extraParams = new Dictionary<string, string>
            {
                { "stage", CurrentStage.Value.ToString() },
            };

            // Lấy level classic hiện tại

            MySonatFramework.customTrackingService.LogLevelStartFeature(
                mode: "pp",
                type: "win_streak",
                continue_count: continueCount,
                start_count: StartCount.Value,
                extraParams: extraParams
            );
        }

        public void LogProgressFeature()
        {
            var currentStage = GetCurrentStage();
            if (currentStage == null) return;

            // continue_count: 0 nếu chưa hoàn thành stage 1, 1 nếu đã hoàn thành stage 1
            int continueCount = CurrentStage.Value > 1 ? 1 : 0;

            // completion: 100 * WinARow / MaxMilestone
            float completion = 100f * WinARow.Value / currentStage.MaxMilestone;

            // milestone: format "{stage}_{step}" với step là WinARow
            string milestone = $"{CurrentStage.Value}_{WinARow.Value}";

            var extraParams = new Dictionary<string, string>
            {
                { "stage", CurrentStage.Value.ToString() },
                { "continue_count", continueCount.ToString() }
            };

            MySonatFramework.customTrackingService.LogLevelProgressFeature(
                mode: "pp",
                type: "win_streak",
                start_count: StartCount.Value,
                continue_count: continueCount,
                completion: completion.ToString("0"),
                percentage_claim_resource: completion.ToString("0"),
                milestone: milestone,
                rank: -1,
                day_active: -1,
                extraParams: extraParams
            );
        }

        public void LogEndFeature(bool success, string endCause)
        {
            var currentStage = GetCurrentStage();
            if (currentStage == null) return;

            // continue_count: 0 nếu chưa hoàn thành stage 1, 1 nếu đã hoàn thành stage 1
            int continueCount = CurrentStage.Value > 1 ? 1 : 0;

            // completion: 100 * WinARow / MaxMilestone
            float completion = 100f * WinARow.Value / currentStage.MaxMilestone;

            // milestone: format "{stage}_{step}" với step là WinARow
            string milestone = $"{CurrentStage.Value}_{WinARow.Value}";

            var extraParams = new Dictionary<string, string>
            {
                { "stage", CurrentStage.Value.ToString() },
                { "continue_count", continueCount.ToString() }
            };

            MySonatFramework.customTrackingService.LogLevelEndFeature(
                mode: "pp",
                type: "win_streak",
                start_count: StartCount.Value,
                continue_count: continueCount,
                success: success,
                completion: completion.ToString("0"),
                percentage_claim_resource: "",
                milestone: milestone,
                rank: -1,
                end_cause: endCause,
                day_active: -1,
                extraParams: extraParams
            );
        }
    }
}
