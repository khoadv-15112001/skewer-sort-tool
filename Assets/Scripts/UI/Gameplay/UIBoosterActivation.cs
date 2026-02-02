using TMPro;
using UnityEngine;

public class UIBoosterActivation : UIBooster
{
    [SerializeField] private TMP_Text txtQuantityInDisableObj;
    [SerializeField] private GameObject priceObjInDisableObj;

    protected override void UpdateData()
    {
        base.UpdateData();
        
        txtQuantityInDisableObj.text = boosterData.quantity.ToString();
        priceObjInDisableObj.SetActive(boosterData.quantity <= 0);
    }
}