using System;
using GrillSort.ConsecutiveWin;
using GrillSort.KitchenMission;
using GrillSort.PreBooster;
using Sonat.Enums;
using SonatFramework.Scripts.Helper;
using SonatFramework.Scripts.UIModule;
using SonatFramework.Scripts.Utils;
using SonatFramework.Systems.SceneManagement;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
public class PopupPrePlay : Panel
{
    [SerializeField] private Button btnPlay;
    [SerializeField] private TMP_Text txtPlay;
    [SerializeField] private Button closeButton;
    [SerializeField] private UIGiftBoxAnim giftBoxAnim;

    [SerializeField] private RectTransform panel;
    [SerializeField] private UIKitchenMissionRaceBanner kitchenRace;
    public static event Action<bool> OnShowPopup;
    private bool clicked = false;
    private Action OnPlayClick;
    private Action OnCloseClick;

    public override void Open(UIData uiData)
    {
        base.Open(uiData);
        if (uiData != null)
        {
            if (uiData.TryGet<string>("txtPlay", out var txtPlay) && !string.IsNullOrEmpty(txtPlay))
            {
                this.txtPlay.SetLocalize(txtPlay);
            }
            if (uiData.TryGet<bool>("HideClose", out var hideClose) && hideClose)
            {
                closeButton.gameObject.SetActive(false);
            }
            else
            {
                closeButton.gameObject.SetActive(true);
            }

            if (!uiData.TryGet("OnPlay", out OnPlayClick))
            {
                OnPlayClick = null;
            }

            if (!uiData.TryGet<Action>("OnClose", out OnCloseClick))
            {
                OnCloseClick = null;
            }
        }


        UIPreBoosterManager.usedBoosters.Clear();
        UICurrencyManager.Instance.ShowPreBoosterCurrency(false);
        clicked = false;

        CheckKitchenRace();

        OnShowPopup?.Invoke(true);
    }

    protected override void OnCloseCompleted()
    {
        base.OnCloseCompleted();
        UICurrencyManager.Instance.ShowPreBoosterCurrency(true);
        OnShowPopup?.Invoke(false);
    }

    private void CheckKitchenRace()
    {
        panel.localPosition = new Vector2(0, kitchenRace.IsActive() ? 180f : 50f);
    }

    // private void TryGoHome()
    // {
    //     if (MySonatFramework.GetService<SceneService>().GetCurrentGamePlacement() == GamePlacement.Gameplay)
    //     {
    //         LoadingScreenInstance.Instance.Show(1.5f);
    //         SonatUtils.DelayCall(0.5f, () =>
    //         {
    //             MySonatFramework.GetService<SceneService>().SwitchScene(GamePlacement.Home);
    //         });
    //     }
    // }

    public void PlayClick()
    {
        // giftBoxAnim.Close(() =>
        // {
        Close();
        OnPlayClick?.Invoke();
        // });

    }

    public void OnClickClose()
    {
        if (clicked) return;
        clicked = true;
        btnPlay.interactable = false;

        // giftBoxAnim.Close(() =>
        // {
        Close();
        OnCloseClick?.Invoke();
        // });
    }
}