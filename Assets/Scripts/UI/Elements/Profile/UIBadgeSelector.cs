using System.Collections;
using System.Collections.Generic;
using SonatFramework.Systems;
using SonatFramework.Systems.ObjectPooling;
using UnityEngine;

public class UIBadgeSelector : MonoBehaviour
{
    [SerializeField] private Transform container;

    private readonly Service<ProfileService> _profileService = new();
    private readonly Service<PoolingContainerService> _poolingContainerService = new();
    private List<UIBadgeItem> badgeItems = new();
    private int _currentBadgeId;
    private PopupProfile _popupProfile;

    public int CurrentBadgeId => _currentBadgeId;

    public void Setup(PopupProfile popupProfile)
    {
        _poolingContainerService.Instance.CleanContainer(container);
        badgeItems.Clear();
        _popupProfile = popupProfile;

        var badges = _profileService.Instance.GetBadges();

        for (int i = 0; i < badges.Count; i++)
        {
            var item = _poolingContainerService.Instance.CreateObject<UIBadgeItem>(container);
            item.Setup(i, this, badges[i].IsAvailable());
            badgeItems.Add(item);

            //item.gameObject.SetActive(badges[i].IsAvailable());
        }
    }

    public void SelectBadge(int id)
    {
        _currentBadgeId = id;
        //_popupProfile.UpdateFrame(id);

        foreach (var item in badgeItems)
        {
            item.Select(item.Id == id);
        }
    }
}
