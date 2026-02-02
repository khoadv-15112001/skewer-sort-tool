using System;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UITargetItem : MonoBehaviour
{
    [ReadOnly]
    public int id;
    [ReadOnly]
    public int quantity;
    public TMP_InputField inputQuantity;
    public TMP_Text txtId;
    public Image imgIcon;
    public Button btnRemove;
    [HideInInspector]
    public UITargetController uiTargetController;

    private void Awake()
    {
        btnRemove.onClick.AddListener(OnRemoveClick);
        inputQuantity.onEndEdit.AddListener(OnQuantityChange);
    }

    private void OnDestroy()
    {
        btnRemove.onClick.RemoveAllListeners();
        inputQuantity.onEndEdit.RemoveAllListeners();
    }

    private void OnQuantityChange(string quantityTxt)
    {
        if (int.TryParse(quantityTxt, out int newQuantity))
        {
            uiTargetController.OnQuantityChange(id, newQuantity);
            quantity = newQuantity;
        }
        else
        {
            inputQuantity.text = quantity.ToString();
        }
    }

    private void OnRemoveClick()
    {
        uiTargetController.RemoveFromTargetList(this);
    }
}
