using I2.Loc;
using Sonat.Enums;
using SonatFramework.Scripts.SonatSDKAdapterModule;
using SonatFramework.Scripts.UIModule.UIElements;
using SonatFramework.Scripts.Utils;
using SonatFramework.Systems;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UITeamOffer : MonoBehaviour
{
    public ShopItemKey key;

    [Header("Self Rewards")]
    [SerializeField] private UIRewardGroup uiSeflRewardGroup;

    [Space(10)]
    [Header("Team Rewards")]
    [SerializeField] private UIRewardItem livesGift;
    [SerializeField] private Image giftIcon;

    [Space(10)]
    [SerializeField] private TextMeshProUGUI priceTxt;
    [SerializeField] private TextMeshProUGUI availableTxt;

    [SerializeField] private TextMeshProUGUI timer;

    private readonly Service<TeamOfferService> teamOfferService = new();
    private readonly Service<TeamService> teamService = new();

    private void Start()
    {
        var selfRewards = teamOfferService.Instance.GetSelfRewards(key);
        var teamRewards = teamOfferService.Instance.GetTeamRewards(key);

        uiSeflRewardGroup.SetData(selfRewards);
        livesGift.Init(GameResource.Lives, teamRewards.resourceDatas.Find(r => r.resource == GameResource.Lives).quantity);

        priceTxt.text = SonatSDKAdapter.GetProductPrice(key);

        availableTxt.text = "1/1 " + LocalizationManager.GetTranslation("Available");
    }

    private void OnEnable()
    {
        TeamOfferService.OnTick += Tick;
    }

    private void OnDisable()
    {
        TeamOfferService.OnTick -= Tick;
    }

    private void Tick()
    {
        timer.text = SonatUtils.GetTimeByFormat(teamOfferService.Instance.GetRemainTime(), TxtTimeFormat.ShortDay_FullTime);
    }

    public void OnClickBuy()
    {
        if (!MySonatFramework.IsNetworkAvailable())
        {
            NoInternet.Instance.ForceShowPopup();
        }
        else
        {
            if (!teamService.Instance.IsJoinedTeam())
            {
                PopupToast.Cretate("You haven't joined a team yet");
            }
            else
            {
                teamOfferService.Instance.ProcessPurchase(true);

                string teamId = teamService.Instance.CurrentTeamData.id;

                SonatSDKAdapter.BuyPack(key, (success) => OnBuyCompleted(success, teamId), "pack");
            }
        }
    }

    private void OnBuyCompleted(bool success, string teamId)
    {
        if (success)
            _ = teamOfferService.Instance.OnPurchasePack(teamId, key);
        else
            teamOfferService.Instance.ProcessPurchase(false);
    }
}
