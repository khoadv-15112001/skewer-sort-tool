using Sonat.Enums;
using SonatFramework.Scripts.Feature.Shop;
using SonatFramework.Scripts.SonatSDKAdapterModule;
using SonatFramework.Scripts.UIModule;
using SonatFramework.Systems;
using SonatFramework.Systems.SceneManagement;
using SonatFramework.Systems.UserData;
using TMPro;
using UnityEngine;

public class PopupSettingsBase : Panel
{
    public GameObject homeBtn;
    public GameObject restoreBtn;
    [SerializeField] protected TMP_Text txtVersion;
    [SerializeField] protected RectTransform panel;
    [SerializeField] protected int levelCanGoHome = 0;
    [SerializeField] protected GameObject[] gameplayOnlyObjs;
    protected readonly Service<SceneService> sceneService = new();
    protected bool clicked = false;
    [SerializeField] protected float sizeFull = 1385f;
    [SerializeField] protected float sizeSmall = 1024f;
    public string currentUserId
    {
        get => PlayerPrefs.GetString("CHECK_CONNECT_SERVER_USER_ID", "");
    }

    public override void OnSetup()
    {
        base.OnSetup();
        txtVersion.text = $"ID: {currentUserId} - Version: {Application.version}";
    }

    public override void Open(UIData uiData)
    {
        base.Open(uiData);

        if (uiData == null)
        {
            bool isHome = sceneService.Instance.GetCurrentGamePlacement() == GamePlacement.Home;
            homeBtn.SetActive(!isHome && SonatSystem.GetService<UserDataService>().GetLevel() > levelCanGoHome);
            if (restoreBtn)
                restoreBtn.SetActive(true);
            foreach (GameObject gameObj in gameplayOnlyObjs)
            {
                gameObj.SetActive(!isHome);
            }

            if (panel)
                panel.sizeDelta = isHome ? new Vector2(panel.sizeDelta.x, sizeSmall) : new Vector2(panel.sizeDelta.x, sizeFull);

            return;
        }

        if (panel)
            panel.sizeDelta = new Vector2(panel.sizeDelta.x, sizeFull);
        foreach (GameObject gameObj in gameplayOnlyObjs)
        {
            gameObj.SetActive(true);
        }

        clicked = false;
    }

    public virtual void RestoreClick()
    {
        Service<ShopService>.Get().RestorePurchase((items) => { PopupToast.Cretate("Restore success!"); });
    }

    public virtual void PolicyClick()
    {
        Application.OpenURL("https://sites.google.com/view/privacy-policy-sonat-game");
    }

    public virtual void HomeClick()
    {
        if (clicked) return;
        clicked = true;
        SonatSDKAdapter.ShowInterAds("go_home", GoHome);
    }

    protected virtual void GoHome()
    {
        CloseImmediately();
        //UIData _uIData = new UIData();
        //_uIData.Add("OnConfirm", (Action)ReduceLiveAndGoHome);
        //PanelManager.Instance.OpenForget<PopupLostLives>(_uIData);
        LoadHomeScene();
    }

    public virtual void ReplayClick()
    {
        if (clicked) return;
        clicked = true;
        SonatSDKAdapter.ShowInterAds("replay", Replay);
    }

    protected virtual void Replay()
    {
        Close();
        //Replay
    }

    protected virtual void ReduceLiveAndGoHome()
    {
        //LivesManager.ReduceLive(1, "go_home");
        SonatSDKAdapter.ShowInterAds("go_home", LoadHomeScene);
    }

    protected virtual void LoadHomeScene()
    {
        //LoadHome
    }
}