using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using GrillSort.BattlePass;
using SonatFramework.Scripts.UIModule;
using SonatFramework.Scripts.UIModule.UIElements;
using SonatFramework.Systems;
using SonatFramework.Systems.InventoryManagement.GameResources;
using SonatFramework.Systems.ObjectPooling;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PopupBattlePass : Panel
{
    [SerializeField] private TMP_Text textNextMilestone;
    [SerializeField] private TMP_Text textCurrentExp;
    [SerializeField] private Slider sliderProgress;
    [SerializeField] private Transform container;
    [SerializeField] private ScrollRect scrollView;

    private readonly Service<BattlePassService> battlePassService = new();
    private readonly Service<PoolingContainerService> poolingService = new();
    private readonly List<UIMileStone> _uiMilestones = new();

    private static float prevValue = 0;
    private bool isUIBuilt = false;
    public GameObject previewObj;
    public UIRewardGrid UIRewardGrid;
    private Transform previewTrf;
    public static Action<RewardData, Transform> OnShowPreview { get; internal set; }

    public override void OnSetup()
    {
        base.OnSetup();

        Init().Forget();

        OnShowPreview += ShowPreview;
    }
    private void OnDestroy()
    {
        OnShowPreview -= ShowPreview;
    }
    private void ShowPreview(RewardData data, Transform transform)
    {

        if (previewTrf == null)
            previewTrf = previewObj.transform;

        previewObj.SetActive(false);

        previewTrf.DOKill();

        previewTrf.position = transform.position;

        previewTrf.SetParent(transform);

        UIRewardGrid.SetReward(data);

        //UIRewardGrid.transform.localScale = Vector3.one * (data.resourceDatas.Count < 3 ? 1.4f : 1f);

        previewObj.SetActive(true);
    }

    public override void Open(UIData uiData)
    {
        battlePassService.Instance.OnUpdateUI += UpdateUI;

        base.Open(uiData);

        if (isUIBuilt)
        {
            UpdateUI();
            ScrollToIndex(battlePassService.Instance.CurrentMilestoneIdx);
        }
    }
    public override void Close()
    {
        base.Close();
        battlePassService.Instance.OnUpdateUI -= UpdateUI;
    }

    public void OpenTut()
    {
        battlePassService.Instance.OpenBattlePassTut();
    }

    public void OpenActivate()
    {
        battlePassService.Instance.OpenBattlePassActivate();
    }

    private new async UniTask Init()
    {
        if (!isUIBuilt)
        {
            BuildUIQuest();
            isUIBuilt = true;
        }

        UpdateUI();

        // đảm bảo layout xong trước khi scroll
        await UniTask.NextFrame();
        ScrollToIndex(battlePassService.Instance.CurrentMilestoneIdx);
    }

    private void BuildUIQuest()
    {
        poolingService.Instance.CleanContainer(container);
        _uiMilestones.Clear();

        var config = battlePassService.Instance.generalConfig;

        // ticket item
        var uiItem = poolingService.Instance.CreateObject<UIItem>(container);
        uiItem.Init(isTicket: true);

        // milestones
        for (int i = 0; i < config.GetMileStoneCount(); i++)
        {
            uiItem = poolingService.Instance.CreateObject<UIItem>(container);
            uiItem.Init(isTicket: false);

            var uiMileStone = uiItem.GetMileStone();
            uiMileStone.Init(i);

            _uiMilestones.Add(uiMileStone);
        }
    }

    public void ScrollToIndex(int index)
    {
        if (index < 0) index = 0;
        if (_uiMilestones.Count <= 0) return;

        DOTween.Kill(scrollView);

        var percentage = Mathf.Max(
            1 - 1.0f / (_uiMilestones.Count - 1) * index,
            0
        );

        // tween mượt
        DOTween.To(
            () => scrollView.verticalNormalizedPosition,
            value => scrollView.verticalNormalizedPosition = value,
            percentage,
            0.6f
        ).SetEase(Ease.OutCubic).SetTarget(scrollView);
    }

    public void UpdateUI(int idx = -1)
    {
        SetProgress();

        // nếu milestone ít thì loop hết cũng ok
        for (int i = 0; i < _uiMilestones.Count; i++)
        {
            _uiMilestones[i].UpdateUI();
        }
    }

    private void SetProgress()
    {
        var currentMilestoneIdx = battlePassService.Instance.CurrentMilestoneIdx;
        var currentExp = battlePassService.Instance.CurrentExp;

        var currentMilestoneExp = battlePassService.Instance.generalConfig.GetTotalExp(currentMilestoneIdx);
        var nextMilestoneExp = battlePassService.Instance.generalConfig.GetTotalExp(currentMilestoneIdx + 1);
        var previousMilestoneExp = battlePassService.Instance.generalConfig.GetTotalExp(currentMilestoneIdx - 1);

        var expInProgress = currentExp - currentMilestoneExp;
        var totalExp = nextMilestoneExp - currentMilestoneExp;

        var value = expInProgress * 1.0f / totalExp;

        if (expInProgress == 0 && currentMilestoneIdx != -1 && !battlePassService.Instance.CheckFull())
        {
            value = 1;
            totalExp = currentMilestoneExp - previousMilestoneExp;
            textCurrentExp.text = $"{totalExp}/{totalExp}";
            textNextMilestone.text = $"{currentMilestoneIdx + 1}";
        }
        else if (!battlePassService.Instance.CheckFull())
        {
            textCurrentExp.text = $"{expInProgress}/{totalExp}";
            textNextMilestone.text = $"{currentMilestoneIdx + 2}";
        }
        else // battle pass full
        {
            value = 1;
            var count = battlePassService.Instance.generalConfig.GetMileStoneCount();
            var maxExp = battlePassService.Instance.generalConfig.GetTotalExp(count - 1) -
                         battlePassService.Instance.generalConfig.GetTotalExp(count - 2);

            textCurrentExp.text = $"{maxExp}/{maxExp}";
            textNextMilestone.text = $"{currentMilestoneIdx + 1}";
        }

        DOTween.Kill(sliderProgress);

        if (prevValue != value)
        {
            if (prevValue > value) sliderProgress.value = 0;
            sliderProgress.DOValue(value, 0.5f).SetEase(Ease.OutCubic);
            prevValue = value;
        }
        else
        {
            sliderProgress.value = value;
        }
    }

#if UNITY_EDITOR
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.A))
        {
            MySonatFramework.GetService<BattlePassService>().AddProgress(1);
        }
    }
#endif
}
