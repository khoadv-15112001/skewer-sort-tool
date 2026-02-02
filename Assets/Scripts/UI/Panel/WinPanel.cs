using System;
using DG.Tweening;
using Gameplay.LevelData;
using GrillSort.LavaQuest;
using GrillSort.LoseRwdService;
using GrillSort.PiggyBank;
using GrillSort.QuestEvent;
using Manager;
using Sonat.Data;
using Sonat.Enums;
using SonatFramework.Scripts.Helper;
using SonatFramework.Scripts.SonatSDKAdapterModule;
using SonatFramework.Scripts.UIModule;
using SonatFramework.Scripts.Utils;
using SonatFramework.Systems;
using SonatFramework.Systems.BoosterManagement;
using SonatFramework.Systems.EventBus;
using SonatFramework.Systems.InventoryManagement;
using SonatFramework.Systems.SceneManagement;
using SonatFramework.Systems.UserData;
using TMPro;
using Unity.Services.Core;
using UnityEngine;
using UnityEngine.UI;

public class WinPanel : WinPanelBase
{
    [Header("Level")][SerializeField] private Image bgLevel;
    [SerializeField] private TMP_Text textLevel;
    [SerializeField] private Sprite[] bgSprites;

    [Header("Wheel")][SerializeField] private UIWheel wheel;
    [SerializeField] private TMP_Text textx2;

    [Header("Chest")][SerializeField] private GameObject chest;

    [Header("No chest")][SerializeField] private GameObject center;
    [SerializeField] private RectTransform buttonGroup;

    [Header("UI Receive")]
    [SerializeField]
    private UIReceivePiggyPoint uiReceivePiggyPoint;

    [SerializeField] private UIReceiveQuestItem uiReceiveQuestItem;
    [SerializeField] private TMP_Text txtStar;
    [SerializeField] private TMP_Text txtQuestEventItem;
    [SerializeField] private GameObject uiQuestEvent;
    private CheckRemoteByDayCounter checkRwdDay;

    private float _multiplier = 2f;

    public override void OnSetup()
    {
        base.OnSetup();
        checkRwdDay = new("by_day_show_rwd_win_x2", 999);
    }


    public override void OnClaimClick()
    {
        if (collected) return;
        collected = true;

        wheel?.StopWheel();
        ClaimNormal(claimBtn, data.reward.quantity);
    }

    private void ClaimNormal(Transform btn, int quantity)
    {
        var log = new EarnResourceLogData()
        {
            spendId = "win",
            spendType = "win"
        };

        Service<InventoryService>.Get().AddResource(data.reward.resource, quantity, log, false);
        EventBus<AddItemEvent>.Raise(new AddItemEvent()
        { resource = data.reward.resource, quantity = quantity, position = btn.position, collectEffect = new CollectEffectMultiple() });
        SonatUtils.DelayCall(delayToCollect, AfterClaim);
        MySonatFramework.audioService.PlaySound(AudioId.Coin_Received);
    }

    protected override void AfterClaim()
    {
        if (MySonatFramework.userDataService.GetLevel() >= GameRemoteConfigValue.levelShowInterWin)
            base.AfterClaim();
        else
        {
            NextLevel();
        }
    }

    public override void OnClaimX2Click()
    {
        if (collected) return;
        // if (SonatSDKAdapter.IsRewardAdsReady())
        // {
        // 	collected = true;
        // 	
        // }
        wheel?.StopWheel();
        MySonatFramework.ShowRewardAds(OnWatchedVideo, "x2_coin_win", "x2_coin_win");
    }

    protected override void OnClaimReward(Transform btn, int quantity)
    {
        base.OnClaimReward(btn, quantity);
        MySonatFramework.audioService.PlaySound(AudioId.Coin_Received);
    }

    protected override void OnWatchedVideo()
    {
        collected = true;
        checkRwdDay.AddValue();
        var log = new EarnResourceLogData()
        {
            spendType = "win_x2",
            spendId = "win_x2"
        };
        Service<InventoryService>.Get().AddResource(data.reward.resource, data.reward.quantity * (int)_multiplier, log, false);
        OnClaimReward(x2CoinBtn, (int)(data.reward.quantity * _multiplier));
        //collectEffectCurveStream.CreateEffect(icon.transform.position, data.reward.resource, 15, NextLevel);
    }

    public override void NextLevel()
    {
        var actionNextLevel = new Action(() =>
        {
            int level = MySonatFramework.GetService<UserDataService>().GetLevel();
            if (CheckForceHome(level))
            {
                CloseImmediately();
                LoadingScreenInstance.Instance.Show(1.5f);
                SonatUtils.DelayCall(0.5f, () => { MySonatFramework.GetService<SceneService>().SwitchScene(GamePlacement.Home); });
            }
            else
            {
                base.NextLevel();
            }
        });

        // ✅ Check và show PopupRate tự động lần đầu tiên khi win level 3
        if (ShouldShowPopupRate())
        {
            // Đánh dấu đã show
            const string AUTO_POPUP_RATE_SHOWED_KEY = "ShowRatePopup";
            PlayerPrefs.SetInt(AUTO_POPUP_RATE_SHOWED_KEY, 1);
            PlayerPrefs.Save();
            
            // Show PopupRate với callback khi đóng popup
            var uiData = new UIData();
            uiData.Add(UIDataKey.CallBackOnClose, (Action)(() =>
            {
                if (LavaQuestService.Instance.isJoined.BoolValue)
                {
                    var lavaQuestData = new UIData();
                    lavaQuestData.Add("eState", PopupLavaQuest.EState.Win);
                    PopupLavaQuest.OnCloseWin += actionNextLevel;
                    PanelManager.Instance.OpenPanel<PopupLavaQuest>(lavaQuestData);
                }
                else
                {
                    actionNextLevel?.Invoke();
                }
            }));
            
            PanelManager.Instance.OpenPanel<PopupRate>(uiData);
            
            Debug.Log("[WinPanel] Auto show PopupRate after claim at level 3");
            return;
        }

        if (LavaQuestService.Instance.isJoined.BoolValue)
        {
            var uiData = new UIData();

            uiData.Add("eState", PopupLavaQuest.EState.Win);

            PopupLavaQuest.OnCloseWin += actionNextLevel;

            PanelManager.Instance.OpenPanel<PopupLavaQuest>(uiData);
            return;
        }

        actionNextLevel?.Invoke();
    }

    private bool ShouldShowPopupRate()
    {
        const string AUTO_POPUP_RATE_SHOWED_KEY = "AUTO_POPUP_RATE_SHOWED";
        
        // Check điều kiện: win level 3 và chưa tự động show popup này bao giờ
        int currentLevel = data.level;
        return currentLevel == 3 && PlayerPrefs.GetInt(AUTO_POPUP_RATE_SHOWED_KEY, 0) == 0;
    }

    private bool CheckForceHome(int level)
    {
        return level >= GameRemoteConfigValue.levelForceHome || level - 1 == GameRemoteConfigValue.levelAppearLuckySpin;
    }

    public override void Open(UIData uiData)
    {
        base.Open(uiData);

        // set bg + title
        textLevel.SetLocalizeParam("LEVEL", data.level.ToString());
        txtStar.text = MySonatFramework.GetService<StarChestService>().Star.ToString();

        QuestEventService questEventService = MySonatFramework.GetService<QuestEventService>();
        if (questEventService.IsUnlocked())
        {
            uiQuestEvent.SetActive(true);
            txtQuestEventItem.text = questEventService.NumCollectedItemInGame.ToString();
        }
        else
        {
            uiQuestEvent.SetActive(false);
        }

        int gamemode = (int)MySonatFramework.GetLevelDifficulty(data.level);
        if (bgSprites?.Length > gamemode)
            bgLevel.sprite = bgSprites[gamemode];

        var chestService = SonatSystem.GetService<ChestRewardService>();
        // set center/ chest
        bool isChest = chestService.configs.active && data.level >= chestService.configs.levelStart;
        if (isChest)
        {
            chest.SetActive(true);
            center.SetActive(false);
        }
        else
        {
            chest.SetActive(false);
            center.SetActive(true);
        }


        bool canX2 = checkRwdDay.CheckCounter();
        // set wheel
        bool isWheel = Service<UserDataService>.Get().GetLevel() >= SonatSDKAdapter.GetRemoteInt("level_start_x2_coin", 5);

        Debug.Log($"anhnt: [UserCampaignSegment] WinPanel Open isWheel={isWheel}; canX2={canX2}");

        _multiplier = 2f;
        wheel.gameObject.SetActive(isWheel && canX2);

        // Căn lại vị trí button
        // if (isChest == false && isWheel == false)
        // {
        //     buttonGroup.anchoredPosition = new Vector2(0, -325);
        // }
        // else
        // {
        //     buttonGroup.anchoredPosition = new Vector2(0, -575);
        // }
        // uiReceivePiggyPoint.gameObject.SetActive(false);
        // uiReceiveQuestItem.gameObject.SetActive(false);
        ShowUIReceive();

        if (x2CoinBtn.gameObject.activeInHierarchy)
        {
            x2CoinBtn.gameObject.SetActive(isWheel && canX2);
        }
        int currentLevel = MySonatFramework.userDataService.GetLevel();
        var userCampaignSegment = UserData.UserCampaignSegment.Value;
        int condition = SonatSDKAdapter.GetValueByLevelSegment("by_day_show_rwd_win_x2", userCampaignSegment, currentLevel, 999);

        Debug.Log($"anhnt: [UserCampaignSegment] by_day_show_rwd_win_x2 = {SonatSDKAdapter.GetRemoteString("by_day_show_rwd_win_x2")}");
        Debug.Log($"anhnt: [UserCampaignSegment] UserCampaignSegment={userCampaignSegment}; rwd={checkRwdDay.GetCurrentValue()}/{condition}");

    }

    private void Update()
    {
        if (wheel != null && wheel.gameObject.activeInHierarchy)
        {
            _multiplier = wheel.GetWheelValue();
            // Debug.Log($"Wheel Multiplier: {_multiplier}");
            textx2.text = $"{_multiplier * data.reward.quantity}";
        }
    }

    private void ShowUIReceive()
    {
        uiReceivePiggyPoint.gameObject.SetActive(true);
        TryShowQuestEvent();
    }

    private void TryShowQuestEvent()
    {
        var config = SonatSystem.GetService<QuestEventService>().config;
        var level = SonatSystem.GetService<UserDataService>().GetLevel();
        var questEventService = SonatSystem.GetService<QuestEventService>();
        if (questEventService.CheckLiveOpsCondition() == true && level - 1 >= config.unlockedLevel && questEventService.CheckCompleteAllQuest() == false)
        {
            uiReceiveQuestItem.gameObject.SetActive(true);
        }
    }
}