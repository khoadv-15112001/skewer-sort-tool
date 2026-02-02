using Cysharp.Threading.Tasks;
using SonatFramework.Systems;
using SuperScrollView;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

public class UILeaderboardTeamScrollView : MonoBehaviour
{
    [SerializeField] private LoopListView2 scroll;

    [Header("Request config")]
    [SerializeField] private LeaderboardGroup requestGroup;
    [SerializeField] private LeaderboardFilterType filterType;
    [SerializeField] private int totalRequest = 100;

    [Header("Loading...")]
    [SerializeField] private GameObject loading;

    private CancellationTokenSource cts;

    private List<LeaderboardTeamData> teams = new();

    private bool isInitListView;

    private readonly Service<LeaderboardService> leaderboardService = new();

    private void OnEnable()
    {
        cts = new();
        FetchData(cts.Token).Forget();
    }

    private void OnDisable()
    {
        if (cts != null)
        {
            cts.Cancel();
            cts.Dispose();
            cts = null;
        }
    }

    private async UniTaskVoid FetchData(CancellationToken ct)
    {
        loading.SetActive(true);

        teams.Clear();
        var response = await leaderboardService.Instance.FetchTeamsLeaderboard(requestGroup, filterType, totalRequest)
            .AttachExternalCancellation(ct);

        if (response.code != ResponseCode.SUCCESS)
        {
            PopupToast.Cretate("Loading failed! Please retry later.");
            return;
        }

        loading.SetActive(false);

        teams.AddRange(response.list);

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
    }

    private LoopListViewItem2 OnGetItemByIndex(LoopListView2 listView, int index)
    {
        if (index < 0 || index >= teams.Count)
        {
            return null;
        }

        LoopListViewItem2 item = listView.NewListViewItem("UILeaderboardTeamItem");

        var script = item.GetComponent<UILeaderboardTeamItem>();

        if (item.IsInitHandlerCalled == false)
        {
            item.IsInitHandlerCalled = true;
            script.Init();
        }

        script.Bind(teams[index], index + 1);

        return item;
    }
}
