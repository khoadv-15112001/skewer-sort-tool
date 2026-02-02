using System;
using GrillSort.DailyGift;
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

public class UISpecialGiftSlider : MonoBehaviour
{
    [SerializeField] private DailyGiftConfig dailyGiftConfig;
    [SerializeField] private Slider slider;
    [SerializeField] private Image imageIcon;
    [SerializeField] private GameObject imgClaimed;
    [SerializeField] private TMP_Text textDay;
    [SerializeField] private GameObject pReward;
    [SerializeField] private UIRewardGrid rewardGrid;
    //[SerializeField] private GameObject bgMultiple;

    public static Action OnClickAction;

    public int day => _day;
    private int _day;
    private readonly Service<DailyGiftService30> dailyGiftService = new();
    private readonly Service<InventoryService> inventoryService = new();
    public void Init(int day, int index)
    {
        _day = day;

        //var value = (float)day / dailyGiftConfig.GetFinishDay();
        //value = Mathf.Pow(value, 1.0f / 1.35f);
        float value = (index + 1) * 1.0f / 4;
        slider.value = value;

        Unselect();
        var gift = dailyGiftConfig.GetGiftForDay(day);
        rewardGrid.SetReward(gift.reward);
        //bgMultiple.SetActive(gift.reward.resourceDatas.Count > 3);

        if (gift.isSpecial)
        {
            imageIcon.sprite = gift.sprite;
            imageIcon.SetNativeSize();
        }

        textDay.text = day.ToString();
    }

    public void SetClaimed()
    {
        textDay.gameObject.SetActive(false);
        imageIcon.gameObject.SetActive(false);
        imgClaimed.SetActive(true);
    }
    public void SetNotClaimed()
    {
        textDay.gameObject.SetActive(true);
        imgClaimed.SetActive(false);
        imageIcon.gameObject.SetActive(true);
    }

    public void SetCanClaimed()
    {
        textDay.gameObject.SetActive(false);
        imgClaimed.SetActive(true);
        imageIcon.gameObject.SetActive(true);
    }

    public void OnClick()
    {
        if (pReward.activeSelf)
        {
            Unselect();
        }
        else
        {
            OnClickAction?.Invoke();
            pReward.SetActive(true);

            OnClickClaimDailyGift();
        }
    }
    public void OnClickClaimDailyGift()
    {
        var canClaim = dailyGiftService.Instance.CanClaim(day);
        if (canClaim)
        {
            dailyGiftService.Instance.ClaimDailyGift(day);
            SetClaimed();

            var gift = dailyGiftService.Instance.dailyGiftConfig.GetGiftForDay(day);

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

        }
    }
    public void Unselect()
    {
        pReward.SetActive(false);
    }
}
