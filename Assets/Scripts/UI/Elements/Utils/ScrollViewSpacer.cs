using UnityEngine;

public class ScrollViewSpacer : MonoBehaviour
{
    [SerializeField] private RectTransform rect;

    public void SetHeight(float height)
    {
        rect.sizeDelta = new Vector2 (rect.sizeDelta.x, height);
    }
}
