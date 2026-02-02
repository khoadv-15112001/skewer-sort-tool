using System;
using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using MyGame.SkewerJam.Scripts.Service;
using SkewerJam.Utils.Effects;
using Sonat.Enums;
using SonatFramework.Scripts.Utils;
using SonatFramework.Systems.EventBus;
using SonatFramework.Systems.InventoryManagement;
using TMPro;
using UnityEngine;

public class UICurrencyEnergyInGame : MonoBehaviour
{
    [SerializeField] private GameObject icon;
    [SerializeField] private Transform spawnPoint_UICollectEffect;
    [SerializeField] private Transform targetPosition;
    [SerializeField] private TMP_Text txtValue;
    [SerializeField] private float delayCollect = 1f;
    [SerializeField] private float duration = 1f;
    [SerializeField] private float delayCountText = 0.5f;
    [SerializeField] private AnimationCurve curveClose;
    [SerializeField] private float delayClose = 1.5f;

    [Header("Shake")]
    [SerializeField] private float durationShake = 1f;
    [SerializeField] private float strengthShake = 10f;
    [SerializeField] private int vibratoShake = 10;
    [SerializeField] private float randomnessShake = 90f;
    [SerializeField] private bool snappingShake = true;
    [SerializeField] private bool fadeOutShake = true;

    private void OnEnable()
    {
        if (!MySonatFramework.GetService<HLWEventService>().IsUnlocked() || MySonatFramework.GetService<HLWEventService>().CheckFinishEvent())
        {
            gameObject.SetActive(false);
            return;
        }

        txtValue.text = "0";
        SonatUtils.DelayCall(delayCollect, () =>
        {
            CollectEffect(GameResource.Energy).Forget();
        }, this);
    }

    private async UniTask CollectEffect(GameResource resource, Action onComplete = null)
    {
        var level = MySonatFramework.userDataService.GetLevel();
        var diff = MySonatFramework.GetService<HLWEventService>().HLWEventConfig.GetEnergyReward(level - 1);

        var collectEffect = new CollectEffectMultipleAtHome()
        {
            collectEffectName = "UICollectResouceEffectInWinPanel",
            isShowText = false
        };

        collectEffect.Collect(resource, diff, spawnPoint_UICollectEffect.position, targetPosition.position, onComplete);

        await UniTask.Delay((int)(delayCountText * 1000));

        if (this == null || transform == null) return;

        _ = txtValue?.DOCounter(0, diff, duration);
        _ = transform.DOShakePosition(durationShake, strengthShake, vibratoShake, randomnessShake,
            snappingShake, fadeOutShake, ShakeRandomnessMode.Full);

        await UniTask.Delay((int)(delayClose * 1000));

        if (this == null || transform == null) return;

        OnClose();
    }

    public void OnClose()
    {
        if (this == null || transform == null) return;
        GetComponent<RectTransform>()?.DOLocalMoveX(250, 0.5f).SetEase(curveClose);
    }


    // private async UniTask ShakeIconPumpkin()
    // {
    //     await UniTask.Delay((int)(delayShake * 1000));
    //     if (icon != null) icon.transform.DOShakePosition(1f, 10, 10, 90, false, true);
    // }
}
