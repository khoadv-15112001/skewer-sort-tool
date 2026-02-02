using Sonat.Enums;
using SonatFramework.Scripts.UIModule;
using SonatFramework.Scripts.UIModule.UIElements;
using TMPro;
using UnityEngine;
using System;
using SonatFramework.Scripts.Utils;
using Spine.Unity;
using GrillSort.BattlePass;
using DG.Tweening;

public class PopupAddTime : Panel
{
    [SerializeField] private SkeletonAnimation giftBox;
    [SerializeField] private TMP_Text txt;
    [SerializeField] private float delayClose = 5f;
    [SerializeField] private float delay = .5f;
    private Action onClose;
    private void OnEnable()
    {
        giftBox.gameObject.SetActive(false);
        DOVirtual.DelayedCall(delay, () => { giftBox.gameObject.SetActive(true); });

    }
    public override void Open(UIData uiData)
    {
        base.Open(uiData);

        SonatUtils.DelayCall(delayClose, Close, this);
        var boosterType = uiData.Get<GameResource>("boosterType");
        var content = uiData.Get<string>("content");
        onClose = uiData.Get<Action>("onClose");
        giftBox.initialSkinName = uiData.Get<string>("giftBox");
        txt.gameObject.SetActive(content != null && content != "");
        txt.text = content;

    }

    public override void Close()
    {
        base.Close();
        onClose?.Invoke();
    }
}