using SonatUI.PageSlider;
using System.Collections.Generic;
using UnityEngine;

public class UIPageSlideShop : MonoBehaviour
{
    public List<PageContainer> pageContainers;
    public List<PageDot> pageDots;
    public PageSlider pageSlider;
    public PageDotsIndicator pageDot;
    //private void Start()
    //{
    //    //pageContainers = GetComponentsInChildren<PageContainer>();
    //    //pageDots = GetComponentsInChildren<PageDot>();
    //    Invoke(nameof(UpdatePageSlide), 0.1f);
    //    //UpdatePageSlide();
    //}
    private void OnEnable()
    {
        Invoke(nameof(UpdatePageSlide), 0.2f);
    }
    private void UpdatePageSlide()
    {
        if (!gameObject.activeSelf)
            return;
        if(pageSlider == null || pageDot == null || pageContainers.Count == 0)
        {
            gameObject.SetActive(false);
            return;
        }

        if (pageDots != null && pageDots.Count > 0)
        {
            var dots = pageDots.FindAll(x => x != null && x.gameObject.activeSelf);
            if (dots.Count == 1)
            {
                foreach (var item in dots)
                {
                    item.gameObject.SetActive(false);
                }
            }
        }
    }

    public void DisAblePage(PageContainer page)
    {
        if (pageSlider != null)
        {
            page.gameObject.SetActive(false);
            pageContainers.Remove(page);
            pageSlider.RemovePage(page);
        }
        UpdatePageSlide();
    }    
    public void DisAbleDot(PageDot dot)
    {
        if (pageDot != null)
        {
            dot.gameObject.SetActive(false);
            pageDots.Remove(dot);
            pageDot.RemoveDot(dot);
        }
        UpdatePageSlide();
    }
}
