using Sonat.Enums;
using System.Collections.Generic;
using UnityEngine;

public class UINavigateBarSwitch : MonoBehaviour
{
    [SerializeField] private Transform buttonContainer;   
    [SerializeField] private Transform tabContainer;      
    [SerializeField] private NavigationType startTab;     

    private Dictionary<NavigationType, UINavigationItemSwitch> items = new();
    private Dictionary<NavigationType, UITabBase> tabs = new();
    private NavigationType currTab = NavigationType.None;

    private void Start()
    {
        Init();
        SetFirstTab(startTab);
    }

    private void Init()
    {
        foreach (Transform child in buttonContainer)
        {
            var item = child.GetComponent<UINavigationItemSwitch>();
            if (item == null) continue;

            item.InitData(this);
            items[item.type] = item;
        }

        foreach (Transform child in tabContainer)
        {
            var tab = child.GetComponent<UITabBase>();
            if (tab == null) continue;

            tabs[tab.type] = tab;
        }
    }

    private void SetFirstTab(NavigationType tab)
    {
        SwitchTab(tab);
    }

    public void SwitchTab(NavigationType type)
    {
        if (type == currTab) return;

        foreach (var kv in items)
        {
            if (kv.Key == type) kv.Value.OnSelected();
            else kv.Value.OnDeselected();
        }

        foreach (var kv in tabs)
        {
            bool active = kv.Key == type;
            kv.Value.gameObject.SetActive(active);

            if (active) kv.Value.OnShow();
            else kv.Value.OnHide();
        }

        currTab = type;
    }
}