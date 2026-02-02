using SonatFramework.Scripts.Feature.Shop.UI;
using SonatFramework.Scripts.SonatSDKAdapterModule;
using SonatFramework.Scripts.UIModule;
using SonatFramework.Scripts.UIModule.SpriteService;
using SonatFramework.Scripts.UIModule.UIElements;
using SonatFramework.Systems;
using SonatFramework.Systems.BoosterManagement;
using SonatFramework.Systems.EventBus;
using SonatFramework.Systems.InventoryManagement;
using SonatFramework.Systems.TrackingModule;
using TMPro;
using UnityEngine;

namespace SonatFramework.Templates.UI.ScriptBase
{
    public class PopupBuyBoosterBase : Panel
    {
        [SerializeField] protected TMP_Text txtTile;
        [SerializeField] protected TMP_Text txtCoinPrice;
        [SerializeField] protected TMP_Text txtValue;
        [SerializeField] protected FixedImageRatio icon;
        [SerializeField] protected string iconNamePattern = "ico_{0}";
        protected UIBoosterBase uIBooster;
        protected BoosterConfig boosterConfig;
        protected readonly Service<BoosterService> boosterService = new();
        protected readonly Service<SpriteAtlasService> spriteService = new();
        [SerializeField] protected GameObject buyWithRwdButton;

        public override void Open(UIData uiData)
        {
            base.Open(uiData);
            boosterConfig = uiData.Get<BoosterConfig>("booster_config");
            txtCoinPrice.text = $"{boosterConfig.price * boosterConfig.value}";
            txtValue.text = $"+{boosterConfig.value}";
            icon.SetSprite(spriteService.Instance.GetSprite(string.Format(iconNamePattern, boosterConfig.booster)));

            //int maxBuyByRwd = SonatSDKAdapter.GetValueByLevel("by_level_show_rwd_booster", 9999);
            //int buyWithRwdCount = SonatSystem.GetService<GameplayAnalyticsService>().levelPlayData.buyBoosterByRwd;
            //buyWithRwdButton.SetActive(buyWithRwdCount < maxBuyByRwd);
        }

        public virtual void OnBuyWithCoinClick()
        {
            if (boosterService.Instance.BuyBooster(boosterConfig.booster, boosterConfig.value))
            {
                EventBus<AddItemEvent>.Raise(new AddItemEvent()
                {
                    resource = boosterConfig.booster, quantity = boosterConfig.value, position = icon.transform.position,
                    collectEffect = new CollectEffectSingle()
                });
                Close();
            }
            else
            {
                PopupToast.Cretate("Not enough coin!");
                PanelManager.Instance.OpenPanelByName<ShopPanelBase>("ShopPanel");
            }
        }

        public virtual void OnBuyWithAdsClick()
        {
            SonatSDKAdapter.ShowRewardAds(OnWatchedAds, "booster", boosterConfig.booster.ToString());
        }

        protected virtual void OnWatchedAds()
        {
            var logEarn = new EarnResourceLogData()
            {
                spendType = "rw_ads",
                spendId = "rw_ads",
                source = "non_iap",
                price = boosterConfig.price
            };
            boosterService.Instance.AddBooster(boosterConfig.booster, 1, logEarn);
            EventBus<AddItemEvent>.Raise(new AddItemEvent()
                { resource = boosterConfig.booster, quantity = 1, position = icon.transform.position, collectEffect = new CollectEffectSingle() });
            SonatSystem.GetService<GameplayAnalyticsService>().levelPlayData.buyBoosterByRwd++;
            Close();
        }
    }
}