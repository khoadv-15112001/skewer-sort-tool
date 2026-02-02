using System;
using System.Collections;
using Sirenix.OdinInspector;
using Sonat;
using Sonat.CustomService;
using Sonat.Enums;
using SonatFramework.Scripts.Helper;
using SonatFramework.Scripts.SonatSDKAdapterModule;
using SonatFramework.Systems;
using SonatFramework.Systems.EventBus;
using SonatFramework.Systems.GameDataManagement;
using SonatFramework.Systems.InventoryManagement;
using SonatFramework.Systems.TimeManagement;
using UnityEngine;
using UnityEngine.Purchasing;

namespace SonatFramework.Scripts.Feature.Lives
{
    [CreateAssetMenu(fileName = "LivesService", menuName = "Sonat Services/Live Service")]
    public class LivesService : SonatServiceSo, IServiceInitialize, IServiceApplicationFocus
    {
        public long timeRemainToRefill;

        [BoxGroup("SERVICES", true)]
        [Required]
        [SerializeField]
        private Service<InventoryService> inventoryService = new();

        [BoxGroup("SERVICES", true)]
        [Required]
        [SerializeField]
        private Service<TimeService> timeService = new();

        [BoxGroup("SERVICES", true)]
        [Required]
        [SerializeField]
        private Service<DataService> dataService = new();

        [BoxGroup("CONFIGS", true)][Required] public LivesConfig config;

        private Coroutine countUnlimitedLives;

        private Coroutine refillCoroutine;
        private LongDataPref timeFinishUnlimited;
        private LongDataPref timeStartCountRefill;

        public IntDataPref isUnlimitedLive;
        public Action onLivesUpdate;
        public IntDataPref refillFreeCount;
        public bool forceInternet = false;
        private readonly Service<CustomTrackingService> _trackingService = new();

        public virtual void Initialize()
        {
            if (dataService.Instance.GetBool("LIVES_SETUP_FIRST_TIME", false)) SetUserFirstTime();

            SetupOnStart();

            Debug.Log($"Lives Service Initialized, refillPrice = {config.refillPrice}");
        }

        protected virtual void SetUserFirstTime()
        {
            inventoryService.Instance.SetResource(GameResource.Lives, config.maxLives);
            dataService.Instance.SetBool("LIVES_SETUP_FIRST_TIME", true);
        }

        protected virtual void SetupOnStart()
        {
            timeFinishUnlimited = new LongDataPref("TimeFinishUnlimitedLive");
            timeStartCountRefill = new LongDataPref("TimeStartCountRefill");
            isUnlimitedLive = new IntDataPref("IsUnLimitedLive");
            refillFreeCount = new IntDataPref("ReFillLivesFreeCount");
            config = SonatSDKAdapter.GetRemoteConfig<LivesConfig>("lives_config", config);
            config.SetSinglePrice();

            CheckOnFocus();
        }

        public virtual void CheckOnFocus()
        {
            CheckUnlimitedLive();
            if (isUnlimitedLive.BoolValue)
            {
            }
            else
            {
                var crrLives = inventoryService.Instance.GetResource(GameResource.Lives);
                if (timeStartCountRefill.Value == 0)
                {
                    if (crrLives <= 0 && (!forceInternet || SonatSdkManager.IsInternetConnection()))
                        timeFinishUnlimited.Value = timeService.Instance.GetUnixTimeSeconds(forceInternet);
                    else
                        return;
                }

                var now = timeService.Instance.GetUnixTimeSeconds(forceInternet);
                var liveAdd = (int)((now - timeStartCountRefill.Value) / config.timeRefillLives);
                if (liveAdd > 0)
                {
                    var newLive = Mathf.Clamp(crrLives + liveAdd, 0, config.maxLives);

                    inventoryService.Instance.SetResource(GameResource.Lives, newLive);

                    var realLiveAdd = newLive - crrLives;
                    EarnResourceEvent eventData = new EarnResourceEvent(GameResource.LivesService_SingleLive, realLiveAdd
                        , new() { spendId = "progress", spendType = "progress" });
                    EventBus<EarnResourceEvent>.Raise(eventData);

                    if (newLive >= config.maxLives)
                        timeStartCountRefill.Value = 0;
                    else
                        timeStartCountRefill.Value += liveAdd * config.timeRefillLives;
                    onLivesUpdate?.Invoke();
                    inventoryService.Instance.NotiUpdateResource(GameResource.Lives);
                }

                if (crrLives + liveAdd < config.maxLives)
                {
                    EarnResourceLogData earnData = new()
                    {
                        spendId = "progress",
                        spendType = "progress"
                    };

                    refillCoroutine = SonatSystem.Instance.StartCoroutine(WaitActionRealtime(GetTimeRefillRemain(),
                        () => RefillLive(1, earnData)));
                }

            }
        }

        public int GetRefillPrice()
        {
            //var crrLive = inventoryService.Instance.GetResource(GameResource.Lives);
            //var livesMiss = config.maxLives - crrLive;
            //var price = config.refillPrice / config.maxLives * livesMiss;
            //return price;
            return config.refillPrice;
        }
        public int GetSingleLivePrice()
        {
            return config.SinglePrice;
        }

        //private void OnEndLevel(bool isWin)
        //{
        //    if (!isWin) ReduceLive(1, "lose");
        //}

        private bool CheckUnlimitedLive()
        {
            if (!isUnlimitedLive.BoolValue) return false;
            var now = timeService.Instance.GetUnixTimeSeconds(forceInternet);
            if (now == 0) return false;
            if (now > timeFinishUnlimited.Value)
            {
                FinishUnlimitedLives();
                return false;
            }

            CountUnlimitedLives(timeFinishUnlimited.Value - now);
            return true;
        }

        public virtual void OnResourceUpdate(GameResource resource)
        {
        }

        public virtual void AddUnlimitedLives(long sec, EarnResourceLogData logData = null)
        {
            if (isUnlimitedLive.BoolValue)
            {
                timeFinishUnlimited.Value += sec;
            }
            else
            {
                var now = timeService.Instance.GetUnixTimeSeconds(forceInternet);
                if (now <= 0) return;
                timeFinishUnlimited.Value = now + sec;
                isUnlimitedLive.BoolValue = true;
            }
            int value = (int)sec / 60; // quy doi sang phut
            _trackingService.Instance.LogEarnCurrency("unlimited_live_m", "lives", value, 
                logData?.spendType ?? "", 
                logData?.spendId ?? "", 
                logData?.source ?? "", 
                logData?.isFirstBuy ?? false);

            CheckUnlimitedLive();
            onLivesUpdate?.Invoke();
            //LogHelper.LogEarnCurrency(GameResource.lives, (int)sec, spendType, spendId, isFirstBuy, source);

        }

        private void FinishUnlimitedLives()
        {
            isUnlimitedLive.BoolValue = false;
            inventoryService.Instance.SetResource(GameResource.Lives, config.maxLives);
            inventoryService.Instance.NotiUpdateResource(GameResource.Lives);
            onLivesUpdate?.Invoke();
        }

        public void CountUnlimitedLives(long sec)
        {
            if (countUnlimitedLives != null) SonatSystem.Instance.StopCoroutine(countUnlimitedLives);

            countUnlimitedLives =
                SonatSystem.Instance.StartCoroutine(WaitActionRealtime(sec, FinishUnlimitedLives));
        }

        public long GetUnlimitedLiveRemain()
        {
            var now = timeService.Instance.GetUnixTimeSeconds(forceInternet);
            if (now <= 0) return 0;
            return timeFinishUnlimited.Value - now;
        }

        public long GetTimeRefillRemain()
        {
            var now = timeService.Instance.GetUnixTimeSeconds(forceInternet);
            if (now <= 0) return 0;
            return config.timeRefillLives - (now - timeStartCountRefill.Value);
        }

        public void ReduceLive(int quantity = 1, SpendResourceLogData spendData = null)
        {
            if (isUnlimitedLive.BoolValue || inventoryService.Instance.GetResource(GameResource.Lives) <= 0) return;

            if (spendData != null)
                spendData.price = GetSingleLivePrice();

            inventoryService.Instance.ReduceResource(GameResource.Lives, quantity, spendData);

            if (refillCoroutine == null) StartCountRefill();

            onLivesUpdate?.Invoke();
        }

        public void StartCountRefill()
        {
            var now = timeService.Instance.GetUnixTimeSeconds(forceInternet);
            if (now == 0) return;
            timeStartCountRefill.Value = now;

            EarnResourceLogData earnData = new()
            {
                spendId = "progress",
                spendType = "progress",
            };

            refillCoroutine =
                SonatSystem.Instance.StartCoroutine(WaitActionRealtime(config.timeRefillLives, () => RefillLive(1, earnData)));
        }

        public void RefillLive(int quantity, EarnResourceLogData logData)
        {
            var currLive = inventoryService.Instance.GetResource(GameResource.Lives);
            var newLive = Mathf.Clamp(currLive + quantity, 0, config.maxLives);
            int livesAdd = newLive - currLive;
            if (livesAdd <= 0) return;

            if (logData != null)
                logData.price = GetSingleLivePrice();

            inventoryService.Instance.AddResource(GameResource.Lives, livesAdd, logData);
            EventBus<AddItemEvent>.Raise(new AddItemEvent() { resource = GameResource.Lives, quantity = livesAdd });
            if (newLive == config.maxLives)
            {
                if (refillCoroutine != null)
                {
                    SonatSystem.Instance.StopCoroutine(refillCoroutine);
                    refillCoroutine = null;
                }

                timeStartCountRefill.Value = 0;
            }
            else
            {
                StartCountRefill();
            }

            onLivesUpdate?.Invoke();

            //LogHelper.LogEarnCurrency(GameResource.lives, quantity, spendType, spendId, false);
        }

        public void RefillOneLive(EarnResourceLogData logData)
        {
            var currLive = inventoryService.Instance.GetResource(GameResource.Lives);
            var newLive = Mathf.Clamp(currLive + 1, 0, config.maxLives);
            var livesAdd = newLive - currLive;
            if (livesAdd <= 0) return;
            if (logData != null)
                logData.price = GetSingleLivePrice();
            inventoryService.Instance.AddResource(GameResource.Lives, livesAdd, logData);
            EventBus<AddItemEvent>.Raise(new AddItemEvent() { resource = GameResource.Lives, quantity = livesAdd });
            if (newLive == config.maxLives)
                if (refillCoroutine != null)
                {
                    SonatSystem.Instance.StopCoroutine(refillCoroutine);
                    refillCoroutine = null;
                    timeStartCountRefill.Value = 0;
                }

            onLivesUpdate?.Invoke();

            //LogHelper.LogEarnCurrency(GameResource.lives, 1, "rw_ads", "rw_ads", false);
        }

        public virtual void RefillFullLive(EarnResourceLogData logData)
        {
            RefillLive(config.maxLives, logData);
        }

        public virtual bool CanRefillFree()
        {
            if (refillFreeCount.Value >= config.refillFree) return false;
            return true;
        }

        public bool IsFullLives()
        {
            if (isUnlimitedLive.BoolValue) return true;
            return inventoryService.Instance.GetResource(GameResource.Lives) == config.maxLives;
        }


        public virtual bool CanPlay()
        {
            if (isUnlimitedLive.BoolValue) return true;
            return inventoryService.Instance.GetResource(GameResource.Lives) > 0;
        }

        public virtual bool CanReplay()
        {
            if (isUnlimitedLive.BoolValue) return true;
            return inventoryService.Instance.GetResource(GameResource.Lives) > 1;
        }

        private IEnumerator WaitActionRealtime(long time, Action callback)
        {
            yield return new WaitForSecondsRealtime(time);
            callback?.Invoke();
        }

        public void SetLimitLives(int limit)
        {
            if (limit == config.maxLives) return;

            var oldLimit = LivesConfig.MaxLives;
            //config.maxLives = limit; // lưu trực tiếp vào SO để những lần vào sau không cần xử lý

            LivesConfig.MaxLives = limit;

            CheckUnlimitedLive();
            if (!isUnlimitedLive.BoolValue)
            {
                if (inventoryService.Instance.GetResource(GameResource.Lives) > limit)
                {
                    // set full lives
                    inventoryService.Instance.SetResource(GameResource.Lives, 0);
                    RefillFullLive(new EarnResourceLogData() { spendType = "lives", spendId = "lives", source = "set_limit_lives" });
                }
                else
                {
                    if (timeStartCountRefill.Value == 0) // đang full ở trạng thái limit cũ
                    {
                        timeStartCountRefill.Value = timeService.Instance.GetUnixTimeSeconds(forceInternet);
                    }
                    else if (timeStartCountRefill.Value > 0)
                    {
                        // tiếp tục đếm refill
                    }
                }
            }

            EventBus<AddLimitItemEvent>.Raise(new AddLimitItemEvent()
            {
                resource = GameResource.Lives,
                quantity = limit - oldLimit
            });
        }

        public void ResetLimitLives()
        {
            SetLimitLives(config.defaultMaxLives);
        }

        public void OnApplicationFocus(bool focus)
        {
            if (focus)
            {
                CheckOnFocus();
            }
        }
    }
}