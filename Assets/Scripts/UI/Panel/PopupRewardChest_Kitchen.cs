using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using GrillSort.KitchenMission;
using MyGame.Modules.CardCollection;
using SonatFramework.Scripts.UIModule;
using SonatFramework.Scripts.UIModule.UIElements;
using SonatFramework.Scripts.Utils;
using SonatFramework.Systems.EventBus;
using SonatFramework.Systems.InventoryManagement.GameResources;
using Spine;
using Spine.Unity;
using UnityEngine;
using UnityEngine.UI;

public class PopupRewardChest_Kitchen : Panel
{
    [SerializeField] private Button tapToClaim;

    [Space(10)][SerializeField] private Transform animRoot;
    [SerializeField] private SkeletonGraphic animation;
    [SerializeField] private UIRewardGroup uiRewardGroup;

    [Space(10)]

    [Header("Move Item")][SerializeField] private float delaySpawnItem = 0.5f;
    [SerializeField] private float durationMoveItem = 1f;
    [SerializeField] private float delayMoveItem = 0.5f;
    [SerializeField] private float durationFadeItem = 0.5f;
    [SerializeField] private AnimationCurve curveMoveItemX;
    [SerializeField] private AnimationCurve curveMoveItemY;
    [SerializeField] private AnimationCurve curveMoveItemScale;

    [Header("Scale Out Item")]
    [SerializeField]
    private float delayScaleOutItem = 0.5f;

    [SerializeField] private float durationScaleOutItem = 0.5f;
    [SerializeField] private AnimationCurve curveScaleOutItem;
    [SerializeField] private float delayItemOut = 0.1f;
    [SerializeField] private float delayPsItemOut = 0.5f;

    [Header("Pumpkin out")]
    [SerializeField]
    private Transform pumpkinOutPos;

    [SerializeField] private float delayPumpkinOut = 0.5f;
    [SerializeField] private float durationPumpkinOut = 0.5f;
    [SerializeField] private AnimationCurve curvePumpkinOut;

    [Header("Close")][SerializeField] private float delayClose = 0.5f;

    [SerializeField] private AudioClip appearClip;
    [SerializeField] private AudioClip openClip;

    [SerializeField] private List<ParticleSystem> openPS = new();

    private bool isClickClaim = false;
    private List<RewardItemEffectController> uiRewardItems = new();
    private List<Vector3> uiRewardItemsPos = new();

    public static int Stage;

    public override void Open(UIData uiData)
    {
        base.Open(uiData);

        tapToClaim.gameObject.SetActive(false);
        isClickClaim = false;

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

        PlayOpenAnimation().Forget();
    }

    public override void OnOpenCompleted()
    {
        base.OnOpenCompleted();
        MySonatFramework.audioService.PlayAudio("", appearClip);
    }

    private async UniTask PlayOpenAnimation()
    {
        var currentStageData = KitchenMissionService.Instance.GetCurrentStageData();

        animation.initialSkinName = currentStageData.nameSkin;
        animation.Initialize(true);

        animation.AnimationState.SetAnimation(0, "Appear", false);

        await UniTask.Delay((int)(delaySpawnItem * 1000));

        openPS[Mathf.Clamp(Stage, 0, openPS.Count - 1)].Play();

        MySonatFramework.audioService.PlayAudio("", openClip);

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

        panelCanvasGroup.alpha = 0;

        await UniTask.WaitUntil(() => MySonatFramework.GetService<CardCollectionService>() != null && !CardCollectionService.IsRunQueueRewardCard);

        await UniTask.Delay((int)(delayClose * 1000));

        Close();
    }
}