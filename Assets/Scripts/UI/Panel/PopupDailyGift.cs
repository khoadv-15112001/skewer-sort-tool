using System;
using System.Collections.Generic;
using GrillSort.DailyGift;
using Sonat.Data;
using Sonat.Enums;
using SonatFramework.Scripts.SonatSDKAdapterModule;
using SonatFramework.Scripts.UIModule;
using SonatFramework.Systems;
using SonatFramework.Systems.InventoryManagement;
using SonatFramework.Systems.InventoryManagement.GameResources;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PopupDailyGift : Panel
{
    private readonly Service<DailyGiftService7> dailyGiftService7 = new();
    private readonly Service<DailyGiftService30> dailyGiftService30 = new();
    private readonly Service<InventoryService> inventoryService = new();
    [SerializeField] private Button btnClaimAll;
    [SerializeField] private Button btnGray, btnCheatNextDay;
    public override void Open(UIData uiData)
    {
        base.Open(uiData);

        if (dailyGiftService7.Instance.CanClaim() == false && dailyGiftService30.Instance.CanClaim() == false)
        {
            btnGray.gameObject.SetActive(true);
            btnClaimAll.gameObject.SetActive(false);
        }
        else
        {
            btnGray.gameObject.SetActive(false);
            btnClaimAll.gameObject.SetActive(true);
        }
        btnCheatNextDay.gameObject.SetActive(PanelManager.Instance.GetPanel<CheatPanel>() != null);
    }
    public void CheatNextDay()
    {
        dailyGiftService7.Instance.CheatNextDay();
        dailyGiftService30.Instance.CheatNextDay();
    }
    public void OnClickClaimAll()
    {
        var rewardData7 = dailyGiftService7.Instance.GetAllReward();
        dailyGiftService7.Instance.ClaimAllDailyGift();

        var rewardData30 = dailyGiftService30.Instance.GetAllReward();
        dailyGiftService30.Instance.ClaimAllDailyGift();

        var rewardData = GetTotalReward(rewardData7, rewardData30);

        if (rewardData.resourceDatas.Count > 0)
        {
            var logData = new EarnResourceLogData
            {
                spendType = "daily_gift",
                spendId = "daily_gift",
                isFirstBuy = false,
                source = "non_iap"
            };
            inventoryService.Instance.AddReward(rewardData, logData);

            // uidata
            UIData uiData = new UIData();
            uiData.Add("Title", "REWARD!");
            uiData.Add("Reward", rewardData);
            uiData.Add("x2", SonatSDKAdapter.GetValueBySegment($"show_rwd_daily_gift", UserData.UserCampaignSegment.Value, true));
            uiData.Add(UIDataKey.CallBackOnClose, (Action)Close);
            PanelManager.Instance.OpenPanel<PopupReward>(uiData);
        }
        else
        {
            PopupToast.Cretate("Come back the next day to get it!");
        }
    }

    private RewardData GetTotalReward(RewardData rewardData7, RewardData rewardData30)
    {
        var dictReward = new Dictionary<GameResource, int>();
        foreach (var resourceData in rewardData7.resourceDatas)
        {
            if (dictReward.ContainsKey(resourceData.resource))
            {
                dictReward[resourceData.resource] += resourceData.quantity;
            }
            else
            {
                dictReward.Add(resourceData.resource, resourceData.quantity);
            }
        }

        foreach (var resourceData in rewardData30.resourceDatas)
        {
            if (dictReward.ContainsKey(resourceData.resource))
            {
                dictReward[resourceData.resource] += resourceData.quantity;
            }
            else
            {
                dictReward.Add(resourceData.resource, resourceData.quantity);
            }
        }

        var rewardData = new RewardData();
        rewardData.resourceDatas = new();
        foreach (var resourceData in dictReward)
        {
            rewardData.resourceDatas.Add(new ResourceData(resourceData.Key, resourceData.Value));
        }
        return rewardData;
    }
}