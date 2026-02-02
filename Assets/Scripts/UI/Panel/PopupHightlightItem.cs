using System;
using System.Collections.Generic;
using Gameplay.Entities;
using SonatFramework.Scripts.Helper;
using SonatFramework.Scripts.UIModule;
using TMPro;
using UnityEngine;

public class PopupHightlightItem : Panel
{
    [SerializeField] private Canvas bgCanvas;
    [SerializeField] private Canvas guideCanvas;
    [SerializeField] private TMP_Text contentText;

    private Action onClose;
    private Action<List<EntityBase>> onSelectEntity;

    public override void Open(UIData data)
    {
        base.Open(data);
        bgCanvas.sortingLayerName = "UI_Top";
        bgCanvas.sortingOrder = 50;

        guideCanvas.sortingLayerName = "UI_Top";
        guideCanvas.sortingOrder = 51;

        if (data != null)
        {
            onClose = data.TryGet<Action>("onClose", out var action) ? action : null;
            if (!data.TryGet<Action<List<EntityBase>>>("onSelectEntity", out onSelectEntity)) onSelectEntity = null;

            if (contentText != null)
            {
                if (data.TryGet<string>("content", out var content))
                {
                    contentText.SetLocalize(content);
                }
                else
                {
                    contentText.gameObject.SetActive(false);
                }
            }
        }
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            var hits = Physics2D.OverlapPointAll(GameplayController.instance.mainCamera.ScreenToWorldPoint(Input.mousePosition));
            if (hits.Length > 0)
            {
                List<EntityBase> entities = new List<EntityBase>();
                foreach (var hit in hits)
                {
                    if (hit.transform.TryGetComponent<EntityBase>(out var entity))
                    {
                        entities.Add(entity);
                    }
                }

                onSelectEntity?.Invoke(entities);
            }

            Close();
        }
    }

    public override void Close()
    {
        onClose?.Invoke();
        base.Close();
    }
}