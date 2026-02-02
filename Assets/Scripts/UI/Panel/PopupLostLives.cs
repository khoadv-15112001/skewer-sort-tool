using System;
using System.Collections.Generic;
using GrillSort.ConsecutiveWin;
using GrillSort.LavaQuest;
using GrillSort.Winstreak;
using I2.Loc;
using MyGame.SkewerJam.Gameplay;
using Sonat.Enums;
using SonatFramework.Scripts.UIModule;
using SonatFramework.Systems.UserData;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
public enum EConfirm
{
    none, home, replay
}
public class PopupLostLives : Panel
{
    [SerializeField] private GameObject liveObj;
    [SerializeField] private GameObject buttonHomeObj;
    [SerializeField] private GameObject buttonReplayObj;
    [SerializeField] private UIGiftBoxAnim giftBoxAnim;
    [SerializeField] private Button[] btnsDisableWhenClose;
    [SerializeField] private UIMultiEventLoseController multiEventLoseController;

    [Header("Next state")]
    [SerializeField] private UIStateChanger _uiStateChanger;
    private Action OnConfirm;

    private Queue<Action> confirmActions;
    [SerializeField] private GameObject missOutObj;
    [SerializeField] private Localize[] titleTexts;
    public override void OnSetup()
    {
        base.OnSetup();
    }

    public override void Open(UIData uiData)
    {
        base.Open(uiData);

        liveObj.SetActive(!MySonatFramework.livesService.isUnlimitedLive.BoolValue);

        OnConfirm = uiData.Get<Action>("OnConfirm");
        EConfirm type = uiData.Get<EConfirm>("ConfirmType");
        buttonHomeObj.SetActive(type == EConfirm.home);
        buttonReplayObj.SetActive(type == EConfirm.replay);

        var title = type == EConfirm.home ? "Quit Level" : "Replay";

        SetTitleTerm($"{title}");

        //starText.text = GamePlayController.Instance.starCount.ToString();
        missOutObj.SetActive(false);

        multiEventLoseController.Setup();

        CreateActions();

        if (MySonatFramework.livesService.isUnlimitedLive.BoolValue)
        {
            OnConfirmClick();
        }
        
        // Ensure game is paused when this popup is open
        GameplayController.instance.gameState = GameState.Paused;
    }

    public override void OnOpenCompleted()
    {
        base.OnOpenCompleted();
        
        // Double check game is paused after open animation completes
        if (GameplayController.instance != null && GameplayController.instance.gameState != GameState.Paused)
        {
            GameplayController.instance.gameState = GameState.Paused;
        }
    }

    public override void Close()
    {
        base.Close();
    }

    protected override void OnCloseCompleted()
    {
        base.OnCloseCompleted();
    }

    public bool CheckConsecutiveWin()
    {
        var service = MySonatFramework.GetService<ConsecutiveWinService>();
        var level = MySonatFramework.userDataService.GetLevel();
        //return service.CheckStart(level) ;
        return service.CheckStart(level) && service.GetConsecutiveWins() > 0;
    }
    public bool CheckWinstreakLives()
    {
        var service = MySonatFramework.GetService<WinStreakManager>();
        return service.CanReduceLives();
    }
    public bool CheckWinstreak()
    {
        var service = MySonatFramework.GetService<WinStreakManager>();
        return !service.CanReduceLives() && service.winningInARow.Value > 0;
    }
    private void SetTitleTerm(string term)
    {
        var level = MySonatFramework.GetService<UserDataService>().GetLevel();
        var levelDifficulty = MySonatFramework.GetLevelDifficulty(level);
        //titleTexts[((int)levelDifficulty)].SetTerm(term);

        var termData = LocalizationManager.GetTermData(term);
        if (termData != null)
            titleTexts[((int)levelDifficulty)].SetTerm(term);
        else
            titleTexts[((int)levelDifficulty)].GetComponent<TextMeshProUGUI>().text = term;
    }
    private void CreateActions()
    {
        confirmActions = new Queue<Action>();

        if (multiEventLoseController.IsAvailable())
        {
            confirmActions.Enqueue(() =>
            {
                _uiStateChanger.SetState(2);
            });
        }
        else
        {
            if (CheckConsecutiveWin())
                confirmActions.Enqueue(() =>
                {
                    _uiStateChanger.SetState(1);
                });
        }

        //if (CheckWinstreakLives())
        //    confirmActions.Enqueue(() =>
        //    {
        //        _uiStateChanger.SetState(2);
        //        SetTitleTerm("Keep your Win Streak!");
        //    });

        //else if (CheckWinstreak())
        //    confirmActions.Enqueue(() =>
        //    {
        //        _uiStateChanger.SetState(3);
        //        SetTitleTerm("Keep your Win Streak!");
        //    });

        //if (LavaQuestService.Instance.isJoined.BoolValue && LavaQuestService.IsClassicMode())
        //{
        //    confirmActions.Enqueue(() =>
        //    {
        //        _uiStateChanger.SetState(4);
        //        SetTitleTerm("Keep your Treasure Quest!");
        //    });
        //}

        confirmActions.Enqueue(OnForceConfirmClick);
    }

    public void OnConfirmClick()
    {
        if (!missOutObj.activeSelf)
            missOutObj.SetActive(true);

        confirmActions.Dequeue()?.Invoke();
    }

    private void OnForceConfirmClick()
    {
        foreach (var btn in btnsDisableWhenClose)
        {
            btn.interactable = false;
        }
        giftBoxAnim.Close(() =>
        {
            if (LavaQuestService.Instance.isJoined.BoolValue && LavaQuestService.IsClassicMode())
            {
                var uiData = new UIData();
                uiData.Add("eState", PopupLavaQuest.EState.Failed);

                PopupLavaQuest.OnCloseFailed += () =>
                {
                    CloseImmediately();
                    OnConfirm?.Invoke();
                };

                PanelManager.Instance.OpenPanel<PopupLavaQuest>(uiData);

                return;
            }

            CloseImmediately();
            OnConfirm?.Invoke();
        });
    }

    public void OnCancelClick()
    {
        foreach (var btn in btnsDisableWhenClose)
        {
            btn.interactable = false;
        }
        giftBoxAnim.Close(() =>
        {
            Close();
        });
    }
}