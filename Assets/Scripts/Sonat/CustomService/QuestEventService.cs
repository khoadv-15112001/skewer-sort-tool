using UnityEngine;
using SonatFramework.Systems;
using Cysharp.Threading.Tasks;
using System.Threading.Tasks;
using SonatFramework.Systems.GameDataManagement;
using System;
using SonatFramework.Scripts.SonatSDKAdapterModule;
using SonatFramework.Systems.EventBus;
using SonatFramework.Scripts.Helper;
using SonatFramework.Systems.TimeManagement;
using SonatFramework.Systems.InventoryManagement;
using Sonat.Enums;

namespace GrillSort.QuestEvent
{
    [CreateAssetMenu(fileName = "QuestEventService", menuName = "My Services/QuestEventService")]
    public class QuestEventService : SonatServiceSo, IServiceInitializeAsync
    {
        public QuestEventConfig config;
        public static Action OnX2Item;
        private readonly Service<DataService> _dataService = new();
        private string DataKey = "QuestEventData";
        private QuestEventData data;

        public Action OnDataChanged;
        public Action OnReceiveReward;
        public Action OnFinishQuestEvent;
        public QuestEventData Data => data;
        public int NumCollectedItemInGame { get; set; }
        public int PreNumItem { get; private set; }

        private IntDataPref _isUnlocked;
        private LongDataPref _expiredTime;
        private readonly Service<TimeService> _timeService = new();
        private readonly Service<InventoryService> inventoryService = new();

        public async UniTaskVoid InitializeAsync()
        {
            await LoadData();
            await LoadConfig();

            PreNumItem = data.numItem;
            new EventBinding<LevelEndedEvent>(OnEndLevel);

            if (IsUnlocked())
            {
                var remainTime = GetRemainTime();
                if (remainTime > 0)
                {
                    StartCheckExpiredTime(remainTime).Forget();
                }
                else
                {
                    Reset();
                }

                CheckActiveX2Item();
            }
        }

        private async UniTask StartCheckExpiredTime(long remainTime)
        {
            // await UniTask.Delay((int)remainTime * 1000);
            await UniTask.Delay(TimeSpan.FromSeconds(remainTime));
            Reset();
            OnFinishQuestEvent?.Invoke();
        }

        private void OnEndLevel(LevelEndedEvent @event)
        {
            if (@event.success == false)
            {
                NumCollectedItemInGame = 0;
            }
        }

        #region Data/ Config

        private async Task LoadConfig()
        {
            // throw new NotImplementedException();
            config.unlockedLevel = SonatSDKAdapter.GetRemoteInt("levelAppearQuestEvent", 15);
        }

        private async Task LoadData()
        {
            data = _dataService.Instance.GetData<QuestEventData>(DataKey);
            if (data == null)
            {
                data = new QuestEventData();
                SaveData();
            }

            _isUnlocked = new IntDataPref(DataKey + "_isUnlocked", 0);
            _expiredTime = new LongDataPref(DataKey + "_expiredTime");
        }

        private void SaveData()
        {
            _dataService.Instance.SetData(DataKey, data);
            OnDataChanged?.Invoke();
        }

        #endregion

        public void AddItemCollectIngame()
        {
            data.numItem += NumCollectedItemInGame;
            NumCollectedItemInGame = 0;
            var maxItem = config.GetMaxNum(data.currentQuestIdx);

            if (data.numItem >= maxItem)
            {
                data.numItem = maxItem;
                data.canClaim = true;
            }

            SaveData();
            PreNumItem = data.numItem;
        }

        public bool CheckCanClaimReward()
        {
            return data.numItem + NumCollectedItemInGame >= config.GetMaxNum(data.currentQuestIdx);
        }

        public void ReceiveReward()
        {
            PreNumItem = 0;
            data.numItem = 0;
            data.currentQuestIdx++;
            data.canClaim = false;

            OnReceiveReward?.Invoke();
            SaveData();
        }

        public int GetCurrentItemId()
        {
            if (data.currentQuestIdx < 0 || data.currentQuestIdx >= config.questDatas.Count)
            {
                return config.questDatas[0].itemId; // default item id
            }

            return config.questDatas[data.currentQuestIdx].itemId;
        }

        public bool CheckCompleteAllQuest()
        {
            return data.currentQuestIdx >= config.GetQuestCount();
        }


        public bool CheckLiveOpsCondition()
        {
            return config.liveOpsPackData.CheckCondition();
        }

        public bool CanUnlock()
        {
            var level = MySonatFramework.userDataService.GetLevel();
            return config.liveOpsPackData.CheckCondition() && level >= config.unlockedLevel;
        }

        public bool IsUnlocked()
        {
            return _isUnlocked.Value == 1;
        }

        //public void Unlock()
        //{
        //    _expiredTime.Value = _timeService.Instance.GetUnixTimeSeconds() + config.duration * 24 * 3600;
        //    _isUnlocked.Value = 1;
        //}
        public void Unlock()
        {
            SetTimeExpired();
            _isUnlocked.Value = 1;
            // Debug.Log(
            //     $"[Unlock] Now={now} ({now.Kind}), NextSunday={nextSunday}, ExpiredUnix={expiredUnix}, LocalTime={DateTimeOffset.FromUnixTimeSeconds(expiredUnix).ToLocalTime()}");
        }

        private void SetTimeExpired()
        {
            var now = _timeService.Instance.GetCurrentTime().AddSeconds(10);

            if (now.Kind == DateTimeKind.Unspecified)
                now = DateTime.SpecifyKind(now, DateTimeKind.Local);

            int daysUntilSunday = ((int)DayOfWeek.Sunday - (int)now.DayOfWeek + 7) % 7;

            var nextSunday = now.Date.AddDays(daysUntilSunday).AddHours(23).AddMinutes(59).AddSeconds(59);

            TimeSpan offset = (now.Kind == DateTimeKind.Utc) ? TimeSpan.Zero : TimeZoneInfo.Local.GetUtcOffset(now);

            var expiredUnix = new DateTimeOffset(nextSunday, offset).ToUnixTimeSeconds();

            _expiredTime.Value = expiredUnix;
        }

        public long GetRemainTime()
        {
            var remainTime = _expiredTime.Value - _timeService.Instance.GetUnixTimeSeconds();

            return remainTime;
        }

        public void Reset()
        {
            SetTimeExpired();
            data = new QuestEventData();
            SaveData();
            StartCheckExpiredTime(GetRemainTime()).Forget();
        }

        public void CheckActiveX2Item()
        {
            if (!_isUnlocked.BoolValue || data.x2) return;

            if (inventoryService.Instance.CanReduce(GameResource.X2ItemQuestEvent, 1))
            {
                Debug.Log("anhnt: reduce x2");
                data.x2 = true;
                SaveData();

                var log = new SpendResourceLogData()
                {
                    earnType = "x2_sausage_gold",
                    earnId = "x2_sausage_gold"
                };
                inventoryService.Instance.ReduceResource(GameResource.X2ItemQuestEvent, 1, log);

                OnX2Item?.Invoke();
            }
        }

        internal bool HasX2Item()
        {
            return data.x2;
        }

        internal void CollectItemInGame(int v)
        {
            NumCollectedItemInGame += v * (data.x2 ? 2 : 1);
        }

        internal void ResetX2Item()
        {
            data.x2 = false;
            SaveData();
        }

        public bool CheckEventActive()
        {
            return IsUnlocked() && CheckOpen();
        }

        public bool CheckOpen()
        {
            return CheckWeekend();
        }

        private bool CheckWeekend()
        {
            var currentTime = MySonatFramework.GetService<SonatTimeService>().GetCurrentTime();
            if (currentTime.DayOfWeek is DayOfWeek.Friday or DayOfWeek.Saturday or DayOfWeek.Sunday)
            {
                return true;
            }

            return false;
        }

        internal void CheatMilestone(int milestone, int numItem)
        {
            data.currentQuestIdx = milestone;
            data.numItem = numItem;
            data.canClaim = true;
            SaveData();
        }
    }

    public class QuestEventData
    {
        public int currentQuestIdx = 0;
        public int numItem = 0;
        public bool canClaim = false;
        public bool x2 = false;
    }
}