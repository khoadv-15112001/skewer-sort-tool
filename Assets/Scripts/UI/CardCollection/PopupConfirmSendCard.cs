using Cysharp.Threading.Tasks;
using GrillSort.OnlineService;
using I2.Loc;
using MyGame.Modules.CardCollection;
using SonatFramework.Scripts.UIModule;
using SonatFramework.Systems;
using System;
using TMPro;
using UnityEngine;

public class PopupConfirmSendCard : Panel
{
    [SerializeField] private UICard card;

    [SerializeField] private UIAvatarBase avatar;

    [SerializeField] private LocalizationParamsManager nameParam;

    private CardType cardType;
    private string userId;
    private bool fromRequest;
    private bool canClick;

    private readonly Service<CardCollectionService> cardService = new();
    private readonly Service<ProfileService> profileService = new();
    private readonly Service<HybridDataService> hybridDataService = new();

    public override void Open(UIData uiData)
    {
        base.Open(uiData);

        canClick = true;

        fromRequest = uiData.Get<bool>("fromRequest");

        BindCard();
        BindUserInfo().Forget();
    }

    private void BindCard()
    {
        cardType = uiData.Get<CardType>("cardType");
        card.Setup(cardType);
        card.SetData(1, false);
    }

    private async UniTask BindUserInfo()
    {
        if (uiData.TryGet("member", out Member member))
        {
            userId = member.id;

            avatar.Init(member.avatar);
            nameParam.SetParameterValue("VALUE", member.name);
        }
        else
        {
            userId = uiData.Get<string>("userId");

            var response = await profileService.Instance.GetUserProfile(userId);

            if (response.code != ResponseCode.SUCCESS)
            {
                Close();
                PopupToast.Cretate("Can't fetch user data");
            }
            else
            {
                avatar.Init(response.data.avatar);
                nameParam.SetParameterValue("VALUE", response.data.name);
            }
        }
    }

    public void OnClickContinue()
    {
        if (!fromRequest)
        {
            if (!canClick) return;
            canClick = false;

            TrySendCard().Forget();
        }
    }

    private async UniTask TrySendCard()
    {
        try
        {
            bool isProcessing = true;

            UIData data = new();
            data.Add("condition", new Func<bool>(() => !isProcessing));
            PanelManager.Instance.OpenForget<PopupProcessing>(data);

            var response = await hybridDataService.Instance.SendResource(new SendResourceRequest()
            {
                userId = userId,
                resource = OnlineResourceType.FormatCardResource(((int)cardType).ToString()),
                amount = 1
            });

            isProcessing = false;

            Close();

            if (response.code != ResponseCode.SUCCESS)
            {
                PopupToast.Cretate("Failed!");
            }
            else
            {
                PopupToast.Cretate("Card sent!");

                cardService.Instance.HandleSendCardSuccess(cardType);

                PanelManager.Instance.ClosePanel<PopupSendCard>();
                PanelManager.Instance.ClosePanel<PopupCard>(true);
                PanelManager.Instance.GetPanel<PopupAlbum>().Refresh();
            }
        }
        catch
        {
            Close();
            PopupToast.Cretate($"Failed!");
        }

        canClick = true;
    }
}
