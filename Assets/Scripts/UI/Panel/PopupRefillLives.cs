using SonatFramework.Scripts.UIModule;
using SonatFramework.Systems.InventoryManagement;
using SonatFramework.Templates.UI.ScriptBase;
using UnityEngine;

public class PopupRefillLives : PopupRefillLivesBase
{
    [Space(10)]
    [Header("Live Offer Pack")]
    [SerializeField]
    private UIPackAppearanceScheduler _packAppearanceScheduler;

    private CheckRemoteByDayCounter checkCanBuyByRwd;

    private void Awake()
    {
        if (_packAppearanceScheduler) _packAppearanceScheduler.gameObject.SetActive(false);
    }

    public override void OnSetup()
    {
        base.OnSetup();
        checkCanBuyByRwd = new CheckRemoteByDayCounter("by_day_show_rwd_add_lives", 999);
    }

    public override void Open(UIData uidata)
    {
        base.Open(uidata);

        if (_packAppearanceScheduler != null)
        {
            // khi hết live hiện thì mới có thể hiện live offer
            if (liveService.Instance.IsFullLives() == false)
            {
                _packAppearanceScheduler.gameObject.SetActive(true);
            }
        }

        refillWithAdsBtn.gameObject.SetActive(checkCanBuyByRwd.CheckCounter());
    }
    public virtual void RefillWithAds()
    {
        MySonatFramework.ShowRewardAds(OnRefillWithAds, "live", "live");
    }
    protected override void OnRefillWithAds()
    {
        liveService.Instance.RefillOneLive(new EarnResourceLogData()
        {
            spendId = "rw_ads",
            spendType = "rw_ads",
        });
        CheckLive();
        checkCanBuyByRwd.AddValue();
        Close();
    }
}