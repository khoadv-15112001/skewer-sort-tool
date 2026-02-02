using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using GrillSort.PiggyBank;
using SonatFramework.Scripts.Utils;
using SonatFramework.Systems;
using SonatFramework.Systems.EventBus;
using SonatFramework.Systems.ObjectPooling;
using Spine.Unity;
using UnityEngine;

public class AnimationPiggyBank : MonoBehaviour
{
    [SerializeField] private UIReceivePiggyPoint uiReceivePiggyPoint;

    [Header("Animation")] [SerializeField] private SkeletonGraphic animationPiggyBank;
    [SerializeField] private string animFull = "PiggyBank_Full";
    [SerializeField] private string animFull1 = "PiggyBank_Full 1";
    [SerializeField] private float delayCollect = 0.85f;
    [SerializeField] private float timeAnimCollect = 0.75f;
    [SerializeField] private string animCollect = "PiggyBank_Drop";
    [SerializeField] private string animIdle = "PiggyBank_Idle";
    private bool isFull = false;
    private readonly Service<PiggyBankService> piggyBankService = new();
    protected EventBinding<AddItemEvent> collectItemEvent;
    private bool isWaitingForIdle = false;

    private void OnEnable()
    {
        isFull = piggyBankService.Instance.CanUpdatePiggyTier();
        if (isFull)
        {
            SonatUtils.DelayCall(1f,
                () =>
                {
                    animationPiggyBank.AnimationState.SetAnimation(0, animFull, false).Complete += (track) =>
                    {
                        animationPiggyBank.AnimationState.SetAnimation(0, animFull1, true);
                    };
                });
        }

        // uiReceivePiggyPoint.onCollect += OnCollect;
        collectItemEvent = new EventBinding<AddItemEvent>(OnCollect);
    }

    private void OnDisable()
    {
        EventBus<AddItemEvent>.Deregister(collectItemEvent);
    }

    // private void OnCollect()
    // {
    //   
    //     SonatUtils.DelayCall(delayCollect, () =>
    //         {
    //             animationPiggyBank.AnimationState.SetAnimation(0, animCollect, false);
    //         });
    //     SonatUtils.DelayCall(delayCollect + timeAnimCollect, () =>
    //     {
    //         animationPiggyBank.AnimationState.SetAnimation(0, animIdle, true);
    //     });
    // }

    private CancellationTokenSource idleTokenSource;

    async void OnCollect(AddItemEvent addItemEvent)
    {
        var currentTrack = animationPiggyBank.AnimationState.GetCurrent(0);
        if (currentTrack != null && currentTrack.Animation.Name == animCollect && !currentTrack.IsComplete)
        {
            return;
        }

        idleTokenSource?.Cancel();
        idleTokenSource = new CancellationTokenSource();

        animationPiggyBank.AnimationState.SetAnimation(0, animCollect, false);

        try
        {
            await UniTask.Delay(300, cancellationToken: idleTokenSource.Token);
            animationPiggyBank.AnimationState.SetAnimation(0, animIdle, true);
        }
        catch (OperationCanceledException)
        {
        }
    }
}