using System.Collections.Generic;
using Gameplay;
using Gameplay.Entities.ItemScripts;
using Gameplay.Entities.Orders;
using SonatFramework.Scripts.Utils;
using SonatFramework.Systems.ObjectPooling;
using UnityEngine;
using DG.Tweening;
using Cysharp.Threading.Tasks;
using Gameplay.Entities;
using System;

public class CollectEffectMagicWand : MonoBehaviour, IPoolingObject
{
    [SerializeField] private Transform container;
    [Header("Appear")]
    [SerializeField] private float appearDuration = 0.3f;
    [SerializeField] private AnimationCurve appearCurve = AnimationCurve.Linear(0, 0, 1, 1);
    [SerializeField] private float appearRange = 1f;

    [Header("Jump")]
    [SerializeField] private AnimationCurve curve = AnimationCurve.Linear(0, 0, 1, 1);
    [SerializeField] private float speed = 30;
    [SerializeField] private float delayByStep = 0.15f;
    [SerializeField] private float jumpPower = 3f;
    [SerializeField] private int numJump = 1;
    [SerializeField] private float timeEffect = 1f;
    [SerializeField] private float minJumpDuration = 0.4f;
    private List<CollectEffectItem> effectItems = new();

    public virtual void SetData(List<SlotBase> selectedItemSlots, int id, Action<SlotBase> callback)
    {
        MySonatFramework.poolingContainer.CleanContainer(container);
        effectItems.Clear();
        for (int i = 0; i < selectedItemSlots.Count; i++)
        {
            var itemEffect = MySonatFramework.poolingContainer.CreateObject<CollectEffectItem>(container);
            itemEffect.transform.localScale = Vector3.one;
            itemEffect.transform.localPosition = Vector3.zero;
            itemEffect.SetData(id);
            effectItems.Add(itemEffect);

            // appear
            var appearPos = (Vector3.right * UnityEngine.Random.Range(-1f, 1f) + Vector3.up * UnityEngine.Random.Range(-1f, 1f)).normalized * appearRange;
            itemEffect.transform.DOLocalMove(appearPos, appearDuration).SetEase(appearCurve);
        }

        var delay = appearDuration;
        var duration = 0f;
        for (int i = 0; i < selectedItemSlots.Count; i++)
        {
            var slot = selectedItemSlots[i];
            var itemEffect = effectItems[i];
            duration = Vector3.Distance(itemEffect.transform.position, slot.transform.position) / speed;
            duration = Mathf.Max(duration, minJumpDuration);
            // nhảy tới
            itemEffect.transform.DOJump(slot.transform.position, jumpPower, numJump, duration)
                .SetEase(curve)
                .SetDelay(delay)
                .OnComplete(() =>
                {
                    callback?.Invoke(slot);
                });

            // scale tại vị trí đích
            itemEffect.transform.DOScale(1.25f, 0.1f).SetEase(Ease.InOutSine).SetLoops(2, LoopType.Yoyo).SetDelay(delay + duration).OnComplete(() =>
            {
                itemEffect.HideSprite();
            });

            delay += delayByStep;
        }

        SonatUtils.DelayCall(delay + duration + 0.2f, () =>
        {
            PlayEffectEnd();
        });
    }

    private void PlayEffectEnd()
    {
        foreach (var itemEffect in effectItems)
        {
            itemEffect.PlayEffect();
        }

        SonatUtils.DelayCall(timeEffect, () =>
        {
            this.gameObject.SetActive(false);
            // GameFactory.ReturnEntity(this);
        });
    }

    public void Setup()
    {
    }

    public void OnCreateObj(params object[] args)
    {
        GameplayController.OnReplay += OnReplay;
        transform.localScale = Vector3.one;
    }

    public void OnReturnObj()
    {
        GameplayController.OnReplay -= OnReplay;
        transform.localScale = Vector3.one;
        transform.DOKill();
    }

    private void OnReplay()
    {
        GameFactory.ReturnEntity(this);
    }
}
