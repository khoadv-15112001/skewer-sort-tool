using Cysharp.Threading.Tasks;
using SonatFramework.Systems;
using SuperScrollView;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

public class UITeamListScrollView : MonoBehaviour
{
    [SerializeField] private LoopListView2 scroll;

    private List<TeamResponse> teams = new();

    private GetListTeamsRequest request;

    private CancellationTokenSource cts;

    private ScrollViewState state;

    private string nextCursor;
    private bool isInitListView;

    private readonly Service<TeamService> teamService = new();

    public void LoadScroll(GetListTeamsRequest request)
    {
        if (state != ScrollViewState.idle) return;
        state = ScrollViewState.fetching;

        this.request = request;

        cts = new();
        FetchData(request, cts.Token).Forget();
    }

    private void OnDisable()
    {
        if (cts != null)
        {
            cts.Cancel();
            cts.Dispose();
            cts = null;
        }

        state = ScrollViewState.idle;
    }

    private async UniTaskVoid FetchData(GetListTeamsRequest request, CancellationToken ct)
    {
        teams.Clear();

        var response = await teamService.Instance.GetListTeams(request).AttachExternalCancellation(ct);

        if (response == null || response.code != ResponseCode.SUCCESS)
        {
            state = ScrollViewState.idle;
            return;
        }

        nextCursor = response.pageInfo.nextCursor;

        teams.AddRange(response.data);

        if (!isInitListView)
        {
            isInitListView = true;

            scroll.InitListView(teams.Count, OnGetItemByIndex);
        }
        else
        {
            scroll.SetListItemCount(teams.Count);
            scroll.RefreshAllShownItem();
        }

        state = ScrollViewState.idle;
    }

    private LoopListViewItem2 OnGetItemByIndex(LoopListView2 listView, int index)
    {
        if (index < 0 || index >= teams.Count)
        {
            return null;
        }

        if (index == teams.Count - 1)
            TryLoadMore().Forget();

        LoopListViewItem2 item = listView.NewListViewItem("UITeamItemView");

        var script = item.GetComponent<UITeamItemView>();

        if (item.IsInitHandlerCalled == false)
        {
            item.IsInitHandlerCalled = true;
            script.Init();
        }

        script.Bind(teams[index]);

        return item;
    }

    private async UniTaskVoid TryLoadMore()
    {
        if (state != ScrollViewState.idle || string.IsNullOrEmpty(nextCursor)) return;

        state = ScrollViewState.loading_more;

        var request = new GetListTeamsRequest()
        {
            limit = this.request.limit,
            order = this.request.order,
            search = this.request.search,
            cursor = nextCursor,
        };

        var newData = await teamService.Instance.GetListTeams(request).AttachExternalCancellation(cts.Token);

        if (newData == null || newData.code != ResponseCode.SUCCESS)
        {
            state = ScrollViewState.idle;
            return;
        }

        teams.AddRange(newData.data);

        scroll.SetListItemCount(teams.Count, false);
        scroll.RefreshAllShownItem();

        state = ScrollViewState.idle;
    }
}
