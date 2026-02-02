using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using Gameplay;
using Gameplay.Entities;
using Gameplay.Entities.Orders;
using Gameplay.LevelData;
using Sonat.Enums;
using SonatFramework.Scripts.UIModule;
using SonatFramework.Scripts.Utils;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class OrderList : MonoBehaviour
{
    [SerializeField] private OrderEntity orderEntity;
    [SerializeField] private Transform container;
    [SerializeField] private float orderSpacing;
    [SerializeField] private TMP_Text txtReward;
    [SerializeField] private TMP_Text txtTime;
    [SerializeField] private Slider slider;
    [SerializeField] private Image iconReward;
    private OrderData orderData;
    private List<OrderItem> orderItems;
    private bool isComplete;

    [SerializeField] private Transform orderTargetContainer;

    //private Coroutine cooldownCoroutine;
    private bool hurryUpVoice;
    private int useBoosterCount;

    public void SetData(OrderData orderData)
    {
        this.orderData = orderData;
        Init();
        useBoosterCount = MySonatFramework.gameplayAnalyticsService.levelPlayData.useBoosterCount;
        MySonatFramework.customTrackingService.OnOrderAppear();
    }

    private void Init()
    {
        MySonatFramework.poolingContainer.CleanContainer(container);
        float offset = (orderData.OrderItem.Count - 1) * orderSpacing / 2f;

        orderItems = new List<OrderItem>();

        // txtReward.text = $"+{orderData.reward}";

        int i = 0;
        foreach (var orderItemData in orderData.OrderItem)
        {
            if (orderItemData.id > 0)
            {
                Vector3 pos = new Vector3(orderSpacing * i - offset, 0, 0);
                var order = MySonatFramework.poolingContainer.CreateObject<OrderItem>(container);
                order.transform.localPosition = pos;
                int item = orderItemData.id;
                order.SetOrder(item, orderItemData.spicy, orderTargetContainer, this);
                orderItems.Add(order);
            }
            else
            {
                Vector3 pos = new Vector3(orderSpacing * i - offset, 0, 0);
                var order = MySonatFramework.poolingContainer.CreateObject<OrderItem>(container);
                order.transform.localPosition = pos;
                int item = orderEntity.GetItemOrder(orderItemData);
                order.SetOrder(item, orderItemData.spicy, orderTargetContainer, this);
                orderItems.Add(order);
            }

            i++;
        }

        isComplete = false;
        hurryUpVoice = false;
        txtTime.text = SonatUtils.FormatTimeFromSec(orderData.time);
        slider.value = 1;
        if (GameplayController.instance.timeManager.timeCounting)
        {
            MySonatFramework.audioService.PlaySound("Oder_Appear_Order_mode");
            StartCountTime();
        }
        else
        {
            GameplayController.instance.timeManager.OnStartCountTime += StartCountTime;
        }
    }


    public bool NeedItem(int itemId, PrimaryGrill grill)
    {
        foreach (var orderItem in orderItems)
        {
            if (orderItem.NeedItem(itemId, grill)) return true;
        }

        return false;
    }

    public void OnAddItem(int itemId, PrimaryGrill grill, float duration)
    {
        if (isComplete || !gameObject.activeInHierarchy) return;
        foreach (var orderItem in orderItems)
        {
            orderItem.AddItem(itemId, grill, duration);
        }

        if (IsComplete())
        {
            Complete(duration);
        }
    }

    public bool IsComplete()
    {
        if (!gameObject.activeInHierarchy) return true;
        foreach (var orderItem in orderItems)
        {
            if (!orderItem.IsCompleted) return false;
        }

        return true;
    }

    public bool CheckPreComplete(int itemId, PrimaryGrill grill)
    {
        if (!gameObject.activeInHierarchy) return true;
        foreach (var orderItem in orderItems)
        {
            if (!orderItem.IsCompleted && !orderItem.NeedItem(itemId, grill)) return false;
        }

        return true;
    }

    private void Complete(float duration)
    {
        isComplete = true;
        GameplayController.instance.timeManager.OnTimeUpdate -= UpdateTime;
        SonatUtils.DelayCall(duration, () =>
        {
            int rand = UnityEngine.Random.Range(1, 5);
            orderEntity.PlayVoice($"Thanks_Oder_mode_{rand}");
        });
        LogCompleteOrder();
    }

    private void OutOfTime()
    {
        //orderEntity.MoveOut(false);
        //GameplayController.instance.Stuck(StuckType.OutOfTimeShipper).Forget();
        GameplayController.instance.MissedOrder(GetData(), StuckType.OutOfTimeShipper).Forget();
        int rand = UnityEngine.Random.Range(1, 3);
        orderEntity.PlayVoice($"Order_fail_Oder_mode_{rand}");
    }

    public void StuckOrder()
    {
        GameplayController.instance.MissedOrder(GetData(), StuckType.OutOfMoveShipper).Forget();
        int rand = UnityEngine.Random.Range(1, 3);
        orderEntity.PlayVoice($"Order_fail_Oder_mode_{rand}");
    }
    
    public void OutOfItemOrder()
    {
        GameplayController.instance.MissedOrder(GetData(), StuckType.OutOfItemOrder).Forget();
        int rand = UnityEngine.Random.Range(1, 3);
        orderEntity.PlayVoice($"Order_fail_Oder_mode_{rand}");
    }


    // public Dictionary<int, int> ItemInOrder()
    // {
    //     Dictionary<int, int> itemInOrder = new Dictionary<int, int>();
    //     if (orderItems != null)
    //     {
    //         foreach (var orderItem in orderItems)
    //         {
    //             var orderRemain = orderItem.OrderRemain();
    //             if (!itemInOrder.TryAdd(orderRemain.Item1, orderRemain.Item2))
    //             {
    //                 itemInOrder[orderRemain.Item1] += orderRemain.Item2;
    //             }
    //         }
    //     }
    //     return itemInOrder;
    // }

    private float targetTime;

    private void StartCountTime()
    {
        GameplayController.instance.timeManager.OnStartCountTime -= StartCountTime;
        targetTime = GameplayController.instance.timeManager.GetTimeRemaining() - orderData.time;
        if (targetTime <= 15) targetTime = 15;
        GameplayController.instance.timeManager.OnTimeUpdate += UpdateTime;
    }

    private void UpdateTime(float currentTime)
    {
        if (isComplete) return;
        float remainTime = currentTime - targetTime;
        if (remainTime <= 0)
        {
            GameplayController.instance.timeManager.OnTimeUpdate -= UpdateTime;
            remainTime = 0;
            OutOfTime();
        }

        if (remainTime <= 15f && !hurryUpVoice)
        {
            hurryUpVoice = true;
            int rand = UnityEngine.Random.Range(1, 4);
            orderEntity.PlayVoice($"Hurry_up_Oder_mode_{rand}");
        }

        slider.value = remainTime / orderData.time;
        txtTime.text = SonatUtils.FormatTimeFromSec((int)remainTime);
    }

    private void OnDisable()
    {
        GameplayController.instance.timeManager.OnTimeUpdate -= UpdateTime;
        GameplayController.instance.timeManager.OnStartCountTime -= StartCountTime;
    }

    public void OnSkipOrderClick()
    {
        PanelManager.Instance.OpenPanel<PopupSkipOrder>(new UIData()
            .Add("OnSkipOrder", (Action)SkipOrder)
            .Add("OrderListData", GetData()));
    }

    public void SkipOrder()
    {
        isComplete = true;
        orderEntity.MoveOut(false).Forget();
    }

    public Data GetData()
    {
        Data data = new Data();
        data.orderItems = new List<OrderItem.Data>();
        foreach (var orderItem in orderItems)
        {
            data.orderItems.Add(orderItem.GetData());
        }

        data.maxTime = orderData.time;
        if (GameplayController.instance.timeManager.timeCounting)
            data.timeRemaining = GameplayController.instance.timeManager.GetTimeRemaining() - targetTime;
        else
        {
            data.timeRemaining = data.maxTime;
        }

        data.useBoosterCount = MySonatFramework.gameplayAnalyticsService.levelPlayData.useBoosterCount - useBoosterCount;

        return data;
    }

    public Transform GetTarget(int id, PrimaryGrill grill)
    {
        foreach (var orderItem in orderItems)
        {
            if (orderItem.NeedItem(id, grill)) return orderItem.GetIconTarget();
        }

        return orderTargetContainer;
    }


    private void LogCompleteOrder()
    {
        MySonatFramework.customTrackingService.OnCompleteOrder();
        int timePlayOrder = orderData.time - (int)(GameplayController.instance.timeManager.GetTimeRemaining() - targetTime);
        int boosterCount = MySonatFramework.gameplayAnalyticsService.levelPlayData.useBoosterCount - useBoosterCount;
        MySonatFramework.customTrackingService.OnOrderEnd("success", orderData.time, timePlayOrder, 100,
            boosterCount);
    }

    // IEnumerator Cooldown()
    // {
    //     float timer = orderData.time;
    //     
    //     while (timer > 0)
    //     {
    //         if (GameplayController.instance.gameState == GameState.Playing)
    //         {
    //             timer -= 1;
    //             slider.value = timer / orderData.time;
    //             txtTime.text = $"{(int)timer}";
    //         }
    //
    //         yield return new WaitForSeconds(1);
    //     }
    //
    //     OutOfTime();
    // }

    public class Data
    {
        public List<OrderItem.Data> orderItems;
        public float timeRemaining;
        public int maxTime;
        public int useBoosterCount;
    }
}