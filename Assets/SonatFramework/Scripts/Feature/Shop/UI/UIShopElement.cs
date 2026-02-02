using System;
using SonatFramework.Scripts.Feature.Shop.UI;
using UnityEngine;

public class UIShopElement : MonoBehaviour
{
    public int priority;
    [SerializeField] private UIShopPackBase[] shopPacks;

    private void OnValidate()
    {
        if (shopPacks == null || shopPacks.Length == 0)
        {
            UIShopPackBase shopPackBase = gameObject.GetComponent<UIShopPackBase>();
            if (shopPackBase != null)
            {
                shopPacks = new[] { shopPackBase };
            }
        }
    }

    public void CheckContent()
    {
        if(shopPacks == null || shopPacks.Length == 0) return;
        foreach (var shopPack in shopPacks)
        {
            if (!shopPack.IsActive())
            {
                shopPack.gameObject.SetActive(false);
            }
        }
    }

    public bool IsEmpty()
    {
        if(shopPacks == null || shopPacks.Length == 0) return true;
        foreach (var shopPack in shopPacks)
        {
            if (shopPack.IsActive())
            {
                return false;
            }
        }
        return true;
    }
}
