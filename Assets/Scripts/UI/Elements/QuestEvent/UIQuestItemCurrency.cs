using Manager;
using SonatFramework.Systems;
using TMPro;
using UnityEngine;
using Cysharp.Threading.Tasks;
using UnityEngine.UI;
using GrillSort.QuestEvent;

public class UIQuestItemCurrency : MonoBehaviour
{
    public Image icon;
    public TMP_Text txtAmount;

    private readonly Service<QuestEventService> questEventService = new();

    public void OnEnable()
    {
        if (questEventService.Instance.config.Active == false)
        {
            gameObject.SetActive(false);
            return;
        }

        // update visual

        var currentItemId = questEventService.Instance.GetCurrentItemId();
        if (icon)
        {
            icon.SetSpriteAsync(PathManager.ItemSprite(currentItemId)).Forget();
            icon.SetNativeSize();
        }
        var current = questEventService.Instance.NumCollectedItemInGame;
        txtAmount.text = current.ToString();

        if (current == 0) gameObject.SetActive(false);
    }

    public void OnDisable()
    {
    }
}
