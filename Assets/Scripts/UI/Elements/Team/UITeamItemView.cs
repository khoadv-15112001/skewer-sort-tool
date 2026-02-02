using Manager;
using SonatFramework.Scripts.UIModule;
using SonatFramework.Systems;
using SonatFramework.Systems.EventBus;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UITeamItemView : MonoBehaviour
{
    [SerializeField] private Image logo;

    [SerializeField] private TextMeshProUGUI capacityTxt;
    [SerializeField] private TextMeshProUGUI teamName;

    [SerializeField] private GameObject viewBtn;
    [SerializeField] private GameObject pendingBtn;

    private EventBinding<TeamService.RequestJoinTeamEvent> requestJoinTeamEvent;

    private TeamResponse data;

    private readonly Service<TeamService> teamService = new();

    private void OnEnable()
    {
        requestJoinTeamEvent = new EventBinding<TeamService.RequestJoinTeamEvent>(OnRequestJoinTeam);
    }

    private void OnDisable()
    {
        EventBus<TeamService.RequestJoinTeamEvent>.Deregister(requestJoinTeamEvent);
    }

    private void OnRequestJoinTeam(TeamService.RequestJoinTeamEvent eventData)
    {
        if (eventData.teamId == data.id)
        {
            pendingBtn.SetActive(true);
            viewBtn.SetActive(false);
        }    
    }

    public void Init()
    {

    }

    public void Bind(TeamResponse data)
    {
        this.data = data;

        if (int.TryParse(data.logo, out var logoId))
        {
            _ = logo.SetSpriteAsync(PathManager.TeamLogoSprite(logoId));
        }
        else
        {
            _ = logo.SetSpriteAsync(PathManager.TeamLogoSprite(1));
        }

        capacityTxt.text = $"{data.memberCount}/{data.capacity}";
        teamName.text = data.name;

        if (teamService.Instance.GetListTeamRequested().Contains(data.id))
        {
            pendingBtn.SetActive(true);
            viewBtn.SetActive(false);
        }
        else
        {
            pendingBtn.SetActive(false);
            viewBtn.SetActive(true);
        }
    }

    public void ClickView()
    {
        UIData data = new();
        data.Add("teamId", this.data.id);
        PanelManager.Instance.OpenForget<PopupTeamInfo>(data);
    }

    public void ClickPending()
    {
        PopupToast.Cretate("Join request already pending");
    }
}
