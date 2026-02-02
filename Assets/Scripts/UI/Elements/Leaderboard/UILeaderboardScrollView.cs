using Cysharp.Threading.Tasks;
using SonatFramework.Systems;
using SuperScrollView;
using System.Threading;
using UnityEngine;
using UnityEngine.UI;

public class UILeaderboardScrollView : MonoBehaviour
{
    [SerializeField] private LoopListView2 scroll;

    [SerializeField] private UILeaderboardItemView myRank;

    [SerializeField] private float paddingBot = 200;

    [Header("Request config")]
    [SerializeField] private LeaderboardGroup requestGroup;
    [SerializeField] private LeaderboardFilterType filterType;
    [SerializeField] private int totalRequest = 100;

    [Header("Loading...")]
    [SerializeField] private GameObject loading;

    private LeaderboardResponseData responseData;

    private CancellationTokenSource cts;

    private int extraCount;
    private bool isInitListView;

    private readonly Service<LeaderboardService> leaderboardService = new();

    private void Awake()
    {
        scroll.GetComponent<ScrollRect>().onValueChanged.AddListener(OnValueChanged);

        extraCount = paddingBot > 0 ? 1 : 0;
    }

    private void OnValueChanged(Vector2 value)
    {
        if (responseData == null)
        {
            myRank?.gameObject.SetActive(false);
            return;
        }

        myRank?.gameObject.SetActive(!IsIndexVisibleNow(responseData.position - 1));
    }

    private void OnEnable()
    {
        myRank?.gameObject.SetActive(false);
        loading.SetActive(true);

        cts = new();

        if (leaderboardService.Instance.IsUnlockFeature())
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
        int limitRequest;

        if (CheatManager.IsOpenCheat())
            limitRequest = LeaderboardService.limitTopLeaderboard > 0 ? LeaderboardService.limitTopLeaderboard : totalRequest;
        else
            limitRequest = totalRequest;

        responseData = await leaderboardService.Instance.FetchLeaderboard(requestGroup, filterType, limitRequest).AttachExternalCancellation(ct);
        if (responseData == null)
        {
            PopupToast.Cretate("Loading failed! Please retry later.");
            return;
        }

        if (!isInitListView)
        {
            isInitListView = true;
            scroll.InitListView(responseData.list.Count + extraCount, OnGetItemByIndex);
        }
        else
        {
            scroll.SetListItemCount(responseData.list.Count + extraCount);
            scroll.RefreshAllShownItem();
        }

        myRank?.Init();
        myRank?.BindSelf(responseData.score, responseData.position);
        myRank?.gameObject.SetActive(!IsIndexVisibleNow(responseData.position - 1));
        loading.SetActive(false);
    }

    private LoopListViewItem2 OnGetItemByIndex(LoopListView2 listView, int index)
    {
        if (index < 0 || index >= responseData.list.Count + extraCount)
        {
            return null;
        }

        if (index == responseData.list.Count - 1 + extraCount && paddingBot > 0)
        {
            LoopListViewItem2 spacer = listView.NewListViewItem("Spacer");
            spacer.GetComponent<ScrollViewSpacer>().SetHeight(paddingBot);
            return spacer;
        }

        LoopListViewItem2 item = listView.NewListViewItem("UILeaderboardItemView");

        var script = item.GetComponent<UILeaderboardItemView>();

        if (item.IsInitHandlerCalled == false)
        {
            item.IsInitHandlerCalled = true;
            script.Init();
        }

        if (index + 1 != responseData.position)
            script.Bind(responseData.list[index], index + 1);
        else
            script.BindSelf(responseData.score, index + 1);

        return item;
    }

    private bool IsIndexVisibleNow(int index) => scroll.GetShownItemByItemIndex(index) != null;
}
