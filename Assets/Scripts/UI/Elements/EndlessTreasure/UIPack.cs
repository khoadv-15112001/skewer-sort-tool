
using System;
using Sonat.Enums;
using SonatFramework.Scripts.Feature.Shop.UI;
using SonatFramework.Scripts.UIModule;
using SonatFramework.Scripts.Utils;
using SonatFramework.Systems;
using SonatFramework.Systems.InventoryManagement;
using SonatFramework.Systems.InventoryManagement.GameResources;
using UnityEngine;
using UnityEngine.UI;

namespace GrillSort.EndlessTreasure
{
    public class UIPack : UIShopPackBase
    {
        [SerializeField] private Button btnFree;

        private readonly Service<EndlessTreasureService> endlessTreasureService = new();
        private readonly Service<InventoryService> inventoryService = new();
        private int packIdx;
        private RewardData currentRewardData;

        protected override void OnEnable()
        {
            shopService.Instance.OnBuySuccess += OnBuySuccess;
        }


        public void SetData(int packIdx, ShopItemKey key)
        {
            this.packIdx = packIdx;
            this.key = key;

            UpdateView();
            SetPackData();

            SetClaim();
        }

        public override void SetPackData()
        {
            uiRewardGroup.Clear();

            if (key == ShopItemKey.None)
            {
                currentRewardData = endlessTreasureService.Instance.GetRewardInPack(packIdx);
                uiRewardGroup.SetData(currentRewardData);
            }
            else
            {
                shopPack = shopService.Instance.GetPackData(key);
                currentRewardData = shopPack.rewardData;
                uiRewardGroup.SetData(currentRewardData);
            }
        }

        public ShopItemKey GetShopItemKey()
        {
            return key;
        }

        protected override void OnBuySuccess(ShopItemKey shopItemKey)
        {
            if (key == shopItemKey)
            {
                BuyComplete();
            }
        }

        protected override void BuyComplete()
        {
            // update data trước
            endlessTreasureService.Instance.ClaimPack(packIdx);
            var uiData = new UIData();
            uiData.Add("IAP", true);
            uiData.Add("Reward", currentRewardData);
            uiData.Add("onClaimComplete", (Action)(() =>
            {
                onBuySuccess?.Invoke();
            }));
            PanelManager.Instance.OpenPanel<PopupReward>(uiData);
            // update layout
            ResetLayout();
        }

        public void OnClickFree()
        {
            var logData = new EarnResourceLogData
            {
                spendType = "endless_treasure",
                spendId = $"endless_treasure_{packIdx}",
                isFirstBuy = false,
                source = "non_iap"
            };

            inventoryService.Instance.AddReward(currentRewardData, logData);

            endlessTreasureService.Instance.ClaimPack(packIdx);
            var uiData = new UIData();
            uiData.Add("Reward", currentRewardData);
            uiData.Add("onClaimComplete", (Action)(() =>
            {
                onBuySuccess?.Invoke();
            }));
            PanelManager.Instance.OpenPanel<PopupReward>(uiData);


        }

        public void HideButtons()
        {
            btnFree.gameObject.SetActive(false);
            buyButton.gameObject.SetActive(false);
        }

        public void SetClaim(bool interactable = true)
        {
            btnFree.gameObject.SetActive(key == ShopItemKey.None);
            buyButton.gameObject.SetActive(key != ShopItemKey.None);
            btnFree.interactable = interactable;
            buyButton.interactable = interactable;
        }

        protected override void ResetLayout()
        {
            transform.GetComponentInParent<UIShopPackContent>()?.UpdateElements();
        }

        protected override void OnBuyClick()
        {
            if (!MySonatFramework.IsNetworkAvailable())
            {
                NoInternet.Instance.ForceShowPopup();
                return;
            }
            base.OnBuyClick();
        }
    }
}
