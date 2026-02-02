using Sonat.Enums;
using SonatFramework.Scripts.Feature.Shop;
using SonatFramework.Systems;
using TMPro;
using UnityEngine;

namespace SonatFramework.Scripts.UIModule.UIElements
{
    public abstract class UIWidgetMoBase : MonoBehaviour
    {
        [SerializeField] private ShopItemKey shopItemKey;
        [SerializeField] private TMP_Text txtContent;
        protected readonly Service<ShopService> shopService = new();

        // Start is called before the first frame update
        protected virtual void Start()
        {
            shopService.Instance.OnBuySuccess += OnBoughtIAP;
        }

        protected virtual void OnDestroy()
        {
            shopService.Instance.OnBuySuccess -= OnBoughtIAP;
        }

        protected virtual void OnBoughtIAP(ShopItemKey key)
        {
            if (key != shopItemKey) return;
            CheckPack();
        }

        protected virtual void CheckPack()
        {
            if (!Verify()) Finish();
        }

        protected virtual bool Verify()
        {
            return shopService.Instance.VerifyPack(shopItemKey);
        }

        protected virtual void Finish()
        {
            gameObject.SetActive(false);
            shopService.Instance.OnBuySuccess -= OnBoughtIAP;
        }

        public abstract void OnClickWidget();
    }
}