using UnityEngine;
using TMPro;
using Sonat.Enums;
using SonatFramework.Scripts.SonatSDKAdapterModule;

public class OriginPrice : MonoBehaviour
{
    [SerializeField] protected ShopItemKey shopItemKey;
    [SerializeField, Range(0, 99)] private int sale;

    [SerializeField] private TMP_Text priceText;

    private void OnEnable()
    {
        SetOrigin();
    }
    public void SetOrigin()
    {
        if (priceText == null) return;
        string discountedPrice = discountedPrice = SonatSDKAdapter.GetProductPrice(shopItemKey);

        if (string.IsNullOrEmpty(discountedPrice)) return;

        string originPrice = PriceCalculator.CalculateOriginalPriceFromText(discountedPrice, sale);

        //Debug.Log($"anhnt: discountedPrice {discountedPrice} originPrice {originPrice}");
        if (!string.IsNullOrEmpty(originPrice))
        {
            priceText.text = originPrice;
        }
        else
        {
            priceText.text = discountedPrice;
        }
    }
}
