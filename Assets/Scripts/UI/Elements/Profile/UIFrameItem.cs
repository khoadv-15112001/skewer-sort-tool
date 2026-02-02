using Manager;
using Sirenix.OdinInspector;
using SonatFramework.Scripts.UIModule.UIElements;
using UnityEngine;
using Cysharp.Threading.Tasks;
using SonatFramework.Systems;

public class UIFrameItem : MonoBehaviour
{
    [SerializeField, ReadOnly] public int Id;
    [SerializeField] private FixedImageRatio frame;
    [SerializeField] private GameObject selected;
    [SerializeField] private GameObject lockObj;
    [SerializeField] private Vector2 offset;

    private UIFrameSelector _uIFrameSelector;
    private bool _isUnlocked;

    public void Setup(int id, UIFrameSelector uIFrameSelector, bool isUnlocked)
    {
        this.Id = id;
        this._uIFrameSelector = uIFrameSelector;
        this._isUnlocked = isUnlocked;
        frame.SetSpriteAsync(PathManager.FrameSprite(id)).Forget();
        lockObj.SetActive(!isUnlocked);
    }

    public void OnClick()
    {
        if (!_isUnlocked)
        {
            var data = SonatSystem.GetService<ProfileService>().GetFrameData(Id);

            PopupProfile.OnShowBubble?.Invoke("Frame",data.termSource, (Vector2)transform.position, offset);
            return;
        }

        _uIFrameSelector.SelectFrame(Id);
    }

    public void Select(bool isSelected)
    {
        selected.SetActive(isSelected);
    }

}
