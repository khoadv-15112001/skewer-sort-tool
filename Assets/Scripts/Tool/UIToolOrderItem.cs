using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Gameplay.LevelData;
using Sonat;
using TMPro;
using Tool.Order;
using UnityEngine;
using UnityEngine.Serialization;

public class UIToolOrderItem : MonoBehaviour
{
    public OrderData orderData { get; private set; }
    [SerializeField] private UIToolOrder uiToolOrder;
    [SerializeField] private TMP_InputField inputFieldCondition;
    //[SerializeField] private TMP_InputField inputFieldReward;
    [SerializeField] private TMP_InputField inputFieldTime;
    [SerializeField] private Transform orderDataContainer;

    public void SetData(OrderData orderData)
    {
        this.orderData = orderData;

        //inputFieldOrder.text = orderData.order.Count > 0 ? string.Join(",", orderData.order) : "";
        //inputOrderId.text = orderData.order.Count > 0 ? string.Join(",", orderData.ids) : "";
        inputFieldCondition.text = this.orderData.appearCondition.ToString();
        //inputFieldReward.text = orderData.reward.ToString();
        inputFieldTime.text = orderData.time.ToString();
        
        //inputFieldOrder.onDeselect.RemoveAllListeners();
        //inputFieldOrder.onDeselect.AddListener(OnInputFieldEndEdit);
        
        //inputOrderId.onDeselect.RemoveAllListeners();
        //inputOrderId.onDeselect.AddListener(OnInputFieldIdsEndEdit);
        
        inputFieldCondition.onDeselect.RemoveAllListeners();
        inputFieldCondition.onDeselect.AddListener(OnInputFieldConditionEndEdit);
        
        //inputFieldReward.onDeselect.RemoveAllListeners();
        //inputFieldReward.onDeselect.AddListener(OnInputFieldRewardEndEdit);
        
        inputFieldTime.onDeselect.RemoveAllListeners();
        inputFieldTime.onDeselect.AddListener(OnInputFieldTimeEndEdit);
        
        MySonatFramework.poolingContainer.CleanContainer(orderDataContainer);

        foreach (var orderItemData in orderData.OrderItem)
        {
            UIToolOrderEntry item = MySonatFramework.poolingContainer.CreateObject<UIToolOrderEntry>(orderDataContainer);
            item.SetData(orderItemData);
        }
    }


    private void OnInputFieldEndEdit(string str)
    {
        if (string.IsNullOrEmpty(str)) return;
        try
        {
            var orders = SonatSdkHelper.GetIntListFromRegex(str);
            if (orders != null && orders.Count > 0)
            {
                orderData.order = orders;
            }
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }
    
    private void OnInputFieldIdsEndEdit(string str)
    {
        if (string.IsNullOrEmpty(str))
        {
            orderData.ids.Clear();
            return;
        }
        try
        {
            var ids = SonatSdkHelper.GetIntListFromRegex(str);
            if (ids != null && ids.Count > 0)
            {
                orderData.ids = ids;
            }
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }

    private void OnInputFieldConditionEndEdit(string str)
    {
        if (string.IsNullOrEmpty(str)) return;

        if (int.TryParse(str, out var condition))
        {
            orderData.appearCondition = condition;
        }
    }
    
    // private void OnInputFieldRewardEndEdit(string str)
    // {
    //     if (string.IsNullOrEmpty(str)) return;
    //
    //     if (int.TryParse(str, out var reward))
    //     {
    //         orderData.reward = reward;
    //     }
    // }
    
    private void OnInputFieldTimeEndEdit(string str)
    {
        if (string.IsNullOrEmpty(str)) return;

        if (int.TryParse(str, out var time))
        {
            orderData.time = time;
        }
    }

    public void RemoveOrder()
    {
        uiToolOrder.RemoveOrder(this);
    }

    public void AddOrderEntry()
    {
        UIToolOrderEntry item = MySonatFramework.poolingContainer.CreateObject<UIToolOrderEntry>(orderDataContainer);
        OrderItemData orderItemData = new OrderItemData();
        orderData.OrderItem.Add(orderItemData);
        item.SetData(orderItemData);
    }

    public void RemoveOrderEntry(OrderItemData orderItemData)
    {
        orderData.OrderItem.Remove(orderItemData);
    }
}