using Sonat.Enums;
using SonatFramework.Scripts.UIModule;
using SonatFramework.Systems.UserData;
using UnityEngine;

public class PopupPack : Panel
{
    [SerializeField] internal ShopItemKey shopItemKey;

    public override void OnSetup()
    {
        base.OnSetup();
    }

    public virtual void OnBuySuccess()
    {
        Close();
    }
}