using System;
using Cysharp.Threading.Tasks;
using GrillSort.ConsecutiveWin;
using Manager;
using MyGame.SkewerJam.Gameplay;
using Sonat.Enums;
using SonatFramework.Scripts.SonatSDKAdapterModule;
using SonatFramework.Scripts.UIModule;
using SonatFramework.Scripts.Utils;
using SonatFramework.Systems.EventBus;
using SonatFramework.Systems.SceneManagement;
using UnityEngine;

public class PopupSettings : PopupSettingsBase
{
    public void ResetClick()
    {
        clicked = false;
    }

    public override void ReplayClick()
    {
        if (clicked) return;

        // if (MySonatFramework.livesService.isUnlimitedLive.BoolValue)
        // {
        //     clicked = true;
        //     SonatSDKAdapter.ShowInterAds("replay", Replay);
        // }
        // else 
        if (MySonatFramework.livesService.CanPlay())
        {
            // Check xem có cần show popup không
            bool isUnlimitedLive = MySonatFramework.livesService.isUnlimitedLive.BoolValue;
            bool hasNoEvents = !CheckHasAnyEvents();
            
            // Nếu unlimited lives và không có event gì, chạy luôn không cần popup
            if (isUnlimitedLive && hasNoEvents)
            {
                clicked = true;
                var level = MySonatFramework.userDataService.GetLevel();
                if (level >= GameRemoteConfigValue.levelShowInterReplay)
                    SonatSDKAdapter.ShowInterAds("replay", CheckCanReplay);
                else
                {
                    CheckCanReplay();
                }
                return;
            }
            
            UIData data = new UIData();
            data.Add("OnConfirm", (Action)ConfirmReplay);
            data.Add("ConfirmType", EConfirm.replay);
            PanelManager.Instance.OpenPanel<PopupLostLives>(data);
        }
        else
        {
            PanelManager.Instance.OpenPanel<PopupRefillLives>();
        }
    }

    protected override void Replay()
    {
        // Close();
        GameplayController.instance.Replay();
    }

    private void ConfirmReplay()
    {
        clicked = true;
        var level = MySonatFramework.userDataService.GetLevel();
        if (level >= GameRemoteConfigValue.levelShowInterReplay)
            SonatSDKAdapter.ShowInterAds("replay", CheckCanReplay);
        else
        {
            CheckCanReplay();
        }
    }

    private bool CheckHasAnyEvents()
    {
        // Check các conditions giống như trong PopupLostLives.CreateActions()
        var multiEventLoseController = FindObjectOfType<UIMultiEventLoseController>();
        if (multiEventLoseController != null && multiEventLoseController.IsAvailable())
            return true;
        
        var consecutiveWinService = MySonatFramework.GetService<ConsecutiveWinService>();
        var level = MySonatFramework.userDataService.GetLevel();
        if (consecutiveWinService.CheckStart(level) && consecutiveWinService.GetConsecutiveWins() > 0)
            return true;
        
        return false;
    }

    private void CheckCanReplay()
    {
        if (MySonatFramework.livesService.CanPlay())
        {
            Replay();
            MySonatFramework.livesService.ReduceLive(1, new() { earnType = "lose", earnId = "replay" });
            //MySonatFramework.livesService.ReduceLive(1, "replay");
        }
        else
        {
            PanelManager.Instance.OpenPanel<PopupRefillLives>(new UIData().Add(UIDataKey.CallBackOnClose, (Action)(AfterRefillLive)));
            PopupToast.Cretate("No more lives left!");
        }
    }

    private void AfterRefillLive()
    {
        if (MySonatFramework.livesService.CanPlay())
        {
            Replay();
        }
        else
        {
            GoHome();
        }
    }

    public override void HomeClick()
    {
        if (clicked) return;

        // if (MySonatFramework.livesService.isUnlimitedLive.BoolValue)
        // {
        //     clicked = true;
        //     SonatSDKAdapter.ShowInterAds("BackHome", GoHome);
        // }
        // else
        // {
        
        // Check xem có cần show popup không
        bool isUnlimitedLive = MySonatFramework.livesService.isUnlimitedLive.BoolValue;
        bool hasNoEvents = !CheckHasAnyEvents();
        
        // Nếu unlimited lives và không có event gì, chạy luôn không cần popup
        if (isUnlimitedLive && hasNoEvents)
        {
            ConfirmGoHome();
            return;
        }
        
        UIData data = new UIData();
        data.Add("OnConfirm", (Action)ConfirmGoHome);
        data.Add("ConfirmType", EConfirm.home);
        PanelManager.Instance.OpenPanel<PopupLostLives>(data);
        // }
    }

    private void ConfirmGoHome()
    {
        clicked = true;
        var level = MySonatFramework.userDataService.GetLevel();
        string cause = GameplayController.instance.levelGenerator.CheckOutOfMove() ? "back_home_ out_of_move" : "back_home";
        EventBus<LevelQuitEvent>.Raise(new LevelQuitEvent() { cause = cause });
        EventBus<LevelEndedEvent>.Raise(new LevelEndedEvent() { gameMode = GameMode.Classic, level = level, success = false });
        //MySonatFramework.livesService.ReduceLive(1, "back_home");
        MySonatFramework.livesService.ReduceLive(1, new() { earnType = "lose", earnId = "back_home" });
        if (level >= GameRemoteConfigValue.levelShowInterLose)
        {
            SonatSDKAdapter.ShowInterAds("BackHome", GoHome);
        }
        else
        {
            GoHome();
        }
    }

    public void GoHome_SkewerJam()
    {
        // PanelManager.Instance.OpenPanelByName<PopupBackHome_SkewerJam>("PopupBackHome_SkewerJam", new UIData().Add("stuckType", StuckType.SkewerJam_OutOfSpace));
        SaveAndGoHome();
    }

    public async UniTask SaveAndGoHome()
    {
        PanelManager.Instance.OpenPanelByName<PopupLoading>("PopupLoading_SkewerJam", new UIData().Add("Time", 1f));
        await GameplayStateSaver.Instance.Save();
        SonatUtils.DelayCall(0.5f, () => { MySonatFramework.GetService<SceneService>().SwitchScene(GamePlacement.Home); });
    }

    protected override void LoadHomeScene()
    {
        LoadingScreenInstance.Instance.Show(1.5f);
        SonatUtils.DelayCall(0.5f, () => { MySonatFramework.GetService<SceneService>().SwitchScene(GamePlacement.Home); });
    }

    public void LanguageClick()
    {
        PanelManager.Instance.OpenPanel<PopupLanguage>();
    }

    public void RateClick()
    {
        PanelManager.Instance.OpenPanel<PopupRate>();
    }

    protected override void GoHome()
    {
        // var consecutiveWinService = MySonatFramework.GetService<ConsecutiveWinService>();
        // var level = MySonatFramework.userDataService.GetLevel();
        // // load lại prewin
        // if (consecutiveWinService.CheckStart(level))
        // {
        //     GameplayController.instance.Replay();
        // }
        // else
        // {
        LoadHomeScene();
        // }
    }

    public void OpenFanPage()
    {
        Application.OpenURL("https://www.facebook.com/people/Grill-Sorting-Food-Challenge/61579717830509/#");
    }

    public void OpenGroup()
    {
        Application.OpenURL("https://www.facebook.com/groups/1610718010312520");
    }
}