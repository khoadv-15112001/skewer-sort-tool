using DG.Tweening;
using MyGame.Modules.CardCollection;
using Sonat.Enums;
using SonatFramework.Scripts.UIModule;
using SonatFramework.Scripts.Utils;
using SonatFramework.Systems;
using System;
using Unity.Services.Core;
using UnityEngine;

public class PopupCard : Panel
{
    [SerializeField] private UICard card;
    [SerializeField] private float scaleUp = 2;
    [SerializeField] private float durationScaleUp = 0.5f;
    [SerializeField] private AnimationCurve curveScaleUp;
    [SerializeField] private AnimationCurve curveMoveUp;

    [SerializeField] private float scaleDown = 1f;
    [SerializeField] private float durationScaleDown = 0.5f;
    [SerializeField] private AnimationCurve curveScaleDown;
    [SerializeField] private AnimationCurve curveMoveDown;

    private CardType cardType;
    private Vector3 startPos;
    private Vector3 startLocalPos;
    private Action onCompleteClose;

    [Space(10)]
    [Header("Send/Request Card")]
    [SerializeField] private UISendCardContainer sendContainer;
    [SerializeField] private UIRequestCardContainer requestContainer;

    private readonly Service<CardCollectionService> _cardCollectionService = new();

    public override void Open(UIData uiData)
    {
        base.Open(uiData);

        cardType = uiData.Get<CardType>("cardType");
        startPos = uiData.Get<Vector3>("position");
        onCompleteClose = uiData.Get<Action>("onCompleteClose");
        card.Setup(cardType);
        card.UpdateData();
        PlayAppearAnimation();

        _cardCollectionService.Instance.RemoveNewCard(cardType);

        CheckSendOrRequestCard();
    }

    private void PlayAppearAnimation()
    {
        card.transform.DOKill();
        card.transform.position = startPos;
        startLocalPos = card.transform.localPosition;
        card.transform.localScale = Vector3.one;
        card.transform.DOScale(scaleUp, durationScaleUp).SetEase(curveScaleUp);
        card.transform.DOLocalMove(Vector3.zero, durationScaleUp).SetEase(curveMoveUp);
    }

    private void PlayDisappearAnimation()
    {
        card.transform.DOKill();
        card.transform.DOScale(scaleDown, durationScaleDown).SetEase(curveScaleDown);
        card.transform.DOLocalMove(startLocalPos, durationScaleDown).SetEase(curveMoveDown);

        sendContainer.Deactive();
        requestContainer.Deactive();
    }

    private bool isClose = false;
    public override void Close()
    {
        if (isClose) return;
        isClose = true;

        PlayDisappearAnimation();

        SonatUtils.DelayCall(durationScaleDown, () =>
        {
            onCompleteClose?.Invoke();
            base.Close();
        });
    }

    private void CheckSendOrRequestCard()
    {
        bool isCardCollected = _cardCollectionService.Instance.IsCardCollected(cardType);

        sendContainer.gameObject.SetActive(isCardCollected);
        requestContainer.gameObject.SetActive(!isCardCollected);

        if (isCardCollected)
            sendContainer.Setup(cardType, durationScaleUp);
        else
            requestContainer.Setup(cardType, durationScaleUp);
    }
}
