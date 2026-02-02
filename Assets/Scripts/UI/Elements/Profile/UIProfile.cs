using Manager;
using SonatFramework.Scripts.UIModule.UIElements;
using SonatFramework.Systems;
using UnityEngine;
using Cysharp.Threading.Tasks;
using SonatFramework.Systems.EventBus;
using Sonat.Enums;
using System.Drawing;
using UnityEngine.UI;
using SonatFramework.Scripts.UIModule;
using Unity.VisualScripting;
using GrillSort.OnlineService;

public class UIProfile : MonoBehaviour
{
    [SerializeField] private Button btn;

    private readonly Service<ProfileService> _profileService = new();
    protected EventBinding<ProfileService.ProfileChangeEvent> profileChangeEvent;

    [SerializeField] private FixedImageRatio _currentAvatarImg;
    [SerializeField] private FixedImageRatio _currentFrameImg;

    private EventBinding<AddItemEvent> addItemEvent;

    private void OnEnable()
    {
        profileChangeEvent = new EventBinding<ProfileService.ProfileChangeEvent>(UpdateProfileUI);
        InitProfileUI();

        addItemEvent = new EventBinding<AddItemEvent>(OnAddCurrency);
    }

    private void OnDisable()
    {
        EventBus<ProfileService.ProfileChangeEvent>.Deregister(profileChangeEvent);
        EventBus<AddItemEvent>.Deregister(addItemEvent);
    }

    private void Start()
    {
        btn.onClick.AddListener(OnClickButton);
    }

    private void OnClickButton()
    {
        UIData data = new();
        data.Add("userId", SonatSystem.GetService<OnlineService>().UserID);
        PanelManager.Instance.OpenForget<PopupUserProfile>(data);
    }

    public void InitProfileUI()
    {
        var idAvatar = _profileService.Instance.AvatarId;
        var idFrame = _profileService.Instance.FrameId;

        _currentAvatarImg.SetSpriteAsync(PathManager.AvatarSprite(idAvatar)).Forget();
        _currentFrameImg.SetSpriteAsync(PathManager.FrameSprite(idFrame)).Forget();
    }

    public void UpdateProfileUI(ProfileService.ProfileChangeEvent profileChangeEvent)
    {
        _currentAvatarImg.SetSpriteAsync(PathManager.AvatarSprite(profileChangeEvent.avatarID)).Forget();
        _currentFrameImg.SetSpriteAsync(PathManager.FrameSprite(profileChangeEvent.frameID)).Forget();
    }

    protected virtual void OnAddCurrency(AddItemEvent eventData)
    {
        var type = GameResourceHelper.ResourceType(eventData.resource);

        if (type != GameResourceType.Avatar && type != GameResourceType.Badge) return;

        if (eventData.collectEffect != null)
        {
            eventData.collectEffect.Collect(eventData.resource, eventData.quantity, eventData.position, transform.transform.position, null);
        }
    }
}
