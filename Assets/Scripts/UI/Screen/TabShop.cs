using System.Collections;
using System.Collections.Generic;
using SonatFramework.Scripts.Feature.Shop.UI;
using UnityEngine;

public class TabShop : UITabBase
{
    [SerializeField] private UIShopPackContent shopPackContent;
    private bool initialized = false;
    
    public override void OnShow()
    {
        base.OnShow();
        if (initialized)
        {
            shopPackContent.UpdateElements();
        }
        else
        {
            initialized = true;
        }
    }
}
