using System;
using Cysharp.Threading.Tasks;
using Sonat.Data;
using Sonat.Enums;
using SonatFramework.Scripts.SonatSDKAdapterModule;
using SonatFramework.Systems;
using SonatFramework.Systems.InventoryManagement;
using SonatFramework.Systems.InventoryManagement.GameResources;
using UnityEngine;

namespace GrillSort.SpeedUp
{
    [CreateAssetMenu(fileName = "SpeedUpService", menuName = "My Services/SpeedUpService")]
    public class SpeedUpService : SonatServiceSo, IServiceInitialize
    {
        [Header("Configuration")]
        [SerializeField] private SpeedUpConfig config;
        
        [Header("Debug")]
        [SerializeField] private bool enableDebugLog = true;
        
        private bool isSpeedUpActive;
        private float speedUpEndTime;
        
        // Events
        public event Action<bool> OnSpeedUpStateChanged;
        public event Action<float> OnSpeedUpProgress; // 0 to 1
        
        public SpeedUpConfig Config => config;
        public bool IsSpeedUpActive => isSpeedUpActive;
        public float SpeedMultiplier => config.speedMultiplier;
        
        public void Initialize()
        {
            if (config == null)
            {
                Debug.LogError("[SpeedUpService] Config is null! Please assign SpeedUpConfig in Inspector.");
                return;
            }
            
            isSpeedUpActive = false;
            Log("SpeedUpService initialized");
        }
        
        /// <summary>
        /// Get coin price based on user's campaign segment
        /// </summary>
        public int GetCoinPrice()
        {
            string campaignSegment = UserData.UserCampaignSegment.Value;
            int price = SonatSDKAdapter.GetValueBySegment(
                config.remoteConfigKey, 
                campaignSegment, 
                config.defaultCoinPrice
            );
            
            Log($"GetCoinPrice: Segment={campaignSegment}, Price={price}");
            return price;
        }
        
        /// <summary>
        /// Try to speed up with coin
        /// </summary>
        public bool TrySpeedUpWithCoin(Action<bool> onComplete = null)
        {
            int price = GetCoinPrice();
            
            if (!MySonatFramework.inventoryService.CanReduce(GameResource.Coin, price))
            {
                Log($"TrySpeedUpWithCoin: Failed - Not enough coins. Required: {price}");
                onComplete?.Invoke(false);
                return false;
            }
            
            // Reduce coin
            SpendResourceLogData logData = new SpendResourceLogData()
            {
                earnType = config.coinEarnType,
                earnId = config.coinEarnId,
                source = "non_iap"
            };
            MySonatFramework.inventoryService.ReduceResource(GameResource.Coin, price, logData);
            
            // Activate speed up
            ActivateSpeedUp("coin");
            
            Log($"TrySpeedUpWithCoin: Success - Spent {price} coins");
            onComplete?.Invoke(true);
            return true;
        }
        
        /// <summary>
        /// Try to speed up with reward ads
        /// </summary>
        public void TrySpeedUpWithAds(Action<bool> onComplete = null)
        {
            if (!config.enableRewardAds)
            {
                Log("TrySpeedUpWithAds: Failed - Reward ads disabled in config");
                onComplete?.Invoke(false);
                return;
            }
            
            MySonatFramework.ShowRewardAds(() =>
            {
                ActivateSpeedUp("ads");
                Log("TrySpeedUpWithAds: Success - Watched reward ad");
                onComplete?.Invoke(true);
            }, config.adsItemType, config.adsItemId);
        }
        
        /// <summary>
        /// Activate speed up effect
        /// </summary>
        private void ActivateSpeedUp(string source)
        {
            isSpeedUpActive = true;
            
            if (config.duration > 0)
            {
                speedUpEndTime = Time.time + config.duration;
                UpdateSpeedUpProgress().Forget();
            }
            
            OnSpeedUpStateChanged?.Invoke(true);
            
            Log($"ActivateSpeedUp: source={source}, duration={config.duration}s, multiplier={config.speedMultiplier}x");
        }
        
        /// <summary>
        /// Deactivate speed up effect
        /// </summary>
        public void DeactivateSpeedUp()
        {
            if (!isSpeedUpActive) return;
            
            isSpeedUpActive = false;
            speedUpEndTime = 0;
            
            OnSpeedUpStateChanged?.Invoke(false);
            OnSpeedUpProgress?.Invoke(0);
            
            Log("DeactivateSpeedUp: Speed up deactivated");
        }
        
        /// <summary>
        /// Update progress over time (if duration > 0)
        /// </summary>
        private async UniTaskVoid UpdateSpeedUpProgress()
        {
            while (isSpeedUpActive && config.duration > 0)
            {
                float remainingTime = speedUpEndTime - Time.time;
                
                if (remainingTime <= 0)
                {
                    DeactivateSpeedUp();
                    break;
                }
                
                float progress = 1f - (remainingTime / config.duration);
                OnSpeedUpProgress?.Invoke(progress);
                
                await UniTask.Yield();
            }
        }
        
        /// <summary>
        /// Get remaining time of speed up effect
        /// </summary>
        public float GetRemainingTime()
        {
            if (!isSpeedUpActive || config.duration <= 0)
                return 0;
            
            return Mathf.Max(0, speedUpEndTime - Time.time);
        }
        
        /// <summary>
        /// Get progress (0 to 1)
        /// </summary>
        public float GetProgress()
        {
            if (!isSpeedUpActive || config.duration <= 0)
                return 0;
            
            float remainingTime = GetRemainingTime();
            return 1f - (remainingTime / config.duration);
        }
        
        /// <summary>
        /// Check if user can afford coin speed up
        /// </summary>
        public bool CanAffordCoinSpeedUp()
        {
            int price = GetCoinPrice();
            return MySonatFramework.inventoryService.CanReduce(GameResource.Coin, price);
        }
        
        private void Log(string message)
        {
            if (enableDebugLog)
            {
                Debug.Log($"[SpeedUpService] {message}");
            }
        }
    }
}

