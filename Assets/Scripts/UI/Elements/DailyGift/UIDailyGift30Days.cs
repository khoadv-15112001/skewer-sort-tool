using System;
using System.Collections.Generic;
using DG.Tweening;
using GrillSort.DailyGift;
using SonatFramework.Scripts.UIModule;
using SonatFramework.Scripts.Utils;
using SonatFramework.Systems;
using SonatFramework.Systems.InventoryManagement;
using SonatFramework.Systems.ObjectPooling;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIDailyGift30Days : MonoBehaviour
{
    [SerializeField] private Image progress;
    [SerializeField] private TMP_Text txtProgress;
    [SerializeField] private Transform container;
    [SerializeField] private int[] days = { 0, 7, 15, 25, 30 };
    [SerializeField] private float[] values = { 0, 0.25f, 0.5f, 0.75f, 1 };
    private readonly Service<DailyGiftService30> dailyGiftService30 = new();
    private readonly Service<PoolingContainerService> poolingService = new();
    private readonly List<UISpecialGiftSlider> specialGifts = new();


    public void OnEnable()
    {
        Setup();
        UpdateUI();

        dailyGiftService30.Instance.onDataChanged += UpdateUI;

        UISpecialGiftSlider.OnClickAction += UnselectAll;
    }

    private void UnselectAll()
    {
        foreach (var gift in specialGifts)
        {
            gift.Unselect();
        }
    }

    public void OnDisable()
    {
        poolingService.Instance.CleanContainer(container);
        UISpecialGiftSlider.OnClickAction -= UnselectAll;
    }


    public void OnDestroy()
    {
        dailyGiftService30.Instance.onDataChanged -= UpdateUI;
    }

    private void Setup()
    {
        poolingService.Instance.CleanContainer(container);
        int index = 0;
        foreach (var gift in dailyGiftService30.Instance.dailyGiftConfig.dailyGifts)
        {
            var item = poolingService.Instance.CreateObject<UISpecialGiftSlider>(container);
            item.Init(gift.day, index);
            specialGifts.Add(item);
            index++;
        }
    }

    private void UpdateUI()
    {
        // progress
        var currentStreakDay = dailyGiftService30.Instance.data.currentStreakDay;
        var finishDay = dailyGiftService30.Instance.dailyGiftConfig.GetFinishDay();
        txtProgress.text = currentStreakDay + "/" + finishDay;

        float value = GetValueProgress(currentStreakDay, finishDay);
        progress.DOFillAmount(value, 0.5f);

        // special gift
        foreach (var gift in specialGifts)
        {
            if (gift.day <= currentStreakDay)
            {
                if (dailyGiftService30.Instance.CanClaim(gift.day))
                {
                    gift.SetCanClaimed();
                    SonatUtils.DelayCall(0.75f, () => gift.OnClickClaimDailyGift());
                }
                else
                {
                    gift.SetClaimed();
                }
            }
            else
            {
                gift.SetNotClaimed();
            }

        }
    }

    private float GetValueProgress(int currentStreakDay, int finishDay)
    {
        for (int i = 1; i < days.Length; i++)
        {
            if (currentStreakDay <= days[i])
            {
                return (values[i] - values[i - 1]) / (days[i] - days[i - 1]) * (currentStreakDay - days[i - 1]) + values[i - 1];
            }
        }
        return 0;
    }
}
