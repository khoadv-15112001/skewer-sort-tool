using Cysharp.Threading.Tasks;
using GrillSort.RealTime;
using Helper;
using Sonat.CustomService;
using Sonat.Enums;
using SonatFramework.Scripts.Helper;
using SonatFramework.Scripts.SonatSDKAdapterModule;
using SonatFramework.Scripts.UIModule;
using SonatFramework.Systems;
using SonatFramework.Systems.EventBus;
using SonatFramework.Systems.InventoryManagement;
using SonatFramework.Systems.InventoryManagement.GameResources;
using System;
using UnityEngine;

[CreateAssetMenu(fileName = "TeamOfferService", menuName = "Sonat Services Custom/Team Offer Service")]
public class TeamOfferService : SonatServiceSo, IServiceInitialize
{
    [SerializeField] private TeamOfferConfig config;

    public static Action OnTick;
    public static Action OnFeatureEnd;

    public IntDataPref isActive;
    public ListDataPref<int> purchasedPacks;
    public LongDataPref timeEnd;
    public IntDataPref startCount;
    public LongDataPref teamJoinedAt;

    private static bool isProcessingPurchase;

    private long now;

    public TeamOfferConfig Config => config;

    private readonly Service<TeamService> _teamService = new();
    private readonly Service<InventoryService> _inventoryService = new();

    public void Initialize()
    {
        isActive = new IntDataPref("TeamOffer_isActive");
        purchasedPacks = new ListDataPref<int>("TeamOffer_purchasedPacks");
        timeEnd = new LongDataPref("TeamOffer_timeEnd");

        startCount = new IntDataPref("team_offer_start_count");
        teamJoinedAt = new LongDataPref("TeamOffer_teamJoinedAt");

        TryActive().Forget();

        TickService.Register(Tick, TickService.TickPhase.TickRealtime);

        new EventBinding<TeamService.LeaveTeamEvent>(OnLeaveTeam);
        new EventBinding<TeamService.JoinTeamEvent>(OnJoinTeam);
    }

    private void Tick()
    {
        if (!isActive.BoolValue) return;

        now = Config.TimeUnix();

        if (now >= timeEnd.Value)
        {
            isActive.BoolValue = false;
            OnFeatureEnd?.Invoke();
        }

        OnTick?.Invoke();
    }

    private void OnLeaveTeam(TeamService.LeaveTeamEvent eventData)
    {
        if (!isActive.BoolValue) return;
        OnLeaveTeamAsync().Forget();
    }

    private void OnJoinTeam(TeamService.JoinTeamEvent eventData)
    {
        // Record the join timestamp
        teamJoinedAt.Value = Config.TimeUnix();
    }

    private async UniTaskVoid OnLeaveTeamAsync()
    {
        await UniTask.WaitUntil(() => !isProcessingPurchase);
        isActive.BoolValue = false;
        teamJoinedAt.Value = 0; // Clear join timestamp when leaving team
        OnFeatureEnd?.Invoke();
    }

    private bool HasBeenInTeamForOneDay()
    {
        // If teamJoinedAt is not set (0), return false
        if (teamJoinedAt.Value == 0) return false;

        DateTime joinDate = DateTimeOffset.FromUnixTimeSeconds(teamJoinedAt.Value).Date;
        DateTime currentDate = Config.TimeNow().Date;
        
        // Check if current date is after join date (next day or later)
        return currentDate > joinDate;
    }

    private bool ValidatePacks()
    {
        if (Config.offerData == null) return false;

        if (GetSelfRewards(ShopItemKey.TeamOffer_Bronze) == null || 
            GetSelfRewards(ShopItemKey.TeamOffer_Silver) == null || 
            GetSelfRewards(ShopItemKey.TeamOffer_Gold) == null) 
            return false;

        if (GetTeamRewards(ShopItemKey.TeamOffer_Bronze) == null ||
            GetTeamRewards(ShopItemKey.TeamOffer_Silver) == null ||
            GetTeamRewards(ShopItemKey.TeamOffer_Gold) == null)
            return false;

        return true;
    }

    public async UniTask TryActive()
    {
        await UniTask.WaitUntil(() => _teamService.Instance.IsTeamInfoLoaded());

        if (!_teamService.Instance.IsJoinedTeam() || isActive.BoolValue) return;

        // Initialize join timestamp for existing team members (first time only)
        if (teamJoinedAt.Value == 0)
        {
            teamJoinedAt.Value = Config.TimeUnix();
        }

        // Check if user has been in team for at least one day
        if (!HasBeenInTeamForOneDay()) return;

        if (IsInValidTimeWindow() && ValidatePacks())
        {
            DateTime now = Config.TimeNow();
            var (_, end) = Config.ResolveWindowFlexible(now);
            long newTimeEnd = end.ToUnixTimeSeconds();

            // If this is a new time window (different from previous), clear purchased packs
            if (timeEnd.Value != newTimeEnd)
                ResetPurchase();

            // If user purchased any packs in the current lifecycle, they cannot activate again
            if (purchasedPacks.Count > 0) return;

            isActive.BoolValue = true;
            timeEnd.Value = newTimeEnd;

            startCount.Value++;
        }
    }

    public int GetNumPurchasedPacks()
    {
        return purchasedPacks.Count;
    }

    public bool IsActive()
    {
        return isActive.BoolValue && _teamService.Instance.IsJoinedTeam();
    }

    public bool IsInValidTimeWindow()
    {
        DateTime now = Config.TimeNow();
        var (start, end) = Config.ResolveWindowFlexible(now);

        return now >= start && now < end;
    }

    public long GetRemainTime()
    {
        long currentTime = Config.TimeUnix();
        return Math.Max(0, timeEnd.Value - currentTime);
    }

    public TeamOfferConfig.TeamOfferData GetOfferData(ShopItemKey key)
    {
        if (Config.offerData == null) return null;
        return Config.offerData.Find(x => x.shopItemKey == key);
    }

    public RewardData GetSelfRewards(ShopItemKey key)
    {
        return GetOfferData(key)?.selfRewards;
    }

    public RewardData GetTeamRewards(ShopItemKey key)
    {
        return GetOfferData(key)?.teamRewards;
    }

    public Sprite GetGiftIcon(ShopItemKey key)
    {
        return GetOfferData(key)?.giftIcon;
    }

    public void ProcessPurchase(bool value)
    {
        isProcessingPurchase = value;
    }

    public async UniTask OnPurchasePack(string teamId, ShopItemKey key)
    {
        var seflRewards = GetSelfRewards(key);

        var logData = new EarnResourceLogData
        {
            spendType = "pack_iap",
            spendId = SonatSDKAdapter.FindProductId(key),
            isFirstBuy = false,
            source = "iap"
        };
        _inventoryService.Instance.AddReward(seflRewards, logData);

        var uiData = new UIData();
        uiData.Add("IAP", true);
        uiData.Add("Reward", seflRewards);
        PanelManager.Instance.OpenPanel<PopupReward>(uiData);

        purchasedPacks.Add((int)key);
        isActive.BoolValue = false;
        OnFeatureEnd?.Invoke();

        int numTry = 5;

        while (numTry > 0)
        {
            var success = await TrySendGiftToTeam(teamId, key);
            if (success) break;

            await UniTask.WaitForSeconds(1);

            numTry--;
        }
    }

    private async UniTask<bool> TrySendGiftToTeam(string teamId, ShopItemKey key)
    {
        try
        {
            string expiresAt = TimeHelper.ConvertUnixToISOUtc(timeEnd.Value);

            var response = await _teamService.Instance.SendTeamRewards(teamId, GetTeamRewards(key), "purchased", expiresAt,
                SonatSDKAdapter.FindProductId(key));

            if (response.code != ResponseCode.SUCCESS)
            {
                return false;
            }
            else
            {
                return true;
            }
        }
        catch
        {
            return false;
        }
    }

    public void ResetPurchase()
    {
        purchasedPacks.Clear();
    }
}
