using System;
using Gameplay.Entities;
using SonatFramework.Scripts.UIModule;
using UnityEngine;

public class PopupGuideMagicKey : PopupHightlightItem
{
    // public Action onClose;
    // private Action<PrimaryGrill> onSelectEntity;
    // [SerializeField] private Canvas bgCanvas;
    //
    // public override void Open(UIData data)
    // {
    //     base.Open(data);
    //     bgCanvas.sortingLayerName = "Object";
    //     bgCanvas.sortingOrder = 50;
    //     onClose = data.TryGet<Action>("onClose", out var action) ? action : null;
    //     if (!data.TryGet<Action<PrimaryGrill>>("onSelectEntity", out onSelectEntity)) onSelectEntity = null;
    // }
    //
    // private void Update()
    // {
    //     if (Input.GetMouseButtonDown(0))
    //     {
    //         var hits = Physics2D.OverlapPointAll(GameplayController.instance.mainCamera.ScreenToWorldPoint(Input.mousePosition));
    //         if (hits.Length > 0)
    //         {
    //             foreach (var hit in hits)
    //             {
    //                 if (hit.transform.TryGetComponent<PrimaryGrill>(out var primaryGrill))
    //                 {
    //                      onSelectEntity?.Invoke(primaryGrill);
    //                 }
    //             }
    //         }
    //     }
    // }
    //
    // public override void Close()
    // {
    //     onClose?.Invoke();
    //     base.Close();
    // }
}