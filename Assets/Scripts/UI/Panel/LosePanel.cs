using System;
using Manager;
using Sonat.Enums;
using SonatFramework.Scripts.SonatSDKAdapterModule;
using SonatFramework.Scripts.UIModule;
using SonatFramework.Scripts.Utils;
using SonatFramework.Systems.SceneManagement;

public class LosePanel : LosePanelBase
{
    private int level;

    public override void Open(UIData uiData)
    {
        base.Open(uiData);
        level = MySonatFramework.userDataService.GetLevel();
    }

    public virtual void RetryClick()
    {
        if (clicked) return;
        if (level >= GameRemoteConfigValue.levelShowInterLose)
            SonatSDKAdapter.ShowInterAds("lose", CheckLive);
        else
        {
            CheckLive();
        }
    }

    private void CheckLive()
    {
        if (MySonatFramework.livesService.isUnlimitedLive.BoolValue || MySonatFramework.livesService.CanPlay())
        {
            Retry();
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
            Retry();
        }
        else
        {
            GoHome();
        }
    }

    protected virtual void Retry()
    {
        clicked = true;
        Close();
        data?.onRetryClick?.Invoke();
    }

    private void GoHome()
    {
        if (level >= GameRemoteConfigValue.levelShowInterLose)
            SonatSDKAdapter.ShowInterAds("lose", LoadHomeScene);
        else
        {
            LoadHomeScene();
        }
    }

    private void LoadHomeScene()
    {
        CloseImmediately();
        LoadingScreenInstance.Instance.Show(1.5f);
        SonatUtils.DelayCall(0.5f, () => { MySonatFramework.GetService<SceneService>().SwitchScene(GamePlacement.Home); });
    }
}