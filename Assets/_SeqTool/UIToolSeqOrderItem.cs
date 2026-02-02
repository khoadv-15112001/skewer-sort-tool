using Manager;
using SonatFramework.Scripts.UIModule.UIElements;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class UIToolSeqOrderItem : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IPointerClickHandler
{
    public enum OrderItemState
    {
        None,
        Selected,
        New,
        // add other states if needed
    }

    [SerializeField] private TextMeshProUGUI txtQuantity;
    [SerializeField] private TextMeshProUGUI txtId;
    [SerializeField] private FixedImageRatio fixedImageRatio;
    [SerializeField] private Image backgroundImage;
    [SerializeField] private Color selectColor;
    [SerializeField] private Color originColor;
    [SerializeField] private Color splitColor;
    // Drag state
    private UIToolSeqOrder toolSeqOrder;
    private Canvas uiCanvas;
    private CanvasGroup canvasGroup;
    private Transform contentContainer; // grid content
    private Transform originalParent;
    private RectTransform rectTransform;
    private int originalSiblingIndex;
    private int currentTargetIndex;
    private int itemId;
    private int quantity;
    private OrderItemState state = OrderItemState.None;
    //
    public int ItemId => itemId;
    public int Quantity => quantity;
    public OrderItemState State => state;

    public void SetData(int itemId, int quantity)
    {
        this.itemId = itemId;
        this.quantity = quantity;
        fixedImageRatio.SetSpriteAsync(PathManager.ItemSprite(itemId));
        txtQuantity.text = quantity.ToString();
        txtId.text = itemId.ToString();

        if (backgroundImage != null)
        {
            backgroundImage.color = originColor;
        }

        state = OrderItemState.None;
    }

    public void Init(UIToolSeqOrder owner, Transform contentContainer)
    {
        uiCanvas = contentContainer.GetComponentInParent<Canvas>();
        rectTransform = GetComponent<RectTransform>();
        canvasGroup = GetComponent<CanvasGroup>();

        this.toolSeqOrder = owner;
        this.contentContainer = contentContainer;
        if (backgroundImage != null)
        {
            backgroundImage.color = originColor;
        }
    }

    public void SetSelected(bool selected)
    {
        if (backgroundImage == null) return;
        backgroundImage.color = selected ? selectColor : originColor;
        state = selected ? OrderItemState.Selected : OrderItemState.None;
    }

    public void SetState(OrderItemState newState)
    {
        state = newState;
        if (backgroundImage == null) return;
        switch (state)
        {
            case OrderItemState.None:
                backgroundImage.color = originColor;
                break;
            case OrderItemState.Selected:
                backgroundImage.color = selectColor;
                break;
            case OrderItemState.New:
                backgroundImage.color = splitColor;
                break;
            default:
                backgroundImage.color = originColor;
                break;
        }
    }

    public void UpdateQuantityText(int cnt)
    {
        txtQuantity.text = cnt.ToString();
    }

    public void UpdateQuantity(int newQuantity)
    {
        quantity = newQuantity;
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if ( toolSeqOrder == null || contentContainer == null) return;

        originalParent = transform.parent;
        originalSiblingIndex = transform.GetSiblingIndex();
        currentTargetIndex = originalSiblingIndex;

        // Remove from layout so remaining items shift into the freed slot
        // Put dragged item on overlay (owner provides DragRoot)
        transform.SetParent(toolSeqOrder.DragRoot, true);

        // Make it follow pointer and allow raycasts to hit children below
        canvasGroup.blocksRaycasts = false;
        canvasGroup.alpha = .5f;

        // Notify owner (optional visual state)
        toolSeqOrder.OnItemDragStarted(this, originalSiblingIndex);
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (uiCanvas == null) return;

        // Move with pointer (screen space)
        Vector3 globalMousePos;
        if (RectTransformUtility.ScreenPointToWorldPointInRectangle(uiCanvas.transform as RectTransform, eventData.position, uiCanvas.worldCamera, out globalMousePos))
        {
            rectTransform.position = globalMousePos;
        }
        else
        {
            rectTransform.position = eventData.position;
        }

        // Determine which index the pointer is over inside contentContainer
        int target = contentContainer.childCount; // default to end
        for (int i = 0; i < contentContainer.childCount; i++)
        {
            var child = contentContainer.GetChild(i) as RectTransform;
            if (child == null) continue;

            // If pointer is over this child's rect -> choose it as target
            if (RectTransformUtility.RectangleContainsScreenPoint(child, eventData.position, uiCanvas.worldCamera))
            {
                target = i;
                break;
            }

            // If pointer Y is above the center of this child and the layout is vertical/horizontal,
            // we could refine, but RectangleContainsScreenPoint is sufficient for grid selection.
        }

        currentTargetIndex = target;
        toolSeqOrder.OnItemDragUpdated(this, originalSiblingIndex, currentTargetIndex);
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if ( toolSeqOrder == null || contentContainer == null) return;

        // Insert back into content container at computed index
        int insertIndex = Mathf.Clamp(currentTargetIndex, 0, contentContainer.childCount);

        transform.SetParent(contentContainer, false);
        transform.SetSiblingIndex(insertIndex);

        canvasGroup.blocksRaycasts = true;
        canvasGroup.alpha = 1f;

        toolSeqOrder.OnItemDropped(this, originalSiblingIndex, insertIndex);

        // clear drag state
        originalParent = null;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (toolSeqOrder == null) return;

        if (state == OrderItemState.None)
        {
            toolSeqOrder.OnItemClicked(this);
        }
        else if (state == OrderItemState.Selected)
        {
            toolSeqOrder.OnItemClicked(this);
        }
    }
}
