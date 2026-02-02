using MyGame.Modules.CardCollection;
using SonatFramework.Systems;
using TMPro;
using SonatUI.PageSlider;
using UnityEngine;

public class UIAlbumNavigator : MonoBehaviour
{
    [SerializeField] private PageSlider pageSlider;
    [SerializeField] private TMP_Text txtAlbumIndex;
    [SerializeField] private UIDetailAlbum[] detailAlbums;
    private readonly Service<CardCollectionService> _cardCollectionService = new();

    private int maxAlbum => _cardCollectionService.Instance.config.albums.Count;
    private int currentIndex;

    void OnEnable()
    {
        pageSlider.OnPageChanged += OnPageChanged;
    }

    void OnDisable()
    {
        pageSlider.OnPageChanged -= OnPageChanged;
    }

    public void Setup(AlbumType albumType)
    {
        currentIndex = (int)albumType;
        txtAlbumIndex.text = $"{currentIndex + 1}/{maxAlbum}";

        SetupDetailAlbums();
    }

    private void SetupDetailAlbums()
    {
        var prev = (AlbumType)((currentIndex - 1 + maxAlbum) % maxAlbum);
        var next = (AlbumType)((currentIndex + 1) % maxAlbum);

        detailAlbums[0].Setup(prev);
        detailAlbums[1].Setup((AlbumType)currentIndex);
        detailAlbums[2].Setup(next);

        foreach (var detailAlbum in detailAlbums)
        {
            detailAlbum.UpdateData();
        }
    }

    public void OnPageChanged(PageContainer page)
    {
        if (page.TryGetComponent<UIDetailAlbum>(out var detailAlbum))
        {
            currentIndex = (int)detailAlbum.AlbumType;
            txtAlbumIndex.text = $"{currentIndex + 1}/{maxAlbum}";

            SetupDetailAlbums();
            pageSlider.SetPage(1);
        }
        else
        {
            //Debug.Log($"anhnt: loi page {page.name}");

        }
    }

    public void OnPrevButtonClicked()
    {
        pageSlider.ScrollToPreviousPage();
    }

    public void OnNextButtonClicked()
    {
        pageSlider.ScrollToNextPage();
    }

    public void SeeNewCard()
    {
        detailAlbums[1].SeeNewCard();
    }

    public void Refresh()
    {
        SeeNewCard();
        Setup((AlbumType)currentIndex);
    }
}