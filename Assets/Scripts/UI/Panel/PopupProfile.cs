using Manager;
using SonatFramework.Scripts.UIModule;
using SonatFramework.Scripts.UIModule.UIElements;
using SonatFramework.Systems;
using TMPro;
using UnityEngine;
using Cysharp.Threading.Tasks;
using System.Text.RegularExpressions;
using System;
using I2.Loc;

public class PopupProfile : Panel
{
    [SerializeField] private TMP_InputField _nameInput;
    [SerializeField] private UIAvatarSelector _avatarSelector;
    [SerializeField] private UIFrameSelector _frameSelector;
    [SerializeField] private UIBadgeSelector _badgeSelector;

    [SerializeField] private FixedImageRatio _currentAvatarImg;
    [SerializeField] private FixedImageRatio _currentFrameImg;
    //[SerializeField] private FixedImageRatio _currentBadgeImg;

    [SerializeField] private UIBubble bubble;
    [SerializeField] private Localize bubbleLocalize;
    [SerializeField] private LocalizationParamsManager bubbleParam;

    private readonly Service<ProfileService> _profileService = new();

    private Regex latinRegex = new Regex("[^a-zA-Z0-9]");

    public static Action<int> OnSelectFrame;
    public static Action<string, string, Vector2, Vector2> OnShowBubble;

    public override void Open(UIData uiData)
    {
        base.Open(uiData);

        _avatarSelector.Setup(this);
        _frameSelector.Setup(this);
        _badgeSelector.Setup(this);
        // SonatUtils.ExecuteNextFrame(() =>
        // {
        var idAvatar = _profileService.Instance.AvatarId;
        var idFrame = _profileService.Instance.FrameId;
        var idBadge = _profileService.Instance.BadgeId;
        _avatarSelector.SelectAvatar(idAvatar);
        _frameSelector.SelectFrame(idFrame);
        _badgeSelector.SelectBadge(idBadge);
        // });

        _nameInput.text = _profileService.Instance.Name;
        _nameInput.characterLimit = _profileService.Instance.ProfileConfig.nameLimitCharacter;
        UpdateAvatar(idAvatar);
        UpdateFrame(idFrame);
        UpdateBadge(idBadge);
    }

    public void OnClickSave()
    {
        if (string.IsNullOrEmpty(_nameInput.text))
        {
            Close();
            return;
        }

        //_profileService.Instance.SetName(_nameInput.text);
        //_profileService.Instance.SetAvatarId(_avatarSelector.CurrentAvatarId);
        _profileService.Instance.UpdateProfileInfo(_nameInput.text, _avatarSelector.CurrentAvatarId, _frameSelector.CurrentFrameId, _badgeSelector.CurrentBadgeId);
        Close();
    }

    public void UpdateAvatar(int id)
    {
        _currentAvatarImg.SetSpriteAsync(PathManager.AvatarSprite(id)).Forget();
    }

    public void UpdateFrame(int id)
    {
        _currentFrameImg.SetSpriteAsync(PathManager.FrameSprite(id)).Forget();
        OnSelectFrame?.Invoke(id);
    }

    public void UpdateBadge(int id)
    {
        //_currentBadgeImg.sprite = MySonatFramework.GetService<ProfileService>().GetSpriteBadge(id);
    }

    private void OnEnable()
    {
        _nameInput.onValueChanged.AddListener(OnValueChanged);
        bubble.Unselect();
        OnShowBubble += ShowBubble;
    }

    private void OnDisable()
    {
        _nameInput.onValueChanged.RemoveListener(OnValueChanged);
        OnShowBubble -= ShowBubble;
    }

    private void OnValueChanged(string text)
    {
        string filtered = latinRegex.Replace(text, "");
        if (filtered != text)
            _nameInput.text = filtered;
    }

    private void ShowBubble(string type, string term, Vector2 pos, Vector2 offset)
    {
        bubble.transform.position = pos;
        bubble.GetComponent<RectTransform>().anchoredPosition += offset;

        bubble.OnClick();
        bubbleLocalize.SetTerm(term);
        bubbleParam.SetParameterValue("value", LocalizationManager.GetTranslation(type).ToLower());
    }
}