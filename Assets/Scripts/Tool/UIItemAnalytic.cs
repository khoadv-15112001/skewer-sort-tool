using System;
using System.Collections;
using System.Collections.Generic;
using Manager;
using SonatFramework.Scripts.UIModule;
using SonatFramework.Scripts.UIModule.UIElements;
using TMPro;
using Tool;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public class UIItemAnalytic : MonoBehaviour
{
    [SerializeField] UIToolAnalytic uIToolAnalytic;
    [SerializeField] private FixedImageRatio fixedImageRatio;
    [SerializeField] private TMP_Text txtId;
    [SerializeField] private TMP_Text txtQuantity;
    [SerializeField] private Image bgr;
    [SerializeField] private Color[] colors;
    private int id;
    private int quantity;

    public void SetData(int id, int quantity)
    {
        if (quantity == 0)
        {
            gameObject.SetActive(false);
            return;
        }

        this.id = id;
        txtId.text = id.ToString();
        fixedImageRatio.SetSpriteAsync(PathManager.ItemSprite(id));
        UpdateQuantity(quantity);
    }

    public void UpdateQuantity(int quantity)
    {
        this.quantity = quantity;
        txtQuantity.text = quantity.ToString();
        UpdateVisual();
    }

    private void UpdateVisual()
    {
        int checkQuantity = ToolManager.numberSlotMax;
        if (quantity % checkQuantity == 0)
        {
            bgr.color = colors[0];
        }
        else
        {
            bgr.color = colors[1];
        }
    }

    public void Select(bool select)
    {
        if (select)
        {
            bgr.color = colors[2];
        }
        else
        {
            UpdateVisual();
        }
    }

    public void OnClickThis()
    {
        if (Input.GetKey(KeyCode.LeftShift))
        {
            ToolItemSelectorPanel.Instance.Open(ChangeAllItem);
            return;
        }

        uIToolAnalytic.OnSelectItemAnalytic(this, id);
    }

    private void ChangeAllItem(int newId)
    {
        if (ToolManager.Instance.GetGrillsHasItem(newId).Count > 0)
        {
            UIData uiData = new();
            uiData.Add("content", $"Item {newId} already exists in this level, Do you want to continue?");
            uiData.Add("onYes", (Action)(() => ConfirmAllItem(newId)));
            PanelManager.Instance.OpenPanel<ConfirmPanel>(uiData);
        }
        else
        {
            ConfirmAllItem(newId);
        }
    }

    private void ConfirmAllItem(int newId)
    {
        ToolManager.Instance.SwitchItemId(this.id, newId);
    }
}