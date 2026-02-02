using SonatFramework.Systems;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using GrillSort.Winstreak;
using Unity.VisualScripting;

public class UIWinstreakCurrency : MonoBehaviour
{
    public Image icon;
    public GameObject iconX, iconCircle;
    public TMP_Text txtAmount, txtLives;

    private readonly Service<WinStreakManager> winstreakEventService = new();

    public void OnEnable()
    {
        Debug.Log($"anhnt: CheckWarning {winstreakEventService.Instance.CheckWarning()}");
        if (!winstreakEventService.Instance.CheckWarning())
        {
            gameObject.SetActive(false);
            return;
        }

        // update visual

        if (winstreakEventService.Instance.CanReduceLives())
        {
            //gameObject.SetActive(false);
            //return; 

            icon.sprite = winstreakEventService.Instance.config.iconLives;

            txtAmount.text = $"-1";
            txtLives.text = $"{winstreakEventService.Instance.lives.Value}";

            iconX.SetActive(false);
            iconCircle.SetActive(true);
        }
        else
        {
            int value = winstreakEventService.Instance.winningInARow.Value;

            icon.sprite = winstreakEventService.Instance.config.iconItem;

            txtAmount.text = $"{value}";

            iconX.SetActive(true);
            iconCircle.SetActive(false);

            if (value == 0) gameObject.SetActive(false);

        }
    }

    public void OnDisable()
    {
    }
}
