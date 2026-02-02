using Sirenix.OdinInspector;
using Sonat.Enums;
using SonatFramework.Scripts.Feature.Shop;
using SonatFramework.Scripts.UIModule.UIElements;
using SonatFramework.Systems;
using UnityEngine;

public class UISaleController : MonoBehaviour
{
    [SerializeField, ReadOnly] private ShopItemKey normalPackKey;
    [SerializeField, ReadOnly] private ShopItemKey salePackKey;

#if UNITY_EDITOR
    void OnValidate()
    {
        normalPackKey = normalPack.GetShopItemKey();
        salePackKey = salePack.GetShopItemKey();
    }

#endif


    [SerializeField] private UIShopPack normalPack;
    [SerializeField] private UIShopPack salePack;
    [SerializeField] private UITimeCounter timeCounter;
    [SerializeField] private bool checkStartSale = false;
    private int _saleSessionId;
    private readonly Service<SaleService> _saleService = new();
    private readonly Service<ShopService> _shopService = new();

    void OnEnable()
    {
        if (checkStartSale == true)
        {
            var sessionId = _saleService.Instance.CheckStartSalePack(normalPackKey);
            if (sessionId != -1)
            {
                _saleService.Instance.StartSale(sessionId);

                SetSalePack();
                _saleService.Instance.OnSaleFinished += OnSaleFinished;
                return;
            }
        }


        // Xác định có đang sale không?
        // Nếu có thì hiện pack sale
        // Nếu không thì kiểm tra xem có pack này hợp lệ không? --> hiển thị
        // Nếu không hợp lệ thì không hiển thị gì cả

        _saleSessionId = _saleService.Instance.GetActiveSessionId(normalPackKey); // lấy phiên sale đang diễn ra
        if (_saleSessionId != -1)
        {
            SetSalePack();
            _saleService.Instance.OnSaleFinished += OnSaleFinished;
        }
        else
        {
            // cả 2 gói đều hợp lệ
            if (_shopService.Instance.VerifyPack(normalPackKey) == true)
            {
                SetNormalPack();
            }
            else
            {
                gameObject.SetActive(false);
            }
        }
    }

    void OnDisable()
    {
        _saleService.Instance.OnSaleFinished -= OnSaleFinished;
    }

    private void OnSaleFinished(int id)
    {
        if (id == _saleSessionId)
        {
            OnEnable();
        }
    }

    public void SetNormalPack()
    {
        normalPack.gameObject.SetActive(true);
        salePack.gameObject.SetActive(false);
        timeCounter.gameObject.SetActive(false);
    }

    public void SetSalePack()
    {
        normalPack.gameObject.SetActive(false);
        salePack.gameObject.SetActive(true);
        timeCounter.gameObject.SetActive(true);
        timeCounter.SetData(_saleService.Instance.GetRemainingTime(_saleSessionId), null);
    }

    public void OnBuyCompleteNormalPack()
    {
        OnEnable();
    }
    public void OnBuyCompleteSalePack()
    {
        _saleService.Instance.BuySuccess(_saleSessionId);
    }
}
