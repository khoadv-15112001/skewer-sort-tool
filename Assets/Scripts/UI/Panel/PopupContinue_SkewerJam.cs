using Sonat.Enums;
using SonatFramework.Scripts.Feature.Shop.UI;
using SonatFramework.Scripts.Helper;
using SonatFramework.Scripts.SonatSDKAdapterModule;
using SonatFramework.Scripts.UIModule;
using SonatFramework.Systems;
using SonatFramework.Systems.InventoryManagement;
using SonatFramework.Systems.TrackingModule;
using UnityEngine;

public class PopupContinue_SkewerJam : PopupContinueBase
{
    [Header("Layout")] [SerializeField] private GameObject[] layouts;
    private CheckRemoteByLevelCounterHLW checkRwdRevive;

    public override void OnSetup()
    {
        base.OnSetup();
        checkRwdRevive = new("by_level_show_rwd_revive_pkr", 9999);
        playOnPrice.quantity = SonatSDKAdapter.GetRemoteInt("coin_revive_pkr_new", 150);
    }

    public override void Open(UIData uiData)
    {
        base.Open(uiData);
        txtPlayonPrice.text = $"{playOnPrice.quantity}";
        if (playOnWithAdsBtn.activeInHierarchy)
            playOnWithAdsBtn.SetActive(checkRwdRevive.CheckCounter());
    }

    protected override void SetLayout()
    {
        foreach (var layout in layouts)
        {
            layout.SetActive(false);
        }

        switch (data.stuckType)
        {
            case StuckType.SkewerJam_OutOfEnergy:
                if (txtLabel) txtLabel.SetLocalize("Not enough Energies!");
                if (txtDescription) txtDescription.SetLocalize("Beat Level to get Energies!");
                if (icon) icon.sprite = iconsSprites[3];
                layouts[0].SetActive(true);
                break;
            case StuckType.SkewerJam_OutOfSpace:
                if (txtLabel) txtLabel.SetLocalize("Continue?");
                if (txtDescription) txtDescription.SetLocalize("Continue with 2 more trays!");
                layouts[1].SetActive(true);
                break;
        }
    }

    public override void PlayOnWithCoinClick()
    {
        if (inventoryService.Instance.CanReduce(playOnPrice.resource, playOnPrice.quantity))
        {
            var log = new SpendResourceLogData()
            {
                earnType = "add_trays",
                earnId = "revive",
            };
            inventoryService.Instance.ReduceResource(playOnPrice.resource, playOnPrice.quantity,
                log);
            PlayOn("play_on_add_trays");
        }
        else
        {
            PopupToast.Cretate("Not enough coin!");
            PanelManager.Instance.OpenPanelByName<ShopPanelBase>("ShopPanel");
        }
    }
    public virtual void PlayOnWithAdsClick()
    {
        MySonatFramework.ShowRewardAds(OnReviveWithAds, "booster", "revive");
    }
    protected override void OnReviveWithAds()
    {
        checkRwdRevive.AddValue();
        PlayOn("play_on_add_trays");
        SonatSystem.GetService<GameplayAnalyticsService>().levelPlayData.reviveByRwd++;
    }


    // public void PlayOnWithBeatLevel()
    // {
    //     PlayOn("play_on_add_energies");
    // }
}