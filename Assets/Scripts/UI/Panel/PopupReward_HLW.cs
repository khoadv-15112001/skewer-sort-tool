using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using SonatFramework.Scripts.UIModule;
using SonatFramework.Scripts.UIModule.UIElements;
using SonatFramework.Scripts.Utils;
using SonatFramework.Systems.EventBus;
using SonatFramework.Systems.InventoryManagement.GameResources;
using Spine;
using Spine.Unity;
using UnityEngine;
using UnityEngine.UI;

public class PopupReward_HLW : Panel
{
    [SerializeField] private Button tapToOpen;
    [SerializeField] private Button tapToClaim;

    [Space(10)] [SerializeField] private Transform animRoot;
    [SerializeField] private SkeletonGraphic animation;
    [SerializeField] private UIRewardGroup uiRewardGroup;

    [Space(10)] [Header("Animation")] [SerializeField]
    private float startScale = 0.35f;

    [SerializeField] private AnimationCurve curveScale;

    [Header("Move")] [SerializeField] private float delayMove = 0.5f;
    [SerializeField] private AnimationCurve curveMoveX;
    [SerializeField] private AnimationCurve curveMoveY;
    [SerializeField] private float durationMove = 0.5f;
    [SerializeField] private ParticleSystem psMove;
    [SerializeField] private float delayMovePs = 0.5f;

    [Header("Move Item")] [SerializeField] private float delaySpawnItem = 0.5f;
    [SerializeField] private float durationMoveItem = 1f;
    [SerializeField] private float delayMoveItem = 0.5f;
    [SerializeField] private float durationFadeItem = 0.5f;
    [SerializeField] private AnimationCurve curveMoveItemX;
    [SerializeField] private AnimationCurve curveMoveItemY;
    [SerializeField] private AnimationCurve curveMoveItemScale;
    [SerializeField] private ParticleSystem psMoveItem;
    [SerializeField] private ParticleSystem psMoveItemAfter;
    [SerializeField] private float delayMovePsAfter = 0.5f;

    [Header("Scale Out Item")] [SerializeField]
    private float delayScaleOutItem = 0.5f;

    [SerializeField] private float durationScaleOutItem = 0.5f;
    [SerializeField] private AnimationCurve curveScaleOutItem;
    [SerializeField] private float delayItemOut = 0.1f;
    [SerializeField] private float delayPsItemOut = 0.5f;

    [Header("Pumpkin out")] [SerializeField]
    private Transform pumpkinOutPos;

    [SerializeField] private float delayPumpkinOut = 0.5f;
    [SerializeField] private float durationPumpkinOut = 0.5f;
    [SerializeField] private AnimationCurve curvePumpkinOut;

    [Header("Close")] [SerializeField] private float delayClose = 0.5f;

    private bool canReceiveReward = false;
    private bool isClickOpen = false;
    private bool isClickClaim = false;
    private List<RewardItemEffectController> uiRewardItems = new();
    private List<Vector3> uiRewardItemsPos = new();

    public override void Open(UIData uiData)
    {
        base.Open(uiData);

        tapToClaim.gameObject.SetActive(false);
        tapToOpen.gameObject.SetActive(true);
        isClickOpen = false;
        isClickClaim = false;

        psMove.gameObject.SetActive(false);
        psMoveItem.gameObject.SetActive(false);
        psMoveItemAfter.gameObject.SetActive(false);
        var rootPos = animRoot.position;
        if (uiData.TryGet("Reward", out RewardData rewardData))
        {
            uiRewardGroup.SetData(rewardData);
            uiRewardItems.Clear();
            uiRewardItemsPos.Clear();
            SonatUtils.ExecuteNextFrame(() =>
            {
                uiRewardItems = uiRewardGroup.GetComponentsInChildren<RewardItemEffectController>().ToList();
                foreach (var uiRewardItem in uiRewardItems)
                {
                    uiRewardItem.SetAlpha(0f);
                    uiRewardItemsPos.Add(uiRewardItem.transform.position); // lưu vị trí ban đầu
                    uiRewardItem.transform.position = rootPos; // đặt vị trí ban đầu
                }
            });
        }

        if (uiData.TryGet("StartPos", out Vector3 startPos))
        {
            canReceiveReward = false;
            animation.AnimationState.SetAnimation(0, "Idle_Before", true);

            animRoot.position = startPos;
            animRoot.localScale = Vector3.one * startScale;

            animRoot.DOLocalMoveX(0, durationMove).SetEase(curveMoveX).SetDelay(delayMove);
            animRoot.DOLocalMoveY(0, durationMove).SetEase(curveMoveY).SetDelay(delayMove);
            animRoot.DOScale(Vector3.one, durationMove).SetEase(curveScale).SetDelay(delayMove).OnComplete(() =>
            {
                canReceiveReward = true;
                animation.AnimationState.SetAnimation(0, "Idle", true);
            });

            SonatUtils.DelayCall(delayMovePs, () =>
            {
                psMove.gameObject.SetActive(true);
                psMove.Play();
            });
        }
    }

    public override void OnOpenCompleted()
    {
        base.OnOpenCompleted();
        MySonatFramework.audioService.PlaySound("Chest_HLW_Appear_Grill_sort");
    }

    public void OnClaimClick()
    {
        if (!canReceiveReward || isClickOpen == true) return;
        isClickOpen = true;
        PlayOpenAnimation().Forget();
    }

    private async UniTask PlayOpenAnimation()
    {
        animation.AnimationState.SetAnimation(0, "Open", false).Complete += (TrackEntry trackEntry) =>
        {
            animation.AnimationState.SetAnimation(0, "Open_Idle", true);
        };

        MySonatFramework.audioService.PlaySound("Chest_HLW_Open_Grill_sort");

        SonatUtils.DelayCall(delayMovePsAfter, () =>
        {
            psMoveItemAfter.gameObject.SetActive(true);
            psMoveItemAfter.Play();
        });

        await UniTask.Delay((int)(delaySpawnItem * 1000));
        psMoveItem.gameObject.SetActive(true);
        psMoveItem.Play();
        for (int i = 0; i < uiRewardItems.Count; i++)
        {
            var idx = i;
            uiRewardItems[i].PlayFade(durationFadeItem);
            uiRewardItems[i].transform.DOScale(1, durationMoveItem).SetEase(curveMoveItemScale).From(0);
            uiRewardItems[i].transform.DOMoveX(uiRewardItemsPos[i].x, durationMoveItem).SetEase(curveMoveItemX);
            uiRewardItems[i].transform.DOMoveY(uiRewardItemsPos[i].y, durationMoveItem).SetEase(curveMoveItemY).OnComplete(() =>
            {
                uiRewardItems[idx].PlayPS();
            });

            await UniTask.Delay((int)(delayMoveItem * 1000));
        }

        tapToOpen.gameObject.SetActive(false);
        tapToClaim.gameObject.SetActive(true);
    }

    public void CustomClose()
    {
        if (isClickClaim == true) return;
        isClickClaim = true;
        PlayClose().Forget();
    }

    private async UniTask PlayClose()
    {
        await UniTask.Delay((int)(delayScaleOutItem * 1000));

        SonatUtils.DelayCall(delayPumpkinOut, () =>
        {
            animRoot.DOMove(pumpkinOutPos.position, durationPumpkinOut).SetEase(curvePumpkinOut);
            animRoot.DOScale(0, durationPumpkinOut).SetEase(curvePumpkinOut);
        });

        // Tất cả scale về 0
        for (int i = 0; i < uiRewardItems.Count; i++)
        {
            uiRewardItems[i].transform.DOScale(0, durationScaleOutItem).SetEase(curveScaleOutItem);
            var idx = i;
            SonatUtils.DelayCall(delayPsItemOut, () => { uiRewardItems[idx].PlayPsHide(); });
            MySonatFramework.audioService.PlaySound("Prize_chest_HLW_Disappear_Grill_sort");
            await UniTask.Delay((int)(delayItemOut * 1000));
        }

        RewardData rewardData = uiRewardGroup.GetData();

        foreach (var resourceData in rewardData.resourceDatas)
        {
            EventBus<AddItemEvent>.Raise(new() { resource = resourceData.resource, quantity = resourceData.quantity });
        }

        await UniTask.Delay((int)(delayClose * 1000));
        Close();
    }
}