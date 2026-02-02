

using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using GrillSort.RealTime;
using Sonat.Enums;
using SonatFramework.Scripts.Feature.Shop;
using SonatFramework.Scripts.Helper;
using SonatFramework.Systems;
using SonatFramework.Systems.GameDataManagement;
using UnityEngine;

[CreateAssetMenu(fileName = "SaleService", menuName = "Sonat/CustomService/SaleService")]
public class SaleService : SonatServiceSo, IServiceInitialize
{
    private const string DATA_KEY = "SALE_SERVICE";
    public SalePackConfig salePackConfig;

    public event Action<int> OnSaleFinished;
    public event Action<ShopItemKey> OnBuySuccess;

    private readonly Service<RealTimeService> _realTimeService = new();
    private readonly Service<ShopService> _shopService = new();
    private Dictionary<int, long> _dictSaleExpirationTimes;
    private List<int> _listCompletedSales;

    private StringDataPref _stringDataPref_listCompletedSales;
    private StringDataPref _stringDataPref_dictSaleExpirationTimes;

    #region Initialize
    public void Initialize()
    {
        LoadConfig();
        LoadData();

        Init();
    }

    private void LoadConfig()
    {

    }

    private void LoadData()
    {
        LoadListCompletedSales();
        LoadDictSaleExpirationTimes();
    }

    private void LoadDictSaleExpirationTimes()
    {
        _stringDataPref_dictSaleExpirationTimes = new StringDataPref($"{DATA_KEY}_dictSaleExpirationTimes", "");

        _dictSaleExpirationTimes = new Dictionary<int, long>();
        if (string.IsNullOrEmpty(_stringDataPref_dictSaleExpirationTimes.Value) == false)
        {
            foreach (var pair in _stringDataPref_dictSaleExpirationTimes.Value.Split(','))
            {
                var parts = pair.Split(':');
                _dictSaleExpirationTimes.Add(int.Parse(parts[0]), long.Parse(parts[1]));
            }
        }
    }

    private void LoadListCompletedSales()
    {
        _stringDataPref_listCompletedSales = new StringDataPref($"{DATA_KEY}_listCompletedSales", "");
        _listCompletedSales = new List<int>();
        if (string.IsNullOrEmpty(_stringDataPref_listCompletedSales.Value) == false)
        {
            _listCompletedSales = _stringDataPref_listCompletedSales.Value.Split(',').Select(int.Parse).ToList();
        }
    }

    private void SaveData()
    {
        _stringDataPref_listCompletedSales.Value = string.Join(",", _listCompletedSales);
        _stringDataPref_dictSaleExpirationTimes.Value = string.Join(",", _dictSaleExpirationTimes.Select(pair => $"{pair.Key}:{pair.Value}"));
    }

    private void Init()
    {
        // Tiếp tục chạy các gói đã bắt đầu
        foreach (var saleExpiration in _dictSaleExpirationTimes.ToList())
        {
            RunSaleTimer(saleExpiration.Key, saleExpiration.Value).Forget();
        }

        // // Kiểm tra và kích hoạt gói sale mới
        // foreach (var salePack in salePackConfig.salePacks)
        // {
        //     if (CheckStartSalePack(salePack.shopItemKey) != -1)
        //     {

        //         StartSale(salePack.SaleSessionId);
        //     }
        // }
    }
    #endregion


    public void StartSale(int saleSessionId)
    {
        var salePack = salePackConfig.GetPack(saleSessionId);

        var expirationTime = _realTimeService.Instance.GetCurrentTime().AddSeconds(salePack.saleDuration);
        var expirationTimeUnix = ((DateTimeOffset)expirationTime).ToUnixTimeSeconds();
        _dictSaleExpirationTimes.Add(salePack.SaleSessionId, expirationTimeUnix);
        SaveData();
        RunSaleTimer(salePack.SaleSessionId, expirationTimeUnix).Forget();
    }

    public void FinishSale(int saleSessionId)
    {
        _dictSaleExpirationTimes.Remove(saleSessionId);
        _listCompletedSales.Add(saleSessionId);
        SaveData();

        OnSaleFinished?.Invoke(saleSessionId);
    }

    public void BuySuccess(int saleSessionId)
    {
        var salePack = salePackConfig.GetPack(saleSessionId);

        var verifyPack = new IntDataPref($"{DATA_KEY}_verifyPack_{salePack.shopItemKey}", 0);
        verifyPack.Value = 1;

        OnBuySuccess?.Invoke(salePack.shopItemKey);

        FinishSale(saleSessionId);
    }


    private async UniTask RunSaleTimer(int saleSessionId, long expirationTimeUnix)
    {
        var currentTimeUnix = ((DateTimeOffset)_realTimeService.Instance.GetCurrentTime()).ToUnixTimeSeconds();
        var delayTime = (expirationTimeUnix - currentTimeUnix) * 1000;
        if (delayTime > 0)
        {
            await UniTask.Delay((int)delayTime);
        }
        FinishSale(saleSessionId);
    }

    public int CheckStartSalePack(ShopItemKey shopItemKey)
    {
        // pack có thể mua
        // pack không đang sale
        // pack sale chưa bị hoàn thành
        foreach (var salePack in salePackConfig.salePacks)
        {
            if (salePack.shopItemKey == shopItemKey
            && _shopService.Instance.VerifyPack(shopItemKey) == true
            && _listCompletedSales.Contains(salePack.SaleSessionId) == false
            && _dictSaleExpirationTimes.ContainsKey(salePack.SaleSessionId) == false
            && CheckConditionSale(salePack.SaleSessionId) == true)
            {
                return salePack.SaleSessionId;
            }
        }
        return -1;
    }

    private bool CheckConditionSale(int saleSessionId)
    {
        var salePack = salePackConfig.GetPack(saleSessionId);

        if (salePack.startFromAppearance == true)
        {
            // chưa có trong list đang thực hiện và đã hoàn thành
            return _listCompletedSales.Contains(saleSessionId) == false && _dictSaleExpirationTimes.ContainsKey(saleSessionId) == false;
        }

        return false;
    }


    public int GetActiveSessionId(ShopItemKey shopItemKey)
    {
        foreach (var salePack in salePackConfig.salePacks)
        {
            if (salePack.shopItemKey == shopItemKey)
            {
                if (IsSaleSessionActive(salePack.SaleSessionId)) return salePack.SaleSessionId;
            }
        }
        return -1;
    }

    public bool IsSaleSessionActive(int saleSessionId)
    {
        return _dictSaleExpirationTimes.ContainsKey(saleSessionId);
    }

    public long GetSaleExpirationTime(int saleSessionId)
    {
        return _dictSaleExpirationTimes[saleSessionId];
    }

    public long GetRemainingTime(int saleSessionId)
    {
        var currentTimeUnix = ((DateTimeOffset)_realTimeService.Instance.GetCurrentTime()).ToUnixTimeSeconds();
        var expirationTimeUnix = GetSaleExpirationTime(saleSessionId);
        return expirationTimeUnix - currentTimeUnix;
    }

    public bool VerifyPack(ShopItemKey shopItemKey)
    {
        if( MySonatFramework.GetService<DataService>().HasKey($"{DATA_KEY}_verifyPack_{shopItemKey}") == false)
        {
            return true;
        }

        var verifyPack = new IntDataPref($"{DATA_KEY}_verifyPack_{shopItemKey}", 0);
        return verifyPack.Value == 0;
    }
}
