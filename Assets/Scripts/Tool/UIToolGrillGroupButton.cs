using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIToolGrillGroupButton : MonoBehaviour
{
    [SerializeField] private UIButtonGrillGroup group;
    public int index;
    public Image image;
    public Color[] colors;

    public CanvasGroup canvasGroup;

    public void SetSelected(bool selected)
    {
        //image.color = selected ? colors[1] : colors[0];
        canvasGroup.alpha = selected ? 1 : 0.5f;
    }

    public void OnClickSelect()
    {
        group.OnButtonSelected(index);
    }
}
