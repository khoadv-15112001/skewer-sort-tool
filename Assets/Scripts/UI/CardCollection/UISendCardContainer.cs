using DG.Tweening;
using GrillSort.RealTime;
using I2.Loc;
using MyGame.Modules.CardCollection;
using SonatFramework.Scripts.UIModule;
using SonatFramework.Scripts.UIModule.UIElements;
using SonatFramework.Systems;
using TMPro;
using UnityEngine;

public class UISendCardContainer : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI limitTxt;

    [SerializeField] private UITimeCounter timer;

    [SerializeField] private GameObject sendBtn;
    [SerializeField] private GameObject noDupBtn;
    [SerializeField] private GameObject reachLimitBtn;

    private Transform scaleTrf;

    private CardType cardType;

    private readonly Service<CardCollectionService> cardService = new();
    private readonly Service<RealTimeService> timeService = new();

    public void Setup(CardType cardType, float delay)
    {
        this.cardType = cardType;

        CheckActiveButton(delay);
    }

    public void Deactive()
    {
        scaleTrf.DOKill();
        limitTxt.transform.DOKill();
        gameObject.SetActive(false);
    }

    private void CheckActiveButton(float delay)
    {
        bool isSendableCard = cardService.Instance.IsSendableCard(cardType);

        if (isSendableCard)
        {
            if (cardService.Instance.GetDuplicatedNumCard(cardType) > 0)
            {
                int sentCards = cardService.Instance.GetSentCardToday();
                int limitSend = cardService.Instance.GetSendCardDailyLimit();

                limitTxt.gameObject.SetActive(true);
                limitTxt.text = LocalizationManager.GetTranslation("Daily Limit:") + $"{limitSend - sentCards}/{limitSend}";
                limitTxt.transform.DOScale(1, 0.5f).From(0).SetDelay(delay + 0.1f).SetEase(Ease.OutBack);

                if (sentCards >= limitSend)
                {
                    reachLimitBtn.SetActive(true);
                    sendBtn.SetActive(false);

                    scaleTrf = reachLimitBtn.transform;

                    timer.SetData((long)timeService.Instance.GetRemainingTimeInDay(), null);
                }
                else
                {
                    reachLimitBtn.SetActive(false);
                    sendBtn.SetActive(true);

                    scaleTrf = sendBtn.transform;
                }

                noDupBtn.SetActive(false);
            }
            else
            {
                limitTxt.gameObject.SetActive(false);
                sendBtn.SetActive(false);
                noDupBtn.SetActive(true);
                reachLimitBtn.SetActive(false);

                scaleTrf = noDupBtn.transform;
            }
            
            scaleTrf.DOScale(1, 0.5f).From(0).SetDelay(delay).SetEase(Ease.OutBack);
        }
        else
        {
            sendBtn.SetActive(false);
            noDupBtn.SetActive(false);
            reachLimitBtn.SetActive(false);
            limitTxt.gameObject.SetActive(false);
        }
    }

    public void OnClickSend()
    {
        UIData data = new();
        data.Add("cardType", cardType);
        PanelManager.Instance.OpenForget<PopupSendCard>(data);
    }

    public void OnClickNoDuplicate()
    {
        PopupToast.Cretate("You need a duplicate to send this card!");
    }

    public void OnClickLimit()
    {
        PopupToast.Cretate("Reach limit today!");
    }
}
