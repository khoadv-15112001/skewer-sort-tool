using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Gameplay.Entities;
using Gameplay.LevelData;
using Sonat.Enums;
using SonatFramework.Scripts.UIModule;
using UnityEngine;


public class PrimaryGrillOvercooked : PrimaryGrill
{
    public static List<PrimaryGrillOvercooked> primaryGrillOvercookeds;
    [SerializeField] private GrillVisualOvercook visualOvercook;

    private bool startCountDown = false;

    private bool overcooked = false;

    private int maxTime = 0;

    private int timeCount;
    private bool isCloseLid = false;

    public override async UniTask SetData(GrillData grillData)
    {
        maxTime = grillData.time;
        _ = base.SetData(grillData);
        visualOvercook.SetUpTimeSlider(maxTime);
        isCloseLid = false;
        overcooked = false;
        startCountDown = false;

        primaryGrillOvercookeds ??= new List<PrimaryGrillOvercooked>();
        primaryGrillOvercookeds.Add(this);
    }

    public void OnEnable()
    {
        grillBaseBehaviorSO.eventSystemSO.RegisterEvents_OnCollectItem(OnCollectItem);
        grillBaseBehaviorSO.eventSystemSO.RegisterEvents_OnDropItem(OnItemDropped);
        TickService.OnTick += OnTick;
        GameplayController.OnSelectItem += OnSelectItem;
    }
    //https://chat.googleapis.com/v1/spaces/AAQAlxswfFI/messages?key=AIzaSyDdI0hCZtE6vySjMm-WEfRq3CPzqKqqsHI&token=qcF0d6Ty_Y_33o2XyF8R1yiZREqmfk6QpKy2NHdCSZI

    private void OnTick()
    {
        if (!startCountDown) return;

        bool isPlaying = GameplayController.instance.gameState is GameState.Playing or GameState.UsingBooster;
        bool isFreeze = GameplayController.instance.timeManager.isFreeze;

        if (!isPlaying || isFreeze) return;
        if (overcooked) return;

        if (timeCount <= 0)
        {
            overcooked = true;
            visualOvercook.UpdateSliderValue(0);
            Debug.LogError("Gameover overcooked!!");
            PreOvercooked();
            //burnedParticle?.Play();
            return;
        }

        timeCount--;
        visualOvercook.UpdateSliderValue(timeCount);
    }

    private void PreOvercooked()
    {
        // if (popupWarningBomb)
        // {
        //     popupWarningBomb.FinishWarning();
        //     popupWarningBomb = null;
        // }

        PanelManager.Instance.OpenPanelByName<PopupSkipOvercook>("PopupSkipOvercook",
            new UIData().Add("Overcook", this).Add("OnSkipOvercook", (Action)SkipOvercook).Add("OnGiveUp", (Action)Overcooked));
    }

    private void Overcooked()
    {
        //burnedParticle?.Play();
        _ = GameplayController.instance.OnOvercooked();
    }

    private void OnDisable()
    {
        grillBaseBehaviorSO.eventSystemSO.UnregisterEvents_OnCollectItem(OnCollectItem);
        grillBaseBehaviorSO.eventSystemSO.UnregisterEvents_OnDropItem(OnItemDropped);
        TickService.OnTick -= OnTick;
        GameplayController.OnSelectItem -= OnSelectItem;
    }

    private void OnSelectItem(Item item)
    {
        GameplayController.OnSelectItem -= OnSelectItem;
        if (!startCountDown)
        {
            startCountDown = true;
            timeCount = maxTime;
            visualOvercook.UpdateSliderValue(timeCount);
        }
    }

    private void OnCollectItem(int itemId)
    {

    }

    private void OnItemDropped(Item item, bool changed)
    {
        CheckEmptyGrill();
    }

    public override void CheckSubGrills(bool forceShowFirst = true)
    {
        base.CheckSubGrills();
        CheckEmptyGrill();
    }

    private void CheckEmptyGrill()
    {
        if (isCloseLid) return;

        foreach (var slot in slots)
        {
            if (!slot.isEmpty())
            {
                return;
            }
        }

        if (subGrills != null && subGrills.Count > 0)
        {
            return;
        }

        // Tất cả slot đều empty - grill hoàn thành
        isCloseLid = true;
        visualOvercook.OnComplete();
        TickService.OnTick -= OnTick;
        SetLockItems(true);
        //Debug.LogError("Close grill overcooked!!");
    }

    protected override void OnComplete()
    {
        base.OnComplete();
        //timeCount = maxTime;
        //visualOvercook.UpdateSliderValue(timeCount);
        CheckEmptyGrill();
    }

    public void SkipOvercook()
    {
        overcooked = false;
        timeCount = maxTime;
        visualOvercook.UpdateSliderValue(timeCount);
    }
}
