using System;
using Cysharp.Threading.Tasks;
using Gameplay.Entities;
using Gameplay.LevelData;
using UnityEngine;

public class PrimaryGrillShutter : PrimaryGrill
{
    [SerializeField] private GrillVisualShutter visualShutter;

    private const int MoveToChangeState = 4;

    private bool isClosed = false;

    private int moveCount = 0;

    public override async UniTask SetData(GrillData grillData)
    {
        _ = base.SetData(grillData);
        isClosed = grillData.isLock;
        lockState = isClosed ? (byte)1 : (byte)0;
        visualShutter.SetUpShutter(isClosed);
        moveCount = 0;
        SetLockItems(isClosed);
        moveCount = 0;
        //Debug.Log($"anhnt: [PrimaryGrillShutter] {gameObject.name} SetData moveCount {moveCount}");

    }

    protected override void ResetData()
    {
        base.ResetData();
        moveCount = 0;
    }

    public void OnEnable()
    {
        grillBaseBehaviorSO.eventSystemSO.RegisterEvents_OnCollectItem(OnCollectItem);
        grillBaseBehaviorSO.eventSystemSO.RegisterEvents_OnDropItem(OnItemDropped);
    }

    public void OnDisable()
    {
        grillBaseBehaviorSO.eventSystemSO.UnregisterEvents_OnCollectItem(OnCollectItem);
        grillBaseBehaviorSO.eventSystemSO.UnregisterEvents_OnDropItem(OnItemDropped);
    }

    private void OnItemDropped(Item item, bool changed)
    {
        moveCount++;
        //Debug.Log($"anhnt: [PrimaryGrillShutter] {gameObject.name} moveCount {moveCount}");
        if (moveCount >= MoveToChangeState)
        {
            isClosed = !isClosed;
            lockState = isClosed ? (byte)1 : (byte)0;
            SetLockItems(isClosed);
            visualShutter.SetUpShutter(isClosed);
            moveCount = 0;
        }
    }

    public override void Unlock()
    {
        SetLockItems(false);
        isClosed = false;
        visualShutter.SetUpShutter(isClosed);
        moveCount = 0;
        GameplayController.OnActionUnlockGrill?.Invoke(this);
    }

    private void OnCollectItem(int itemId)
    {

    }

}
