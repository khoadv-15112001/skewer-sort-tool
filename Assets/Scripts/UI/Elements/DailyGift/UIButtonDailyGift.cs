using System;
using GrillSort.DailyGift;
using I2.Loc;
using Newtonsoft.Json;
using Sonat.Data;
using SonatFramework.Scripts.SonatSDKAdapterModule;
using SonatFramework.Scripts.UIModule;
using SonatFramework.Scripts.UIModule.UIElements;
using SonatFramework.Systems;
using SonatFramework.Systems.InventoryManagement;
using SonatFramework.Systems.InventoryManagement.GameResources;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIButtonDailyGift : MonoBehaviour
{
    public int day;
    [SerializeField] private LocalizationParamsManager txtDay;
    [SerializeField] private LocalizationParamsManager txtDay2;
    [SerializeField] private Image imgClaimed;
    [SerializeField] private Image bgClaimed;
    [SerializeField] private UIRewardGrid rewardGrid;
    [SerializeField] private UIRewardItem rewardItem;
    [SerializeField] private GameObject[] objsClaimEffect;

    private readonly Service<DailyGiftService7> dailyGiftService7 = new();
    private readonly Service<InventoryService> inventoryService = new();
    private bool checkMultiple;
    public bool canClaim { get; private set; }

    public void Init()
    {
        txtDay.SetParameterValue("VALUE", day.ToString());
        txtDay2.SetParameterValue("VALUE", day.ToString());
        var config = dailyGiftService7.Instance.dailyGiftConfig;
        var gift = config.GetGiftForDay(day);
        var reward = gift.reward;

        checkMultiple = reward.resourceDatas.Count > 1;
        rewardGrid.gameObject.SetActive(checkMultiple);
        rewardItem.gameObject.SetActive(!checkMultiple);
        if (checkMultiple)
        {
            rewardGrid.SetReward(reward);
        }
        else
        {
            rewardItem.Init(reward.resourceDatas[0].resource, reward.resourceDatas[0].quantity);
        }
    }

    public void SetClaimed()
    {
        imgClaimed.gameObject.SetActive(true);
        bgClaimed.gameObject.SetActive(false);
        ShowClaimEffect(false);
        if (checkMultiple)
        {
            rewardGrid.gameObject.SetActive(false);
        }
        else
        {
            rewardItem.gameObject.SetActive(false);
        }
    }

    public void SetCanClaim()
    {
        imgClaimed.gameObject.SetActive(false);
        bgClaimed.gameObject.SetActive(true);
        ShowClaimEffect(true);

        if (checkMultiple)
        {
            rewardGrid.gameObject.SetActive(true);
        }
        else
        {
            rewardItem.gameObject.SetActive(true);
        }
        canClaim = true;
    }

    public void SetNotClaimed()
    {
        imgClaimed.gameObject.SetActive(false);
        bgClaimed.gameObject.SetActive(false);
        ShowClaimEffect(false);
        if (checkMultiple)
        {
            rewardGrid.gameObject.SetActive(true);
        }
        else
        {
            rewardItem.gameObject.SetActive(true);
        }
        canClaim = false;
    }

    private void ShowClaimEffect(bool show)
    {
        foreach (var obj in objsClaimEffect)
        {
            obj.SetActive(show);
        }
    }
    public void OnClickClaimDailyGift()
    {
        var canClaim = dailyGiftService7.Instance.CanClaim(day);
        if (canClaim)
        {
            dailyGiftService7.Instance.ClaimDailyGift(day);
            SetClaimed();
            SetToday();
            var gift = dailyGiftService7.Instance.dailyGiftConfig.GetGiftForDay(day);

            //inventory
            RewardData rewardData = gift.reward;
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
            PanelManager.Instance.OpenPanel<PopupReward>(uiData);

            PanelManager.Instance.ClosePanel<PopupDailyGift>();
        }
        else
        {
            if (imgClaimed.gameObject.activeInHierarchy)
            {
                PopupToast.Cretate("Claimed!");
            }
            else
            {
                PopupToast.Cretate("Come back the next day to get it!");
            }
            
        }
    }

    public void SetToday()
    {
        bgClaimed.gameObject.SetActive(true);
    }
}
