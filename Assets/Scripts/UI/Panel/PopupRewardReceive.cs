using System;
using System.Collections;
using System.Collections.Generic;
using Sonat.Enums;
using SonatFramework.Scripts.UIModule;
using SonatFramework.Scripts.UIModule.UIElements;
using SonatFramework.Scripts.Utils;
using SonatFramework.Systems.EventBus;
using SonatFramework.Systems.InventoryManagement.GameResources;
using UnityEngine;

public class PopupRewardReceive : Panel
{
    [SerializeField] private UIRewardGrid uiRewardGrid;

    [SerializeField] private List<UIRewardItem> uiRewardItems;

    public static Action OnClose { get; set; }

    private RewardData rewardData;
    public override void Open(UIData uiData)
    {
        base.Open(uiData);
        rewardData = uiData.Get<RewardData>("RewardData");

        uiRewardGrid.SetReward(rewardData);
        uiRewardItems = GetUIRewardItems();
        StartCoroutine(PlaySound());
    }

    IEnumerator PlaySound()
    {
        yield return new WaitForSeconds(0.3f);
        MySonatFramework.audioService.PlaySound(AudioId.Chest_Level_Open);
        //yield return new WaitForSeconds(0.75f);
        //MySonatFramework.audioService.PlaySound(AudioId.Items_Merge);
    }

    private List<UIRewardItem> GetUIRewardItems()
    {
        uiRewardItems = new List<UIRewardItem>();
        foreach (Transform child in uiRewardGrid.transform)
        {
            if (child.gameObject.activeSelf == true && child.TryGetComponent<UIItemGroup>(out var uiItemGroup))
            {
                foreach (Transform item in uiItemGroup.transform)
                {
                    if (item.gameObject.activeSelf == true && item.TryGetComponent<UIRewardItem>(out var uiRewardItem))
                    {
                        uiRewardItems.Add(uiRewardItem);
                    }
                }
            }
        }
        return uiRewardItems;
    }

    public void OnClaimClick()
    {
        foreach (var uIRewardItem in uiRewardItems)
        {
            SonatCollectEffect effect = new CollectEffectSingle(){
                scale = 1.5f
            };
            EventBus<AddItemEvent>.Raise(new AddItemEvent()
            {
                resource = uIRewardItem.Resource,
                quantity = uIRewardItem.Quantity,
                position = uIRewardItem.transform.position,
                collectEffect = effect
            });
        }
        Close();
    }

    public override void Close()
    {
        base.Close();
        
        // Lưu callback trước khi clear
        Action onCloseCallback = OnClose;
        OnClose = null; // Clear để tránh memory leak
        
        onCloseCallback?.Invoke();
    }
}
