using Cysharp.Threading.Tasks;
using DG.Tweening;
using GrillSort.RealTime;
using I2.Loc;
using MyGame.Modules.CardCollection;
using SonatFramework.Scripts.UIModule;
using SonatFramework.Scripts.UIModule.UIElements;
using SonatFramework.Systems;
using System;
using UnityEngine;

public class UIRequestCardContainer : MonoBehaviour
{
    [SerializeField] private UITimeCounter timer;

    [SerializeField] private Localize hint;

    [SerializeField] private GameObject requestBtn;
    [SerializeField] private GameObject countdownBtn;

    private Transform scaleTrf;

    private CardType cardType;

    private bool canClickRequestCard = true;

    private readonly Service<CardCollectionService> cardService = new();
    private readonly Service<TeamService> teamService = new();

    public void Setup(CardType cardType, float delay)
    {
        this.cardType = cardType;

        CheckActiveButton(delay);
    }

    public void Deactive()
    {
        scaleTrf.DOKill();
        hint.transform.DOKill();
        gameObject.SetActive(false);
    }

    private void CheckActiveButton(float delay)
    {
        bool isSendableCard = cardService.Instance.IsSendableCard(cardType);

        if (isSendableCard)
        {
            hint.SetTerm("You can collect this card from events, rewards and team members!");

            if (cardService.Instance.CanRequestCard())
            {
                requestBtn.SetActive(true);
                countdownBtn.SetActive(false);

                scaleTrf = requestBtn.transform;
            }
            else
            {
                requestBtn.SetActive(false);
                countdownBtn.SetActive(true);

                timer.SetData(cardService.Instance.GetTimeUntilNextRequest(), () => CheckActiveButton(0));

                scaleTrf = countdownBtn.transform;
            }

            scaleTrf.DOScale(1, 0.5f).From(0).SetDelay(delay + 0.1f).SetEase(Ease.OutBack);
        }
        else
        {
            hint.SetTerm("You can collect this card from events, rewards!");

            requestBtn.SetActive(false);
            countdownBtn.SetActive(false);
        }

        hint.transform.DOScale(1, 0.5f).From(0).SetDelay(delay).SetEase(Ease.OutBack);
    }

    public void OnClickRequest()
    {
        if (!canClickRequestCard) return;

        TryRequestCard().Forget();
    }

    public void OnClickCountdown()
    {

    }

    private async UniTask TryRequestCard()
    {
        canClickRequestCard = false;

        bool isProcessing = true;

        UIData data = new();
        data.Add("condition", new Func<bool>(() => !isProcessing));
        PanelManager.Instance.OpenForget<PopupProcessing>(data);

        var response = await teamService.Instance.SendMessage(OnlineResourceType.Card, (int)cardType);

        isProcessing = false;

        if (response.code == ResponseCode.SUCCESS)
        {
            PopupToast.Cretate("Request sent!");

            cardService.Instance.HandleRequestCardSuccess();

            CheckActiveButton(0);
        }
        else
        {
            PopupToast.Cretate(response.message);
        }

        canClickRequestCard = true;
    }
}
