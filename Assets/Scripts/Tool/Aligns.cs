using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Aligns : MonoBehaviour
{
    public void AlignObjectsToLeftBounds(List<GameObject> objs, Bounds bound)
    {
        if (objs == null || objs.Count == 0)
        {
            Debug.LogError("No objects to align.");
            return;
        }

        var listAlignUndo = new List<AlignUndo>();

        // Calculate the leftmost position of the combined bounds
        float leftMostPosition = bound.min.x;

        // Align all objects to the leftmost position
        foreach (GameObject obj in objs)
        {
            Renderer[] renderers = obj.GetComponentsInChildren<Renderer>();
            if (renderers != null && renderers.Length > 0)
            {
                Bounds renderBounds = renderers[0].bounds;
                for(int i = 1; i < renderers.Length; i++)
                {
                    renderBounds.Encapsulate(renderers[i].bounds);
                }

                Vector3 objPosition = obj.transform.position;

                listAlignUndo.Add(new AlignUndo()
                {
                    obj = obj,
                    pos = objPosition
                });

                objPosition.x = leftMostPosition + renderBounds.extents.x;
                obj.transform.position = objPosition;
            }
        }

        ToolManager.Instance.undoController.AddAlign(listAlignUndo);
    }

    public void AlignObjectsHorizontally(List<GameObject> objs, Bounds bound)
    {
        if (objs == null || objs.Count == 0)
        {
            Debug.LogError("No objects to align.");
            return;
        }

        var listAlignUndo = new List<AlignUndo>();

        // Calculate the center position of the combined bounds
        float centerPosition = bound.center.x;

        // Align all objects to the center position
        foreach (GameObject obj in objs)
        {
            Vector3 objPosition = obj.transform.position;

            listAlignUndo.Add(new AlignUndo()
            {
                obj = obj,
                pos = objPosition
            });

            objPosition.x = centerPosition;
            obj.transform.position = objPosition;
        }

        ToolManager.Instance.undoController.AddAlign(listAlignUndo);
    }

    public void AlignObjectsToRightBounds(List<GameObject> objs, Bounds bound)
    {
        if (objs == null || objs.Count == 0)
        {
            Debug.LogError("No objects to align.");
            return;
        }

        var listAlignUndo = new List<AlignUndo>();

        // Calculate the leftmost position of the combined bounds
        float rightMostPosition = bound.max.x;

        // Align all objects to the leftmost position
        foreach (GameObject obj in objs)
        {
            Renderer[] renderers = obj.GetComponentsInChildren<Renderer>();
            if (renderers != null && renderers.Length > 0)
            {
                Bounds renderBounds = renderers[0].bounds;
                for(int i = 1; i < renderers.Length; i++)
                {
                    renderBounds.Encapsulate(renderers[i].bounds);
                }

                Vector3 objPosition = obj.transform.position;

                listAlignUndo.Add(new AlignUndo()
                {
                    obj = obj,
                    pos = objPosition
                });

                objPosition.x = rightMostPosition - renderBounds.extents.x;
                obj.transform.position = objPosition;
            }
        }

        ToolManager.Instance.undoController.AddAlign(listAlignUndo);
    }

    public void AlignObjectsToTopBounds(List<GameObject> objs, Bounds bound)
    {
        if (objs == null || objs.Count == 0)
        {
            Debug.LogError("No objects to align.");
            return;
        }

        var listAlignUndo = new List<AlignUndo>();


        // Calculate the topmost position of the combined bounds
        float topMostPosition = bound.max.y;

        // Align all objects to the topmost position
        foreach (GameObject obj in objs)
        {
            Renderer[] renderers = obj.GetComponentsInChildren<Renderer>();
            if (renderers != null && renderers.Length > 0)
            {
                Bounds renderBounds = renderers[0].bounds;
                for(int i = 1; i < renderers.Length; i++)
                {
                    renderBounds.Encapsulate(renderers[i].bounds);
                }

                Vector3 objPosition = obj.transform.position;

                listAlignUndo.Add(new AlignUndo()
                {
                    obj = obj,
                    pos = objPosition
                });

                objPosition.y = topMostPosition - renderBounds.extents.y;
                obj.transform.position = objPosition;
            }
        }

        ToolManager.Instance.undoController.AddAlign(listAlignUndo);
    }

    public void AlignObjectsVertically(List<GameObject> objs, Bounds bound)
    {
        if (objs == null || objs.Count == 0)
        {
            Debug.LogError("No objects to align.");
            return;
        }

        var listAlignUndo = new List<AlignUndo>();

        // Calculate the center position of the combined bounds
        float centerPosition = bound.center.y;

        // Align all objects to the center position
        foreach (GameObject obj in objs)
        {
            Vector3 objPosition = obj.transform.position;

            listAlignUndo.Add(new AlignUndo()
            {
                obj = obj,
                pos = objPosition
            });

            objPosition.y = centerPosition;
            obj.transform.position = objPosition;
        }

        ToolManager.Instance.undoController.AddAlign(listAlignUndo);
    }

    public void AlignObjectsToBottomBounds(List<GameObject> objs, Bounds bound)
    {
        if (objs == null || objs.Count == 0)
        {
            Debug.LogError("No objects to align.");
            return;
        }

        var listAlignUndo = new List<AlignUndo>();

        // Calculate the bottommost position of the combined bounds
        float bottomMostPosition = bound.min.y;

        // Align all objects to the bottommost position
        foreach (GameObject obj in objs)
        {
            Renderer[] renderers = obj.GetComponentsInChildren<Renderer>();
            if (renderers != null && renderers.Length > 0)
            {
                Bounds renderBounds = renderers[0].bounds;
                for(int i = 1; i < renderers.Length; i++)
                {
                    renderBounds.Encapsulate(renderers[i].bounds);
                }

                Vector3 objPosition = obj.transform.position;

                listAlignUndo.Add(new AlignUndo()
                {
                    obj = obj,
                    pos = objPosition
                });

                objPosition.y = bottomMostPosition + renderBounds.extents.y;
                obj.transform.position = objPosition;
            }
        }

        ToolManager.Instance.undoController.AddAlign(listAlignUndo);
    }

    public void DistributeObjectsAlongAxis(List<GameObject> objs, Bounds totalBounds, bool horizontally)
    {
        if (objs == null || objs.Count == 0)
        {
            Debug.LogError("No objects to distribute.");
            return;
        }

        var listAlignUndo = new List<AlignUndo>();

        // Calculate total width/height available within bounds
        float totalWidth = totalBounds.size.x;
        float totalHeight = totalBounds.size.y;

        float totalObjectsWidth = 0;
        float totalObjectsHeight = 0;

        foreach (GameObject go in objs)
        {
            totalObjectsWidth += go.GetComponent<Collider2D>().bounds.size.x;
            totalObjectsHeight += go.GetComponent<Collider2D>().bounds.size.y;
        }

        float deltaDistanceX = (totalWidth - totalObjectsWidth) / (objs.Count - 1);
        float deltaDistanceY = (totalHeight - totalObjectsHeight) / (objs.Count - 1);

        if (horizontally)
        {
            // Start from the leftmost position
            float initX = totalBounds.min.x;
            objs = objs.OrderBy(choosedObject => choosedObject.transform.position.x).ToList();

            foreach (GameObject go in objs)
            {
                float boxWidth = go.GetComponent<Collider2D>().bounds.size.x;
                // Set position along the X axis

                listAlignUndo.Add(new AlignUndo()
                {
                    obj = go,
                    pos = go.transform.position
                });

                go.transform.position = new Vector3(initX + boxWidth / 2, go.transform.position.y, go.transform.position.z);
                // Move initX to the next position
                initX += boxWidth + deltaDistanceX;
            }
        }
        else
        {
            // Start from the bottommost position
            float initY = totalBounds.min.y;
            objs = objs.OrderBy(choosedObject => choosedObject.transform.position.x).ToList();

            foreach (GameObject go in objs)
            {
                float boxHeight = go.GetComponent<Collider2D>().bounds.size.y;
                // Set position along the Y axis

                listAlignUndo.Add(new AlignUndo()
                {
                    obj = go,
                    pos = go.transform.position
                });

                go.transform.position = new Vector3(go.transform.position.x, initY + boxHeight / 2, go.transform.position.z);
                // Move initY to the next position
                initY += boxHeight + deltaDistanceY;
            }
        }

        ToolManager.Instance.undoController.AddAlign(listAlignUndo);
    }
}