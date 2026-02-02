using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using Sonat.Enums;
using SonatFramework.Scripts.SonatSDKAdapterModule;
using SonatFramework.Systems;
using SonatFramework.Systems.ConfigManagement;
using SonatFramework.Systems.EventBus;
using SonatFramework.Systems.InventoryManagement;
using UnityEngine;

namespace SonatFramework.Scripts.Feature.Shop
{
    [CreateAssetMenu(fileName = "ShopService", menuName = "Sonat Services/Shop Service")]
    public class ShopService : SonatServiceSo, IServiceInitialize
    {
        [BoxGroup("SERVICES", true)] [Required] [SerializeField]
        private Service<InventoryService> inventory = new();

        [BoxGroup("CONFIG", true)] [Required] [SerializeField]
        private Config<ShopConfig> shopConfig;

        private bool isFistBuyPack;

        private ShopItemKey packBuying = ShopItemKey.None;
        public Action<ShopItemKey> OnBuySuccess { get; set; }
        //public Action<ShopItemKey> OnBuyFailed { get; set; }

        public void Initialize()
        {
            packBuying = ShopItemKey.None;
        }

        public void BuyPack(ShopItemKey key)
        {
            if (packBuying != ShopItemKey.None)
            {
                return;
            }

            packBuying = key;
            isFistBuyPack = !SonatSDKAdapter.CheckPackBought(key);
            SonatSDKAdapter.BuyPack(key, OnBuyComplete, "pack");
        }

        public bool IsBuying()
        {
            return packBuying != ShopItemKey.None;
        }

        private void OnBuyComplete(bool success)
        {
            if (success)
            {
                ClaimPack(packBuying);
                
                OnBuySuccess?.Invoke(packBuying);
            }

            packBuying = ShopItemKey.None;
        }


        public void RestorePurchase(Action<List<int>> onSuccess)
        {
#if sonat_sdk_v2
            SonatSDKAdapter.Restore((List<int> itemBought) =>
            {
                foreach (var key in itemBought)
                {
                    var packData = GetPackData((ShopItemKey)key);
                    if (packData is { noAds: true } or { noAdsFree: true })
                    {
                        OnRestoreSuccess();
                        break;
                    }
                }

                onSuccess?.Invoke(itemBought);
            });
#else
            SonatSDKAdapter.Restore(() =>
            {
                OnRestoreSuccess();
                onSuccess?.Invoke(new List<int>());
            });
#endif
        }

        public void OnRestoreSuccess()
        {
            packBuying = ShopItemKey.no_ads;
            OnBuyComplete(true);
        }

        public void ClaimPack(ShopItemKey key)
        {
            var packData = GetPackData(key);
            if (packData == null) return;
            if (packData.noAds || packData.noAdsFree)
            {
                SonatSDKAdapter.SetNoAds(true);
                
                // Log earn NoAds resource
                var noAdsLogData = new EarnResourceLogData
                {
                    spendType = "pack_iap",
                    spendId = SonatSDKAdapter.FindProductId(key),
                    isFirstBuy = isFistBuyPack,
                    source = "iap"
                };
                EventBus<EarnResourceEvent>.Raise(new EarnResourceEvent(GameResource.NoAds, 1, noAdsLogData));
            }
            if (packData.rewardData is { resourceDatas: { Count: > 0 } })
            {
                var logData = new EarnResourceLogData
                {
                    spendType = "pack_iap",
                    spendId = SonatSDKAdapter.FindProductId(key),
                    isFirstBuy = isFistBuyPack,
                    source = "iap"
                };
                inventory.Instance.AddReward(packData.rewardData, logData, true);
            }
        }

        public ShopPack GetPackData(ShopItemKey key)
        {
            return shopConfig.config.packs.Find(e => e.key == key);
        }

        public List<ShopPack> GetPacksData(int group)
        {
            return shopConfig.config.packs.FindAll(e => e.Group == group);
        }

        private readonly Service<SaleService> _saleService = new();

        public bool VerifyPack(ShopItemKey shopItemKey)
        {
            var packData = GetPackData(shopItemKey);
            if (packData is not { active: true }) return false;
            if (packData.oneTimePurchase)
            {
                List<ShopPack> packsInGroup = GetPacksData(packData.Group);
                if (packsInGroup == null) return false;
                foreach (var pack in packsInGroup)
                {
                    if (SonatSDKAdapter.CheckPackBought(pack.key)) return false;
                }
            }

            if (packData.noAds && SonatSDKAdapter.IsNoads()) return false;

            if (_saleService.Instance.VerifyPack(shopItemKey) == false) return false;
            return true;
        }

        // public class Builder
        // {
        // 	private LoadObjectServiceAsync loadObjectServiceAsync = new SonatLoadResourcesAsync("Configs/Shop");
        // 	private string configKey = "ShopConfigs";
        //
        // 	public Builder WithLoadObjectAsync(LoadObjectServiceAsync loadObjectServiceAsync)
        // 	{
        // 		this.loadObjectServiceAsync = loadObjectServiceAsync;
        // 		return this;
        // 	}
        //
        // 	public Builder WithConfigKey(string configKey)
        // 	{
        // 		this.configKey = configKey;
        // 		return this;
        // 	}
        //
        // 	public ShopService Build()
        // 	{
        // 		ShopService shopService = new ShopService()
        // 		{
        // 			loadObjectServiceAsync = this.loadObjectServiceAsync,
        // 			configKey = this.configKey
        // 		};
        // 		return shopService;
        // 	}
        // }
    }
}