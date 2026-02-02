using DG.Tweening;
using Gameplay.LevelData;
using SonatFramework.Scripts.UIModule;
using SonatFramework.Scripts.UIModule.UIElements;
using SonatFramework.Scripts.Utils;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Security.Policy;
using _SeqTool;
using Tool;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UIToolSeqOrder : MonoBehaviour
{
    [SerializeField] private Transform contentContainer;
    [SerializeField] private List<Button> listSplitButtons;
    [SerializeField] private Button mergeButton;
    [SerializeField] private Button deleteButton;
    [SerializeField] private Button loadSavedButton;
    [SerializeField] private Button genSeqOrderButton;
    [SerializeField] private Toggle generateToggle;
    [SerializeField] private LayerMask selectableLayerMask;
    //
    private List<UIToolSeqOrderItem> seqItems = new();
    private List<UIToolSeqOrderItem> listOrderItemSelecteds = new List<UIToolSeqOrderItem>();
    private UIToolSeqOrderItem orderItemSelected;
    private LevelData levelData;
    private Transform dragRoot;
    private Canvas uiCanvas;
    private Camera mainCamera;
    //
    public Transform DragRoot
    {
        get
        {
            if (dragRoot == null)
            {
                uiCanvas = contentContainer.GetComponentInParent<Canvas>();
                if (uiCanvas == null)
                    uiCanvas = FindObjectOfType<Canvas>();

                var go = new GameObject("UIToolSeqOrder_DragRoot");
                var rt = go.AddComponent<RectTransform>();
                go.transform.SetParent(uiCanvas.transform, false);
                rt.anchorMin = Vector2.zero;
                rt.anchorMax = Vector2.one;
                rt.sizeDelta = Vector2.zero;
                dragRoot = go.transform;
            }
            return dragRoot;
        }
    }

    private void Awake()
    {
        //generateToggle.onValueChanged.RemoveAllListeners();
        //generateToggle.onValueChanged.AddListener(OnToggleGenerateChanged);
    }

    private void Start()
    {
        mainCamera = Camera.main;
        SonatUtils.DelayCall(.5f, Clear, this);
        //
        if (levelData == null)
            levelData = UIToolPanel.Instance.LevelData;
    }

    private void Update()
    {
        HandleInput();
    }

    public void Initialize(LevelData levelData)
    {
        this.levelData = levelData;
        if(levelData != null && levelData.sequenceOrderData != null && levelData.sequenceOrderData.Count > 0)
        {
            OnClickLoadSavedSeqOrders();
            generateToggle.isOn = true;
        }
        else
        {
            generateToggle.isOn = false;
        }
        generateToggle.onValueChanged.RemoveAllListeners();
        generateToggle.onValueChanged.AddListener(OnToggleGenerateChanged);
    }

    public void Clear()
    {
        MySonatFramework.poolingContainer.CleanContainer(contentContainer);
        seqItems.Clear();
        listOrderItemSelecteds.Clear();
        levelData = null;
    }

    public bool ValidateDataBewforeSave(Dictionary<int, int> dictItemQuantity)
    {
        if(levelData.sequenceOrderData == null || levelData.sequenceOrderData.Count == 0)
            return true;

        int itemCount = 0;
        foreach (var pair in dictItemQuantity)
        {
            itemCount += pair.Value;
        }
        
        foreach(var seqOrder in levelData.sequenceOrderData)
        {
            itemCount -= seqOrder.quantity;
        }
        if(itemCount != 0)
            return false;
        return true;
    }

    public void GenerateSeqOrders()
    {
        bool isDropMode = UIToolPanel.Instance.DropLevelToggle.isOn;
        var sortedGrills = SortOrder(isDropMode);

        // flatten logic: iterate layer index from 0..maxLayers-1, for each layer iterate all grills
        List<int> flatIds = new();
        int maxLayerCount = 0;
        foreach (var g in sortedGrills)
        {
            if (g.layer != null && g.layer.Count > maxLayerCount) maxLayerCount = g.layer.Count;
        }

        for (int layerIndex = 0; layerIndex < maxLayerCount; layerIndex++)
        {
            foreach (var grill in sortedGrills)
            {
                if (grill.layer == null) continue;
                if (layerIndex >= grill.layer.Count) continue;

                var layer = grill.layer[layerIndex];
                if (layer == null || layer.itemData == null) continue;

                // iterate slots left-to-right (index order)
                for (int i = 0; i < layer.itemData.Length; i++)
                {
                    var item = layer.itemData[i];
                    if (item != null && item.id > 0)
                    {
                        flatIds.Add(item.id);
                    }
                }
            }
        }

        List<SequenceOrderData> seq = isDropMode ? CreateSeqOrderDropMode(flatIds) : CreateSeqOrder(flatIds);
        //foreach (var id in flatIds)
        //{
        //    int lastIndex = seq.FindLastIndex(s => s.itemId == id && s.quantity < 3);
        //    if (lastIndex >= 0)
        //    {
        //        seq[lastIndex].quantity++;
        //    }
        //    else
        //    {
        //        seq.Add(new SequenceOrderData() { itemId = id, quantity = 1 });
        //    }
        //}
        levelData.sequenceOrderData = seq;
        //ToolManager.Instance.ToolSeqOrder.GenerateSeqOrderDatas();

        MySonatFramework.poolingContainer.CleanContainer(contentContainer);
        seqItems.Clear();

        if (levelData.sequenceOrderData != null)
        {
            foreach (var seqItem in levelData.sequenceOrderData)
            {
                var uiSeqOrder = MySonatFramework.poolingContainer.CreateObject<UIToolSeqOrderItem>(contentContainer);
                uiSeqOrder.SetData(seqItem.itemId, seqItem.quantity);
                uiSeqOrder.Init(this, contentContainer);
                uiSeqOrder.SetState(UIToolSeqOrderItem.OrderItemState.None);
                seqItems.Add(uiSeqOrder);
            }
        }

    }

    #region ___Item Events____
    public void OnItemClicked(UIToolSeqOrderItem item)
    {
        if (item == null) return;

        if (item.State == UIToolSeqOrderItem.OrderItemState.None)
        {
            item.SetState(UIToolSeqOrderItem.OrderItemState.Selected);
            if (!listOrderItemSelecteds.Contains(item)) 
                listOrderItemSelecteds.Add(item);
        }
        else if (item.State == UIToolSeqOrderItem.OrderItemState.Selected)
        {
            item.SetState(UIToolSeqOrderItem.OrderItemState.None);
            listOrderItemSelecteds.Remove(item);
        }

        // update single selected reference
        orderItemSelected = listOrderItemSelecteds.Count == 1 ? listOrderItemSelecteds[0] : null;

        // Update split buttons: only when exactly one item selected
        if (orderItemSelected != null)
        {
            for (int i = 0; i < listSplitButtons.Count; i++)
            {
                if (listSplitButtons[i] != null)
                {
                    SetActiveBtn(listSplitButtons[i], (i) < orderItemSelected.Quantity);
                }
                SetActiveBtn(deleteButton, true);
            }
        }
        else
        {
            for (int i = 0; i < listSplitButtons.Count; i++)
            {
                if (listSplitButtons[i] != null) 
                    SetActiveBtn(listSplitButtons[i], false);
            }
            SetActiveBtn(deleteButton, false);

        }

        // Update merge button: enabled if exactly two items selected and mergeable
        if (mergeButton != null)
        {
            bool canMerge = false;
            if (listOrderItemSelecteds.Count == 2)
            {
                var a = listOrderItemSelecteds[0];
                var b = listOrderItemSelecteds[1];
                if (a != null && b != null && a.ItemId == b.ItemId)
                {
                    int sum = a.Quantity + b.Quantity;
                    canMerge = a.Quantity < 3 && b.Quantity < 3 && sum <= 3;
                }
            }
            SetActiveBtn(mergeButton, canMerge);
        }
        
        if (deleteButton != null)
        {
            SetActiveBtn(deleteButton, listOrderItemSelecteds.Count > 0);
        }
    }

    public void OnItemDragStarted(UIToolSeqOrderItem item, int originalIndex)
    {
    }

    public void OnItemDragUpdated(UIToolSeqOrderItem item, int originalIndex, int targetIndex)
    {
    }

    public void OnItemDropped(UIToolSeqOrderItem item, int fromIndex, int toIndex)
    {
        if (item == null) return;

        if (!seqItems.Contains(item))
        {
            seqItems.Add(item);
        }

        seqItems.Remove(item);

        int insertIndex = Mathf.Clamp(toIndex, 0, seqItems.Count);
        seqItems.Insert(insertIndex, item);

        for (int i = 0; i < seqItems.Count; i++)
        {
            seqItems[i].transform.SetSiblingIndex(i);
        }

        if (levelData != null)
        {
            levelData.sequenceOrderData = seqItems.Select(s => new SequenceOrderData() { itemId = s.ItemId, quantity = s.Quantity }).ToList();
        }

        foreach (var levelData in levelData.sequenceOrderData)
        {
            Debug.Log(" ==> Seq Order Item ID: " + levelData.itemId + " Quantity: " + levelData.quantity);
        }
    }
    #endregion

    #region ___Button Events___
    public void OnClickLoadSavedSeqOrders()
    {
        if (levelData == null)
            levelData = UIToolPanel.Instance.LevelData;
        MySonatFramework.poolingContainer.CleanContainer(contentContainer);
        seqItems.Clear();
        if (levelData.sequenceOrderData != null)
        {
            foreach (var seqItem in levelData.sequenceOrderData)
            {
                var uiSeqOrder = MySonatFramework.poolingContainer.CreateObject<UIToolSeqOrderItem>(contentContainer);
                uiSeqOrder.SetData(seqItem.itemId, seqItem.quantity);
                uiSeqOrder.Init(this, contentContainer);
                uiSeqOrder.SetState(UIToolSeqOrderItem.OrderItemState.None);
                seqItems.Add(uiSeqOrder);
            }
        }
    }

    public void OnClickGenerateOrders()
    {
        if (levelData == null)
            levelData = UIToolPanel.Instance.LevelData;
        
        if(levelData != null)
        {
            if (levelData.sequenceOrderData != null && levelData.sequenceOrderData.Count > 0)
            {
                UIData data = new UIData();
                data.Add("content", "Gen new SeqOrders will clear the current sequence orders. Are you sure you want to proceed?");
                data.Add("onYes", new System.Action(() =>
                {
                    ClearSequenceOrders();
                    GenerateSeqOrders();
                }));
                PanelManager.Instance.OpenForget<ConfirmPanel>(data);
            }
            else
            {
                GenerateSeqOrders();
            }
        }
    }

    public void OnToggleGenerateChanged(bool isOn)
    {
        if (isOn)
        {
        }
        else
        {
            if(levelData != null && levelData.sequenceOrderData != null && levelData.sequenceOrderData.Count > 0)
            {
                UIData data = new UIData();
                data.Add("content", "Disabling will clear the current sequence orders. Are you sure you want to proceed?");
                data.Add("onYes", new System.Action(() =>
                {
                    ClearSequenceOrders();
                }));
                data.Add("onNo", new System.Action(() =>
                {
                    generateToggle.isOn = true;
                }));
                PanelManager.Instance.OpenForget<ConfirmPanel>(data);
            }
        }
        SetActiveBtn(loadSavedButton, isOn);
        SetActiveBtn(genSeqOrderButton, isOn);
        SetActiveBtn(mergeButton, isOn);
        SetActiveBtn(deleteButton, isOn);
        for (int i = 0; i < listSplitButtons.Count; i++)
            if (listSplitButtons[i] != null)
                SetActiveBtn(listSplitButtons[i], isOn);

    }

    public void OnClickDeleteSelected()
    {
        if (listOrderItemSelecteds.Count == 0) return;

        var indices = new List<int>();
        foreach (var it in listOrderItemSelecteds)
        {
            if (it == null) 
                continue;
            int idx = seqItems.IndexOf(it);
            if (idx >= 0) indices.Add(idx);
            else Destroy(it.gameObject);
        }

        indices.Sort();
        indices.Reverse();

        // remove from levelData
        //ToolManager.Instance.ToolSeqOrder.DeleteSeqOrderDatas(indices);
        foreach (var idx in indices)
        {
            if (levelData != null && levelData.sequenceOrderData != null && idx < levelData.sequenceOrderData.Count)
            {
                levelData.sequenceOrderData.RemoveAt(idx);
            }

            // destroy UI and remove from seqItems
            var removed = seqItems[idx];
            if (removed != null) Destroy(removed.gameObject);
            seqItems.RemoveAt(idx);
        }

        // reindex siblings
        for (int i = 0; i < seqItems.Count; i++) 
            seqItems[i].transform.SetSiblingIndex(i);

        // clear selection
        foreach (var it in listOrderItemSelecteds)
        {
            if (it != null) 
                it.SetState(UIToolSeqOrderItem.OrderItemState.None);
        }
        listOrderItemSelecteds.Clear();
        orderItemSelected = null;

        // disable buttons
        for (int i = 0; i < listSplitButtons.Count; i++)
        {
            if (listSplitButtons[i] != null) 
                SetActiveBtn(listSplitButtons[i], false);
        }
        if (mergeButton != null) 
            SetActiveBtn(mergeButton, false);
        if (deleteButton != null) 
            SetActiveBtn(deleteButton, false);

    }

    public void OnClickMergeBtn()
    {
        if (listOrderItemSelecteds.Count != 2) return;
        var a = listOrderItemSelecteds[0];
        var b = listOrderItemSelecteds[1];
        if (a == null || b == null) 
            return;
        if (a.ItemId != b.ItemId) 
            return;
        int sum = a.Quantity + b.Quantity;
        if (a.Quantity >= 3 || b.Quantity >= 3 || sum > 3) 
            return;

        // determine indices in seqItems
        int indexA = seqItems.IndexOf(a);
        int indexB = seqItems.IndexOf(b);
        if (indexA < 0 || indexB < 0) return;

        // merge into the earlier index (min)
        int targetIndex = Mathf.Min(indexA, indexB);
        int removeIndex = Mathf.Max(indexA, indexB);

        // update data
        //ToolManager.Instance.ToolSeqOrder.MergeSeqOrderDatas(targetIndex, removeIndex, sum);
        if (levelData != null && levelData.sequenceOrderData != null)
        {
            levelData.sequenceOrderData[targetIndex].quantity = sum;
            levelData.sequenceOrderData.RemoveAt(removeIndex);
        }

        // update UI
        var targetItem = seqItems[targetIndex];
        targetItem.UpdateQuantity(sum);
        targetItem.UpdateQuantityText(sum);

        var removedItem = seqItems[removeIndex];
        seqItems.RemoveAt(removeIndex);
        Destroy(removedItem.gameObject);

        // reindex siblings
        for (int i = 0; i < seqItems.Count; i++) 
            seqItems[i].transform.SetSiblingIndex(i);

        // clear selection
        foreach (var it in listOrderItemSelecteds)
        {
            if (it != null) 
                it.SetState(UIToolSeqOrderItem.OrderItemState.None);
        }
        listOrderItemSelecteds.Clear();
        orderItemSelected = null;

        // disable buttons
        for (int i = 0; i < listSplitButtons.Count; i++) 
            if (listSplitButtons[i] != null) 
                SetActiveBtn(listSplitButtons[i], false);
        if (mergeButton != null) 
            SetActiveBtn(mergeButton, false);

        foreach (SequenceOrderData sod in levelData.sequenceOrderData)
        {
            Debug.Log(" ==> Seq Order Item ID: " + sod.itemId + " Quantity: " + sod.quantity);
        }
    }

    public void OnClickSplitBtn(int quantity)
    {
        if (orderItemSelected == null) return;
        if (orderItemSelected.Quantity == quantity) return;

        int remain = orderItemSelected.Quantity - quantity;
        int index = seqItems.IndexOf(orderItemSelected);

        // Cap nhat Data
        //var newOrder = ToolManager.Instance.ToolSeqOrder.SplitSeqOrderData(index, quantity);
        var newOrder = new SequenceOrderData() { itemId = orderItemSelected.ItemId, quantity = remain };
        levelData.sequenceOrderData[index].quantity = quantity;
        levelData.sequenceOrderData.Insert(index + 1, newOrder);

        // Cap nhat UI
        orderItemSelected.UpdateQuantity(quantity);
        orderItemSelected.UpdateQuantityText(quantity);

        // Khoi tao order moi
        var uiSeqOrder = MySonatFramework.poolingContainer.CreateObject<UIToolSeqOrderItem>(contentContainer);
        uiSeqOrder.SetData(newOrder.itemId, newOrder.quantity);
        uiSeqOrder.Init(this, contentContainer);
        seqItems.Insert(index + 1, uiSeqOrder);

        for (int i = 0; i < seqItems.Count; i++)
        {
            seqItems[i].transform.SetSiblingIndex(i);
        }

        for (int i = 0; i < listSplitButtons.Count; i++)
        {
            if (listSplitButtons[i] != null)
            {
                SetActiveBtn(listSplitButtons[i], (i) < orderItemSelected.Quantity);
            }
        }
    }
    #endregion

    private void SetActiveBtn(Button btn, bool isActive)
    {
        Color color = btn.image.color;
        color.a = isActive ? 1f : 0.5f;
        btn.image.color = color;

        btn.interactable = isActive;
    }

    private void HandleInput()
    {

        if (Input.GetMouseButtonDown(0))
        {
            var clickedOrderItem = GetOrderAtMousePosition();
            if (clickedOrderItem == null)
            {
                var panelRt = this.transform as RectTransform;
                bool clickInsidePanel = IsScreenPointInsideRectTransform(panelRt, Input.mousePosition);
                if (!clickInsidePanel)
                {
                    foreach (var it in listOrderItemSelecteds)
                    {
                        if (it != null) it.SetState(UIToolSeqOrderItem.OrderItemState.None);
                    }
                    listOrderItemSelecteds.Clear();
                    orderItemSelected = null;
                    for (int i = 0; i < listSplitButtons.Count; i++) if (listSplitButtons[i] != null) SetActiveBtn(listSplitButtons[i], false);
                    if (mergeButton != null) SetActiveBtn(mergeButton, false);
                    if (deleteButton != null) SetActiveBtn(deleteButton, false);
                }
            }
        }
    }

    private bool IsScreenPointInsideRectTransform(RectTransform rt, Vector2 screenPoint)
    {
        if (rt == null) return false;
        if (uiCanvas == null && contentContainer != null)
        {
            uiCanvas = contentContainer.GetComponentInParent<Canvas>();
        }
        Camera cam = null;
        if (uiCanvas != null && uiCanvas.renderMode != RenderMode.ScreenSpaceOverlay)
        {
            cam = uiCanvas.worldCamera != null ? uiCanvas.worldCamera : mainCamera;
        }
        return RectTransformUtility.RectangleContainsScreenPoint(rt, screenPoint, cam);
    }

    private UIToolSeqOrderItem GetOrderAtMousePosition()
    {
        if (EventSystem.current == null) return null;
        var pointerData = new PointerEventData(EventSystem.current) { position = Input.mousePosition };
        var results = new List<RaycastResult>();
        // Prefer canvas's GraphicRaycaster if available
        var gr = contentContainer.GetComponentInParent<GraphicRaycaster>();
        if (gr != null)
        {
            gr.Raycast(pointerData, results);
        }
        else
        {
            EventSystem.current.RaycastAll(pointerData, results);
        }

        foreach (var res in results)
        {
            var item = res.gameObject.GetComponentInParent<UIToolSeqOrderItem>();
            if (item != null) return item;
        }
        return null;
    }

    private void ClearSequenceOrders()
    {
        if (dragRoot != null)
        {
            Destroy(dragRoot.gameObject);
            dragRoot = null;
        }

        if (levelData != null && levelData.sequenceOrderData != null)
        {
            levelData.sequenceOrderData.Clear();
            levelData.sequenceOrderData = null;
        }

        MySonatFramework.poolingContainer.CleanContainer(contentContainer);
        seqItems.Clear();

        // clear selection
        foreach (var it in listOrderItemSelecteds)
        {
            if (it != null)
                it.SetState(UIToolSeqOrderItem.OrderItemState.None);
        }
        listOrderItemSelecteds.Clear();
        orderItemSelected = null;

        // hide buttons
        for (int i = 0; i < listSplitButtons.Count; i++)
            if (listSplitButtons[i] != null)
                SetActiveBtn(listSplitButtons[i], false);
        if (mergeButton != null)
            SetActiveBtn(mergeButton, false);
        if (deleteButton != null)
            SetActiveBtn(deleteButton, false);
    }

    private List<GrillData> SortOrder(bool isDropMode)
    {
        var sortedGrills = new List<GrillData>();
        if (!isDropMode)
        {
            sortedGrills = levelData.grillData
                .Where(g => g != null && g.position != null)
                .OrderByDescending(g => g.position.y)
                .ThenByDescending(g => g.position.x)
                .ToList();
        }
        else {

            sortedGrills = levelData.grillData
                .Where(g => g != null && g.position != null)
                .OrderBy(g => g.position.y)
                .ThenBy(g => g.position.x)
                .ToList();
        }
        return sortedGrills;
    }

    private List<SequenceOrderData> CreateSeqOrder(List<int> flatIds)
    {
        List<SequenceOrderData> seq = new List<SequenceOrderData>();
        foreach (var id in flatIds)
        {
            int lastIndex = seq.FindLastIndex(s => s.itemId == id && s.quantity < 3);
            if (lastIndex >= 0)
            {
                seq[lastIndex].quantity++;
            }
            else
            {
                seq.Add(new SequenceOrderData() { itemId = id, quantity = 1 });
            }
        }
        return seq;
    }

    private List<SequenceOrderData> CreateSeqOrderDropMode(List<int> flatIds)
    {
        List<SequenceOrderData> seq = new List<SequenceOrderData>();
        foreach (var id in flatIds)
        {
            int lastIndex = seq.FindLastIndex(s => s.itemId == id && s.quantity < 3);
            if (lastIndex >= 0)
            {
                seq[lastIndex].quantity++;
                if (seq[lastIndex].quantity == 3)
                {
                    int swapIndex = -1;
                    for (int i = 0; i < lastIndex; i++)
                    {
                        if (seq[i].quantity < 3)
                        {
                            swapIndex = i;
                            break;
                        }
                    }

                    //int swapIndex = seq.FindIndex(0, lastIndex, s => s.quantity < 3);

                    if (swapIndex >= 0 && swapIndex < lastIndex)
                    {
                        var temp = seq[swapIndex];
                        seq[swapIndex] = seq[lastIndex];
                        seq[lastIndex] = temp;
                    }
                }
            }
            else
            {
                seq.Add(new SequenceOrderData() { itemId = id, quantity = 1 });
            }
        }
        return seq;
    }

}
