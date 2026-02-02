using Sonat.Enums;
using SonatFramework.Scripts.Feature.Shop.UI;
using SonatFramework.Scripts.SonatSDKAdapterModule;
using SonatFramework.Scripts.UIModule;
using SonatFramework.Scripts.UIModule.SpriteService;
using SonatFramework.Scripts.UIModule.UIElements;
using SonatFramework.Systems;
using SonatFramework.Systems.BoosterManagement;
using SonatFramework.Systems.EventBus;
using SonatFramework.Systems.InventoryManagement;
using TMPro;
using UnityEngine;

namespace GrillSort.PreBooster
{
    public class PopupBuyPreBooster : Panel
    {
        [SerializeField] protected TMP_Text txtCoinPrice;
        [SerializeField] protected TMP_Text txtValue;
        [SerializeField] protected FixedImageRatio icon;
        [SerializeField] protected string iconNamePattern = "ico_{0}";
        [SerializeField] protected GameObject descPreBoosterFreeze;
        [SerializeField] protected GameObject descPreBoosterMagnet;
        [SerializeField] protected GameObject descPreBoosterDoubleStar;
        protected UIBoosterBase uIBooster;
        protected BoosterConfig boosterConfig;
        protected readonly Service<PreBoosterService> preBoosterService = new();
        protected readonly Service<SpriteAtlasService> spriteService = new();
        [SerializeField] private GameObject buyWithAds;
        private CheckRemoteByDayCounter checkRwdByDayCounter;
        private CheckRemoteByLevelCounter checkRwdByLevelCounter;


        public override void Open(UIData uiData)
        {
            base.Open(uiData);
            boosterConfig = uiData.Get<BoosterConfig>("booster_config");
            txtCoinPrice.text = $"{boosterConfig.price * boosterConfig.value}";
            txtValue.text = $"+{boosterConfig.value}";
            icon.SetSprite(spriteService.Instance.GetSprite(string.Format(iconNamePattern, boosterConfig.booster)));


            descPreBoosterFreeze.SetActive(false);
            descPreBoosterMagnet.SetActive(false);
            descPreBoosterDoubleStar.SetActive(false);
            switch (boosterConfig.booster)
            {
                case GameResource.PreBoosterFreeze:
                    descPreBoosterFreeze.SetActive(true);
                    break;
                case GameResource.PreBoosterMagnet:
                    descPreBoosterMagnet.SetActive(true);
                    break;
                case GameResource.PreBoosterDoubleStar:
                    descPreBoosterDoubleStar.SetActive(true);
                    break;
            }
            
            checkRwdByDayCounter = new CheckRemoteByDayCounter("by_day_show_rwd_pre_booster", boosterConfig.booster.ToString() , 999);
            checkRwdByLevelCounter = new("by_level_show_rwd_pre_booster", boosterConfig.booster.ToString(), 999);
            buyWithAds.SetActive(checkRwdByDayCounter.CheckCounter() && checkRwdByLevelCounter.CheckCounter());
        }

        public virtual void OnBuyWithCoinClick()
        {
            if (preBoosterService.Instance.BuyBooster(boosterConfig.booster, boosterConfig.value))
            {
                EventBus<AddItemEvent>.Raise(new AddItemEvent()
                {
                    resource = boosterConfig.booster,
                    quantity = boosterConfig.value,
                    position = icon.transform.position,
                    collectEffect = new CollectEffectSingle()
                    {
                        collectEffectName = "CollectResourceSingleItemPreBooster"
                    }
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
            MySonatFramework.ShowRewardAds(OnWatchedAds, "booster", boosterConfig.booster.ToString());
        }

        protected virtual void OnWatchedAds()
        {
            var logEarn = new EarnResourceLogData()
            {
                spendType = "rw_ads",
                spendId = "rw_ads",
                source = "non_iap"
            };
            preBoosterService.Instance.AddBooster(boosterConfig.booster, 1, logEarn);
            EventBus<AddItemEvent>.Raise(new AddItemEvent()
            { resource = boosterConfig.booster, quantity = 1, position = icon.transform.position, collectEffect = new CollectEffectSingle() });
            checkRwdByDayCounter.AddValue();
            checkRwdByLevelCounter.AddValue();
            Close();
        }
    }
}