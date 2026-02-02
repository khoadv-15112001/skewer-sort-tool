using Sonat.Enums;
using SonatFramework.Scripts.SonatSDKAdapterModule;

using UnityEngine;

public class ShopPackWidget : PackIapWidget
{
    [SerializeField] private ShopItemKey[] shopItemKeys;
    [SerializeField] private string[] popupNames;

    public override void Setup()
    {
        if (active)
        {
            int userDay = MySonatFramework.userDataService.UserDay;
            this.shopItemKey = shopItemKeys[(userDay) % shopItemKeys.Length];
            this.popupName = popupNames[(userDay) % popupNames.Length];
        }
        base.Setup();
    }
    public override bool CheckShowPopup()
    {
        return SonatSDKAdapter.GetRemoteBool("auto_show_shop_pack_widget", false);
    }
}