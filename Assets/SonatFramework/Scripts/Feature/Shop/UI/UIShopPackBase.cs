using Sonat.Enums;
using SonatFramework.Scripts.SonatSDKAdapterModule;
using SonatFramework.Scripts.UIModule.UIElements;
using SonatFramework.Systems;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace SonatFramework.Scripts.Feature.Shop.UI
{
    public class UIShopPackBase : MonoBehaviour
    {
        [SerializeField] protected ShopItemKey key;

        [SerializeField] protected TMP_Text textPrice;
        [SerializeField] protected Button buyButton;
        [SerializeField] protected UIRewardGroup uiRewardGroup;
        [SerializeField] protected UnityEvent onBuySuccess;

        protected readonly Service<ShopService> shopService = new();

        protected ShopPack shopPack;

        protected virtual void Start()
        {
            UpdateView();
            SetPackData();
            buyButton.onClick.AddListener(OnBuyClick);
        }

        protected virtual void OnEnable()
        {
            if (!shopService.Instance.VerifyPack(key))
            {
                gameObject.SetActive(false);
                return;
            }

            shopService.Instance.OnBuySuccess += OnBuySuccess;
        }

        protected virtual void OnDisable()
        {
            shopService.Instance.OnBuySuccess -= OnBuySuccess;
        }

        protected virtual void UpdateView()
        {
            if (textPrice != null && key != ShopItemKey.None)
                textPrice.text = $"{SonatSDKAdapter.GetProductPrice(key)}";
        }

        public virtual void SetPackData()
        {
            LoadPackData();
            if (shopPack is not { active: true })
            {
                gameObject.SetActive(false);
                return;
            }

            uiRewardGroup?.SetData(shopPack.rewardData);
        }

        public bool IsActive()
        {
            LoadPackData();
            return shopService.Instance.VerifyPack(key);
        }

        protected void LoadPackData()
        {
            shopPack ??= shopService.Instance.GetPackData(key);
        }


        protected virtual void OnBuyClick()
        {
            shopService.Instance.BuyPack(key);
        }

        protected virtual void OnBuySuccess(ShopItemKey shopItemKey)
        {
            if (shopItemKey != key)
            {
                if (!shopService.Instance.VerifyPack(key))
                {
                    gameObject.SetActive(false);
                }
                return;
            }
            BuyComplete();
        }

        protected virtual void BuyComplete()
        {
            ResetLayout();
            onBuySuccess?.Invoke();
        }

        protected virtual void ResetLayout()
        {
            if (!shopService.Instance.VerifyPack(key))
            {
                gameObject.SetActive(false);
                return;
            }
            transform.GetComponentInParent<UIShopPackContent>()?.UpdateElements();
        }
    }
}