using MyGame.Modules.CardCollection;
using Sirenix.OdinInspector;
using SonatFramework.Systems;
using TMPro;
using UnityEngine;

public class CardNotificationBadge : MonoBehaviour
{
    private readonly Service<CardCollectionService> _cardCollectionService = new();
    [SerializeField] private bool isAllAlbum = true;
    [SerializeField, ReadOnly, ShowIf("@!isAllAlbum")] private AlbumType albumType;
    [SerializeField] private TMP_Text txtCount;

    private void Awake()
    {
        _cardCollectionService.Instance.OnNewCardCountChanged += UpdateData;
        UpdateData();
    }

    private void OnDestroy()
    {
        _cardCollectionService.Instance.OnNewCardCountChanged -= UpdateData;
    }

    public void SetData(AlbumType albumType)
    {
        this.albumType = albumType;
        UpdateData();
    }

    public void UpdateData()
    {
        if (isAllAlbum)
        {
            var numCard = _cardCollectionService.Instance.GetNewCardCount();
            txtCount.text = numCard.ToString();
            gameObject.SetActive(numCard > 0);
        }
        else
        {
            var numCard = _cardCollectionService.Instance.GetNewCardCount(albumType);
            txtCount.text = numCard.ToString();
            gameObject.SetActive(numCard > 0);
        }
    }
}
