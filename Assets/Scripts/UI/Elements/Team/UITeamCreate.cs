using Cysharp.Threading.Tasks;
using Manager;
using SonatFramework.Scripts.Feature.Shop.UI;
using SonatFramework.Scripts.UIModule;
using SonatFramework.Systems;
using SonatFramework.Systems.EventBus;
using System.Text.RegularExpressions;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UITeamCreate : MonoBehaviour
{
    [SerializeField] private Image logo;

    [SerializeField] private TextMeshProUGUI costTxt;

    [SerializeField] private TMP_InputField nameInput;
    [SerializeField] private TMP_InputField descriptionInput;

    [SerializeField] private EnumStepper<TeamType> teamTypeStepper;
    [SerializeField] private NumberStepper requiredLevelStepper;

    private EventBinding<UITeamLogoItem.ChooseTeamLogoEvent> chooseTeamLogoEvent;

    private int logoId = 1;

    private bool isCreating;

    private readonly Service<TeamService> teamService = new();

    private static readonly Regex Strip = new(@"[^\x20-\x7E]", RegexOptions.Compiled);

    private void Awake()
    {
        SetupInputField(nameInput);
        SetupInputField(descriptionInput);
    }

    private void OnEnable()
    {
        chooseTeamLogoEvent = new EventBinding<UITeamLogoItem.ChooseTeamLogoEvent>(OnChooseTeamLogo);

        costTxt.text = teamService.Instance.createCost.ToString();
    }

    private void OnDisable()
    {
        EventBus<UITeamLogoItem.ChooseTeamLogoEvent>.Deregister(chooseTeamLogoEvent);
    }

    private void OnChooseTeamLogo(UITeamLogoItem.ChooseTeamLogoEvent data)
    {
        logoId = data.id;
        _ = logo.SetSpriteAsync(PathManager.TeamLogoSprite(logoId));
    }

    public void ClickChooseLogo()
    {
        PanelManager.Instance.OpenForget<PopupChooseTeamLogo>();
    }

    public void ClickCreate()
    {
        if (nameInput.text.Length == 0)
        {
            PopupToast.Cretate("Name is required!");
            return;
        }

        if (descriptionInput.text.Length == 0)
        {
            PopupToast.Cretate("Description is required!");
            return;
        }

        if (MySonatFramework.inventoryService.CanReduce(Sonat.Enums.GameResource.Coin, teamService.Instance.createCost))
        {
            _ = TryCreateTeam();
        }
        else
        {
            PopupToast.Cretate("Not enough coin!");
            PanelManager.Instance.OpenPanelByName<ShopPanelBase>("ShopPanel");
        }
    }

    private async UniTaskVoid TryCreateTeam()
    {
        if (isCreating) return;
        isCreating = true;

        var request = new TeamRequest 
        {
            name =  nameInput.text,
            description = descriptionInput.text,
            logo = logoId.ToString(),
            type = teamTypeStepper.Current,
            requirements = new() { minLevel = requiredLevelStepper.Value }
        };

        await UniTask.WaitUntil(() => !teamService.Instance.isUpdatingCache);

        var response = await teamService.Instance.CreateTeam(request);

        if (response.code == ResponseCode.SUCCESS)
        {
            PopupToast.Cretate("Create successfully!");

            MySonatFramework.inventoryService.ReduceResource(Sonat.Enums.GameResource.Coin, teamService.Instance.createCost,
                new SonatFramework.Systems.InventoryManagement.SpendResourceLogData
                {
                    earnType = "MT",
                    earnId = "create_team",
                    source = "non_iap"
                });

            teamService.Instance.JoinTeam(response);
        }
        else
        {
            PopupToast.Cretate("Create failed!");
        }

        isCreating = false;
    }

    #region InputField
    private void SetupInputField(TMP_InputField inputField)
    {
        inputField.characterValidation = TMP_InputField.CharacterValidation.None;

        inputField.onValidateInput += ValidateChar;
        inputField.onValueChanged.AddListener((str) => Sanitize(inputField, str));
    }

    private char ValidateChar(string text, int charIndex, char addedChar)
    {
        // Cho phép ASCII in được: từ space (32) tới '~' (126)
        return (addedChar >= 32 && addedChar <= 126)
            ? addedChar
            : '\0';
    }

    private void Sanitize(TMP_InputField inputField, string value)
    {
        var cleaned = Strip.Replace(value, "");
        if (cleaned != value)
        {
            inputField.onValueChanged.RemoveListener((str) => Sanitize(inputField, str));
            inputField.text = cleaned;
            inputField.onValueChanged.AddListener((str) => Sanitize(inputField, str));
        }
    }
    #endregion
}