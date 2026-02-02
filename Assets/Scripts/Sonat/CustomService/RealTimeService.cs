using UnityEngine;
using SonatFramework.Systems;
using Cysharp.Threading.Tasks;
using System.Threading.Tasks;
using System;
using SonatFramework.Systems.TimeManagement;
using SonatFramework.Systems.GameDataManagement;

namespace GrillSort.RealTime
{
    [CreateAssetMenu(fileName = "RealTimeService", menuName = "My Services/Real Time Service")]
    public class RealTimeService : SonatServiceSo, IServiceInitializeAsync
    {
#if UNITY_EDITOR
        // cheat time
        public int AddedDays { get => PlayerPrefs.GetInt("AddedDays", 0); set => PlayerPrefs.SetInt("AddedDays", value); }
        public int AddedHours { get => PlayerPrefs.GetInt("AddedHours", 0); set => PlayerPrefs.SetInt("AddedHours", value); }
        public int AddedMinutes { get => PlayerPrefs.GetInt("AddedMinutes", 0); set => PlayerPrefs.SetInt("AddedMinutes", value); }
        public int AddedSeconds { get => PlayerPrefs.GetInt("AddedSeconds", 0); set => PlayerPrefs.SetInt("AddedSeconds", value); }
#endif

        public Action OnNextDay;

        private readonly Service<DataService> _dataService = new();
        private readonly Service<TimeService> _timeService = new();
        public RealTimeData data;
        public static string DataKey = "RealTime_Data";

        private float lastRealtime;
        private int secondsUntilEndOfDay;

        private float beginLoopTime; // thời gian bắt đầu loop

        public async UniTaskVoid InitializeAsync()
        {
            await LoadData();
            await LoadConfig();

            // check new day
            var currentTime = GetCurrentTime();

            if (data.loginTimeInDay.Date != currentTime.Date)
            {
                OnNextDay?.Invoke();
            }

            // check loop
            InitOnLogin();
        }

        #region Data/ Config
        private async Task LoadConfig()
        {
#if UNITY_EDITOR
            if (AddedDays == 0 && AddedHours == 0 && AddedMinutes == 0 && AddedSeconds == 0)
            {
                AddedDays = 0;
                AddedHours = 0;
                AddedMinutes = 0;
                AddedSeconds = 0;
            }
#endif
        }

        private async Task LoadData()
        {
            data = _dataService.Instance.GetData<RealTimeData>(DataKey);
            if (data == null)
            {
                data = new RealTimeData();
            }
        }

        private void SaveData()
        {
            _dataService.Instance.SetData(DataKey, data);
        }

        public void InitOnLogin()
        {
            data.loginTimeInDay = GetCurrentTime();
            SaveData();
            lastRealtime = Time.realtimeSinceStartup;
            Debug.Log("InitOnLogin: " + data.loginTimeInDay);
            secondsUntilEndOfDay = Mathf.CeilToInt((float)(data.loginTimeInDay.Date.AddDays(1) - data.loginTimeInDay).TotalSeconds);
            CheckLoop().Forget();
        }

        private async UniTaskVoid CheckLoop()
        {
            // await UniTask.Delay(secondsUntilEndOfDay * 1000, DelayType.Realtime);

            beginLoopTime = Time.realtimeSinceStartup;
            while (true)
            {
                if (Time.realtimeSinceStartup - beginLoopTime <= secondsUntilEndOfDay)
                {
                    await UniTask.Delay(1000, DelayType.Realtime);
                }
                else
                {
                    break;
                }
            }

            OnNextDay?.Invoke();
            InitOnLogin();
        }

        #endregion

        public int GetRemainingTimeInDay()
        {
            var remainingTime = Mathf.FloorToInt(secondsUntilEndOfDay - (Time.realtimeSinceStartup - lastRealtime));
            if (remainingTime == 24 * 3600 || remainingTime <= 0)
            {
                remainingTime = 24 * 3600 - 1;
            }
            return remainingTime;
        }

        public DateTime GetCurrentTime()
        {
            var currentTime = _timeService.Instance.GetCurrentTime();
#if UNITY_EDITOR
            currentTime = currentTime.AddDays(AddedDays);
            currentTime = currentTime.AddHours(AddedHours);
            currentTime = currentTime.AddMinutes(AddedMinutes);
            currentTime = currentTime.AddSeconds(AddedSeconds);
#endif
            return currentTime;
        }

        public int GetDaysPassed(DateTime lastLogin, DateTime dateTime)
        {
            return _timeService.Instance.GetDaysPassed(lastLogin, dateTime);
        }

        public long GetCurrentTimeUnix()
        {
            return ((DateTimeOffset)GetCurrentTime()).ToUnixTimeSeconds();
        }
    }

    public class RealTimeData
    {
        public DateTime loginTimeInDay;
    }

}