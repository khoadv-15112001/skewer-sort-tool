using System;
using GrillSort.RealTime;
using Sirenix.OdinInspector;
using SonatFramework.Scripts.UIModule.UIElements;
using SonatFramework.Systems;
using SonatFramework.Systems.GameDataManagement;
using UnityEngine;

public class UIDailyShopPack : UIShopPack
{
    [SerializeField] private bool isShowTimeCounter = true;
    [SerializeField, ShowIf("isShowTimeCounter")] private UITimeCounter _timeCounter;
    private readonly Service<RealTimeService> _realTimeService = new();
    private readonly Service<DataService> _dataService = new();

    private DateTime purchasedTime
    {
        get
        {
            var str = _dataService.Instance.GetString($"DailyShopPack_{key}", "");
            if (string.IsNullOrEmpty(str))
                return DateTime.MinValue;
            return DateTime.Parse(str);
        }
        set
        {
            _dataService.Instance.SetString($"DailyShopPack_{key}", value.ToString());
        }
    }
    protected override void OnEnable()
    {
        base.OnEnable();

        // kiểm tra gói đã mua hôm nay chưa
        if (purchasedTime == _realTimeService.Instance.GetCurrentTime().Date)
        {
            gameObject.SetActive(false);
        }
        else
        {
            if (isShowTimeCounter)
            {
                long remainingTime = _realTimeService.Instance.GetRemainingTimeInDay();
                _timeCounter.SetData(remainingTime, null);
            }

            _realTimeService.Instance.OnNextDay += OnNextDay;
        }
    }

    protected override void OnDisable()
    {
        base.OnDisable();
        _realTimeService.Instance.OnNextDay -= OnNextDay;
    }

    private void OnNextDay()
    {
        if (isShowTimeCounter)
        {
            long remainingTime = _realTimeService.Instance.GetRemainingTimeInDay();
            _timeCounter.SetData(remainingTime, null);
        }
    }

    protected override void BuyComplete()
    {
        base.BuyComplete();
        purchasedTime = _realTimeService.Instance.GetCurrentTime().Date;

        gameObject.SetActive(false);
    }
}