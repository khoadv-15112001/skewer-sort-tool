using System.Collections;
using System.Collections.Generic;
using SonatFramework.Systems;
using SonatFramework.Systems.ObjectPooling;
using UnityEngine;

public class UIAvatarSelector : MonoBehaviour
{
    [SerializeField] private Transform container;

    private readonly Service<ProfileService> _profileService = new();
    private readonly Service<PoolingContainerService> _poolingContainerService = new();
    private List<UIAvatarItem> _avatarItems = new();
    private int _currentAvatarId;
    private PopupProfile _popupProfile;

    public int CurrentAvatarId => _currentAvatarId;

    public void Setup(PopupProfile popupProfile)
    {
        _poolingContainerService.Instance.CleanContainer(container);
        _avatarItems.Clear();
        _popupProfile = popupProfile;

        var avatars = _profileService.Instance.GetAvatars();

        for (int i = 0; i < avatars.Count; i++)
        {
            var item = _poolingContainerService.Instance.CreateObject<UIAvatarItem>(container);
            item.Setup(i, this, avatars[i].IsAvailable());
            _avatarItems.Add(item);

            //item.gameObject.SetActive(avatars[i].IsAvailable());
        }
    }

    public void SelectAvatar(int id)
    {
        _currentAvatarId = id;
        _popupProfile.UpdateAvatar(id);

        foreach (var item in _avatarItems)
        {
            item.Select(item.Id == id);
        }
    }
}
