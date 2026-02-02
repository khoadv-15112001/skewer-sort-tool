using Cysharp.Threading.Tasks;
using SonatFramework.Scripts.UIModule;
using SonatFramework.Systems;
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class PopupTeamMemberAction : Panel
{
    [SerializeField] private GameObject promoteBtn;
    [SerializeField] private GameObject demoteBtn;
    [SerializeField] private GameObject kickBtn;

    [SerializeField] private RectTransform panel;

    [SerializeField] private Image bubble;

    [SerializeField] private Sprite bubbleUp;
    [SerializeField] private Sprite bubbleDown;

    [SerializeField] private VerticalLayoutGroup verticalLayoutGroup;

    private Member member;
    private Vector3 position;
    private float minY;
    private bool isReady;

    private readonly Service<TeamService> teamService = new();

    public override void OnSetup()
    {
        base.OnSetup();

        StartCoroutine(IeWait());
    }

    private IEnumerator IeWait()
    {
        isReady = false;

        yield return new WaitForEndOfFrame();

        minY = -transform.root.GetComponent<RectTransform>().sizeDelta.y / 2f + panel.sizeDelta.y;

        isReady = true;
    }

    public override void Open(UIData uiData)
    {
        base.Open(uiData);

        position = uiData.Get<Vector3>("position");

        string teamId = uiData.Get<string>("teamId");
        TeamRole role = uiData.Get<TeamRole>("role");
        TeamRole myRole = teamService.Instance.GetMyRole();
        member = uiData.Get<Member>("member");

        if (myRole == TeamRole.None || teamId != teamService.Instance.CurrentTeamData.id || myRole == TeamRole.Member)
        {
            promoteBtn.SetActive(false);
            demoteBtn.SetActive(false);
            kickBtn.SetActive(false);
        }
        else
        {
            promoteBtn.SetActive(myRole == TeamRole.Leader && role == TeamRole.Member);
            demoteBtn.SetActive(myRole == TeamRole.Leader && role == TeamRole.Co_Leader);

            if (myRole == TeamRole.Leader)
            {
                kickBtn.SetActive(true);
            }
            else if (myRole == TeamRole.Co_Leader)
            {
                kickBtn.SetActive(role == TeamRole.Member);
            }
        }

        StartCoroutine(IeCheckPanelPosition());
    }

    private IEnumerator IeCheckPanelPosition()
    {
        yield return new WaitUntil(() => isReady);
        CheckPanelPosition();
    }

    private void CheckPanelPosition()
    {
        panel.position = position;

        if (panel.anchoredPosition.y < minY)
        {
            bubble.sprite = bubbleUp;
            panel.pivot = new Vector2(0.5f, 0);
            verticalLayoutGroup.padding.top = 20;
            verticalLayoutGroup.padding.bottom = 40;
        }
        else
        {
            bubble.sprite = bubbleDown;
            panel.pivot = new Vector2(0.5f, 1);
            verticalLayoutGroup.padding.top = 40;
            verticalLayoutGroup.padding.bottom = 20;
        }

        panel.position = position;
    }

    public void ClickPromote()
    {
        TryPromote().Forget();
    }

    public void ClickDemote()
    {
        TryDemote().Forget();
    }

    public void ClickKick()
    {
        Close();

        UIData data = new();
        data.Add("member", member);
        PanelManager.Instance.OpenForget<PopupConfirmKickOut>(data);
    }

    public void ClickView()
    {
        Close();

        UIData data = new();
        data.Add("userId", member.id);
        PanelManager.Instance.OpenForget<PopupUserProfile>(data);
    }

    private async UniTaskVoid TryPromote()
    {
        bool isProcessing = true;

        UIData data = new();
        data.Add("condition", new Func<bool>(() => !isProcessing));
        PanelManager.Instance.OpenForget<PopupProcessing>(data);

        var response = await teamService.Instance.AssignCoLeader(teamService.Instance.CurrentTeamData.id, 
            new TeamUserModifyRequest()
            {
                userId = member.id,
            });

        isProcessing = false;
        Close();

        if (response.code != ResponseCode.SUCCESS)
            PopupToast.Cretate(response.message);
        else
            PanelManager.Instance.GetPanel<PopupTeamInfo>()?.ForceReload();
    }

    private async UniTaskVoid TryDemote()
    {
        bool isProcessing = true;

        UIData data = new();
        data.Add("condition", new Func<bool>(() => !isProcessing));
        PanelManager.Instance.OpenForget<PopupProcessing>(data);

        var response = await teamService.Instance.RemoveCoLeader(teamService.Instance.CurrentTeamData.id,
            new TeamUserModifyRequest()
            {
                userId = member.id,
            });

        isProcessing = false;
        Close();

        if (response.code != ResponseCode.SUCCESS)
            PopupToast.Cretate(response.message);
        else
            PanelManager.Instance.GetPanel<PopupTeamInfo>()?.ForceReload();
    }
}
