using Cysharp.Threading.Tasks;
using GrillSort.OnlineService;
using MyGame.Modules.CardCollection;
using SonatFramework.Scripts.UIModule;
using SonatFramework.Systems;
using SonatFramework.Systems.EventBus;
using SuperScrollView;
using System;
using System.Collections.Generic;
using System.Threading;
using Unity.Services.Core;
using UnityEngine;

public class UITeamMessages : MonoBehaviour
{
    [SerializeField] private LoopListView2 scroll;

    [SerializeField] private CanvasGroup canvasGroup;

    [SerializeField] private float paddingBot = 320f;
    [SerializeField] private float paddingTop = 40f;

    [SerializeField] private UITeamJoined uITeamJoined;

    private int extraCount;
    private ScrollViewState state;
    private bool isInitListView;

    private List<TeamMessageData> messages = new();

    private CancellationTokenSource cts;

    private EventBinding<TeamService.UpdateMessageEvent> updateMessageEvent;

    private readonly Service<TeamService> teamService = new();
    private readonly Service<OnlineService> onlineService = new();

    private void Awake()
    {
        extraCount = (paddingBot > 0 ? 1 : 0) + (paddingTop > 0 ? 1 : 0);
        state = ScrollViewState.idle;
    }

    private void OnEnable()
    {
        updateMessageEvent = new EventBinding<TeamService.UpdateMessageEvent>(OnUpdateMessage);

        cts = new();
        FetchData(cts.Token).Forget();
    }

    private void OnDisable()
    {
        EventBus<TeamService.UpdateMessageEvent>.Deregister(updateMessageEvent);

        if (cts != null)
        {
            cts.Cancel();
            cts.Dispose();
            cts = null;
        }

        state = ScrollViewState.idle;
    }

    private void OnUpdateMessage(TeamService.UpdateMessageEvent eventData)
    {
        if (state == ScrollViewState.fetching) return;

        //Debug.LogError("abc update message: " + Newtonsoft.Json.JsonConvert.SerializeObject(eventData));
        //if(messages == null) return;
        scroll.SetListItemCount(messages.Count + extraCount, eventData.refreshScroll);
        scroll.RefreshAllShownItem();
    }

    private async UniTaskVoid FetchData(CancellationToken ct)
    {
        state = ScrollViewState.fetching;

        UIData data = new();
        data.Add("condition", new Func<bool>(() => state == ScrollViewState.idle));
        PanelManager.Instance.OpenForget<PopupProcessing>(data);

        canvasGroup.interactable = false;
        canvasGroup.alpha = 0;

        messages.Clear();
        messages = await teamService.Instance.GetTeamChat(false).AttachExternalCancellation(ct);

        //foreach (var message in messages) 
        //    Debug.LogError("abc fetch: " + Newtonsoft.Json.JsonConvert.SerializeObject(message));

        if (!isInitListView)
        {
            isInitListView = true;

            scroll.InitListView(messages.Count + extraCount, OnGetItemByIndex);
        }
        else
        {
            scroll.SetListItemCount(messages.Count + extraCount);
            scroll.RefreshAllShownItem();
        }

        canvasGroup.interactable = true;
        canvasGroup.alpha = 1;

        state = ScrollViewState.idle;

        uITeamJoined.UpdateRequestBtn().Forget();
    }

    private LoopListViewItem2 OnGetItemByIndex(LoopListView2 listView, int index)
    {
        if (index < 0 || index >= messages.Count + extraCount)
        {
            return null;
        }

        if (index == 0 && paddingBot > 0)
        {
            LoopListViewItem2 spacer = listView.NewListViewItem("Spacer");
            spacer.GetComponent<ScrollViewSpacer>().SetHeight(paddingBot);
            return spacer;
        }

        if (index == messages.Count + extraCount - 1)
        {
            TryLoadMore().Forget();

            if (paddingTop > 0)
            {
                LoopListViewItem2 spacer = listView.NewListViewItem("Spacer");
                spacer.GetComponent<ScrollViewSpacer>().SetHeight(paddingTop);
                return spacer;
            }
        }

        var message = messages[index - 1];
        if (message == null)
        {
            return null;
        }

        LoopListViewItem2 item;

        switch (message.type)
        {
            case TeamMessageType.text:
                if (message.sender.id != onlineService.Instance.UserID)
                    item = listView.NewListViewItem("UIChatMessageItem");
                else
                    item = listView.NewListViewItem("UIMyChatMessageItem");

                var script = item.GetComponent<UIChatMessageItem>();

                if (item.IsInitHandlerCalled == false)
                {
                    item.IsInitHandlerCalled = true;
                    script.Init();
                }

                script.Bind(message);

                break;

            case TeamMessageType.help_request:
                int cardId = OnlineResourceType.ParseCardId(message.resource);
                if (cardId >= 0)
                {
                    if (message.sender.id != onlineService.Instance.UserID)
                        item = listView.NewListViewItem("UIRequestCardItem");
                    else
                        item = listView.NewListViewItem("UIMyRequestCardItem");

                    var script1 = item.GetComponent<UIRequestCardItem>();

                    if (item.IsInitHandlerCalled == false)
                    {
                        item.IsInitHandlerCalled = true;
                        script1.Init();
                    }

                    script1.Bind(message, message.id, (CardType)cardId);
                }
                else
                {
                    if (message.sender.id != onlineService.Instance.UserID)
                        item = listView.NewListViewItem("UIRequestItem");
                    else
                        item = listView.NewListViewItem("UIMyRequestItem");

                    var script1 = item.GetComponent<UIRequestItem>();

                    if (item.IsInitHandlerCalled == false)
                    {
                        item.IsInitHandlerCalled = true;
                        script1.Init();
                    }

                    script1.Bind(message, message.id);
                }
                break;

            case TeamMessageType.join_request_pending:
                item = listView.NewListViewItem("UIRequestItem");
                var script2 = item.GetComponent<UIRequestItem>();

                if (item.IsInitHandlerCalled == false)
                {
                    item.IsInitHandlerCalled = true;
                    script2.Init();
                }

                script2.Bind(message, message.id);

                break;

            case TeamMessageType.team_reward:
                item = listView.NewListViewItem("UITeamRewardItem");
                var script4 = item.GetComponent<UITeamRewardItem>();

                if (item.IsInitHandlerCalled == false)
                {
                    item.IsInitHandlerCalled = true;
                    script4.Init();
                }

                script4.Bind(message);

                break;

            default:
                item = listView.NewListViewItem("UISystemMessageItem");

                var script3 = item.GetComponent<UISystemMessageItem>();
                script3.Bind(message);

                break;
        }

        return item;
    }

    private async UniTaskVoid TryLoadMore()
    {
        if (state != ScrollViewState.idle || teamService.Instance.ChatListToken == null) return;

        if (!teamService.Instance.CanLoadMoreMessages()) return;

        state = ScrollViewState.loading_more;

        var newMessages = await teamService.Instance.GetTeamChat(true);

        state = ScrollViewState.idle;

        messages = newMessages;

        scroll.SetListItemCount(messages.Count + extraCount, false);
        scroll.RefreshAllShownItem();
    }
}