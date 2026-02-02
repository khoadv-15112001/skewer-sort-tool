using System.Collections.Generic;
using SkewerJam.Features.Leaderboard;
using SkewerJam.Features.Leaderboard.Service;
using SonatFramework.Scripts.UIModule;
using SonatFramework.Systems;
using SonatFramework.Systems.InventoryManagement.GameResources;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using SonatFramework.Systems.InventoryManagement;
using SonatFramework.Scripts.UIModule.UIElements;
using MyGame.SkewerJam.Scripts.Service;
using Cysharp.Threading.Tasks;
using SonatFramework.Scripts.Utils;
using DG.Tweening;
using System;
using Sonat.Enums;

public class PanelLeaderboard_SkewerJam : Panel
{
    [SerializeField] private TMP_Text txtPumpkin;
    [SerializeField] private Slider sliderPumpkin;
    [SerializeField] private UITimeCounter timeCounter;
    [SerializeField] private GameObject startObj;
    [SerializeField] private GameObject finishObj;

    [Header("Animation")][SerializeField] private float durationUpdatePumpkin = 1.5f;
    [SerializeField] private float delayUpdatePumpkin = 0.5f;
    [SerializeField] private float delayReceiveRewardMilestone = 1f;
    [SerializeField] private ParticleSystem[] psComplete;
    [SerializeField] private UIRewardMilestone[] uiRewardMilestones;
    private Service<LeaderboardHLWService> leaderboardService = new();
    private Service<HLWEventService> hlwEventService = new();

    private int _preNumPumpkin
    {
        get => PlayerPrefs.GetInt("PreNumPumpkin_HLW", 0);
        set => PlayerPrefs.SetInt("PreNumPumpkin_HLW", value);
    }

    public override void Open(UIData uiData)
    {
        base.Open(uiData);
        inputField.gameObject.SetActive(false);

        var pumpkin = MySonatFramework.GetService<InventoryService>().GetResource(Sonat.Enums.GameResource.Pumpkin);
        var maxPumpkin = leaderboardService.Instance.config.GetMaxPumpkinMilestone();
        PlayUpdatePumpkin(pumpkin, maxPumpkin);


        var remainTime = hlwEventService.Instance.GetRemainTime();
        if (remainTime <= 0)
        {
            FinishEvent();
            return;
        }
        else
        {
            startObj.SetActive(true);
            finishObj.gameObject.SetActive(false);
            timeCounter.SetData(remainTime, null);
        }

        hlwEventService.Instance.OnFinishEvent += FinishEvent;
        CheckRewardUpdate();
    }

    private void OnDisable()
    {
        hlwEventService.Instance.OnFinishEvent -= FinishEvent;
    }

    private void FinishEvent()
    {
        startObj.gameObject.SetActive(false);
        finishObj.SetActive(true);

        if (hlwEventService.Instance.CheckReceiveEventReward == false)
        {
            PlayReceiveEventReward();
        }
    }

    private async UniTask PlayReceiveEventReward()
    {
        var response = await leaderboardService.Instance.FetchLeaderboard();

        if (response == null) return;

        var rank = response.position;
        var reward = leaderboardService.Instance.GetEventReward(rank);
        if (reward == null)
        {
            hlwEventService.Instance.ReceiveEventReward();
            return;
        }

        await UniTask.Delay(1000);

        var log = new EarnResourceLogData
        {
            spendType = "leaderboard_event",
            spendId = "leaderboard_event",
            isFirstBuy = false,
            source = "non_iap"
        };
        MySonatFramework.GetService<InventoryService>().AddReward(reward, log);

        hlwEventService.Instance.ReceiveEventReward();

        OpenPopupRewardChest(rank, reward);
    }

    private void OpenPopupRewardChest(int rank, RewardData reward)
    {
        var uiData = new UIData();
        uiData.Add("Rank", rank);
        uiData.Add("Reward", reward);
        PanelManager.Instance.OpenPanelByName<PopupRewardChest_HLW>("PopupRewardChest_HLW", uiData);
    }

    private void PlayUpdatePumpkin(int pumpkin, int maxPumpkin)
    {
        sliderPumpkin.value = (float)_preNumPumpkin / maxPumpkin;
        txtPumpkin.text = $"{_preNumPumpkin}/{maxPumpkin}";

        DOTween.To(() => sliderPumpkin.value, x => sliderPumpkin.value = x, (float)pumpkin / maxPumpkin, durationUpdatePumpkin)
            .SetEase(Ease.OutSine)
            .OnUpdate(() => txtPumpkin.text = $"{(int)(sliderPumpkin.value * maxPumpkin)}/{maxPumpkin}").SetDelay(delayUpdatePumpkin)
            .OnComplete(() =>
            {
                // Canvas.ForceUpdateCanvases();
                _preNumPumpkin = pumpkin;
                if (leaderboardService.Instance.CheckRewardMilestone() == true)
                {
                    leaderboardService.Instance.UpdateMilestoneClaimed();

                    var idx = leaderboardService.Instance.CurrentMilestoneIdx;
                    uiRewardMilestones[idx].SetComplete(true);
                    psComplete[idx].Play();
                    TryReceiveRewardMilestone(idx).Forget();
                }
            });
    }

    private async UniTask TryReceiveRewardMilestone(int idx)
    {
        // await UniTask.Delay((int)delayReceiveRewardMilestone * 1000);
        var reward = leaderboardService.Instance.config.rewardMilestone[idx].reward;
        var log = new EarnResourceLogData
        {
            spendType = "leaderboard_milestone",
            spendId = "leaderboard_milestone",
            isFirstBuy = false,
            source = "non_iap"
        };
        MySonatFramework.GetService<InventoryService>().AddReward(reward, log);
        // var data = new UIData();
        // data.Add("Title", "REWARD!");
        // data.Add("Reward", reward);
        // PanelManager.Instance.OpenPanel<PopupReward>(data);
        var uiData = new UIData();
        uiData.Add("StartPos", psComplete[idx].transform.position);
        uiData.Add("Reward", leaderboardService.Instance.config.rewardMilestone[idx].reward);
        MySonatFramework.audioService.PlaySound("Progress_HLW_Unlock_chest_Grill_sort");
        PanelManager.Instance.OpenPanelByName<PopupReward_HLW>("PopupReward_HLW", uiData);
    }

    private int count = 0;

    [Space(10)]
    [Header("Test Effect Reward Chest")]
    [SerializeField]
    private TMP_InputField inputField;

    public void OnClickTestEffectRewardChest(int idx)
    {
        count++;
        if (count > 20)
        {
            inputField.gameObject.SetActive(true);
        }

        if (inputField.text == "Sonat@111")
        {
            OpenPopupRewardChest(idx, leaderboardService.Instance.config.rewardTop[idx]);
        }
    }

    private void CheckRewardUpdate()
    {
        if (PlayerPrefs.HasKey("RewardUpdate_HLW")) return;
        PlayerPrefs.SetInt("RewardUpdate_HLW", 1);
        int level = MySonatFramework.userDataService.GetLevel(GameMode.SkewerJam);
        if (level <= 1) return;
        PanelManager.Instance.OpenPanel<PopupRewardEnergyUpdate>();
    }


#if UNITY_EDITOR
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            // MySonatFramework.GetService<InventoryService>().AddResource(Sonat.Enums.GameResource.Pumpkin, 100);
        }

        if (Input.GetKeyDown(KeyCode.E))
        {
            TryReceiveRewardMilestone(0).Forget();
        }

        if (Input.GetKeyDown(KeyCode.W))
        {
            TryReceiveRewardMilestone(1).Forget();
        }

        if (Input.GetKeyDown(KeyCode.Q))
        {
            TryReceiveRewardMilestone(2).Forget();
        }

        if (Input.GetKeyDown(KeyCode.Z))
        {
            OpenPopupRewardChest(1, leaderboardService.Instance.config.rewardTop[0]);
        }

        if (Input.GetKeyDown(KeyCode.X))
        {
            OpenPopupRewardChest(2, leaderboardService.Instance.config.rewardTop[1]);
        }

        if (Input.GetKeyDown(KeyCode.C))
        {
            OpenPopupRewardChest(3, leaderboardService.Instance.config.rewardTop[2]);
        }
    }
#endif
}