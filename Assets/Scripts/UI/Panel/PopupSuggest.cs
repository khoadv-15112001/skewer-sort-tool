using DG.Tweening;
using Sonat.Enums;
using SonatFramework.Scripts.UIModule;
using SonatFramework.Systems.EventBus;
using UnityEngine;
using UnityEngine.UI;

public class PopupSuggest : Panel
{
    [SerializeField] private Transform handTransform;

    public override void Open(UIData uiData)
    {
        base.Open(uiData);

        var target = uiData.Get<Vector3>("targetPosition");
        handTransform.position = target;
        // canvas = handTransform.gameObject.AddComponent<Canvas>();
        // // raycaster = uiBooster.gameObject.AddComponent<GraphicRaycaster>();
        // canvas.overrideSorting = true;
        // canvas.sortingLayerName = "UI_Top";
        // canvas.sortingOrder = 1;
    }
}