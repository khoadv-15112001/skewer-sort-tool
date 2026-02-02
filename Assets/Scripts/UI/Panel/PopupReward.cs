using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using I2.Loc;
using MyGame.Modules.CardCollection;
using Sonat.Enums;
using SonatFramework.Scripts.SonatSDKAdapterModule;
using SonatFramework.Scripts.UIModule;
using SonatFramework.Scripts.UIModule.UIElements;
using SonatFramework.Scripts.Utils;
using SonatFramework.Systems.EventBus;
using SonatFramework.Systems.InventoryManagement;
using SonatFramework.Systems.InventoryManagement.GameResources;
using UnityEngine;
using UnityEngine.UI;

public class PopupReward : Panel
{
    [SerializeField] private List<Localize> localizes;
    [SerializeField] private Localize localize;
    [SerializeField] private UIRewardGrid rewardGrid;
    [SerializeField] private UIRewardGrid rewardGridCard;
    [SerializeField] private Button btnX2;
    [SerializeField] private Image bg;
    [SerializeField] private Transform btnObj, titleObj;
    [SerializeField] private List<Transform> transformsScaleZeroOnClaim = new();
    [SerializeField] private GameObject effectObj;

    [SerializeField] private float timeWaitReceiveRewards;

    private RewardData rewardData;
    private Action onClaimComplete;
    private bool receivedX2;

    public override void Open(UIData uiData)
    {
        Debug.Log($"PopupReward Open: {uiData.Get("Reward")}");
        base.Open(uiData);

        if (uiData.TryGet("IAP", out bool isIAP) && isIAP)
        {
            if (effectObj)
                effectObj.SetActive(true);

            MySonatFramework.audioService.PlaySound(AudioId.IAP_Purchase_completed_Grill_sort);
        }
        else
        {
            if (effectObj)
                effectObj.SetActive(false);
        }

        if (uiData.TryGet("Title", out string title))
        {
            foreach (var lz in localizes)
            {
                lz.SetTerm(title);
            }

            localize.SetTerm(title);
        }
        else
        {
            localize.SetTerm("REWARD!");
        }

        if (uiData.TryGet("Reward", out rewardData))
        {
            rewardData = ValidateReward(rewardData);

            RewardData rewardDataBase = new(); // rewards thuong
            RewardData rewardDataCard = new(); // reward = card collection

            foreach (var reward in rewardData.resourceDatas)
            {
                if (GameResourceHelper.ResourceType(reward.resource) == GameResourceType.Card)
                {
                    rewardDataCard.AddReward(reward);
                }
                else
                {
                    rewardDataBase.AddReward(reward);
                }
            }

            rewardGrid.gameObject.SetActive(rewardDataBase.resourceDatas != null && rewardDataBase.resourceDatas.Count > 0);

            if (rewardGridCard)
                rewardGridCard.gameObject.SetActive(rewardDataCard.resourceDatas != null && rewardDataCard.resourceDatas.Count > 0);

            if (rewardDataBase.resourceDatas != null && rewardDataBase.resourceDatas.Count > 0)
            {
                rewardGrid.SetReward(rewardDataBase);
                //rewardGrid.transform.localScale = Vector3.one * (rewardDataBase.resourceDatas.Count > 3? 1 : 1.5f);
            }
            if (rewardDataCard.resourceDatas != null && rewardDataCard.resourceDatas.Count > 0)
            {
                if (rewardGridCard)
                    rewardGridCard.SetReward(rewardDataCard);
            }

            btnX2.gameObject.SetActive(false);
            if (uiData.TryGet("x2", out bool x2))
            {
                btnX2.gameObject.SetActive(x2);
            }

            if (uiData.TryGet("onClaimComplete", out Action action))
            {
                onClaimComplete = action;
            }
            clicked = false;
            receivedX2 = false;
        }
    }

    private bool clicked = false;
    public void ClaimClick()
    {
        if (clicked) return;
        clicked = true;
        ClaimClickAsync().Forget();

        if (rewardGridCard)
            for (int i = 0; i < rewardGridCard.transform.childCount; i++)
            {
                rewardGridCard.transform.GetChild(i).DOScale(0, 0.3f).SetEase(Ease.InBack);
            }
        DisableAllUI();
    }
    private void DisableAllUI()
    {
        titleObj.DOKill();
        btnObj.DOKill();
        titleObj.DOScale(0, 0.3f).SetEase(Ease.InBack);
        btnObj.DOScale(0, 0.3f).SetEase(Ease.InBack);

        foreach (var trans in transformsScaleZeroOnClaim)
        {
            trans.DOScale(0, 0.3f).SetEase(Ease.InBack);
        }
    }
    private async UniTask ClaimClickAsync()
    {
        var token = this.GetCancellationTokenOnDestroy();

        try
        {
            await ReceiveItems().AttachExternalCancellation(token);
            if (panelCanvasGroup != null && panelCanvasGroup)
                panelCanvasGroup.alpha = 0f;

            await UniTask.WaitUntil(
                () => MySonatFramework.GetService<CardCollectionService>() != null && !CardCollectionService.IsRunQueueRewardCard,
                cancellationToken: token
            );

            onClaimComplete?.Invoke();
            Close();
        }
        catch (OperationCanceledException)
        {
            Debug.Log("[ClaimClickAsync] Cancelled due to object destroy");
        }
        catch (Exception ex)
        {
            Debug.LogError($"[ClaimClickAsync] Unexpected exception: {ex}");
        }
    }


    public void ClaimX2Click()
    {
        if (clicked) return;
        if (SonatSDKAdapter.IsRewardAdsReady())
            clicked = true;
        MySonatFramework.ShowRewardAds(OnClickX2Reward, "x2_reward", "x2_reward");

        DisableAllUI();
    }

    private void OnClickX2Reward()
    {
        OnClickX2RewardAsync();
    }

    private async UniTask OnClickX2RewardAsync()
    {
        receivedX2 = true;
        await ReceiveItemsX2(); // chờ chạy xong toàn bộ animation

        panelCanvasGroup.alpha = 0f;
        await UniTask.WaitUntil(() => MySonatFramework.GetService<CardCollectionService>() != null && !CardCollectionService.IsRunQueueRewardCard);

        onClaimComplete?.Invoke();
        Close();

    }

    private async UniTask ReceiveItemsX2()
    {
        bg.DOFade(0, 0.3f).SetEase(Ease.OutCubic);
        var logData = new EarnResourceLogData
        {
            spendType = "x2_reward",
            spendId = "x2_reward",
            isFirstBuy = false,
            source = "non_iap"
        };

        MySonatFramework.GetService<InventoryService>().AddReward(rewardData, logData);
        foreach (var resource in rewardData.resourceDatas)
        {
            if (resource.resource == GameResource.Coin)
            {
                SonatCollectEffect collectEffect = new CollectEffectMultiple();
                EventBus<AddItemEvent>.Raise(new AddItemEvent()
                {
                    position = Vector3.zero,
                    resource = resource.resource,
                    quantity = resource.quantity * 2,
                    collectEffect = collectEffect
                });
            }
            else
            {
                SonatCollectEffect collectEffect = new CollectEffectSingle();
                EventBus<AddItemEvent>.Raise(new AddItemEvent()
                {
                    position = transform.position,
                    resource = resource.resource,
                    quantity = resource.quantity * 2,
                    collectEffect = collectEffect
                });
            }


            await UniTask.Delay(200); // thay cho yield return WaitForSeconds(0.2f)
        }

        MySonatFramework.GetService<InventoryService>().NotiUpdateResource();

        await UniTask.WaitForSeconds(timeWaitReceiveRewards);
    }

    private async UniTask ReceiveItems()
    {
        bg.DOFade(0, 0.3f).SetEase(Ease.OutCubic);
        foreach (var resource in rewardData.resourceDatas)
        {
            if (resource.resource == GameResource.Coin)
            {
                UIRewardItem rewardItem = rewardGrid.GetRewardItem(resource.resource);

                if (rewardItem != null)
                {
                    rewardItem.GetComponent<CanvasGroup>().alpha = 0;
                }
                SonatCollectEffect collectEffect = new CollectEffectMultiple();
                EventBus<AddItemEvent>.Raise(new AddItemEvent()
                {
                    position = rewardItem != null ? rewardItem.transform.position : Vector3.zero,
                    resource = resource.resource,
                    quantity = resource.quantity,
                    collectEffect = collectEffect
                });
            }
            else
            {
                UIRewardItem rewardItem = rewardGrid.GetRewardItem(resource.resource);
                if (rewardItem != null)
                {
                    rewardItem.GetComponent<CanvasGroup>().alpha = 0;
                }

                SonatCollectEffect collectEffect = new CollectEffectMultiple()
                {
                    count = Mathf.Min(10, resource.quantity)
                };
                EventBus<AddItemEvent>.Raise(new AddItemEvent()
                {
                    position = rewardItem != null ? rewardItem.transform.position : Vector3.zero,
                    resource = resource.resource,
                    quantity = resource.quantity,
                    collectEffect = collectEffect
                });
            }

            await UniTask.Delay(200); // thay cho yield return WaitForSeconds(0.2f)
        }

        MySonatFramework.GetService<InventoryService>().NotiUpdateResource();

        await UniTask.WaitForSeconds(timeWaitReceiveRewards);
    }

    private void OnApplicationFocus(bool hasFocus)
    {
        if (hasFocus)
        {
            SonatUtils.DelayCall(1f, () =>
            {
                if (clicked && !receivedX2) Close();
            }, this);
        }
    }

    private RewardData ValidateReward(RewardData rewardData)
    {
        var newRewardData = new RewardData();
        foreach (var resource in rewardData.resourceDatas)
        {
            if (GameResourceHelper.ResourceType(resource.resource) == GameResourceType.Card)
            {
                var cardCollectionService = MySonatFramework.GetService<CardCollectionService>();
                if (cardCollectionService == null || cardCollectionService.IsUnlocked() == false)
                {
                    continue;
                }
            }
            newRewardData.AddReward(resource);
        }
        return newRewardData;
    }
}
