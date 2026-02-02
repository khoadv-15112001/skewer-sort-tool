using UnityEngine;
using System.Collections.Generic;

public class MultiSelect2D : MonoBehaviour
{
    private Vector3 dragStartPosition;
    private bool isDragging = false;
    private List<GameObject> selectedObjects = new List<GameObject>();

    private void Update()
    {
        if (Input.GetMouseButtonDown(0)) // Left mouse button pressed
        {
            dragStartPosition = Input.mousePosition;
            isDragging = true;
        }
        else if (Input.GetMouseButtonUp(0) && isDragging) // Left mouse button released
        {
            isDragging = false;
            SelectObjectsInRect(dragStartPosition, Input.mousePosition);
        }
    }

    private void OnGUI()
    {
        if (isDragging)
        {
            // Draw the selection rectangle
            Rect selectionRect = new Rect(dragStartPosition, Input.mousePosition - dragStartPosition);
            GUI.Box(selectionRect, "");
        }
    }

    private void SelectObjectsInRect(Vector2 start, Vector2 end)
    {
        selectedObjects.Clear();
        Rect selectionRect = new Rect(start, end - start);

        // Find all GameObjects in the scene
        GameObject[] allObjects = FindObjectsOfType<GameObject>();

        foreach (GameObject obj in allObjects)
        {
            // Check if the object is a 2D object (has a SpriteRenderer)
            SpriteRenderer spriteRenderer = obj.GetComponent<SpriteRenderer>();
            if (spriteRenderer != null)
            {
                // Convert the object's position to screen coordinates
                Vector2 screenPos = Camera.main.WorldToScreenPoint(obj.transform.position);

                // Check if the object is within the selection rectangle
                if (selectionRect.Contains(screenPos))
                {
                    selectedObjects.Add(obj);
                }
            }
        }

        // Log the selected objects
        foreach (GameObject obj in selectedObjects)
        {
            Debug.Log("Selected: " + obj.name);
        }
    }
}