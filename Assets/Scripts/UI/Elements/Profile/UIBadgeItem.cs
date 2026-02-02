using Manager;
using Sirenix.OdinInspector;
using SonatFramework.Scripts.UIModule.UIElements;
using UnityEngine;
using Cysharp.Threading.Tasks;
using SonatFramework.Systems;

public class UIBadgeItem : MonoBehaviour
{
    [SerializeField, ReadOnly] public int Id;
    [SerializeField] private FixedImageRatio frame;
    [SerializeField] private GameObject selected;
    [SerializeField] private GameObject lockObj;
    [SerializeField] private Vector2 offset;

    private UIBadgeSelector _uIBadgeSelector;
    private bool _isUnlocked;

    public void Setup(int id, UIBadgeSelector uIFrameSelector, bool isUnlocked)
    {
        this.Id = id;
        this._uIBadgeSelector = uIFrameSelector;
        this._isUnlocked = isUnlocked;
        frame.sprite = MySonatFramework.GetService<ProfileService>().GetSpriteBadge(id);
        lockObj.SetActive(!isUnlocked);
    }

    public void OnClick()
    {
        if (!_isUnlocked)
        {
            var data = SonatSystem.GetService<ProfileService>().GetBadgeData(Id);

            PopupProfile.OnShowBubble?.Invoke("Badge", data.termSource, (Vector2)transform.position, offset);
            return;
        }

        _uIBadgeSelector.SelectBadge(Id);
    }

    public void Select(bool isSelected)
    {
        selected.SetActive(isSelected);
    }

}
