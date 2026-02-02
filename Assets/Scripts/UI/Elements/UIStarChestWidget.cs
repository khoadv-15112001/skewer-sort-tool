using System;
using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using Sonat;
using Sonat.Enums;
using SonatFramework.Scripts.Helper;
using SonatFramework.Scripts.UIModule;
using SonatFramework.Scripts.Utils;
using SonatFramework.Systems.EventBus;
using SonatFramework.Systems.InventoryManagement;
using SonatFramework.Systems.InventoryManagement.GameResources;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIStarChestWidget : UIHomeWidget
{
    [SerializeField] private Image progressFill;
    [SerializeField] private Transform iconTransfrorm;
    [SerializeField] private TMP_Text txtProgress;
    [SerializeField] private UIReceiveCollectEffect receiveCollectEffect;
    private StarChestService starChestService;
    private InventoryService inventoryService;
    private PlayerPrefInt lastStar;
    private int currentStar = 0;
    private int collectedStar = 0;
    private StarChest starChest;
    private bool isProcessing = false;
    private int delayNext = 0;
    [SerializeField] private ParticleSystem collectEffect;

    public override void Setup()
    {
        lastStar = new PlayerPrefInt("LastStar", -1);
        inventoryService = MySonatFramework.GetService<InventoryService>();
        starChestService = MySonatFramework.GetService<StarChestService>();
        starChest = starChestService.CurrentChest;
        if (lastStar.Value == -1)
        {
            lastStar.Value = inventoryService.GetResourceView(GameResource.Star);
        }

        UpdateValue(lastStar.Value, false);
        base.Setup();
        if (collectEffect)
        {
            collectEffect.gameObject.SetActive(false);
        }
    }

    // private void Update()
    // {
    //     if (Input.GetKeyDown(KeyCode.A))
    //     {
    //         lastStar.Value = 10;
    //         currentStar = 500;
    //         Collect();
    //     }
    // }

    private void Collect()
    {
        var collect = new CollectEffectMultipleStar();
        collect.collectEffectName = "CollectResourceMultipleStarHome";
        collect.Collect(GameResource.Star, 10, Vector3.zero, iconTransfrorm.position, () => { receiveCollectEffect.PlayCollectEffect(); });
        MySonatFramework.audioService.PlaySound(AudioId.Items_Fly_Whoosh);

        StartCoroutine(PlaySoundCollectFly());

        SonatUtils.DelayCall(1.2f, () =>
        {
            int currentStar = inventoryService.GetResource(GameResource.Star);
            UpdateValue(currentStar, true);
            CheckOpenChest(1);
            if (collectEffect)
            {
                collectEffect.gameObject.SetActive(true);
                collectEffect.Play();
            }
        }, this);
    }

    IEnumerator PlaySoundCollectFly()
    {
        yield return new WaitForSeconds(0.85f);
        for (int i = 0; i < 4; i++)
        {
            yield return new WaitForSeconds(0.165f);
            MySonatFramework.audioService.PlaySound(AudioId.Stars_Fill, 0.65f);
        }

        MySonatFramework.audioService.PlaySound(AudioId.Items_Collected);
    }

    private void UpdateValue(int value, bool counter)
    {
        if (value > starChest.starRequire)
        {
            value = starChest.starRequire;
        }

        if (counter)
        {
            // string textProgressFormat = "{0}/" + starChest.starRequire;
            // txtProgress.DOCounterFormat(lastStar.Value, value, textProgressFormat, 0.3f);
            txtProgress.DOCounter(lastStar.Value, value, 0.3f);
            progressFill.fillAmount = lastStar.Value * 1.0f / starChest.starRequire;
            float progress = value * 1.0f / starChest.starRequire;
            progressFill.DOFillAmount(progress, 0.3f);
        }
        else
        {
            //txtProgress.text = $"{value}/{starChest.starRequire}";
            txtProgress.text = value.ToString();
            float progress = value * 1.0f / starChest.starRequire;
            progressFill.fillAmount = progress;
        }

        inventoryService.NotiUpdateResource(GameResource.Star);
        lastStar.Value = value;
    }

    private void CheckOpenChest(float delay = 0)
    {
        if (currentStar >= starChest.starRequire)
        {
            delayNext = 1000;
            SonatUtils.DelayCall(delay, () =>
            {
                SpendResourceLogData spendData = new()
                {
                    earnId = "star_chest",
                    earnType = "feature"
                };

                inventoryService.ReduceResource(GameResource.Star, starChest.starRequire, spendData, noti: false);
                OpenStarChest();
            }, this);
        }
        else
        {
            delayNext = 0;
            isProcessing = false;
        }
    }

    private void OpenStarChest()
    {
        RewardData rewardData = starChest.reward;
        var logData = new EarnResourceLogData
        {
            spendType = "star_chest",
            spendId = "star",
            isFirstBuy = false,
            source = "non_iap"
        };
        inventoryService.AddReward(rewardData, logData);
        UIData data = new UIData();
        data.Add("Title", "Star Chest");
        data.Add("Reward", rewardData);
        data.Add(UIDataKey.CallBackOnClose, (Action)OnClaimChest);
        PanelManager.Instance.OpenPanel<PopupReward>(data);
        MySonatFramework.customTrackingService.OnShowPopup(gameObject.name.ToLogString(), "pop_up", "non_iap", "auto");
        starChestService.NextStarChest();
        lastStar.Value = 0;
    }

    private void OnClaimChest()
    {
        starChest = starChestService.CurrentChest;
        currentStar = inventoryService.GetResource(GameResource.Star);
        //lastStar.Value = 0;
        UpdateValue(currentStar, true);
        CheckOpenChest(1);
    }

    public override async UniTask<bool> ProcessTask()
    {
        currentStar = inventoryService.GetResource(GameResource.Star);
        isProcessing = true;
        delayNext = 0;
        if (currentStar > lastStar.Value)
        {
            SonatUtils.DelayCall(1, Collect, this);
        }
        else
        {
            CheckOpenChest();
        }

        await UniTask.WaitUntil(() => !isProcessing);
        if (delayNext > 0)
            await UniTask.Delay(delayNext);
        return false;
    }

    public void OnOpenStarChest()
    {
        PanelManager.Instance.OpenPanel<PopupStarChest>();
        MySonatFramework.customTrackingService.OnShowPopup(gameObject.name.ToLogString(), "widget", "non_iap", "user");
    }

    // #if UNITY_EDITOR
    //     private void Update()
    //     {
    //         if (Input.GetKeyDown(KeyCode.Space))
    //         {
    //             Collect();
    //         }
    //     }
    // #endif
}