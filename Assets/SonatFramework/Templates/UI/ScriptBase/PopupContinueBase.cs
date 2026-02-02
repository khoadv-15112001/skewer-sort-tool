using System;
using System.Runtime.Remoting.Services;
using Sirenix.OdinInspector;
using Sonat.CustomService;
using Sonat.Enums;
using SonatFramework.Scripts.Feature.Shop.UI;
using SonatFramework.Scripts.Gameplay;
using SonatFramework.Scripts.SonatSDKAdapterModule;
using SonatFramework.Scripts.UIModule;
using SonatFramework.Scripts.UIModule.UIElements;
using SonatFramework.Systems;
using SonatFramework.Systems.ConfigManagement;
using SonatFramework.Systems.InventoryManagement;
using SonatFramework.Systems.InventoryManagement.GameResources;
using SonatFramework.Systems.TrackingModule;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PopupContinueBase : Panel
{
    public class Data : UIData
    {
        public Action<string, object[]> onPlayOn;
        public Action onClose;
        public StuckType stuckType;
    }

    protected Data data;
    protected int maxReviveWithAds;
    public GameObject playOnWithAdsBtn;
    protected readonly Service<InventoryService> inventoryService = new();

    [SerializeField] protected TMP_Text txtLabel;
    [SerializeField] protected TMP_Text txtDescription;
    [SerializeField] protected FixedImageRatio icon;
    [SerializeField] protected Sprite[] iconsSprites;
    [SerializeField] protected bool showNative;
    [SerializeField] protected TMP_Text txtPlayonPrice;
    protected ResourceData playOnPrice;
    [Required] [SerializeField] protected Config<GamePlayConfig> gameConfig;

    protected bool nativeHided;

    public override void OnSetup()
    {
        base.OnSetup();
        playOnPrice = gameConfig.config.playOnPrice;
        txtPlayonPrice.text = playOnPrice.quantity.ToString();
    }

    public override void Open(UIData uiData)
    {
        base.Open(uiData);
        data = (Data)uiData;

        maxReviveWithAds = SonatSDKAdapter.GetValueByLevel("by_level_show_rwd_revive", 9999);
        int reviveWithAds = SonatSystem.GetService<GameplayAnalyticsService>().levelPlayData.reviveByRwd;
        playOnWithAdsBtn.SetActive(reviveWithAds < maxReviveWithAds);

        SetLayout();

        if (showNative)
        {
            SonatSDKAdapter.ShowNativeAds();
        }
    }

    protected virtual void SetLayout()
    {
        switch (data.stuckType)
        {
            case StuckType.OutOfTime:
                if (txtLabel) txtLabel.text = "Out Of Time";
                if (txtDescription) txtDescription.text = "Add 30s to continue";
                if (icon) icon.sprite = iconsSprites[0];
                break;
            case StuckType.OutOfMove:
                if (txtLabel) txtLabel.text = "Out Of Move";
                if (txtDescription) txtDescription.text = "Merge 3 items to continue";
                if (icon) icon.sprite = iconsSprites[1];
                break;
        }
    }

    public override void Close()
    {
        base.Close();
        if (showNative)
        {
            SonatSDKAdapter.HideNavtiveAds();
        }
    }

    public override void OnFocus()
    {
        base.OnFocus();
        if (showNative && nativeHided)
        {
            SonatSDKAdapter.ShowNativeAds();
        }
    }

    public override void OnFocusLost()
    {
        base.OnFocusLost();
        if (!showNative) return;
        nativeHided = true;
        SonatSDKAdapter.HideNavtiveAds();
    }

    public virtual void PlayOnWithCoinClick()
    {
        if (inventoryService.Instance.CanReduce(playOnPrice.resource, playOnPrice.quantity))
        {
            var log = new SpendResourceLogData()
            {
                earnType = "booster",
                earnId = "revive",
            };
            inventoryService.Instance.ReduceResource(playOnPrice.resource, playOnPrice.quantity,
                log);
            PlayOn("play_on_coin");
        }
        else
        {
            PopupToast.Cretate("Not enough coin!");
            PanelManager.Instance.OpenPanelByName<ShopPanelBase>("ShopPanel");
        }
    }

    public virtual void PlayOnWithAdsClick()
    {
        SonatSDKAdapter.ShowRewardAds(OnReviveWithAds, "booster", "revive");
    }

    protected virtual void OnReviveWithAds()
    {
        PlayOn("play_on_ads");
        SonatSystem.GetService<GameplayAnalyticsService>().levelPlayData.reviveByRwd++;
    }

    protected virtual void PlayOn(string by, object[] objectParams = null)
    {
        Close();
        data?.onPlayOn?.Invoke(by, objectParams);
    }

    public virtual void OnGiveUpClick()
    {
        Close();
        data?.onClose?.Invoke();
    }
}