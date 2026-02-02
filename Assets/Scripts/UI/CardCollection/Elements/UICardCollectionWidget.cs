using Cysharp.Threading.Tasks;
using GrillSort.OnlineService;
using MyGame.Modules.CardCollection;
using MyGame.Modules.CardCollection.Animation;
using SonatFramework.Scripts.UIModule;
using SonatFramework.Systems;
using SonatFramework.Systems.InventoryManagement;
using SonatFramework.Systems.InventoryManagement.GameResources;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class UICardCollectionWidget : UIHomeWidget
{
    private readonly Service<CardCollectionService> _cardCollectionService = new();

    public override void Setup()
    {
        if (_cardCollectionService.Instance.IsUnlocked() == false)
        {
            if (_cardCollectionService.Instance.CanUnlock() == true)
            {
                _cardCollectionService.Instance.Unlock();
                return;
            }
        }

    }

    public override void OnFocus()
    {
        
    }

    public override void OnLoseFocus()
    {

    }

    // Chỉ hiện ở phiên đầu tiên trong ngày
    public override async UniTask<bool> ProcessTask()
    {
        if (_cardCollectionService.Instance.IsUnlocked() == false) return false;

        var open = false;
        // Hiện tut
        if (PlayerPrefs.HasKey($"{CardCollectionService.DATA_KEY}_AppearTut") == false)
        {
            PlayerPrefs.SetInt($"{CardCollectionService.DATA_KEY}_AppearTut", 1);

            UIData uiData = new();
            uiData.Add(UIDataKey.CallBackOnClose, (Action)(() =>
            {
                RewardUnlockFeature();
            }));

            _ = HomeManager.Instance.SwitchTab(Sonat.Enums.NavigationType.Collection, 0.25f);

            var popup = PanelManager.Instance.OpenPanelByName<Panel>("PopupTutCardCollection", uiData);
            await UniTask.WaitUntil(() => popup == null || popup.gameObject.activeInHierarchy == false);

            var popup2 = PanelManager.Instance.GetPanel<PopupReward>();
            await UniTask.WaitUntil(() => popup2 == null || popup2.gameObject.activeInHierarchy == false);
           
            open = true;
        }

        // Hiện popup complete album
        var openCompleteAlbum = await _cardCollectionService.Instance.RunQueueCompleteAlbum();
        open = open || openCompleteAlbum;
        return open;

    }

    private void RewardUnlockFeature()
    {
        RewardData reward = _cardCollectionService.Instance.config.RewardUnlock;
        MySonatFramework.inventoryService.AddReward(reward, new EarnResourceLogData
        {
            spendType = "card_collection",
            spendId = "card_collection",
            isFirstBuy = false,
            source = "non_iap"
        });
        UIData uiData = new UIData();
        uiData.Add("Title", "REWARD!");
        uiData.Add("Reward", reward);
        uiData.Add("x2", false);
        PanelManager.Instance.OpenPanel<PopupReward>(uiData);
    }

    public override async UniTask ProcessOnFocus()
    {
        if (!_cardCollectionService.Instance.IsUnlocked())
        {
            return;
        }

        _cardCollectionService.Instance.FetchPendingCards().Forget();

        var pendingCards = _cardCollectionService.Instance.pendingCards;
        if (pendingCards.Count > 0)
        {
            List<CardType> cards = new();
            List<UserProfile> senders = new();

            foreach (var card in pendingCards)
            {
                int id = OnlineResourceType.ParseCardId(card.key);
                if (id < 0) continue;

                try
                {
                    var success = await MySonatFramework.GetService<HybridDataService>().ClaimPendingResource(card.id);
                    if (!success) continue;

                    for (int i = 0; i < card.amountChange; i++)
                    {
                        cards.Add((CardType)id);

                        if (card.additionalInfo == null || card.additionalInfo.sender == null)
                            senders.Add(null);
                        else
                            senders.Add(card.additionalInfo.sender);
                    }
                }
                catch
                {
                    continue;
                }
            }

            while (cards.Count > 0)
            {
                int take = Math.Min(cards.Count, 6);
                UIData data = new();
                data.Add("cards", cards.Take(take).ToList());
                data.Add("senders", senders.Take(take).ToList());

                cards.RemoveRange(0, take);
                senders.RemoveRange(0, take);

                var popup = PanelManager.Instance.OpenPanel<PopupReceiveCard_Online>(data);
                if (popup != null)
                    await UniTask.WaitUntil(() => popup == null || popup.gameObject.activeInHierarchy == false);
            }
        }
    }


    // #if UNITY_EDITOR
    //     void Update()
    //     {
    //         if (Input.GetKeyDown(KeyCode.Space))
    //         {
    //             var albumType = AlbumType.Album_0;
    //             PanelManager.Instance.OpenPanel<PopupCompleteAlbum>(new UIData().Add("AlbumType", albumType));
    //         }
    //     }
    // #endif
}
