using System;
using System.Collections;
using GrillSort.RealTime;
using Sirenix.OdinInspector;
using Sonat.Enums;
using SonatFramework.Scripts.Helper;
using SonatFramework.Scripts.UIModule.UIElements;
using SonatFramework.Systems;
using UnityEngine;

[RequireComponent(typeof(UIShopPack))]
// quyết định xem pack này hiện mấy lần trong ngày và trong bao lâu
public class UIPackAppearanceScheduler : MonoBehaviour
{
    [InfoBox("Lên kế hoạch xuất hiện pack trong ngày")] [SerializeField]
    private int maxPurchaseCount = 0;

    [SerializeField, BoxGroup("Custom Settings")]
    private bool custom = true;

    [SerializeField, BoxGroup("Custom Settings"), ShowIf("custom")]
    private int maxAppearances;

    [SerializeField, BoxGroup("Custom Settings"), ShowIf("custom")]
    private int appearanceDuration;

    // [SerializeField] private bool isDaily;
    // [SerializeField] private bool isWeekly;
    [SerializeField] private bool isShowTimeCounter = true;

    [SerializeField, ShowIf("isShowTimeCounter")]
    private UITimeCounter _timeCounter;


    private readonly Service<RealTimeService> _realTimeService = new();

    private ShopItemKey _key;
    private UIShopPack _shopPack;
    private IntDataPref _purchasedCountInToday;
    private LongDataPref _appearanceDate;
    private LongDataPref _expiredTime;
    private IntDataPref _currentAppearanceCount;
    private const string DATA_KEY = "PACK_APPEARANCE_SCHEDULER";
    [SerializeField] private PackSwitcher packSwitcher;

    private Coroutine _displayPackCoroutine;

    public bool UnlimitedPurchases => maxPurchaseCount == -1;

    public bool IsPurchasedToday
    {
        get
        {
            if (_purchasedCountInToday == null)
            {
                LoadData();
            }

            return _purchasedCountInToday.Value > 0;
        }
    }

    protected void OnEnable()
    {
        // LoadConfig();
        // CHECK DAILY
        LoadData();

        // CHECK STATE
        if (_purchasedCountInToday.Value >= maxPurchaseCount && UnlimitedPurchases == false) // đã mua đủ số lượng hôm nay
        {
            HidePack();
        }
        else // Nếu chưa mua
        {
            var currentTime = _realTimeService.Instance.GetCurrentTimeUnix();
            if (CheckExpired() == false) // hết hạn
            {
                _currentAppearanceCount.Value += 1;

                if (custom)
                {
                    if (_currentAppearanceCount.Value > maxAppearances)
                    {
                        _currentAppearanceCount.Value = maxAppearances;
                        HidePack();
                        return;
                    }

                    _expiredTime.Value = currentTime + appearanceDuration;
                }
                else
                {
                    // hiển thị 1 lần: Đặt thời gian reset đến cuối ngày
                    _expiredTime.Value = currentTime + _realTimeService.Instance.GetRemainingTimeInDay();
                }
            }

            long remainingTime = _expiredTime.Value - currentTime;
            if (isShowTimeCounter)
            {
                _timeCounter.SetData(remainingTime, null);
            }

            // _realTimeService.Instance.OnNextDay += OnNextDay;

            //_displayPackCoroutine = StartCoroutine(DisplayPack((int)remainingTime));
            _displayPackCoroutine = StartCoroutine(DisplayPack((int)remainingTime));
        }
    }

    private void ResetData()
    {
        _purchasedCountInToday.Value = 0;
        _currentAppearanceCount.Value = 0;
        _expiredTime.Value = 0;
    }

    protected void OnDisable()
    {
        if (_displayPackCoroutine != null)
        {
            StopCoroutine(_displayPackCoroutine);
        }
        // _realTimeService.Instance.OnNextDay -= OnNextDay;
    }


    private void LoadData()
    {
        _shopPack = GetComponent<UIShopPack>();
        _key = _shopPack.GetShopItemKey();

        _purchasedCountInToday = new IntDataPref($"{DATA_KEY}_{_key}_isPurchasedToday");
        _appearanceDate = new LongDataPref($"{DATA_KEY}_{_key}_appearanceDate");
        _expiredTime = new LongDataPref($"{DATA_KEY}_{_key}_expiredTime");
        _currentAppearanceCount = new IntDataPref($"{DATA_KEY}_{_key}_currentAppearanceCount", 0);

        var appearanceDate = DateTime.UnixEpoch.AddSeconds(_appearanceDate.Value).Date;
        if (_realTimeService.Instance.GetCurrentTime().Date != appearanceDate) // Nếu sang ngày mới thì reset data
        {
            ResetData();
            _appearanceDate.Value = _realTimeService.Instance.GetCurrentTimeUnix();
        }
    }

    private IEnumerator DisplayPack(int remainingTime)
    {
        yield return new WaitForSeconds(remainingTime);

        _expiredTime.Value = 0;
        HidePack();
    }

    private void HidePack()
    {
        gameObject.SetActive(false);
    }

    public bool CheckExpired()
    {
        if (_expiredTime == null)
        {
            LoadData();
        }

        return _expiredTime.Value >= _realTimeService.Instance.GetCurrentTimeUnix();
    }

    public void BuyComplete()
    {
        _purchasedCountInToday.Value += 1;
        _expiredTime.Value = 0;
        HidePack();
        packSwitcher?.CheckActivePack();
    }
}