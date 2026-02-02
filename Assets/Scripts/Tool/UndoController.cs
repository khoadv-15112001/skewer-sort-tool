using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using Gameplay.LevelData;
using Tool;
using UnityEngine;

public class UndoController : MonoBehaviour
{
    public List<UndoData> undoDatas = new List<UndoData>();
    [SerializeField] private int maxUndo = 30;

    public void ClearUndo()
    {
        undoDatas.Clear();
    }

    public void AddDelete(GameObject go)
    {
        undoDatas.Add(new UndoData
        {
            eTypeUndo = ETypeUndo.Delete,
            obj = go
        });

        go.SetActive(false);
        if (undoDatas.Count > maxUndo)
        {
            undoDatas.RemoveAt(undoDatas.Count - 1);
        }
    }

    public void AddPos(GameObject go, Vector3 pos)
    {
        undoDatas.Add(new UndoData
        {
            eTypeUndo = ETypeUndo.Pos,
            obj = go,
            prePos = pos
        });
        if (undoDatas.Count > maxUndo)
        {
            undoDatas.RemoveAt(undoDatas.Count - 1);
        }
    }

    public void AddScale(GameObject go, Vector3 scale)
    {
        undoDatas.Add(new UndoData
        {
            eTypeUndo = ETypeUndo.Scale,
            obj = go,
            preScale = scale
        });
        if (undoDatas.Count > maxUndo)
        {
            undoDatas.RemoveAt(undoDatas.Count - 1);
        }
    }

    public void AddRotation(GameObject go, Vector3 rotation)
    {
        undoDatas.Add(new UndoData
        {
            eTypeUndo = ETypeUndo.Rotation,
            obj = go,
            preRotate = rotation
        });
        if (undoDatas.Count > maxUndo)
        {
            undoDatas.RemoveAt(undoDatas.Count - 1);
        }
    }

    public void AddAlign(List<AlignUndo> aligns)
    {
        undoDatas.Add(new UndoData
        {
            eTypeUndo = ETypeUndo.Align,
            alignsUndo = aligns
        });
        if (undoDatas.Count > maxUndo)
        {
            undoDatas.RemoveAt(undoDatas.Count - 1);
        }
    }

    // LevelData related undo operations
    public void AddGrillOperation(GrillData grillData, GrillOperationType operationType, object previousData = null)
    {
        undoDatas.Add(new UndoData
        {
            eTypeUndo = ETypeUndo.GrillOperation,
            grillData = grillData,
            grillOperationType = operationType,
            previousData = previousData
        });
        if (undoDatas.Count > maxUndo)
        {
            undoDatas.RemoveAt(undoDatas.Count - 1);
        }
    }

    public void AddItemOperation(ItemData itemData, ItemOperationType operationType, object previousData = null)
    {
        undoDatas.Add(new UndoData
        {
            eTypeUndo = ETypeUndo.ItemOperation,
            itemData = itemData,
            itemOperationType = operationType,
            previousData = previousData
        });
        if (undoDatas.Count > maxUndo)
        {
            undoDatas.RemoveAt(undoDatas.Count - 1);
        }
    }

    public void AddOrderOperation(OrderData orderData, OrderOperationType operationType, object previousData = null)
    {
        undoDatas.Add(new UndoData
        {
            eTypeUndo = ETypeUndo.OrderOperation,
            orderData = orderData,
            orderOperationType = operationType,
            previousData = previousData
        });
        if (undoDatas.Count > maxUndo)
        {
            undoDatas.RemoveAt(undoDatas.Count - 1);
        }
    }

    public void AddLayerOperation(LayerData layerData, LayerOperationType operationType, object previousData = null)
    {
        undoDatas.Add(new UndoData
        {
            eTypeUndo = ETypeUndo.LayerOperation,
            layerData = layerData,
            layerOperationType = operationType,
            previousData = previousData
        });

        if (undoDatas.Count > maxUndo)
        {
            undoDatas.RemoveAt(undoDatas.Count - 1);
        }
    }

    public void Undo()
    {
        if (undoDatas.Count == 0) return;

        var undoData = undoDatas[undoDatas.Count - 1];
        undoDatas.Remove(undoData);

        switch (undoData.eTypeUndo)
        {
            case ETypeUndo.Delete:
                UndoDelete(undoData.obj);
                break;

            case ETypeUndo.Pos:
                UndoPos(undoData);
                break;

            case ETypeUndo.Rotation:
                UndoRotation(undoData);
                break;

            case ETypeUndo.Scale:
                UndoScale(undoData);
                break;

            case ETypeUndo.Align:
                UndoAlign(undoData);
                break;

            case ETypeUndo.GrillOperation:
                UndoGrillOperation(undoData);
                break;

            case ETypeUndo.ItemOperation:
                UndoItemOperation(undoData);
                break;

            case ETypeUndo.OrderOperation:
                UndoOrderOperation(undoData);
                break;

            case ETypeUndo.LayerOperation:
                UndoLayerOperation(undoData);
                break;
        }
    }

    public void UndoDelete(GameObject go)
    {
        go.SetActive(true);
    }

    public void UndoPos(UndoData undoData)
    {
        undoData.obj.transform.position = undoData.prePos;

        DOVirtual.DelayedCall(0.05f, () =>
        {
            ToolManager.Instance.DrawBox();
        });
    }

    public void UndoScale(UndoData undoData)
    {
        undoData.obj.transform.localScale = undoData.preScale;

        DOVirtual.DelayedCall(0.05f, () =>
        {
            ToolManager.Instance.DrawBox();
        });
    }

    public void UndoRotation(UndoData undoData)
    {
        undoData.obj.transform.eulerAngles = undoData.preRotate;

        DOVirtual.DelayedCall(0.05f, () =>
        {
            ToolManager.Instance.DrawBox();
        });
    }

    public void UndoAlign(UndoData undoData)
    {
        var undoAligns = undoData.alignsUndo;

        foreach(var undo in undoAligns)
        {
            undo.obj.transform.position = undo.pos;
        }

        DOVirtual.DelayedCall(0.05f, () =>
        {
            ToolManager.Instance.DrawBox();
        });
    }

    private void UndoGrillOperation(UndoData undoData)
    {
        var grillData = undoData.grillData;
        var operationType = undoData.grillOperationType;
        var previousData = undoData.previousData;

        switch (operationType)
        {
            case GrillOperationType.Add:
                // Remove the grill that was added
                var toolGrill = ToolManager.Instance.GetToolGrill(grillData.id);
                if (toolGrill != null)
                {
                    UIToolPanel.Instance.RemoveGrill(toolGrill);
                }
                break;

            case GrillOperationType.Delete:
                // Restore the deleted grill
                if (previousData is GrillData previousGrillData)
                {
                    var newToolGrill = ToolManager.Instance.CreateToolGrill(previousGrillData.grillType, previousGrillData.SlotCount);
                    newToolGrill.SetData(previousGrillData);
                    UIToolPanel.Instance.LevelData.grillData.Add(previousGrillData);
                }
                break;

            case GrillOperationType.Switch:
                // Switch back the grill types
                if (previousData is GrillType previousType)
                {
                    var currentGrill = ToolManager.Instance.GetToolGrill(grillData.id);
                    if (currentGrill != null)
                    {
                        UIToolPanel.Instance.SwitchGrillType(currentGrill, previousType);
                    }
                }
                break;

            case GrillOperationType.Move:
                // Restore previous position
                if (previousData is Vector3Data previousPos)
                {
                    var grill = ToolManager.Instance.GetToolGrill(grillData.id);
                    if (grill != null)
                    {
                        grill.transform.position = previousPos.ToVector3();
                        grill.UpdatePosition();
                        ToolManager.Instance.OnGrillChanged?.Invoke(grill);
                    }
                }
                break;
        }
    }

    private void UndoItemOperation(UndoData undoData)
    {
        var itemData = undoData.itemData;
        var operationType = undoData.itemOperationType;
        var previousData = undoData.previousData;

        switch (operationType)
        {
            case ItemOperationType.Add:
                // Remove the item that was added
                itemData.id = 0;
                itemData.itemType = ItemType.Normal;
                break;

            case ItemOperationType.Delete:
                // Restore the deleted item
                if (previousData is ItemData previousItemData)
                {
                    itemData.id = previousItemData.id;
                    itemData.itemType = previousItemData.itemType;
                }
                break;

            case ItemOperationType.Switch:
                // Switch back the item ID
                if (previousData is int previousId)
                {
                    itemData.id = previousId;
                }
                break;

            case ItemOperationType.ChangeType:
                // Restore previous item type
                if (previousData is ItemType previousType)
                {
                    itemData.itemType = previousType;
                }
                break;
        }

        // Update the UI
        UIToolPanel.Instance.UpdateGrill();
    }

    private void UndoOrderOperation(UndoData undoData)
    {
        var orderData = undoData.orderData;
        var operationType = undoData.orderOperationType;
        var previousData = undoData.previousData;

        switch (operationType)
        {
            case OrderOperationType.Add:
                // Remove the order that was added
                UIToolPanel.Instance.LevelData.orderData.Remove(orderData);
                break;

            case OrderOperationType.Delete:
                // Restore the deleted order
                if (previousData is OrderData previousOrderData)
                {
                    UIToolPanel.Instance.LevelData.orderData.Add(previousOrderData);
                }
                break;

            case OrderOperationType.Modify:
                // Restore previous order data
                if (previousData is OrderData previousOrder)
                {
                    orderData.OrderItem.Clear();
                    if (previousOrder.OrderItem != null)
                    {
                        orderData.OrderItem.AddRange(previousOrder.OrderItem);
                    }
                }
                break;
        }

        // Update the order UI
        var orderUI = FindObjectOfType<UIToolOrder>();
        if (orderUI != null)
        {
            orderUI.SetData(UIToolPanel.Instance.LevelData);
        }
    }

    private void UndoLayerOperation(UndoData undoData)
    {
        var layerData = undoData.layerData;
        var operationType = undoData.layerOperationType;
        var previousData = undoData.previousData;

        switch (operationType)
        {
            case LayerOperationType.Add:
                // Remove the layer that was added
                var grillData = FindGrillDataContainingLayer(layerData);
                if (grillData != null)
                {
                    grillData.layer.Remove(layerData);
                }
                break;

            case LayerOperationType.Delete:
                // Restore the deleted layer
                if (previousData is LayerData previousLayerData)
                {
                    var grill = FindGrillDataContainingLayer(layerData);
                    if (grill != null)
                    {
                        grill.layer.Add(previousLayerData);
                    }
                }
                break;
        }

        // Update the UI
        UIToolPanel.Instance.UpdateGrill();
    }

    private GrillData FindGrillDataContainingLayer(LayerData layerData)
    {
        foreach (var grillData in UIToolPanel.Instance.LevelData.grillData)
        {
            if (grillData.layer != null && grillData.layer.Contains(layerData))
            {
                return grillData;
            }
        }
        return null;
    }
}

[Serializable]
public class UndoData
{
    public ETypeUndo eTypeUndo;
    public List<AlignUndo> alignsUndo;
    public GameObject obj;
    public Vector3 prePos;
    public Vector3 preRotate;
    public Vector3 preScale;

    // LevelData related fields
    public GrillData grillData;
    public GrillOperationType grillOperationType;
    public ItemData itemData;
    public ItemOperationType itemOperationType;
    public OrderData orderData;
    public OrderOperationType orderOperationType;
    public LayerData layerData;
    public LayerOperationType layerOperationType;
    public object previousData;
}

[Serializable]
public class AlignUndo
{
    public GameObject obj;
    public Vector3 pos;
}

public enum ETypeUndo
{
    Delete = 0,
    Pos = 1,
    Rotation = 2,
    Scale = 3,
    Align = 4,
    GrillOperation = 5,
    ItemOperation = 6,
    OrderOperation = 7,
    LayerOperation = 8
}

public enum GrillOperationType
{
    Add,
    Delete,
    Switch,
    Move
}

public enum ItemOperationType
{
    Add,
    Delete,
    Switch,
    ChangeType
}

public enum OrderOperationType
{
    Add,
    Delete,
    Modify
}

public enum LayerOperationType
{
    Add,
    Delete
}