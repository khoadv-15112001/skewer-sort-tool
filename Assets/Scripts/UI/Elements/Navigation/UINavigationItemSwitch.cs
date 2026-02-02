using Sonat.Enums;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UINavigationItemSwitch : MonoBehaviour
{
    public NavigationType type;

    [SerializeField] private Image background;   
    [SerializeField] private TextMeshProUGUI label;
    [SerializeField] private TextMeshProUGUI labelSelected;
    [SerializeField] private Sprite selectedSprite;
    [SerializeField] private Sprite deselectedSprite;

    private UINavigateBarSwitch navigateBar;

    public void InitData(UINavigateBarSwitch navigateSwipeBar)
    {
        this.navigateBar = navigateSwipeBar;
    }

    public void OnClickThis()
    {
        navigateBar.SwitchTab(this.type);
    }

    public void OnSelected()
    {
        if (background != null && selectedSprite != null)
            background.sprite = selectedSprite;

        if (label != null && labelSelected != null)
        {
            labelSelected.gameObject.SetActive(true);
            label.gameObject.SetActive(false);
        }
    }

    public void OnDeselected()
    {
        if (background != null && deselectedSprite != null)
            background.sprite = deselectedSprite;

        if (label != null && labelSelected != null)
        {
            labelSelected.gameObject.SetActive(false);
            label.gameObject.SetActive(true);
        }
    }
}
