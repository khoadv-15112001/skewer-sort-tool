using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using Sonat.Enums;
using SonatFramework.Systems.EventBus;
using UnityEngine;

public abstract class UINavigateBarBase : MonoBehaviour
{
    protected Dictionary<NavigationType, UINavigationItem> items = new Dictionary<NavigationType, UINavigationItem>();
    protected Dictionary<NavigationType, UITabBase> tabs = new Dictionary<NavigationType, UITabBase>();
    
    public Transform container;
    public RectTransform highlight;
    public NavigationType tabStart;
    protected NavigationType currTab = NavigationType.None;
    public Transform tabContainer;
    protected List<NavigationType> navigationTypes;
    
    public virtual void Init()
    {
        
        //PoolingManager.CleanContainer(container);
        navigationTypes = new List<NavigationType>();
        foreach (Transform child in tabContainer)
        {
            //child.gameObject.SetActive(true);
            var tab = child.GetComponent<UITabBase>();
            if(tab == null) continue;
            tabs.Add(tab.type, tab);
            navigationTypes.Add(tab.type);
        }

        foreach(Transform child in container)
        {
            var item = child.GetComponent<UINavigationItem>();
            item.InitData(this);
            items.Add(item.type, item);
        }

        //Canvas.ForceUpdateCanvases();
        // DOVirtual.DelayedCall(0.25f, () =>
        // {
        //     var rect = items[NavigationType.Home].GetComponent<RectTransform>();
        //     highlight.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, rect.rect.width);
        // });

        EventBus<UpdateScreenEvent>.Raise(new (){screen = "H"});

    }

    public virtual void SetFirstTab(NavigationType tab)
    {
        
    }

    public abstract void SwitchTab(NavigationType type);
}

