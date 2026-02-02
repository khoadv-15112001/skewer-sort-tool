using System;
using Cysharp.Threading.Tasks;
using Helper;
using Sirenix.OdinInspector;
using Sonat.CustomService;
using Sonat.Enums;
using SonatFramework.Scripts.Helper;
using SonatFramework.Scripts.SonatSDKAdapterModule;
using SonatFramework.Systems;
using SonatFramework.Systems.ConfigManagement;
using SonatFramework.Systems.EventBus;
using SonatFramework.Systems.InventoryManagement;
using SonatFramework.Systems.TimeManagement;
using UnityEngine;

namespace MyGame.SkewerJam.Scripts.Service
{
    [CreateAssetMenu(fileName = "HLWEventService", menuName = "MyGame/SkewerJam/HLW Event/HLW Event Service")]
    public class HLWEventService : SonatServiceSo, IServiceInitialize
    {
        [SerializeField] private HLWEventConfig _HLWEventConfig;

        private string DataKey = "HLW_EVENT_DATA";

        private IntDataPref _isUnlocked;
        private LongDataPref _expiredTime;
        private IntDataPref _checkReceiveEventReward; // kiểm tra đã nhận quà kết thúc event chưa
        private LongDataPref _timeActive;

        private readonly Service<TimeService> _timeService = new();

        public HLWEventConfig HLWEventConfig => _HLWEventConfig;

        public bool CheckReceiveEventReward => _checkReceiveEventReward.Value == 1;

        public int Rank { get; set; }

        public event Action OnReceiveEventReward;

        public event Action OnStartEvent;
        public event Action OnJoinEvent;
        public event Action OnFinishEvent;

        private bool isFinished;

        public void Initialize()
        {
            LoadConfig();
            LoadData();


            if (CanUnlock())
            {
                Unlock();
            }

            if (IsUnlocked())
            {
                CheckExpiredTime().Forget();
                if (CheckFinishEvent())
                {
                    FinishEvent();
                }
            }
            if (!CheckFinishEvent())
                new EventBinding<LevelEndedEvent>(OnLevelEnded);
            // new EventBinding<EarnResourceEvent>(OnEarnResource);
        }

        // private void OnEarnResource(EarnResourceEvent eventData)
        // {
        //     switch (eventData.resource)
        //     {
        //         case GameResource.Pumpkin:
        //             if (CheckFinishEvent())
        //             {
        //                 FinishEvent();
        //             }
        //             break;
        //     }
        // }

        public bool CheckFinishEvent()
        {
            var currentTime = _timeService.Instance.GetUnixTimeSeconds();
            return _expiredTime.Value <= currentTime;
        }

        private void LoadConfig()
        {
            _HLWEventConfig.energyUnlockEvent = SonatSDKAdapter.GetRemoteInt("HLW_energyUnlockEvent", _HLWEventConfig.energyUnlockEvent);
            _HLWEventConfig.energyNormal = SonatSDKAdapter.GetRemoteInt("HLW_energyNormal", _HLWEventConfig.energyNormal);
            _HLWEventConfig.energyHard = SonatSDKAdapter.GetRemoteInt("HLW_energyHard", _HLWEventConfig.energyHard);
            _HLWEventConfig.energySuper = SonatSDKAdapter.GetRemoteInt("HLW_energySuper", _HLWEventConfig.energySuper);
        }

        private void LoadData()
        {
            _isUnlocked = new IntDataPref($"{DataKey}_isUnlocked", 0);
            _expiredTime = new LongDataPref($"{DataKey}_expiredTime");
            _checkReceiveEventReward = new IntDataPref($"{DataKey}_checkReceiveEventReward", 0);
            _timeActive = new LongDataPref($"{DataKey}_timeActive");

            // Handle for old user
            if (_isUnlocked.BoolValue)
            {
                if (_timeActive.Value == 0)
                    _timeActive.Value = _timeService.Instance.GetUnixTimeSeconds();
            }
        }

        private void OnLevelEnded(LevelEndedEvent eventData)
        {
            if (eventData.gameMode != GameMode.Classic) return;

            if (eventData.success)
            {
                if (IsUnlocked())
                {
                    var log = new EarnResourceLogData()
                    {
                        spendType = "energy",
                        spendId = "energy",
                        source = "home"
                    };
                    MySonatFramework.GetService<InventoryService>().AddResource(GameResource.Energy, HLWEventConfig.GetEnergyReward(eventData.level), log, false);
                }
            }
        }

        private async UniTaskVoid CheckExpiredTime()
        {
            var remainTime = GetRemainTime();
            if (remainTime > 0)
            {
                await UniTask.Delay(TimeSpan.FromSeconds(remainTime), DelayType.Realtime);
            }
            FinishEvent();
        }

        public long GetRemainTime()
        {
            var currentTime = _timeService.Instance.GetUnixTimeSeconds();
            return _expiredTime.Value - currentTime;
        }

        public bool IsUnlocked()
        {
            return _isUnlocked.Value == 1;
        }

        public bool CanUnlock()
        {
            return _isUnlocked.Value == 0 && _HLWEventConfig.liveOpsPackData.CheckCondition();
        }

        public void Unlock()
        {
            ResetData();

            _timeActive.Value = _timeService.Instance.GetUnixTimeSeconds();
            _isUnlocked.Value = 1;
            _expiredTime.Value = _HLWEventConfig.GetExpireTime();
            OnStartEvent?.Invoke();
        }

        public void JoinEvent()
        {
            OnJoinEvent?.Invoke();
        }

        [ContextMenu("Finish")]
        public void FinishEvent()
        {
            if (isFinished) return;

            isFinished = true;

            //_isUnlocked.Value = 0;
            ResetData();
            OnFinishEvent?.Invoke();
        }

        public void ResetData()
        {
            _expiredTime.Value = 0;
            MySonatFramework.GetService<InventoryService>().SetResource(GameResource.Energy, 0);
            MySonatFramework.GetService<InventoryService>().SetResource(GameResource.Pumpkin, 0);
            isFinished = false;
        }

        public void ReceiveEventReward()
        {
            _checkReceiveEventReward.Value = 1;
            OnReceiveEventReward?.Invoke();
        }

        public int GetDayActive()
        {
            DateTime start = DateTimeOffset.FromUnixTimeSeconds(_timeActive.Value).ToLocalTime().Date;
            DateTime now = DateTimeOffset.FromUnixTimeSeconds(_timeService.Instance.GetUnixTimeSeconds()).ToLocalTime().Date;
            return (now - start).Days;
        }

    }
}
