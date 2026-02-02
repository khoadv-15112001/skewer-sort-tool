
using GrillSort.PiggyBank;
using TMPro;
using UnityEngine;
using UnityEngine.U2D;
using UnityEngine.UI;

public class UIRewardSlider : MonoBehaviour
{
    [SerializeField] private Slider slider;
    [SerializeField] private Image imgReward;
    [SerializeField] private TMP_Text txtReward;
    [SerializeField] private SpriteAtlas iconSpriteAtlas;
    private string iconNameFormat = "coin_{0}";
    private int _quantity;
    private int _maxQuantity;

    public void Setup(int maxQuantity, int iconSpriteIdx)
    {
        _maxQuantity = maxQuantity;
        SetIcon(iconSpriteIdx);
    }

    public void SetData(int quantity, int point)
    {
        _quantity = quantity;
        slider.value = (float)point / _maxQuantity;
        txtReward.text = _quantity.ToString();
    }

    private void SetIcon(int lvl)
    {
        imgReward.sprite = iconSpriteAtlas.GetSprite(string.Format(iconNameFormat, lvl));
        imgReward.SetNativeSize();
    }
}
