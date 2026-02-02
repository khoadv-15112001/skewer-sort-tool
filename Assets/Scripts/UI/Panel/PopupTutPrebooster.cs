using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using Sonat.Enums;
using SonatFramework.Scripts.UIModule;
using SonatFramework.Systems.EventBus;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class PopupTutPrebooster : Panel
{
    public class Data : UIData
    {
        public Transform preboosterTransform;
    }

    [SerializeField] private Transform arrowTransform;
    [SerializeField] private float delayTime = 1f;
    private Canvas canvas;
    private GraphicRaycaster raycaster;
    private bool canClose = false;

    public override void Open(UIData uiData)
    {
        base.Open(uiData);
        Data data = (Data)uiData;
        arrowTransform.position = data.preboosterTransform.position;
        //arrowTransform.gameObject.SetActive(false);
        canvas = data.preboosterTransform.gameObject.AddComponent<Canvas>();
        raycaster = data.preboosterTransform.gameObject.AddComponent<GraphicRaycaster>();
        canvas.overrideSorting = true;
        canvas.sortingLayerName = "UI_Top";
        canvas.sortingOrder = 1;

        canClose = false;
        DOVirtual.DelayedCall(delayTime, () => canClose = true);
    }

    private void Update()
    {
        if (Input.GetMouseButtonUp(0) && canClose)
        {
            OnUseBooster();
        }
    }

    private void OnUseBooster()
    {
        Destroy(raycaster);
        Destroy(canvas);
        Close();
    }
}