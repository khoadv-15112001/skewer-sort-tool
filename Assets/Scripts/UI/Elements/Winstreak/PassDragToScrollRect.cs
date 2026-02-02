using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class PassDragToScrollRect : MonoBehaviour,
    IBeginDragHandler, IDragHandler, IEndDragHandler, IInitializePotentialDragHandler
{
    private ScrollRect _scrollRect;

    void Awake()
    {
        // Tìm ScrollRect gần nhất ở cha
        _scrollRect = GetComponentInParent<ScrollRect>();
    }

    public void OnInitializePotentialDrag(PointerEventData eventData)
    {
        if (_scrollRect == null) return;
        // Đảm bảo ScrollRect biết trước để bỏ ngưỡng kéo nếu cần
        ExecuteEvents.Execute(_scrollRect.gameObject, eventData, ExecuteEvents.initializePotentialDrag);
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (_scrollRect == null) return;
        ExecuteEvents.Execute(_scrollRect.gameObject, eventData, ExecuteEvents.beginDragHandler);
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (_scrollRect == null) return;
        ExecuteEvents.Execute(_scrollRect.gameObject, eventData, ExecuteEvents.dragHandler);
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (_scrollRect == null) return;
        ExecuteEvents.Execute(_scrollRect.gameObject, eventData, ExecuteEvents.endDragHandler);
    }
}
