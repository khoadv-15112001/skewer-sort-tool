using Manager;
using Sirenix.OdinInspector;
using SonatFramework.Scripts.UIModule.UIElements;
using UnityEngine;
using Cysharp.Threading.Tasks;
using UnityEngine.UI;
using SonatFramework.Systems;

public class UIAvatarItem : MonoBehaviour
{
    [SerializeField, ReadOnly] public int Id;
    [SerializeField] private FixedImageRatio avatar;
    [SerializeField] private Image frame;
    [SerializeField] private GameObject selected;
    [SerializeField] private GameObject lockObj;
    [SerializeField] private Vector2 offset;

    private UIAvatarSelector _uIAvatarSelector;
    private bool _isUnlocked;

    private void Start()
    {
        ChangeFrame(SonatSystem.GetService<ProfileService>().FrameId);
        PopupProfile.OnSelectFrame += ChangeFrame;
    }

    private void OnDestroy()
    {
        PopupProfile.OnSelectFrame -= ChangeFrame;
    }

    public void Setup(int id, UIAvatarSelector uIAvatarSelector, bool isUnlocked)
    {
        this.Id = id;
        this._uIAvatarSelector = uIAvatarSelector;
        this._isUnlocked = isUnlocked;
        avatar.SetSpriteAsync(PathManager.AvatarSprite(id)).Forget();
        lockObj.SetActive(!isUnlocked);
    }

    public void OnClick()
    {
        if (!_isUnlocked)
        {
            var data = SonatSystem.GetService<ProfileService>().GetAvatarData(Id);

            PopupProfile.OnShowBubble?.Invoke("Avatar", data.termSource, (Vector2)transform.position, offset);
            return;
        }

        _uIAvatarSelector.SelectAvatar(Id);
    }

    public void Select(bool isSelected)
    {
        selected.SetActive(isSelected);
    }

    private void ChangeFrame(int idFrame)
    {
        frame.SetSpriteAsync(PathManager.FrameSprite(idFrame)).Forget();
    }

}
