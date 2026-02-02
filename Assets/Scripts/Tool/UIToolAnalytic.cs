using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Manager;
using SonatFramework.Scripts.Utils;
using TMPro;
using Tool;
using UnityEngine;

public class UIToolAnalytic : MonoBehaviour
{
    [SerializeField] private Transform container;
    [SerializeField] private TMP_Text txtTotal;
    [SerializeField] private TMP_Text txtUnique;
    private Dictionary<int, UIItemAnalytic> uiItemAnalytics = new Dictionary<int, UIItemAnalytic>();
    [SerializeField] private Transform highlightContainer;
    private UIItemAnalytic itemSelected;
    private RectTransform rectTransform;

    private void Start()
    {
        rectTransform = GetComponent<RectTransform>();
        SonatUtils.DelayCall(1f, Clear, this);
    }

    public void Clear()
    {
        MySonatFramework.poolingContainer.CleanContainer(container);
        uiItemAnalytics.Clear();
        txtTotal.text = "0";
        MySonatFramework.poolingContainer.CleanContainer(highlightContainer);
        itemSelected = null;
        txtUnique.text = "0";
    }

    public void UpdateData(Dictionary<int, int> idsCount)
    {
        if (idsCount.Count == 0)
        {
            Clear();
            return;
        }

        int count = 0;
        List<int> itemUnique = new List<int>();
        foreach (var idCount in idsCount)
        {
            count += idCount.Value;

            ItemId itemId = (ItemId)idCount.Key;

            if (itemId == ItemId.Item_Golden_1500)
            {
                continue;
            }

            var str = itemId.ToString();
            try
            {
                int uniqueId = int.Parse(str.Split('_')[1]);
                if (!itemUnique.Contains(uniqueId))
                    itemUnique.Add(uniqueId);

                if (uiItemAnalytics.ContainsKey(idCount.Key))
                {
                    uiItemAnalytics[idCount.Key].UpdateQuantity(idCount.Value);
                }
                else
                {
                    var item = MySonatFramework.poolingContainer.CreateObject<UIItemAnalytic>(container);
                    item.SetData(idCount.Key, idCount.Value);
                    uiItemAnalytics.Add(idCount.Key, item);
                }
            }
            catch (Exception e)
            {
                Debug.Log($"Error parsing item ID: {e.Message}");
                continue;
            }

        }

        foreach (var itemAnalytic in uiItemAnalytics.Keys.ToList())
        {
            if (!idsCount.ContainsKey(itemAnalytic))
            {
                uiItemAnalytics[itemAnalytic].gameObject.SetActive(false);
                uiItemAnalytics.Remove(itemAnalytic);
            }
        }

        List<int> idSorted = uiItemAnalytics.Keys.ToList();
        idSorted.Sort((a, b) => a.CompareTo(b));
        foreach (var itemAnalytic in uiItemAnalytics)
        {
            itemAnalytic.Value.transform.SetSiblingIndex(idSorted.IndexOf(itemAnalytic.Key));
        }

        txtTotal.text = count.ToString();
        txtUnique.text = itemUnique.Count.ToString();
    }

    public void OnSelectItemAnalytic(UIItemAnalytic itemAnalytic, int id)
    {
        if (itemSelected != null)
        {
            itemSelected.Select(false);
        }

        itemAnalytic.Select(true);
        itemSelected = itemAnalytic;

        List<ToolGrill> grills = ToolManager.Instance.GetGrillsHasItem(id);
        MySonatFramework.poolingContainer.CleanContainer(highlightContainer);
        foreach (var toolGrill in grills)
        {
            var highlight = MySonatFramework.poolingContainer.CreateObject<Transform>(highlightContainer);
            highlight.position = toolGrill.transform.position;
        }
    }

    private void ClearSelected()
    {
        if (itemSelected != null)
        {
            itemSelected.Select(false);
        }

        itemSelected = null;
        MySonatFramework.poolingContainer.CleanContainer(highlightContainer);
    }

    private void Update()
    {
        if (itemSelected != null && Input.GetMouseButtonDown(0))
        {
            if (!RectTransformUtility.RectangleContainsScreenPoint(rectTransform, Input.mousePosition))
            {
                ClearSelected();
            }
        }
    }
}