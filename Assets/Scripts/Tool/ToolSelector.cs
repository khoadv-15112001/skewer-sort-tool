using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ToolSelector : MonoBehaviour
{
    private Vector3 clickOffset;
    private bool clicked = false;

    private void OnMouseDown()
    {
        if(!ToolManager.selectAvailable) return;
        clickOffset = transform.position - ToolManager.Instance.mainCamera.ScreenToWorldPoint(Input.mousePosition);
        clickOffset.z = 0;
        clicked = true;
    }

    private void OnMouseDrag()
    {
        if (!clicked) return;
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
    }
}
