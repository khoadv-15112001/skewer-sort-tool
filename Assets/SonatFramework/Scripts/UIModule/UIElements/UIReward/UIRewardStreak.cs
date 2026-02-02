using Sonat.Enums;
using SonatFramework.Scripts.UIModule.SpriteService;
using TMPro;
using UnityEngine.UI;
using UnityEngine;
using SonatFramework.Scripts.UIModule.UIElements;
using SonatFramework.Systems;

public class UIRewardStreak : MonoBehaviour
{
    public Image redRibbon;
    public Image blueRibbon;
    public Image iconReward;
    public TMP_Text amountTmp;

    public void Setup(GameResource resource, int amount)
    {
        redRibbon.gameObject.SetActive(false);
        blueRibbon.gameObject.SetActive(false);
        SetSprite(resource);
        SetAmount(resource, amount);
    }

    private void SetAmount(GameResource resource, int amount)
    {
        switch (resource)
        {
            case GameResource.Coin:
                amountTmp.text = amount.ToString();
                redRibbon.gameObject.SetActive(true);
                break;
            case GameResource.Lives:
                amountTmp.text = FormatRewardSO.FormatShortTime(amount);
                blueRibbon.gameObject.SetActive(true);
                break;
            default:
                amountTmp.gameObject.SetActive(false);
                break;
        }
    }

    private void SetSprite(GameResource resource)
    {
        //iconReward.sprite = Services.SpriteAtlasService.GetSprite($"ico_{resource}");
        iconReward.sprite = Service<SpriteAtlasService>.Get().GetSprite($"ico_{resource}");
    }
}
