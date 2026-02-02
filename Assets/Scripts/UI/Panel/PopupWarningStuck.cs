using DG.Tweening;
using Gameplay;
using Manager;
using MyGame.SkewerJam.Gameplay;
using Sonat;
using Sonat.Enums;
using SonatFramework.Scripts.Helper;
using SonatFramework.Scripts.UIModule;
using SonatFramework.Scripts.UIModule.UIElements;
using SonatFramework.Scripts.Utils;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PopupWarningStuck : Panel
{
    [SerializeField] private float delayClose = 0.5f;
    [SerializeField] private TMP_Text txtLabel;
    [SerializeField] private TMP_Text txtDescription;
    [SerializeField] private FixedImageRatio icon;
    [SerializeField] private Sprite[] iconsSprites;
    [SerializeField] private Button btnClose;
    [SerializeField] private Transform panel;
    [SerializeField] private UILevel uiLevel;

    [Header("Animation")]
    [SerializeField] private Transform startPos;
    [SerializeField] private Transform holdToViewPos;
    [SerializeField] private Transform endPos;
    [SerializeField] private float duration = 0.5f;
    [SerializeField] private Image imgIcon;
    [SerializeField] private Ease ease;


    private PopupContinueBase.Data data;
    private bool isClosed = false;
    private string popupContinueName;
    private bool isUseUILevel;
    private bool showPopupContinue = true;

    public override void Open(UIData uiData)
    {
        base.Open(uiData);
        data = (PopupContinueBase.Data)uiData;

        if (uiData.TryGet<string>("PopupContinueName", out popupContinueName))
        {

        }
        else
        {
            popupContinueName = "PopupContinue";
        }

        if (uiData.TryGet<bool>("UseUILevel", out isUseUILevel))
        {
            uiLevel.gameObject.SetActive(isUseUILevel);
        }

        if (uiData.TryGet<bool>("ShowPopupContinue", out showPopupContinue))
        {
        }
        else
        {
            showPopupContinue = true;
        }

        panel.position = startPos.position;
        SetLayout();

        btnClose.gameObject.SetActive(false);
        MySonatFramework.audioService.PlaySound(AudioId.Lose_HLW_Panel_Outofmove_Appear_Grill_sort);
        panel.DOMove(holdToViewPos.position, duration).SetEase(ease).OnComplete(() =>
        {
            btnClose.gameObject.SetActive(true);
        });

        isClosed = false;
        SonatUtils.DelayCall(delayClose, Close, this);
    }

    private void SetLayout()
    {
        switch (data.stuckType)
        {
            case StuckType.OutOfTime:
                if (txtLabel) txtLabel.SetLocalize("Out Of Time");
                if (txtDescription)
                {
                    txtDescription.SetLocalize("Add 30s to continue");
                    txtDescription.SetLocalizeParam("VALUE", GameRemoteConfigValue.timeRevive.ToString());
                }
                if (icon) icon.sprite = iconsSprites[0];
                break;
            case StuckType.OutOfMove:
                if (txtLabel) txtLabel.SetLocalize("Out Of Move");
                if (txtDescription) txtDescription.SetLocalize("Merge 3 items to continue");
                if (icon) icon.sprite = iconsSprites[1];
                break;
            case StuckType.OutOfTimeShipper:
                if (txtLabel) txtLabel.SetLocalize("Order Out Of Time");
                if (txtDescription) txtDescription.SetLocalize("Skip the order to continue");
                if (icon) icon.sprite = iconsSprites[0];
                break;
            case StuckType.SkewerJam_OutOfSpace:
                if (txtLabel) txtLabel.SetLocalize("Out Of Space!");
                if (txtDescription) txtDescription.SetLocalize("Continue to continue");
                if (icon) icon.sprite = iconsSprites[2];
                break;
            case StuckType.SkewerJam_OutOfEnergy:
                if (txtLabel) txtLabel.SetLocalize("Out Of Energy!");
                if (txtDescription) txtDescription.SetLocalize("Continue to continue");
                if (icon) icon.sprite = iconsSprites[3];
                break;
        }
    }

    public override void Close()
    {
        if (isClosed) return;
        isClosed = true;
        imgIcon.transform.DOScale(0, duration).SetEase(Ease.InSine);
        MySonatFramework.audioService.PlaySound(AudioId.Lose_HLW_Panel_Outofmove_Out_Grill_sort);
        panel.transform.DOMove(endPos.position, duration).SetEase(ease).OnComplete(() =>
        {
            base.Close();
            if (showPopupContinue)
            {
                PanelManager.Instance.OpenPanelByName<PopupContinueBase>(popupContinueName, data);
            }
            else{
                data.onClose?.Invoke();
            }
        });
    }
}
