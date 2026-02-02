using System.Collections;
using DG.Tweening;
using I2.Loc;
using SonatFramework.Scripts.SonatSDKAdapterModule;
using SonatFramework.Scripts.UIModule;
using SonatFramework.Scripts.UIModule.UIElements;
using SonatFramework.Systems.EventBus;
using SonatFramework.Systems.InventoryManagement;
using SonatFramework.Systems.InventoryManagement.GameResources;
using UnityEngine;
using UnityEngine.UI;

public class PopupRewardLuckySpin : Panel
{
    [SerializeField] private Localize localize;
    [SerializeField] private UIRewardGrid rewardGrid;
    [SerializeField] private Button btnX2;
    [SerializeField] private Image imgFlare;

    private RewardData rewardData;

    public override void Open(UIData uiData)
    {
        base.Open(uiData);
        if (uiData.TryGet("Title", out string title))
        {
            localize.SetTerm(title);
        }

        if (uiData.TryGet("Reward", out rewardData))
        {
            rewardGrid.SetReward(rewardData);
        }

        btnX2.gameObject.SetActive(false);
        if (uiData.TryGet("x2", out bool x2))
        {
            btnX2.gameObject.SetActive(x2);
        }

        imgFlare.transform.DORotate(new Vector3(0, 0, 360), 4f, RotateMode.FastBeyond360).SetEase(Ease.Linear).SetLoops(-1, LoopType.Restart);
    }

    public void ClaimClick()
    {
        StartCoroutine(ReceiveItems());
        Close();
    }

    public void ClaimX2Click()
    {
        MySonatFramework.ShowRewardAds(ClaimX2, "x2_reward", "x2_reward");
    }

    private void ClaimX2()
    {
        StartCoroutine(ReceiveItemsX2());
        Close();
    }

    IEnumerator ReceiveItemsX2()
    {
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
            SonatCollectEffect collectEffect = new CollectEffectSingle();
            EventBus<AddItemEvent>.Raise(new AddItemEvent()
            {
                position = transform.position,
                resource = resource.resource,
                quantity = resource.quantity * 2,
                collectEffect = collectEffect
            });
            yield return new WaitForSeconds(0.2f);
        }
        MySonatFramework.GetService<InventoryService>().NotiUpdateResource();
    }

    IEnumerator ReceiveItems()
    {
        foreach (var resource in rewardData.resourceDatas)
        {
            SonatCollectEffect collectEffect = new CollectEffectSingle();
            EventBus<AddItemEvent>.Raise(new AddItemEvent()
            {
                position = transform.position,
                resource = resource.resource,
                quantity = resource.quantity,
                collectEffect = collectEffect
            });
            yield return new WaitForSeconds(0.2f);
        }
        MySonatFramework.GetService<InventoryService>().NotiUpdateResource();
    }
}
