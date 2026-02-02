using System.Collections;
using System.Collections.Generic;
using Base.Singleton;
using GrillSort.PreBooster;
using SonatFramework.Scripts.UIModule.UIElements;
using UnityEngine;

public class UICurrencyManager : Singleton<UICurrencyManager>
{
    protected override void OnAwake()
    {
    }

    [SerializeField] private List<UIButtonPreBooster> preboosterCurrencyItems;

    private void OnEnable()
    {
        ShowPreBoosterCurrency(true);
    }

    public void ShowPreBoosterCurrency(bool isShow)
    {
        if (preboosterCurrencyItems == null) return;
        foreach (var item in preboosterCurrencyItems)
        {
            item.gameObject.SetActive(isShow);
        }
    }

}
