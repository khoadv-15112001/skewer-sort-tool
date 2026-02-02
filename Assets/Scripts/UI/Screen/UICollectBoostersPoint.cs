using System;
using System.Collections;
using System.Collections.Generic;
using Base.Singleton;
using DG.Tweening;
using Sonat.Enums;
using SonatFramework.Scripts.UIModule;
using SonatFramework.Systems.EventBus;
using UnityEngine;

public class UICollectBoostersPoint: MonoBehaviour
{

    private EventBinding<AddItemEvent> collectItemEvent;
    private bool active = true;
    private void OnEnable()
    {
        collectItemEvent = new EventBinding<AddItemEvent>(OnCollectResource);

        PopupPrePlay.OnShowPopup += OnShowPopup;
    }

    private void OnDisable()
    {
        EventBus<AddItemEvent>.Deregister(collectItemEvent);
        PopupPrePlay.OnShowPopup -= OnShowPopup;
    }

    protected virtual void OnCollectResource(AddItemEvent eventData)
    {
        if (!active) return;

        var resourceType = eventData.resource.ResourceType();
        if (resourceType != GameResourceType.Booster && resourceType != GameResourceType.PreBooster) return;

        if (resourceType == GameResourceType.PreBooster)
        {
            var popupPreplay = PanelManager.Instance.GetPanel<PopupPrePlay>();
            if (popupPreplay != null) return;
        }

        if (eventData.collectEffect != null)
        {
            eventData.collectEffect.Collect(eventData.resource, eventData.quantity, eventData.position, transform.position, () =>
            {
                float defaultScale = transform.localScale.x;
                transform.DOKill();
                transform.DOScale(defaultScale * 1.1f, 0.075f).SetLoops(2, LoopType.Yoyo);
            });
        }
    }

    private void OnShowPopup(bool isOpen)
    {
        active = !isOpen;
    }
}
