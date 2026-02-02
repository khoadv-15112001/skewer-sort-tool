using Cysharp.Threading.Tasks;
using GrillSort.OnlineService;
using GrillSort.Stats;
using I2.Loc;
using SonatFramework.Scripts.UIModule;
using SonatFramework.Systems;
using SonatFramework.Systems.EventBus;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PopupUserProfile : Panel
{
    [SerializeField] private Button editBtn;

    [SerializeField] private GameObject content;
    [SerializeField] private GameObject localizedTeamName;

    [SerializeField] private UIAvatarBase avatar;

    [SerializeField] private TextMeshProUGUI username;
    [SerializeField] private TextMeshProUGUI teamName;

    [SerializeField] private LocalizationParamsManager level;

    [SerializeField] private TMP_Text dateTxt;
    [SerializeField] private LocalizationParamsManager dateParam;

    [SerializeField] private UIStatController uiStatController;

    private readonly Service<ProfileService> profileService = new();
    private readonly Service<OnlineService> onlineService = new();

    protected EventBinding<ProfileService.ProfileChangeEvent> profileChangeEvent;

    private UserProfileResponse response;
    private bool _isSelf;

    public override void Open(UIData uiData)
    {
        base.Open(uiData);

        string userId = uiData.Get<string>("userId");
        LoadUserProfile(userId).Forget();
    }

    private async UniTaskVoid LoadUserProfile(string userId)
    {
        content.SetActive(false);

        bool isProcessing = true;

        UIData data = new();
        data.Add("condition", new Func<bool>(() => !isProcessing));
        PanelManager.Instance.OpenForget<PopupProcessing>(data);

        response = await profileService.Instance.GetUserProfile(userId);

        isProcessing = false;

        if (response.code != ResponseCode.SUCCESS || response.data == null)
        {
            Close();
            PopupToast.Cretate("Loading failed! Please retry later.");
            return;
        }

        content.SetActive(true);

        _isSelf = response.data.id == onlineService.Instance.UserID;

        if (_isSelf)
            avatar.InitSelf();
        else
            avatar.Init(response.data.avatar);

        editBtn.gameObject.SetActive(_isSelf);
        SetupName();
        level.SetParameterValue("LEVEL", $"<br>{response.data.level}");

        SetupDate();

        if (response.data.team == null)
        {
            localizedTeamName.SetActive(true);
            teamName.gameObject.SetActive(false);
        }
        else
        {
            localizedTeamName.SetActive(false);
            teamName.gameObject.SetActive(true);
            teamName.text = response.data.team.name;
        }

        SetupAction();

        if (_isSelf)
            uiStatController.BindSelf();
        else
            uiStatController.Bind(response.data.generalStats);
    }

    private void SetupAction()
    {
        if (!_isSelf) return;

        profileChangeEvent = new EventBinding<ProfileService.ProfileChangeEvent>(UpdateProfileUI);
    }

    private void SetupName()
    {
        if (!_isSelf)
            username.text = response.data.name;
        else
            username.text = MySonatFramework.GetService<ProfileService>().Name;
    }

    private void SetupDate()
    {
        string created_at = response.data.created_at;

        if (DateTime.TryParse(created_at, out DateTime parsedDate))
        {
            var txt = parsedDate.ToString("MM/yyyy");

            dateTxt.text = txt;
            dateParam.SetParameterValue("value", txt);
        }
        else
        {
            dateTxt.text = "-";
            Debug.LogWarning("Invalid date format: " + created_at);
        }
    }

    private void OnDestroy()
    {
        EventBus<ProfileService.ProfileChangeEvent>.Deregister(profileChangeEvent);
    }

    public void UpdateProfileUI(ProfileService.ProfileChangeEvent profileChangeEvent)
    {
        username.text = profileChangeEvent.name;
        avatar.Init(profileChangeEvent.frameID, profileChangeEvent.avatarID);
    }
}
