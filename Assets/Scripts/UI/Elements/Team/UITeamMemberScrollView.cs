using GrillSort.OnlineService;
using SonatFramework.Systems;
using SuperScrollView;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class UITeamMemberScrollView : MonoBehaviour
{
    [SerializeField] private LoopListView2 scroll;

    private TeamResponse responseData;
    private List<Member> members = new();

    private bool isInitListView;

    private readonly Service<OnlineService> onlineService = new();
    private readonly Service<TeamService> teamService = new();

    public void Init(TeamResponse data)
    {
        responseData = data;

        members.Clear();
        members.AddRange(responseData.members);
        members.AddRange(responseData.coLeaders);
        members.Add(responseData.leader);
        members = members.OrderByDescending(m => m.level).ToList();

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

        LoopListViewItem2 item = listView.NewListViewItem("UITeamMemberItemView");

        var script = item.GetComponent<UITeamMemberItemView>();

        if (item.IsInitHandlerCalled == false)
        {
            item.IsInitHandlerCalled = true;
            script.Init();
        }

        TeamRole role = teamService.Instance.GetRole(responseData, members[index].id);

        if (members[index].id != onlineService.Instance.UserID)
            script.Bind(members[index], index + 1, role, responseData.id);
        else
            script.BindSelf(members[index], index + 1, role, responseData.id);

        return item;
    }
}
