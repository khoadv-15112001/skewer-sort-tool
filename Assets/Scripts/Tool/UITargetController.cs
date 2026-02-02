using System;
using System.Collections.Generic;
using System.Linq;
using Gameplay.LevelData;
using Manager;
using Tool;
using UnityEngine;

public class UITargetController : MonoBehaviour
{
    public Transform itemTargetContainer;

    public List<TargetData> targetList = new();

    public List<UITargetItem> targetItemList = new();

    public UITargetItem itemPrefab;

    public void SetData(List<TargetData> targetData)
    {
        Clear();
        if (targetData is null || targetData.Count == 0)
        {
            return;
        }
        foreach (var item in targetData)
        {
            AddItem(item.id, item.quantity);
        }
    }

    public void Clear()
    {
        targetList.Clear();
        if (targetItemList is not null && targetItemList.Count > 0)
        {
            foreach (var item in targetItemList)
            {
                Destroy(item.gameObject);
            }
        }
        targetItemList.Clear();
    }

    public void OnAddItemClick()
    {
        ToolItemSelectorPanel.Instance.Open(SetTargetItem);
    }

    private void SetTargetItem(int itemId)
    {
        TargetData targetData = targetList.FirstOrDefault(e => e.id == itemId);
        if (targetData != null)
        {
            targetData.quantity++;
            targetItemList.FirstOrDefault(e => e.id == itemId).inputQuantity.text = targetData.quantity.ToString();
            return;
        }

        AddItem(itemId, 1);
    }

    private void AddItem(int itemId, int quantity)
    {
        UITargetItem item = Instantiate(itemPrefab, itemTargetContainer);
        item.uiTargetController = this;
        item.gameObject.SetActive(true);
        item.id = itemId;
        item.quantity = quantity;
        item.inputQuantity.text = item.quantity.ToString();
        item.txtId.text = itemId.ToString();
        _ = item.imgIcon.SetSpriteAsync(PathManager.ItemSprite(itemId));
        targetItemList.Add(item);
        targetList.Add(new TargetData()
        {
            id = item.id,
            quantity = item.quantity
        });
    }

    public void OnQuantityChange(int id, int quantity)
    {
        TargetData targetData = targetList.FirstOrDefault(e => e.id == id);
        if (targetData != null)
        {
            targetData.quantity = quantity;
        }
    }

    public void RemoveFromTargetList(UITargetItem item)
    {
        int index = targetItemList.IndexOf(item);
        targetList.RemoveAt(index);
        Destroy(targetItemList[index].gameObject);
        targetItemList.RemoveAt(index);
    }
}
