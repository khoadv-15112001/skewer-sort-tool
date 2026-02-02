using Cysharp.Threading.Tasks;
using SonatFramework.Systems;
using SonatFramework.Systems.EventBus;
using UnityEngine;
using UnityEngine.UI;

public class UIHelpButton : MonoBehaviour
{
    [SerializeField] private Sprite activeSprite;
    [SerializeField] private Sprite inactiveSprite;

    [SerializeField] private GameObject activeLabel;
    [SerializeField] private GameObject inactiveLabel;

    [SerializeField] private Image bg;

    private bool isActive;
    private bool canClick;

    private string requestId;

    private readonly Service<TeamService> teamService = new();

    public void Bind(bool isActive, string requestId)
    {
        this.isActive = isActive;
        this.requestId = requestId;

        canClick = true;

        if (isActive)
        {
            bg.sprite = activeSprite;
            activeLabel.SetActive(true);
            inactiveLabel.SetActive(false);
        }
        else
        {
            bg.sprite = inactiveSprite;
            activeLabel.SetActive(false);
            inactiveLabel.SetActive(true);
        }
    }

    public void OnClick()
    {
        if (!isActive)
        {
            PopupToast.Cretate("You already helped this request");
            return;
        }

        if (!canClick) return;
        canClick = true;

        SendLives().Forget();
    }

    private async UniTaskVoid SendLives()
    {
        var response = await teamService.Instance.HelpRequest(teamService.Instance.CurrentTeamData.id, requestId);
        if (response == null)
        {
            canClick = true;
            return;
        }

        if (response.code != ResponseCode.SUCCESS)
            PopupToast.Cretate(response.message);

        canClick = true;

        Bind(false, requestId);
    }
}
