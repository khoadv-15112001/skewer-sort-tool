using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using Sonat.Enums;
using UnityEngine;
using UnityEngine.UI;

public class UINavigateBarSlide : UINavigateBarBase
{
    private RectTransform[] tabRectTransforms;
    [SerializeField] private float animationDuration = 0.3f;
    [SerializeField] private Ease animationEase = Ease.OutQuad;

    private Vector2[] tabOriginalPositions;
    private Vector2 centerPosition;
    private float tabWidth;
    private bool animating = false;

    private void Start()
    {
        Init();
        DOVirtual.DelayedCall(0.25f, () => { SetFirstTab(tabStart); });
    }

    public override void Init()
    {
        base.Init();
        tabRectTransforms = new RectTransform[tabs.Count];
        int m = 0;
        foreach (var tab in tabs.Values)
        {
            tabRectTransforms[m] = tab.GetComponent<RectTransform>();
            m++;
        }

        // Initialize tab positions for animations
        tabOriginalPositions = new Vector2[tabRectTransforms.Length];
        for (int i = 0; i < tabRectTransforms.Length; i++)
        {
            tabOriginalPositions[i] = tabRectTransforms[i].anchoredPosition;
        }

        // Get center position (assuming the parent canvas is aligned to center)
        if (tabRectTransforms.Length > 0)
        {
            centerPosition = new Vector2(0, tabRectTransforms[0].anchoredPosition.y);
            tabWidth = tabRectTransforms[0].rect.width;
        }
    }

    public override void SetFirstTab(NavigationType tab)
    {
        base.SetFirstTab(tab);
        foreach (var item in items)
        {
            if (item.Key == tab)
            {
                item.Value.OnSelected();
            }
            else
            {
                item.Value.OnDeselected();
            }
        }
        
        
        Canvas.ForceUpdateCanvases();
        highlight.transform.SetParent(items[tab].transform);
        highlight.SetAsFirstSibling();
        highlight.localPosition = Vector3.zero;
        var rect = items[tab].GetComponent<RectTransform>();
        highlight.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, rect.rect.width);

        tabs[tab].gameObject.SetActive(true);
        tabs[tab].OnShow();
        currTab = tab;
    }

    public void SwitchToHome(){
        SwitchTab(NavigationType.Home);
    }
    
    public override void SwitchTab(NavigationType type)
    {
        if (type == currTab || animating) return;

        animating = true;

        var lastTab = currTab;
        // Hide previous tab with animation (if any)
        if (tabs.ContainsKey(currTab))
        {
            int prevIndex = navigationTypes.IndexOf(currTab);
            if (prevIndex >= 0 && prevIndex < tabRectTransforms.Length)
            {
                // Determine the direction to animate out (opposite from where it came)
                bool animateToRight = navigationTypes.IndexOf(type) < prevIndex;
                Vector2 targetPosition = tabOriginalPositions[prevIndex];

                // If the new tab index is less than current, animate current tab to right
                // Otherwise, animate to left
                if (animateToRight)
                {
                    targetPosition = new Vector2(tabWidth, targetPosition.y);
                }
                else
                {
                    targetPosition = new Vector2(-tabWidth, targetPosition.y);
                }


                // Animate the tab out
                tabRectTransforms[prevIndex].DOAnchorPos(targetPosition, animationDuration)
                    .SetEase(animationEase)
                    .OnComplete(() => { tabs[lastTab].OnHide(); });
            }
        }


        // Update current tab reference


        // Update navigation buttons
        foreach (var item in items)
        {
            if (item.Key == type)
            {
                item.Value.OnSelected();
            }
            else
            {
                item.Value.OnDeselected();
            }
        }

        // Move highlight to selected button
        highlight.transform.SetParent(items[type].transform);
        highlight.SetAsFirstSibling();
        highlight.DOLocalMove(Vector3.zero, 0.2f);

        // Show new tab with animation
        int newIndex = navigationTypes.IndexOf(type);
        if (newIndex >= 0 && newIndex < tabRectTransforms.Length)
        {
            // Place the tab off-screen based on direction (right if index is less than previous, left otherwise)
            bool comingFromRight = newIndex > navigationTypes.IndexOf(lastTab);
            Vector2 startPosition;

            if (comingFromRight)
            {
                startPosition = new Vector2(tabWidth, tabOriginalPositions[newIndex].y);
            }
            else
            {
                startPosition = new Vector2(-tabWidth, tabOriginalPositions[newIndex].y);
            }

            tabRectTransforms[newIndex].DOKill();
            // Set initial position
            tabRectTransforms[newIndex].anchoredPosition = startPosition;
            // Make sure the tab is visible before animation
            tabs[type].gameObject.SetActive(true);
            tabs[type].OnShow();

            // Animate to center
            tabRectTransforms[newIndex].DOAnchorPos(centerPosition, animationDuration)
                .SetEase(animationEase);
        }

        DOVirtual.DelayedCall(animationDuration, () =>
        {
            currTab = type;
            animating = false;
        });
    }
}