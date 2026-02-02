using Cysharp.Threading.Tasks;
using Manager;
using SonatFramework.Scripts.UIModule;
using SonatFramework.Systems;
using SonatFramework.Systems.EventBus;
using System;
using System.Text.RegularExpressions;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PopupEditTeam : Panel
{
    [SerializeField] private Image logo;

    [SerializeField] private TMP_InputField nameInput;
    [SerializeField] private TMP_InputField descriptionInput;

    [SerializeField] private EnumStepper<TeamType> teamTypeStepper;
    [SerializeField] private NumberStepper requiredLevelStepper;

    private EventBinding<UITeamLogoItem.ChooseTeamLogoEvent> chooseTeamLogoEvent;

    private int logoId;

    private bool isSaving;

    private readonly Service<TeamService> teamService = new();

    private static readonly Regex Strip = new(@"[^\x20-\x7E]", RegexOptions.Compiled);

    public override void OnSetup()
    {
        base.OnSetup();

        SetupInputField(nameInput);
        SetupInputField(descriptionInput);
    }

    public override void Open(UIData uiData)
    {
        base.Open(uiData);

        chooseTeamLogoEvent = new EventBinding<UITeamLogoItem.ChooseTeamLogoEvent>(OnChooseTeamLogo);

        TeamResponse data = teamService.Instance.CurrentTeamData;
        _ = logo.SetSpriteAsync(PathManager.TeamLogoSprite(int.Parse(data.logo)));
        nameInput.text = data.name;
        descriptionInput.text = data.description;
        teamTypeStepper.Set(data.type);
        requiredLevelStepper.Value = data.requirements.minLevel;
        logoId = int.Parse(data.logo);
    }

    protected override void OnCloseCompleted()
    {
        base.OnCloseCompleted();

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

    public void ClickSave()
    {
        if (isSaving) return;
        isSaving = true;

        SaveAsync().Forget();
    }

    private async UniTaskVoid SaveAsync()
    {
        UIData data = new();
        data.Add("condition", new Func<bool>(() => isSaving == false));
        PanelManager.Instance.OpenForget<PopupProcessing>(data);

        var response = await teamService.Instance.UpdateTeam(teamService.Instance.CurrentTeamData.id, new()
        {
            name = nameInput.text,
            description = descriptionInput.text,
            logo = logoId.ToString(),
            type = teamTypeStepper.Current,
            requirements = new() { minLevel = requiredLevelStepper.Value },
        });

        PopupToast.Cretate(response.message);

        Close();
        isSaving = false;

        if (response.code == ResponseCode.SUCCESS)
        {
            teamService.Instance.UpdateTeam(response);
        }
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
