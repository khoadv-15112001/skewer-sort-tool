using System.Collections;
using System.Collections.Generic;
using Gameplay.LevelData;
using UnityEngine;

public class UIToolOrder : MonoBehaviour
{
    [SerializeField] private Transform container;
    private List<UIToolOrderItem> items = new List<UIToolOrderItem>();
    private LevelData levelData;

    public void SetData(LevelData levelData)
    {
        this.levelData = levelData;
        SetOrder();
    }

    public void Clear()
    {
        items.Clear();
        levelData = null;
    }

    private void SetOrder()
    {
        MySonatFramework.poolingContainer.CleanContainer(container);
        items.Clear();
        if (levelData.orderData != null)
        {
            int index = 0;
            foreach (var orderData in levelData.orderData)
            {
                var item = MySonatFramework.poolingContainer.CreateObject<UIToolOrderItem>(container);
                item.SetData(orderData);
                items.Add(item);
            }
        }
    }

    public void AddOrder()
    {
        var item = MySonatFramework.poolingContainer.CreateObject<UIToolOrderItem>(container);
        OrderData orderData = new OrderData();
        item.SetData(orderData);
        items.Add(item);
        if (levelData.orderData == null) levelData.orderData = new();
        levelData.orderData.Add(orderData);
        
        // Add undo for adding order
        ToolManager.Instance.undoController.AddOrderOperation(orderData, OrderOperationType.Add);
    }

    public void RemoveOrder(UIToolOrderItem orderItem)
    {
        // Store the order data for undo before removing
        var orderData = orderItem.orderData;
        
        items.Remove(orderItem);
        levelData.orderData.Remove(orderItem.orderData);
        orderItem.gameObject.SetActive(false);
        
        // Add undo for deleting order
        ToolManager.Instance.undoController.AddOrderOperation(orderData, OrderOperationType.Delete, orderData);
    }
}