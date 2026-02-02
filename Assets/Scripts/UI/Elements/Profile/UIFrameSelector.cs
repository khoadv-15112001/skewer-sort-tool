using System.Collections;
using System.Collections.Generic;
using System.Linq;
using SonatFramework.Systems;
using SonatFramework.Systems.ObjectPooling;
using UnityEngine;

public class UIFrameSelector : MonoBehaviour
{
    [SerializeField] private Transform container;

    private readonly Service<ProfileService> _profileService = new();
    private readonly Service<PoolingContainerService> _poolingContainerService = new();
    private List<UIFrameItem> _frameItems = new();
    private int _currentFrameId;
    private PopupProfile _popupProfile;

    public int CurrentFrameId => _currentFrameId;

    public void Setup(PopupProfile popupProfile)
    {
        _poolingContainerService.Instance.CleanContainer(container);
        _frameItems.Clear();
        _popupProfile = popupProfile;

        var frames = _profileService.Instance.GetFrames();

        for (int i = 0; i < frames.Count; i++)
        {
            var item = _poolingContainerService.Instance.CreateObject<UIFrameItem>(container);
            item.Setup(i, this, frames[i].IsAvailable());
            _frameItems.Add(item);

            //item.gameObject.SetActive(frames[i].IsAvailable());
        }
    }

    public void SelectFrame(int id)
    {
        _currentFrameId = id;
        _popupProfile.UpdateFrame(id);

        foreach (var item in _frameItems)
        {
            item.Select(item.Id == id);
        }
    }
}
