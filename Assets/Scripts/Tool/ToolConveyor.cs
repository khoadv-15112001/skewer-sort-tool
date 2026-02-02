using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Gameplay.LevelData;
using Sirenix.OdinInspector;
using SonatFramework.Scripts.Utils;
using Tool;
using UnityEngine;

public class ToolConveyor : MonoBehaviour
{
    [SerializeField] private Transform direction;
    [SerializeField] private Transform container;
    public ConveyorData conveyData { get; private set; }

    // Start is called before the first frame update
    void Start()
    {
        ToolManager.Instance.OnGrillChanged += OnToolChanged;
    }

    void OnDestroy()
    {
        if (ToolManager.Instance != null)
            ToolManager.Instance.OnGrillChanged -= OnToolChanged;
    }

    public void SetData(ConveyorData conveyData)
    {
        this.conveyData = conveyData;
        SetDirection();

        if (conveyData.grillIds != null && conveyData.grillIds.Count > 0)
        {
            foreach (var grillId in conveyData.grillIds)
            {
                ToolGrill toolGrill = ToolManager.Instance.GetToolGrill(grillId);
                toolGrill.transform.SetParent(container);
                toolGrill.transform.SetLocalPositionZ(0);
            }
        }
    }

    private void SetDirection()
    {
        if (conveyData.moveType == MoveType.Horizontal)
        {
            direction.SetLocalRotateZ(0);
        }
        else if (conveyData.moveType == MoveType.Vertical)
        {
            direction.SetLocalRotateZ(90);
        }
    }

    private void OnToolChanged(ToolGrill toolGrill)
    {
        CheckGrill();
    }

    public void CheckGrill()
    {
        var allGrill = ToolManager.Instance.AllGrills;
        foreach (var toolGrill in allGrill)
        {
            if (conveyData.conveyorType == ConveyorType.HorizontalSimple && toolGrill.GrillData.grillType != GrillType.Simple) continue;
            if (CheckGrillOnConveyor(toolGrill.transform.position))
            {
                toolGrill.transform.parent = container;
                toolGrill.transform.SetLocalPositionZ(0);
                switch (conveyData.moveType)
                {
                    case MoveType.Horizontal:
                        toolGrill.transform.SetLocalPositionY(0);
                        break;
                    case MoveType.Vertical:
                        toolGrill.transform.SetLocalPositionX(0);
                        break;
                }

                if (!conveyData.grillIds.Contains(toolGrill.GrillData.id))
                {
                    conveyData.grillIds.Add(toolGrill.GrillData.id);
                }
            }
            else
            {
                if (conveyData.grillIds.Contains(toolGrill.GrillData.id))
                {
                    conveyData.grillIds.Remove(toolGrill.GrillData.id);
                    toolGrill.transform.parent = null;
                    toolGrill.transform.SetLocalPositionZ(0);
                }
            }
        }

        UpdatePosition();
        UIToolPanel.Instance.UpdateConveyorData();
    }

    private bool CheckGrillOnConveyor(Vector3 position)
    {
        if (conveyData.moveType == MoveType.Horizontal)
        {
            if (Mathf.Abs(transform.position.y - position.y) < 1f)
            {
                return true;
            }
        }
        else
        {
            if (Mathf.Abs(transform.position.x - position.x) < 1f)
            {
                return true;
            }
        }

        return false;
    }


    private Vector3 clickOffset;
    private bool clicked = false;
    private Vector3 lastClick;

    private void OnMouseDown()
    {
        if (!ToolManager.selectAvailable) return;

        if (Input.GetKey(KeyCode.D))
        {
            UIToolPanel.Instance.RemoveConveyor(this);
            return;
        }

        ToolManager.Instance.toolGrillSelector.SetBlockDraw(true);
        lastClick = Input.mousePosition;
        OnSelectConveyor();
        clickOffset = transform.position - ToolManager.Instance.mainCamera.ScreenToWorldPoint(Input.mousePosition);
        clickOffset.z = 0;
        clicked = true;
    }

    public void OnSelectConveyor()
    {
        UIToolPanel.Instance.OnSelectToolConveyor(this);
    }

    private void OnMouseDrag()
    {
        if (!clicked || Vector3.Distance(Input.mousePosition, lastClick) < 0.75f) return;
        Vector3 target = DragPos();
        transform.position = target;
    }

    private Vector3 DragPos()
    {
        Vector3 pos = ToolManager.Instance.mainCamera.ScreenToWorldPoint(Input.mousePosition) + clickOffset;
        pos.z = 0;
        if (ToolManager.Instance.gridSnap == 0) return pos;
        float posX = conveyData.moveType == MoveType.Horizontal ? 0 : Mathf.Round(pos.x / ToolManager.Instance.gridSnap) * ToolManager.Instance.gridSnap;
        float posY = conveyData.moveType == MoveType.Vertical ? 0 : Mathf.Round(pos.y / ToolManager.Instance.gridSnap) * ToolManager.Instance.gridSnap;
        pos = new Vector3(posX, posY, 0);
        return pos;
    }

    private void OnMouseUp()
    {
        clicked = false;
        UpdatePosition();
        ToolManager.Instance.toolGrillSelector.SetBlockDraw(false);
        //CheckGrill();
    }

    public void UpdatePosition()
    {
        foreach (var grillId in conveyData.grillIds.ToList())
        {
            var toolGrill = ToolManager.Instance.GetToolGrill(grillId);
            if (toolGrill == null)
            {
                conveyData.grillIds.Remove(grillId);
                continue;
            }

            toolGrill.UpdatePosition();
        }

        conveyData.position = new Vector3Data(transform.position);
    }
}