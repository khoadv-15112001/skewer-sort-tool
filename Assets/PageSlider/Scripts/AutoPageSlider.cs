using SonatUI.PageSlider;
using System.Collections;
using UnityEngine;

public class AutoPageSlider : MonoBehaviour
{
    [SerializeField] PageSlider pageSlider;
    [SerializeField] float interval = 2f;
    private int maxPage;
    Coroutine sliderCoroutine;

    private void OnEnable()
    {
        sliderCoroutine = StartCoroutine(RunPageSlider());
    }

    private void OnDisable()
    {
        StopSlideCoroutine();
    }
    private void Start()
    {
        LoadMaxPage();
    }
    private void LoadMaxPage()
    {
        maxPage = pageSlider.Pages.FindAll(x=> x.gameObject.activeSelf).Count;
    }
    private IEnumerator RunPageSlider()
    {
        while (true)
        {
            yield return new WaitForSeconds(interval);
            
            LoadMaxPage();

            int current = pageSlider.CurrentPage;

            if (current >= maxPage - 1)
                pageSlider.ScrollToPage(0);
            else
                pageSlider.ScrollToNextPage();
        }
    }

    public void OnBeginDragSlider()
    {
        StopSlideCoroutine();
    }

    public void OnEndDragSlider()
    {
        StopSlideCoroutine();
        sliderCoroutine = StartCoroutine(RunPageSlider());
    }

    private void StopSlideCoroutine()
    {
        if (sliderCoroutine != null)
        {
            StopCoroutine(sliderCoroutine);
            sliderCoroutine = null;
        }
    }
}
