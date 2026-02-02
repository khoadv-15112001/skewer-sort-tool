using UnityEngine;
using SonatFramework.Systems;
using Cysharp.Threading.Tasks;
using System;
using System.Threading.Tasks;
using SonatFramework.Systems.GameDataManagement;
using SonatFramework.Systems.EventBus;
using SonatFramework.Scripts.Utils;
using SonatFramework.Scripts.SonatSDKAdapterModule;
using SonatFramework.Scripts.UIModule;
using Sonat.Enums;
using SonatFramework.Systems.ObjectPooling;
using Gameplay;
using SonatFramework.Systems.UserData;
using Sonat.Data;
using Newtonsoft.Json;

namespace GrillSort.ConsecutiveWin
{

    [CreateAssetMenu(fileName = "ConsecutiveWinService", menuName = "My Services/Consecutive Win Service")]
    public class ConsecutiveWinService : SonatServiceSo, IServiceInitializeAsync
    {
        [SerializeField] public ConsecutiveWinConfig Config;

        private readonly Service<DataService> _dataService = new();
        private ConsecutiveWinData _data;
        private const string DataKey = "ConsecutiveWinData";

        #region Initialize
        public async UniTaskVoid InitializeAsync()
        {
            await LoadData();
            await LoadConfig();

            new EventBinding<LevelEndedEvent>(OnLevelEnded);
            new EventBinding<LevelStartedEvent>(OnLevelStarted);
        }

        private async UniTask LoadData()
        {
            _data = _dataService.Instance.GetData<ConsecutiveWinData>(DataKey);
            if (_data == null)
            {
                _data = new ConsecutiveWinData();
            }

            if (_data.isLevelStarted) // đã bắt đầu level mà kill app thì coi như thua
            {
                _data.isLevelStarted = false;
                _data.consecutiveWins = 0;
            }
            SaveData();
        }

        private async UniTask LoadConfig()
        {
            var userCampaignSegment = UserData.UserCampaignSegment.Value;

            var key = $"{userCampaignSegment}_consecutive_win_config";


            Config = SonatSDKAdapter.GetRemoteConfig<ConsecutiveWinConfig>(key, Config);
            //Config.timeToRemove = 60;
            //Config.timeToAdd = new int[] { 5, 7, 10};

            Debug.Log($"anhnt: [ConsecutiveWinService] camp = {userCampaignSegment}, Config.timeToRemove={Config.timeToRemove}, timeToAdd={JsonConvert.SerializeObject(Config.timeToAdd)}");

            //Config.appearLevel = SonatSDKAdapter.GetRemoteInt("ConsecutiveWin_AppearLevel", Config.appearLevel);
            //Config.appearDay = SonatSDKAdapter.GetRemoteInt("ConsecutiveWin_AppearDay", Config.appearDay);
        }

        private void SaveData()
        {
            _dataService.Instance.SetData(DataKey, _data);
        }
        #endregion


        public int GetConsecutiveWins()
        {
            return _data.consecutiveWins;
        }

        private void OnLevelEnded(LevelEndedEvent @event)
        {
            if (CheckStart(@event.level) == false) return;
            _data.isLevelStarted = false;
            if (@event.success)
            {
                _data.consecutiveWins++;

                var max = Config.GetMaxConsecutiveWins();
                if (_data.consecutiveWins >= max)
                {
                    _data.consecutiveWins = max;
                }
            }
            else
            {
                _data.consecutiveWins = 0;
            }
            SaveData();
        }

        private void OnLevelStarted(LevelStartedEvent @event)
        {
            if (CheckStart(@event.level) == false) return;
            _data.isLevelStarted = true;
            if (_data.consecutiveWins == 0) return;

            var timeToAdd = GetTimeToAdd(_data.consecutiveWins);
            UIFlowController.isShowedConsecutiveWin = true;
            // SonatUtils.DelayCall(1.5f, () =>
            // {
            var uiData = new UIData();
            uiData.Add("boosterType", GameResource.PreBoosterFreeze);
            uiData.Add("content", $"+{timeToAdd}s");
            uiData.Add("giftBox", $"{_data.consecutiveWins}");
            uiData.Add("onClose", (Action)(() =>
            {
                var effect = SonatSystem.GetService<PoolingService>().Create<UIAddTimeEffect>("UIAddTimeEffect", Vector3.zero, PanelManager.Instance.transform, "ConsecutiveWin");
                effect.Setup(Vector3.zero, timeToAdd, () =>
                {
                    MySonatFramework.audioService.PlaySound(AudioId.ButtonClick);
                    GameplayController.instance.AddTimeWhenStart(timeToAdd);
                    UIFlowController.isShowedConsecutiveWin = false;
                });
            }));

            ShowPopupAddTime(uiData).Forget();
            // });
            SaveData();


            MySonatFramework.inventoryService.AddResource(GameResource.PreBoosterAddTimeWinstreak, 1, new() { spendType = "feature", spendId = "prebooster_winstreak" });
            MySonatFramework.inventoryService.ReduceResource(GameResource.PreBoosterAddTimeWinstreak, 1, new() { earnId = "_", earnType = "_" });
        }

        private async UniTask ShowPopupAddTime(UIData uiData)
        {
            await UniTask.WaitUntil(() => UIFlowController.CheckConditionShowEffectConsecutiveWin());
            PanelManager.Instance.OpenPanel<PopupAddTime>(uiData);
        }

        public bool CheckStart(int level)
        {
            var currentDay = MySonatFramework.GetService<UserDataService>().UserDay;
            //var session = MySonatFramework.GetService<UserDataService>().SessionToday;
            return (level >= Config.appearLevel && Config.appearDay == 0) || (currentDay >= Config.appearDay && Config.appearDay != 0);
        }

        private int GetTimeToAdd(int consecutiveWins)
        {
            if (consecutiveWins == 0)
            {
                return 0;
            }
            return Config.GetTimeToAdd(consecutiveWins);
        }

        internal void LoadRemote()
        {
            LoadConfig().Forget();
        }

        [Serializable]
        public class ConsecutiveWinData
        {
            public int consecutiveWins = 0;
            public bool isLevelStarted = false;
        }
    }
}