using GrillSort.OnlineService;
using MyGame.Modules.CardCollection;
using SonatFramework.Systems;
using SuperScrollView;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using UnityEngine;

public class UITeamMemberSendCard : MonoBehaviour
{
    [SerializeField] private LoopListView2 scroll;

    private List<Member> members = new();

    private CardType cardType;

    private bool isInitListView;

    private readonly Service<TeamService> teamService = new();
    private readonly Service<OnlineService> onlineService = new();

    public void Init(CardType cardType)
    {
        if (!teamService.Instance.IsJoinedTeam())
        {
            gameObject.SetActive(false);
            return;
        }

        this.cardType = cardType;

        TeamResponse teamData = teamService.Instance.CurrentTeamData;

        members.Clear();
        members.AddRange(teamData.members);
        members.AddRange(teamData.coLeaders);
        members.Add(teamData.leader);

        members = members.Where(m => m.id != onlineService.Instance.UserID).ToList();

        if (!isInitListView)
        {
            isInitListView = true;

            scroll.InitListView(members.Count, OnGetItemByIndex);
        }
        else
        {
            scroll.SetListItemCount(members.Count);
            scroll.RefreshAllShownItem();
        }
    }

    private LoopListViewItem2 OnGetItemByIndex(LoopListView2 listView, int index)
    {
        if (index < 0 || index >= members.Count)
        {
            return null;
        }

        LoopListViewItem2 item = listView.NewListViewItem("UISendCardItemView");

        var script = item.GetComponent<UISendCardItemView>();

        if (item.IsInitHandlerCalled == false)
        {
            item.IsInitHandlerCalled = true;
            script.Init(cardType);
        }

        script.Bind(members[index]);

        return item;
    }
}
