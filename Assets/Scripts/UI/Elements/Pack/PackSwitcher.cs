using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PackSwitcher : MonoBehaviour
{
    [SerializeField] private UIPackAppearanceScheduler[] packs;
    [SerializeField] private GameObject hideableObject;
    private void OnEnable()
    {
        CheckActivePack();
    }

    public void CheckActivePack()
    {
        HideAllPacks();
        // Hiển thị gói đầu tiên khả dụng
        foreach (var pack in packs)
        {
            if (pack.IsPurchasedToday == false || pack.UnlimitedPurchases == true)
            {
                pack.gameObject.SetActive(true);
                return;
            }
        }

        // bị ẩn
        hideableObject.SetActive(false);
    }

    private void HideAllPacks()
    {
        foreach (var pack in packs)
        {
            pack.gameObject.SetActive(false);
        }
    }
}
