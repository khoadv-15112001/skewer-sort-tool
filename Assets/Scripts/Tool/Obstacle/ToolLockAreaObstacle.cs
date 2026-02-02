using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Gameplay.LevelData;
using SonatFramework.Scripts.Utils;
using Tool;
using UnityEngine;

public class ToolLockAreaObstacle : ToolObstacleBase
{
    [SerializeField] private Transform[] slots;
    [SerializeField] private BoxCollider2D boxCollider;

    void Start()
    {
        ToolManager.Instance.OnGrillChanged += OnToolChanged;
    }

    void OnDestroy()
    {
        if (ToolManager.Instance != null)
            ToolManager.Instance.OnGrillChanged -= OnToolChanged;
    }

    public override void SetData(ObstacleData data)
    {
        base.SetData(data);
        transform.position = data.position.ToVector3();
    }

    public override void SetGrills(List<ToolGrill> toolGrills)
    {
        base.SetGrills(toolGrills);
        for (int i = 0; i < toolGrills.Count; i++)
        {
            if (i < slots.Length)
            {
                toolGrills[i].transform.SetParent(slots[i]);
                if (obstacleType == ObstacleType.LockAreaHorizontal)
                {
                    toolGrills[i].transform.SetLocalPositionY(0);
                }
                else if (obstacleType == ObstacleType.LockAreaVertical)
                {
                    toolGrills[i].transform.SetLocalPositionX(0);
                }
            }
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
            if (CheckGrillOnObstacle(toolGrill.transform.position))
            {
                if (!obstacleData.grillIds.Contains(toolGrill.GrillData.id))
                {
                    Transform slot = FindSlot();
                    if (slot != null)
                    {
                        toolGrill.transform.SetParent(slot);
                        obstacleData.grillIds.Add(toolGrill.GrillData.id);
                        toolGrills.Add(toolGrill);
                        //toolGrill.transform.localPosition = Vector3.back * 0.2f;
                        toolGrill.transform.SetLocalPositionZ(-0.2f);
                        if (obstacleType == ObstacleType.LockAreaHorizontal)
                        {
                            toolGrill.transform.SetLocalPositionY(0);
                        }
                        else if (obstacleType == ObstacleType.LockAreaVertical)
                        {
                            toolGrill.transform.SetLocalPositionX(0);
                        }
                    }
                }
            }
            else
            {
                if (obstacleData.grillIds.Contains(toolGrill.GrillData.id))
                {
                    obstacleData.grillIds.Remove(toolGrill.GrillData.id);
                    toolGrills.Remove(toolGrill);
                    toolGrill.transform.parent = null;
                    toolGrill.transform.SetLocalPositionZ(0);
                }
            }
        }

        UpdatePosition();
    }

    public Transform FindSlot()
    {
        return slots.FirstOrDefault(e => e.childCount == 0);
    }

    private bool CheckGrillOnObstacle(Vector2 position)
    {
        return boxCollider.bounds.Contains(position);
    }

    public void UpdatePosition()
    {
        foreach (var toolGrill in toolGrills)
        {
            if (toolGrill == null)
            {
                obstacleData.grillIds.Remove(toolGrill.GrillData.id);
                continue;
            }

            toolGrill.transform.SetLocalPositionZ(-0.2f);
            if (obstacleType == ObstacleType.LockAreaHorizontal)
            {
                toolGrill.transform.SetLocalPositionY(0);
            }
            else if (obstacleType == ObstacleType.LockAreaVertical)
            {
                toolGrill.transform.SetLocalPositionX(0);
            }
            toolGrill.UpdatePosition();
        }

        obstacleData.position = new Vector3Data(transform.position);
    }

    public override void ValidateObstacle()
    {
        base.ValidateObstacle();
        CheckGrill();
    }

    #region Move

    private Vector3 clickOffset;
    private bool clicked = false;
    private Vector3 lastClick;

    private void OnMouseDown()
    {
        if (!ToolManager.selectAvailable) return;

        if (ToolManager.Instance.toolGrillSelector.SelectedGrills.Count > 0) return;

        if (Input.GetKey(KeyCode.D))
        {
            UIToolPanel.Instance.RemoveObstacle(this);
            return;
        }

        ToolManager.Instance.toolGrillSelector.SetBlockDraw(true);
        lastClick = Input.mousePosition;
        clickOffset = transform.position - ToolManager.Instance.mainCamera.ScreenToWorldPoint(Input.mousePosition);
        clickOffset.z = 0;
        clicked = true;
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
        float posX = Mathf.Round(pos.x / ToolManager.Instance.gridSnap) * ToolManager.Instance.gridSnap;
        float posY = Mathf.Round(pos.y / ToolManager.Instance.gridSnap) * ToolManager.Instance.gridSnap;
        pos = new Vector3(posX, posY, 0);
        return pos;
    }

    private void OnMouseUp()
    {
        clicked = false;
        CheckGrill();
        ToolManager.Instance.toolGrillSelector.SetBlockDraw(false);
    }

    #endregion
}