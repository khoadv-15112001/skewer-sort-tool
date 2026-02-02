using Cysharp.Threading.Tasks;
using Helper;
using Sirenix.OdinInspector;
using Sonat.CustomService;
using Sonat.Enums;
using SonatFramework.Scripts.Helper;
using SonatFramework.Scripts.SonatSDKAdapterModule;
using SonatFramework.Systems;
using SonatFramework.Systems.EventBus;
using SonatFramework.Systems.InventoryManagement.GameResources;
using SonatFramework.Systems.TimeManagement;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace GrillSort.DailyMission
{
    [CreateAssetMenu(fileName = "DailyMissionService", menuName = "My Services/DailyMissionService")]
    public class DailyMissionService : SonatServiceSo, IServiceInitializeAsync
    {
        public static DailyMissionService Instance => SonatSystem.GetService<DailyMissionService>();

        [SerializeField] private DailyMissionConfig _config;
        public DailyMissionConfig Config => _config;

        [SerializeField] private List<DailyMissionBase> listDailyMission = new();

        public IntDataPref isUnlocked;
        public LongDataPref lastDate;
        public IntDataPref currentMissionIndex;
        public ListDataPref<int> listCompletedMission;
        public IntDataPref numItem;
        public ListDataPref<int> listIndexReward;
        public ListDataPref<int> listClaimedBaseStageIndex;

        public IntDataPref isXLStage;
        public IntDataPref isShownTut;

        public IntDataPref start_count;
        public IntDataPref continue_count;

        public LongDataPref timeEnd;

        public IntDataPref isX2;
        public IntDataPref x2DataIndex;
        public LongDataPref timeEndX2;

        public static Action OnChangeMission;
        public static Action OnClaimStage;
        public static Action OnChangeItem;
        public static Action OnShowTut;
        public static Action OnBuyX2;
        public static Action OnExpiredX2;
        public static Action OnTick;

        public static int SaveItem;

        private TimeService timeService => MySonatFramework.GetService<TimeService>();
        private CustomTrackingService trackingService => MySonatFramework.GetService<CustomTrackingService>();

        public async UniTaskVoid InitializeAsync()
        {
            isUnlocked = new IntDataPref("daily_mission_is_unlocked");
            lastDate = new LongDataPref("daily_mission_last_date");
            currentMissionIndex = new IntDataPref("daily_mission_current_mission_index");
            listCompletedMission = new ListDataPref<int>("daily_mission_list_completed_mission");
            numItem = new IntDataPref("daily_mission_num_item");
            listIndexReward = new ListDataPref<int>("daily_mission_list_index_reward");
            listClaimedBaseStageIndex = new ListDataPref<int>("daily_mission_list_claimed_base_stage_index");
            isXLStage = new IntDataPref("daily_mission_is_xl_stage");

            isShownTut = new IntDataPref("daily_mission_is_shown_tut");

            start_count = new IntDataPref("daily_mission_start_count");
            continue_count = new IntDataPref("daily_mission_continue_count");

            timeEnd = new LongDataPref("daily_mission_time_end");

            isX2 = new IntDataPref("daily_mission_is_x2");
            x2DataIndex = new IntDataPref("daily_mission_x2_data_index");
            timeEndX2 = new LongDataPref("daily_mission_time_end_x2");

            await UniTask.Yield();

            Validate();

            ActivateMission(GetCurrentDailyMission().MissionType);

            TryActive();

            new EventBinding<LevelEndedEvent>(OnLevelEnd);

            TickService.Register(Tick, TickService.TickPhase.TickRealtime);
        }

        private void Tick()
        {
            if (!isUnlocked.BoolValue) return;

            long now = GetTimeUnixNow();

            if (isX2.BoolValue)
            {
                if (now >= timeEndX2.Value)
                {
                    ExpiredX2();
                }
            }

            if (now >= timeEnd.Value)
            {
                LogEndFeature(false);
                StartEvent();
            }

            OnTick?.Invoke();
        }

        private void Validate()
        {
            if (!isUnlocked.BoolValue) return;

            var mission = listDailyMission.Find(x => ((int)x.MissionType) == currentMissionIndex.Value);

            if (mission == null)
            {
                Debug.LogError("DailyMissionService Validate Null");
                RandomMission();
            }
        }

        public void TryActive()
        {
            if (isUnlocked.BoolValue) return;

            if (MySonatFramework.userDataService.GetLevel() >= GetLevelStart())
            {
                isUnlocked.BoolValue = true;
                StartEvent();
            }
        }

        public int GetLevelStart()
        {
            return SonatSDKAdapter.GetRemoteInt($"daily_mission_level_start", Config.levelStart);
        }

        private void OnLevelEnd(LevelEndedEvent levelEndedEvent)
        {
            if (levelEndedEvent.gameMode != GameMode.Classic) return;

            switch (levelEndedEvent.success)
            {
                case true:
                    OnWin();
                    break;
                case false:
                    OnLose();
                    break;
            }
        }

        public void StartEvent()
        {
            //Debug.LogError("DailyMissionService: StartEvent");

            lastDate.Value = GetTimeNow().Date.ToUnixTimeSeconds();
            listCompletedMission.Clear();

            RandomMission();

            continue_count.Value = 0;
            start_count.Value++;
            LogStartFeature();
        }

        public void CompleteMission()
        {
            //Debug.LogError("DailyMissionService: CompleteMission");

            LogEndFeature(true);

            listCompletedMission.Add(currentMissionIndex.Value);

            RandomMission();
        }

        private void ActivateMission(DailyMissionConfig.EMission missionType)
        {
            if (!isUnlocked.BoolValue) return;

            foreach (var mission in listDailyMission)
            {
                mission.Deactivate();

                if (mission.MissionType == missionType)
                {
                    mission.Activate();
                }
            }

            //Debug.LogError($"DailyMissionService: Activate Mission={missionType}");
        }

        private void RandomMission()
        {
            const int maxRepeat = 2;

            var availableMissions = listDailyMission
                .Where(m => listCompletedMission.Value.Count(x => x == (int)m.MissionType) < maxRepeat)
                .ToList();

            if (availableMissions.Count == 0)
            {
                //Debug.LogError("No available mission left to random!, Will Reset Pool");
                listCompletedMission.Clear();
                RandomMission();
                return;
            }

            var randomMission = availableMissions[UnityEngine.Random.Range(0, availableMissions.Count)];
            int missionIndex = (int)randomMission.MissionType;

            int count = listCompletedMission.Value.Count(x => x == missionIndex);
            isXLStage.BoolValue = count >= 1;

            currentMissionIndex.Value = missionIndex;

            Debug.LogError($"DailyMissionService: Mission={randomMission.MissionType}, XL={isXLStage.BoolValue}");

            ActivateMission(randomMission.MissionType);
            numItem.Value = 0;
            GenerateListIndexReward();
            listClaimedBaseStageIndex.Clear();

            timeEnd.Value = (GetTimeNow().AddHours(randomMission.TimeDuration_Hour)).ToUnixTimeSeconds();

            ResetX2();

            OnChangeMission?.Invoke();
        }

        public DailyMissionBase GetCurrentDailyMission()
        {
            return listDailyMission.Find(x => (int)x.MissionType == currentMissionIndex.Value);
        }

        private void GenerateListIndexReward()
        {
            listIndexReward.Clear();

            for (int i = 0; i < Config.BasicStageRewards.Count; i++)
            {
                int numIndexReward = Config.BasicStageRewards[i].rewardDatas.Count;

                listIndexReward.Add(UnityEngine.Random.Range(0, numIndexReward));
            }
        }

        public RewardData GetRewardBaseStage(int stage)
        {
            var rewardsData = Config.BasicStageRewards[stage].rewardDatas;
            int index = Mathf.Clamp(listIndexReward.Value[stage], 0, rewardsData.Count - 1);

            return Config.BasicStageRewards[stage].GetRewardData(index);
        }

        public RewardData GetRewardXLStage()
        {
            return Config.XLStageReward.GetRewardData(0);
        }

        public void AddTempItem(int item)
        {
            SaveItem += item;
        }

        private void AddItem(int item)
        {
            int max = GetCurrentDailyMission().GetMaxNumItem();

            if (isX2.BoolValue)
                item *= 2;

            numItem.Value = Mathf.Clamp(numItem.Value + item, 0, max);

            OnChangeItem?.Invoke();

            Debug.LogError($"DailyMissionService: AddItem={item}, from={GetCurrentDailyMission().MissionType}");
        }

        public void CheatSetItem(int item)
        {
            int max = GetCurrentDailyMission().GetMaxNumItem();

            numItem.Value = Mathf.Clamp(item, 0, max);

            OnChangeItem?.Invoke();
        }

        public void ClaimBaseStageIndex(int index)
        {
            if (listClaimedBaseStageIndex.Contains(index)) return;

            if (listClaimedBaseStageIndex.Value.Count == 0)
            {
                continue_count.Value++;
                LogStartFeature();
            }

            listClaimedBaseStageIndex.Add(index);

            if (listClaimedBaseStageIndex.Value.Count >= 3)
            {
                //LogEndFeature(true);

                CompleteMission();
            }

            OnClaimStage?.Invoke();
        }

        public void ClaimXLStage()
        {
            continue_count.Value++;
            LogStartFeature();

            //LogEndFeature(true);
        }

        private void OnWin()
        {
            if (!isUnlocked.BoolValue)
            {
                TryActive();
                return;
            }

            AddItem(SaveItem);
            SaveItem = 0;
        }

        private void OnLose()
        {
            //Debug.LogError("OnLose");
            SaveItem = 0;
        }

        public bool CanShowBubbleX2()
        {
            if (numItem.Value <= 0) return false;

            return !isX2.BoolValue;
        }

        public void BuyX2()
        {
            var x2Data = GetCurrentX2Data();

            isX2.BoolValue = true;

            x2DataIndex.Value++;

            timeEndX2.Value = GetTimeUnixNow() + x2Data.TimeDuration_Second;

            OnBuyX2?.Invoke();
        }

        private void ExpiredX2()
        {
            isX2.BoolValue = false;

            OnExpiredX2?.Invoke();
        }

        private void ResetX2()
        {
            isX2.BoolValue = false;
            x2DataIndex.Value = 0;
            timeEndX2.Value = 0;
        }

        public DailyMissionConfig.X2Data GetCurrentX2Data()
        {
            var data = Config.ListX2Data[Mathf.Clamp(x2DataIndex.Value, 0, Config.ListX2Data.Count - 1)];

            return data;
        }

        public bool CanNoti()
        {
            if (!isShownTut.BoolValue) return true;

            var currentMission = GetCurrentDailyMission();

            if (isXLStage.BoolValue)
            {
                return numItem.Value >= currentMission.GetNumItemXLStage();
            }

            for (int i = 0; i < currentMission.BasicStageData.Targets.Count; i++)
            {
                int targetNum = currentMission.GetNumItemBaseStage(i);

                if (numItem.Value >= targetNum && !listClaimedBaseStageIndex.Contains(i))
                    return true;
            }

            return false;
        }

        public void LogStartFeature()
        {
            //Debug.LogError($"[DailyMissionService]: LogStartFeature: start_count={start_count.Value},mission={GetMissionTypeTracking()} ,continue_count={continue_count.Value}");

            Dictionary<string, string> extraParams = new()
            {
                { "mission_type", GetMissionTypeTracking()}
            };

            trackingService.LogLevelStartFeature("dm", "daily_quest", start_count.Value, continue_count.Value, extraParams);
        }

        public void LogEndFeature(bool success)
        {
            float completion = isXLStage.BoolValue ? (success ? 1 : 0) : listClaimedBaseStageIndex.Value.Count * 1f / 3f;
            string percentage_claim_resource = "";
            int milestone = isXLStage.BoolValue ? (success ? 1 : 0) : (listClaimedBaseStageIndex.Value.Any() ? listClaimedBaseStageIndex.Value.Max() + 1 : 0);
            int rank = 0;
            string end_cause = success ? "user_win" : "feature_end";

            Dictionary<string, string> extraParams = new()
            {
                { "mission_type", GetMissionTypeTracking()}
            };

            trackingService.LogLevelEndFeature("dm", "daily_quest", start_count.Value, continue_count.Value, success, completion.ToString(),
                percentage_claim_resource, milestone.ToString(), rank, end_cause, -1, extraParams);
        }

        private string GetMissionTypeTracking()
        {
            var type = GetCurrentDailyMission().MissionType.ToString().ToLower();
            var stage = isXLStage.BoolValue ? "xl" : "basic";

            return $"{type}_{stage}";
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
    }
}