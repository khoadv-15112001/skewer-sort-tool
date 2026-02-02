using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using Cysharp.Threading.Tasks;
using Newtonsoft.Json;
using Sonat.Enums;
using SonatFramework.Scripts.Feature.Shop;
using SonatFramework.Scripts.SonatSDKAdapterModule;
using SonatFramework.Scripts.UIModule;
using SonatFramework.Scripts.Utils;
using SonatFramework.Systems;
using SonatFramework.Systems.TimeManagement;
using SonatFramework.Systems.UserData;
using UnityEngine;

public class PackIapWidget : UIHomeWidget
{
    private bool isProcessing = false;
    [SerializeField] protected string popupName;
    [SerializeField] protected ShopItemKey shopItemKey;
    private readonly Service<ShopService> shopService = new();
    private readonly Service<SaleService> saleService = new();
    [SerializeField] private LiveOpsPackData liveOpsPackData = new();
    protected bool showPopup = false;

    public override void Setup()
    {
        base.Setup();

        liveOpsPackData = SonatSDKAdapter.GetRemoteConfig($"live_ops_{gameObject.name}", liveOpsPackData);

        active = active && shopService.Instance.VerifyPack(shopItemKey) && CheckActive();
        gameObject.SetActive(active);
        showPopup = CheckShowPopup();
    }

    public virtual bool CheckShowPopup()
    {
        return active && liveOpsPackData.CheckCondition();
    }

    public override void OnFocus()
    {
        base.OnFocus();
        active = shopService.Instance.VerifyPack(shopItemKey);
        if (active)
        {
            shopService.Instance.OnBuySuccess += OnBuySuccess;
            saleService.Instance.OnBuySuccess += OnBuySuccess;
        }
        else
        {
            gameObject.SetActive(active);
        }
    }

    public override void OnLoseFocus()
    {
        base.OnLoseFocus();
        shopService.Instance.OnBuySuccess -= OnBuySuccess;
        saleService.Instance.OnBuySuccess -= OnBuySuccess;
    }

    private void OnDisable()
    {
        shopService.Instance.OnBuySuccess -= OnBuySuccess;
        saleService.Instance.OnBuySuccess -= OnBuySuccess;
    }

    private void OnDestroy()
    {
        shopService.Instance.OnBuySuccess -= OnBuySuccess;
        saleService.Instance.OnBuySuccess -= OnBuySuccess;
    }

    public override async UniTask<bool> ProcessTask()
    {
        if (!showPopup) return false;
        isProcessing = true;
        PanelManager.Instance.OpenPanelByName<Panel>(popupName, new UIData().Add(UIDataKey.CallBackOnClose, (Action)OnClosePopup));
        MySonatFramework.customTrackingService.OnShowPopup(SonatSDKAdapter.FindProductId(shopItemKey), "pop_up", "iap", "auto");

        await UniTask.WaitUntil(() => !isProcessing);
        await UniTask.Delay(750);
        return true;
    }

    private void OnClosePopup()
    {
        isProcessing = false;
    }

    public virtual void OpenPopup()
    {
        PanelManager.Instance.OpenPanelByName<Panel>(popupName, new UIData().Add(UIDataKey.CallBackOnClose, (Action)OnClosePopup));
        MySonatFramework.customTrackingService.OnShowPopup(SonatSDKAdapter.FindProductId(shopItemKey), "widget", "iap", "user");
    }

    private void CheckProcessing()
    {
    }

    protected virtual void OnBuySuccess(ShopItemKey shopItemKey)
    {
        if (!shopService.Instance.VerifyPack(this.shopItemKey))
        {
            gameObject.SetActive(false);
            return;
        }

        if (shopItemKey != this.shopItemKey) return;
        BuyComplete();
    }

    protected virtual void BuyComplete()
    {
    }
}

[Serializable]
public class LiveOpsPackData
{
    public ConditionEntity levelCondition;
    public ConditionEntity levelCondition2;
    public ConditionEntity dayCondition;
    public ConditionEntity sessionCondition;
    public ConditionEntity homeCondition;
    public ConditionEntity dayOfWeekCondition;
    public DateTimeCondition dateTimeCondition;

    public bool CheckCondition()
    {
        UserDataService userDataService = MySonatFramework.GetService<UserDataService>();
        int currentLevel = userDataService.GetLevel();
        int userDay = userDataService.UserDay;
        int session = userDataService.SessionToday;

        DateTime currentTime = SonatSystem.GetService<TimeService>().GetCurrentTime();

        ConditionEntity.PassType pass = ConditionEntity.PassType.None;

        if (levelCondition != null)
        {
            var _pass = levelCondition.CheckCondition(currentLevel);
            if (_pass == ConditionEntity.PassType.NotPass) return false;
            if (_pass == ConditionEntity.PassType.Passed) pass = _pass;
        }

        if (levelCondition2 != null)
        {
            var _pass = levelCondition2.CheckCondition(currentLevel);
            if (_pass == ConditionEntity.PassType.NotPass) return false;
            if (_pass == ConditionEntity.PassType.Passed) pass = _pass;
        }

        if (dayCondition != null)
        {
            var _pass = dayCondition.CheckCondition(userDay);
            if (_pass == ConditionEntity.PassType.NotPass) return false;
            if (_pass == ConditionEntity.PassType.Passed) pass = _pass;
        }

        if (sessionCondition != null)
        {
            var _pass = sessionCondition.CheckCondition(session);
            if (_pass == ConditionEntity.PassType.NotPass) return false;
            if (_pass == ConditionEntity.PassType.Passed) pass = _pass;
        }

        if (homeCondition != null)
        {
            var _pass = homeCondition.CheckCondition(HomeWidgetManager.homeCount);
            if (_pass == ConditionEntity.PassType.NotPass) return false;
            if (_pass == ConditionEntity.PassType.Passed) pass = _pass;
        }

        if (dayOfWeekCondition != null)
        {
            var _pass = dayOfWeekCondition.CheckCondition((int)currentTime.DayOfWeek);
            if (_pass == ConditionEntity.PassType.NotPass) return false;
            if (_pass == ConditionEntity.PassType.Passed) pass = _pass;
        }

        if (dateTimeCondition != null)
        {
            var _pass = dateTimeCondition.CheckCondition(currentTime);
            if (_pass == ConditionEntity.PassType.NotPass) return false;
            if (_pass == ConditionEntity.PassType.Passed) pass = _pass;
        }

        return pass == ConditionEntity.PassType.Passed;
    }
}

[Serializable]
public class ConditionEntity
{
    public enum CompareType : byte
    {
        Equals = 0,
        NotEquals = 1,
        LessThan = 2,
        LessThanOrEquals = 3,
        GreaterThan = 4,
        GreaterThanOrEquals = 5,
    }

    public enum PassType : byte
    {
        None = 0,
        NotPass = 1,
        Passed = 2,
    }

    public CompareType compareType = CompareType.Equals;
    public int value = -1;
    public int[] values;
    public ConditionEntity subCondition;

    public PassType CheckCondition(int currentValue)
    {
        if (value >= 0)
        {
            return Compare(currentValue, value, compareType) ? PassType.Passed : PassType.NotPass;
        }
        else if (values is { Length: > 0 })
        {
            for (int i = 0; i < values.Length; i++)
            {
                if (currentValue == values[i]) return PassType.Passed;
            }

            return PassType.NotPass;
        }

        return PassType.None;
    }

    private bool Compare(int value1, int value2, CompareType compareType = CompareType.Equals)
    {
        switch (compareType)
        {
            case CompareType.Equals:
                return value1 == value2;
            case CompareType.NotEquals:
                return value1 != value2;
            case CompareType.LessThan:
                return value1 < value2;
            case CompareType.LessThanOrEquals:
                return value1 <= value2;
            case CompareType.GreaterThan:
                return value1 > value2;
            case CompareType.GreaterThanOrEquals:
                return value1 >= value2;
            default:
                return false;
        }
    }
}

[Serializable]
public class DateTimeCondition
{
    public string startTime;
    public string endTime;

    public ConditionEntity.PassType CheckCondition(DateTime currentTime)
    {
        if (string.IsNullOrEmpty(startTime) || string.IsNullOrEmpty(endTime)) return ConditionEntity.PassType.None;
        DateTime start = DateTime.ParseExact(startTime, "dd/MM/yyyy hh:mm", CultureInfo.InvariantCulture);
        DateTime end = DateTime.ParseExact(endTime, "dd/MM/yyyy hh:mm", CultureInfo.InvariantCulture);
        if (DateTime.Compare(currentTime, start) >= 0 && DateTime.Compare(currentTime, end) <= 0) return ConditionEntity.PassType.Passed;
        return ConditionEntity.PassType.NotPass;
    }
}