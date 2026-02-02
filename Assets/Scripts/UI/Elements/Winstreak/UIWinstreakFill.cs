using DG.Tweening;
using GrillSort.Winstreak;
using UnityEngine;
using UnityEngine.UI;

public class UIWinstreakFill : MonoBehaviour
{
    [Header("Fill Amount")]
    public Image Fill;
    public Slider slide;
    [Header("Winstreak Character")]
    public Image character;
    public Sprite[] characterSprites;
    [Header("Is Winstreak Lives")]
    public bool isLives = true;

    private void OnEnable()
    {
        UpdateUI();
    }

    private void UpdateUI()
    {
        var service = MySonatFramework.GetService<WinStreakManager>();
        var value = service.lives.Value;

        if (isLives)
        {
            if (Fill)
            {
                Fill.fillAmount = (float)value / service.config.maxLives;

                Fill.DOFillAmount((float)(value - 1) / service.config.maxLives, 0.5f).SetDelay(0.5f);
            }
            if (slide)
            {
                slide.value = (float)value / service.config.maxLives;

                slide.DOValue((float)(value - 1) / service.config.maxLives, 0.5f).SetDelay(0.5f);
            }

        }
        else
        {
            if (Fill)
            {
                Fill.fillAmount = 1;

                Fill.DOFillAmount(0, 0.5f).SetDelay(1f);
            }
            if (slide)
            {
                slide.value = 1;

                slide.DOValue(0, 0.5f).SetDelay(1f);
            }

        }

        if (character)
        {

            character.sprite = characterSprites[value];

            DOVirtual.DelayedCall(0.75f, () =>
            {
                character.sprite = characterSprites[value - 1];

            });
        }
    }
}
