using Cysharp.Threading.Tasks;
using DG.Tweening;
using GrillSort.ConsecutiveWin;
using GrillSort.LavaQuest;
using GrillSort.Winstreak;
using I2.Loc;
using Manager;
using Sonat.Enums;
using SonatFramework.Scripts.Helper;
using SonatFramework.Scripts.SonatSDKAdapterModule;
using SonatFramework.Scripts.UIModule;
using SonatFramework.Systems.UserData;
using System;
using System.Collections.Generic;
using SonatFramework.Scripts.Utils;
using SonatFramework.Systems.SceneManagement;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using GrillSort.UI;
using GrillSort.LoseRwdService;
using Sonat.Data;
using Sonat;
using SonatFramework.Scripts.Feature.Shop.UI;
using SonatFramework.Systems.InventoryManagement;
using MyGame.SkewerJam.Gameplay;
using UnityEngine.Purchasing;

public class PopupContinue : PopupContinueBase
{
    [SerializeField] private Button[] btnsContinue;
    [SerializeField] private UIGiftBoxAnim giftBoxAnim;
    [SerializeField] private UIMultiEventLoseController multiEventLoseController;

    [Header("Next continue")]
    [SerializeField]
    private UIStateChanger _uiStateChanger;


    private Queue<Action> confirmActions;
    [SerializeField] private GameObject missOutObj;
    [SerializeField] private Localize[] titleTexts;
    [SerializeField] private LoseRwdUIController loseRwdUIController;
    private LoseRwdService _loseRwdService;

    private CheckRemoteByDayCounter checkRwdByDay;
    private CheckRemoteByLevelCounter checkRwdByLevel;
    private float multiplierReviveTime;
    private float multiplierReviveCoin;
    private int LevelStartCount => MySonatFramework.gameplayAnalyticsService.levelStartCount;

    // Local variables để tính giá trị revive (không ghi đè lên GameRemoteConfigValue)
    private int calculatedCoinRevive;
    private int calculatedTimeRevive;

    public int ReviveLevel
    {
        get => PlayerPrefs.GetInt("popup_continue_revive_level", 0);
        set => PlayerPrefs.SetInt("popup_continue_revive_level", value);
    }

    public override void OnSetup()
    {
        base.OnSetup();

        checkRwdByDay = new("by_day_show_rwd_revive", 999);

        checkRwdByLevel = new("by_level_show_rwd_revive", 999);

        string campaignSegment = UserData.UserCampaignSegment.Value;

        multiplierReviveCoin = SonatSDKAdapter.GetValueBySegment("multiplier_from_2nd_revive_coin_by_segment", campaignSegment, 2);

        multiplierReviveTime = SonatSDKAdapter.GetValueBySegment("multiplier_from_2nd_revive_time_by_segment", campaignSegment, 1.5f);

        if (ReviveLevel != LevelStartCount)
        {
            ReviveLevel = 0;
        }

        // Tính toán giá trị local (KHÔNG ghi đè lên GameRemoteConfigValue global)
        calculatedCoinRevive = (int)(GameRemoteConfigValue.coinRevive * (ReviveLevel > 0 ? multiplierReviveCoin : 1));
        calculatedTimeRevive = (int)(GameRemoteConfigValue.timeRevive * (ReviveLevel > 0 ? multiplierReviveTime : 1));

        playOnPrice.quantity = calculatedCoinRevive;

        txtPlayonPrice.text = playOnPrice.quantity.ToString();

        _loseRwdService = MySonatFramework.GetService<GrillSort.LoseRwdService.LoseRwdService>();
    }

    public override void Open(UIData uiData)
    {
        base.Open(uiData);

        confirmActions ??= new Queue<Action>();
        confirmActions.Clear();

        MySonatFramework.customTrackingService.OnShowPopup("safety_net_bundle", "banner", "iap", "auto");

        // setup lose offer
        var isOutOfTime = data.stuckType is StuckType.OutOfTime or StuckType.OutOfTimeShipper;
        // setup button xem rwd
        if (MySonatFramework.GetService<LoseRwdService>().Config.active)
        {
            playOnWithAdsBtn.SetActive(false);

            int currentLevel = MySonatFramework.userDataService.GetLevel();
            var userCampaignSegment = UserData.UserCampaignSegment.Value;
            int condition = SonatSDKAdapter.GetValueByLevelSegment("by_level_show_rwd_revive", userCampaignSegment, currentLevel, 999);

            //Debug.Log($"anhnt: [UserCampaignSegment] by_level_show_rwd_revive = {SonatSDKAdapter.GetRemoteString("by_level_show_rwd_revive")}");
            //Debug.Log($"anhnt: [UserCampaignSegment] UserCampaignSegment={userCampaignSegment}; CurrentStage={_loseRwdService.CurrentStage}/{condition}");

            if (_loseRwdService == null || !_loseRwdService.CanShowLoseRwd() || !(checkRwdByLevel.CheckCounter() && checkRwdByLevel.CheckCounter()))
            {
                loseRwdUIController.gameObject.SetActive(false);
            }
            else
            {
                loseRwdUIController.gameObject.SetActive(true);

                _loseRwdService._config.SetLoop(checkRwdByLevel.CheckCounter());
                //Debug.Log($"anhnt: lose rwd loop=" + _loseRwdService._config.loopByLevel);

                loseRwdUIController.RefreshUI(_loseRwdService._config, _loseRwdService.CurrentStage);
            }
        }
        else
        {
            loseRwdUIController.gameObject.SetActive(false);

            if (playOnWithAdsBtn.activeInHierarchy)
            {
                playOnWithAdsBtn.SetActive(checkRwdByDay.CheckCounter() && checkRwdByLevel.CheckCounter());
            }
        }

        multiEventLoseController.Setup();

        CreateActions().Forget();
    }

    public override void OnOpenCompleted()
    {
        base.OnOpenCompleted();

        //if (loseRwdUIController != null && loseRwdUIController.gameObject.activeSelf)
    }

    private bool CheckConsecutiveWin()
    {
        var service = MySonatFramework.GetService<ConsecutiveWinService>();
        var level = MySonatFramework.userDataService.GetLevel();
        return service.CheckStart(level);
    }
    private bool CheckWinstreakLives()
    {
        var service = MySonatFramework.GetService<WinStreakManager>();
        return service.CanReduceLives();
    }
    private bool CheckWinstreak()
    {
        var service = MySonatFramework.GetService<WinStreakManager>();
        return !service.CanReduceLives() && service.winningInARow.Value > 0;
    }
    private void SetTitleTerm(string term)
    {
        var level = MySonatFramework.GetService<UserDataService>().GetLevel();
        var levelDifficulty = MySonatFramework.GetLevelDifficulty(level);
        //titleTexts[((int)levelDifficulty)].SetTerm(term);

        var termData = LocalizationManager.GetTermData(term);
        if (termData != null)
            titleTexts[((int)levelDifficulty)].SetTerm(term);
        else
            titleTexts[((int)levelDifficulty)].GetComponent<TextMeshProUGUI>().text = term;
    }
    private async UniTask CreateActions()
    {
        await UniTask.WaitForSeconds(0.5f);
        SetTitleTerm("Continue");

        if (multiEventLoseController.IsAvailable())
        {
            confirmActions.Enqueue(() =>
            {
                _uiStateChanger.SetState(2);
                //Debug.Log("anhnt: Enqueue 2");
            });
        }
        else if (CheckConsecutiveWin())
            confirmActions.Enqueue(() =>
            {
                _uiStateChanger.SetState(1);
                //Debug.Log("anhnt: Enqueue 1");

            });

        //if (CheckConsecutiveWin())
        //    confirmActions.Enqueue(() =>
        //    {
        //        _uiStateChanger.SetState(1);
        //        SetTitleTerm("Continue");
        //    });

        //if (CheckWinstreakLives())
        //    confirmActions.Enqueue(() =>
        //    {
        //        _uiStateChanger.SetState(2);
        //        SetTitleTerm("Keep your Win Streak!");
        //    });

        //else if (CheckWinstreak())
        //    confirmActions.Enqueue(() =>
        //    {
        //        _uiStateChanger.SetState(3);
        //        SetTitleTerm("Keep your Win Streak!");
        //    });

        //if (LavaQuestService.Instance.isJoined.BoolValue && LavaQuestService.IsClassicMode())
        //{
        //    confirmActions.Enqueue(() =>
        //    {
        //        _uiStateChanger.SetState(4);
        //        SetTitleTerm("Keep your Treasure Quest!");
        //    });
        //}

        confirmActions.Enqueue(OnForceConfirmClick);
        //Debug.Log("anhnt: confirmActions= " + confirmActions.Count);

    }

    private void OnForceConfirmClick()
    {
        OnForceGiveUpClick();
    }

    public void OnConfirmClick()
    {
        if (!missOutObj.activeSelf)
            missOutObj.SetActive(true);

        if (confirmActions != null && confirmActions.Count > 0)
        {
            var action = confirmActions.Dequeue();
            //Debug.Log("anhnt: confirmActions= " + confirmActions.Count);

            action?.Invoke();
        }
        else
        {
            Debug.LogWarning("[PopupContinue] confirmActions is empty, skipping Invoke.");
        }
    }

    public override void OnGiveUpClick()
    {
        OnConfirmClick();
        //// Nếu chưa đến level xuất hiện consecutive win
        //// thì không hiện state 2
        //var consecutiveWinService = MySonatFramework.GetService<ConsecutiveWinService>();
        //var level = MySonatFramework.userDataService.GetLevel();

        //if (consecutiveWinService.CheckStart(level) == false)
        //{
        //    OnForceGiveUpClick();
        //    return;
        //}

        //switch (_uiStateChanger.CurrentState)
        //{
        //    case 0:
        //        _uiStateChanger.NextState();
        //        break;
        //    case 1:
        //        OnForceGiveUpClick();
        //        break;
        //}
    }

    private void OnForceGiveUpClick()
    {
        if (MySonatFramework.livesService.CanPlay())
        {
            // UIData data = new UIData();
            // data.Add("OnConfirm", (Action)OnConfirmGiveUp);
            // PanelManager.Instance.OpenPanel<PopupLostLives>(data);
            OnConfirmGiveUp();
        }
        else
        {
            //PanelManager.Instance.OpenPanel<PopupRefillLives>();
            // foreach (var btn in btnsContinue)
            // {
            //     if (btn != null)
            //         btn.interactable = false;
            // }
            //
            // giftBoxAnim.Close(() =>
            // {
            //     Close();
            //     data?.onClose?.Invoke();
            // });
            PanelManager.Instance.OpenPanel<PopupRefillLives>(new UIData().Add(UIDataKey.CallBackOnClose, (Action)(AfterRefillLive)));
            PopupToast.Cretate("No more lives left!");
        }
    }

    private void AfterRefillLive()
    {
        if (MySonatFramework.livesService.CanPlay())
        {
            OnConfirmGiveUp();
        }
        else
        {
            GoHome();
        }
    }

    private void GoHome()
    {
        LoadingScreenInstance.Instance.Show(1.5f);
        SonatUtils.DelayCall(0.5f, () => { MySonatFramework.GetService<SceneService>().SwitchScene(GamePlacement.Home); });
    }


    protected override void SetLayout()
    {
        switch (data.stuckType)
        {
            case StuckType.OutOfTime:
                if (txtLabel) txtLabel.SetLocalize("Out Of Time");
                if (txtDescription)
                {
                    txtDescription.SetLocalize("Add 30s to continue");
                    txtDescription.SetLocalizeParam("VALUE", calculatedTimeRevive.ToString());
                }

                if (icon) icon.sprite = iconsSprites[0];
                break;
            case StuckType.OutOfMove:
                if (txtLabel) txtLabel.SetLocalize("Out Of Move");
                if (txtDescription) txtDescription.SetLocalize("Merge 3 items to continue");
                if (icon) icon.sprite = iconsSprites[1];
                break;
            case StuckType.OutOfTimeShipper:
                if (txtLabel) txtLabel.SetLocalize("Order Out Of Time");
                if (txtDescription) txtDescription.SetLocalize("Skip the order to continue");
                if (icon) icon.sprite = iconsSprites[0];
                break;
        }
    }

    private void OnConfirmGiveUp()
    {
        MySonatFramework.livesService.ReduceLive(1, new() { earnType = "lose", earnId = data.stuckType.ToString().ToLogString() });
        //MySonatFramework.livesService.ReduceLive(1, "lose");
        giftBoxAnim.Close(() =>
        {
            if (LavaQuestService.Instance.isJoined.BoolValue && LavaQuestService.IsClassicMode())
            {
                var uiData = new UIData();
                uiData.Add("eState", PopupLavaQuest.EState.Failed);

                PopupLavaQuest.OnCloseFailed += () =>
                {
                    CloseImmediately();
                    data?.onClose?.Invoke();
                };

                PanelManager.Instance.OpenPanel<PopupLavaQuest>(uiData);

                return;
            }

            Close();
            data?.onClose?.Invoke();
        });
    }

    public void HoldToView()
    {
        panelCanvasGroup.DOFade(0, 0.2f);
    }

    public void FinishHoldToView()
    {
        panelCanvasGroup.DOFade(1, 0.2f);
    }

    public void PlayOnWithOffer(int timeRevive)
    {
        PlayOn("play_on_offer", new object[] { timeRevive });
    }

    protected override void PlayOn(string by, object[] objectParams = null)
    {
        foreach (var btn in btnsContinue)
        {
            btn.interactable = false;
        }

        giftBoxAnim.Close(() => { base.PlayOn(by, objectParams); });
    }

    public virtual void PlayOnWithAdsClick() => MySonatFramework.ShowRewardAds(OnReviveWithAds, "booster", "revive");

    protected override void OnReviveWithAds()
    {
        checkRwdByLevel.AddValue();
        checkRwdByDay.AddValue();

        base.OnReviveWithAds();

        if (_loseRwdService.Config.active)
        {
            _loseRwdService.OnReviveSuccess();
        }
    }

    public override void PlayOnWithCoinClick()
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
            PlayOn("play_on_coin", new object[] { calculatedTimeRevive });

            ReviveLevel = LevelStartCount;
        }
        else
        {
            PopupToast.Cretate("Not enough coin!");
            PanelManager.Instance.OpenPanelByName<ShopPanelBase>("ShopPanel");
        }
    }
}