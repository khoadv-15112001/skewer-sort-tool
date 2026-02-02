using GrillSort.LoseRwdService;
using Sonat.Data;
using Sonat.Enums;
using SonatFramework.Scripts.Feature.Shop.UI;
using SonatFramework.Scripts.SonatSDKAdapterModule;
using SonatFramework.Scripts.UIModule;
using SonatFramework.Scripts.Utils;
using SonatFramework.Systems;
using SonatFramework.Systems.EventBus;
using SonatFramework.Systems.InventoryManagement;
using SonatFramework.Systems.TrackingModule;
using SonatFramework.Templates.UI.ScriptBase;
using UnityEngine;

public class PopupBuyBooster : PopupBuyBoosterBase
{
    [SerializeField] private UIBoosterOffers uiBoosterOffers;

    [SerializeField] GameObject objBoosterFreeze;
    [SerializeField] GameObject objBoosterMagnet;
    [SerializeField] GameObject objBoosterShuffle;
    [SerializeField] GameObject objBoosterMagicKey;
    [SerializeField] GameObject objBoosterBlowTorch;

    [SerializeField] private float delaySound = 1f;

    private CheckRemoteByDayCounter checkRwdByDay;
    private CheckRemoteByLevelCounter checkRwdByLevel;

    public override void Open(UIData uiData)
    {
        base.Open(uiData);
        uiBoosterOffers.InitData(boosterConfig.booster);
        MySonatFramework.customTrackingService.OnShowPopup($"booster_offer_{boosterConfig.booster}".ToLogString(), "banner", "iap", "auto");

        //
        objBoosterFreeze.SetActive(false);
        objBoosterMagnet.SetActive(false);
        objBoosterShuffle.SetActive(false);
        objBoosterMagicKey.SetActive(false);
        objBoosterBlowTorch.SetActive(false);

        switch (boosterConfig.booster)
        {
            case GameResource.BoosterFreeze:
                objBoosterFreeze.SetActive(true);
                break;
            case GameResource.BoosterMagnet:
                objBoosterMagnet.SetActive(true);
                break;
            case GameResource.BoosterShuffle:
                objBoosterShuffle.SetActive(true);
                break;
            case GameResource.BoosterMagicKey:
                objBoosterMagicKey.SetActive(true);
                break;
            case GameResource.BoosterBlowTorch:
                objBoosterBlowTorch.SetActive(true);
                break;

        }

        checkRwdByDay = new("by_day_show_rwd_booster", boosterConfig.booster.ToString(), 999);
        checkRwdByLevel = new("by_level_show_rwd_booster", boosterConfig.booster.ToString(), 99999);

        int currentLevel = MySonatFramework.userDataService.GetLevel();
        var userCampaignSegment = UserData.UserCampaignSegment.Value;
        int condition = SonatSDKAdapter.GetValueByLevelSegment("by_level_show_rwd_booster", userCampaignSegment, currentLevel, 9999);

        Debug.Log($"anhnt: [UserCampaignSegment] by_level_show_rwd_booster = {SonatSDKAdapter.GetRemoteString("by_level_show_rwd_booster")}");
        Debug.Log($"anhnt: [UserCampaignSegment] UserCampaignSegment={userCampaignSegment} cur={checkRwdByLevel.GetCurrentValue()}/{condition}");

        if (buyWithRwdButton.activeInHierarchy)
            buyWithRwdButton.SetActive(checkRwdByDay.CheckCounter() && checkRwdByLevel.CheckCounter());
    }

    public override string GetPlacement()
    {
        string placement = "";
        switch (boosterConfig.booster)
        {
            case GameResource.BoosterMagnet:
                placement = "GP:::magnet";
                break;
            case GameResource.BoosterShuffle:
                placement = "GP:::shuffle";
                break;
            case GameResource.BoosterFreeze:
                placement = "GP:::freeze";
                break;
            default:
                placement = base.GetPlacement();
                break;
        }
        return placement;
    }
    public override void OnBuyWithAdsClick()
    {
        MySonatFramework.ShowRewardAds(OnWatchedAds, "booster", boosterConfig.booster.ToString());
    }
    protected override void OnWatchedAds()
    {
        checkRwdByDay.AddValue();
        checkRwdByLevel.AddValue();
        base.OnWatchedAds();
        SonatUtils.DelayCall(delaySound, () =>
        {
            MySonatFramework.audioService.PlaySound(AudioId.Booster_Received_Grill_sort);
        });
    }

    public override void OnBuyWithCoinClick()
    {
        if (boosterService.Instance.BuyBooster(boosterConfig.booster, boosterConfig.value))
        {
            EventBus<AddItemEvent>.Raise(new AddItemEvent()
            {
                resource = boosterConfig.booster,
                quantity = boosterConfig.value,
                position = icon.transform.position,
                collectEffect = new CollectEffectSingle()
            });

            SonatUtils.DelayCall(delaySound, () =>
            {
                MySonatFramework.audioService.PlaySound(AudioId.Booster_Received_Grill_sort);
            });
            Close();
        }
        else
        {
            PopupToast.Cretate("Not enough coin!");
            PanelManager.Instance.OpenPanelByName<ShopPanelBase>("ShopPanel");
        }
    }
}