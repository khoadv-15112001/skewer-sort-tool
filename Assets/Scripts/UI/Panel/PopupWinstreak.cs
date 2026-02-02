using Cysharp.Threading.Tasks;
using DG.Tweening;
using GrillSort.Winstreak;
using Sonat.Enums;
using SonatFramework.Scripts.UIModule;
using SonatFramework.Scripts.UIModule.UIElements;
using SonatFramework.Scripts.Utils;
using SonatFramework.Systems;
using SonatFramework.Systems.ObjectPooling;
using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PopupWinstreak : Panel
{
    [Header("UI")]
    public TMP_Text txtTime;
    public TMP_Text textMilestone;
    public TMP_Text textLives;
    public Transform milestoneContainer, pointStateContainer, lineContainer;
    public GameObject finishedObj, topBtnObj;
    public CurvedText curvedText;
    public ScrollRect scrollRect;
    public Slider slider;

    [SerializeField] private UITimeCounter timeCounter;
    [Header("Anim")]
    public SpineAnimationController skeletonChar;
    public AnimState animStateLose;
    public AnimState[] animStates;

    [Header("Services")]
    private readonly Service<WinStreakManager> _winstreakManager = new();
    private readonly Service<PoolingContainerService> poolingService = new();

    // milestones (levelWinRequired) tăng dần
    private List<int> _levels;
    private List<UIWinStreakMilestone> uIWinStreakMilestones;

    // cache
    private RectTransform _sliderRT;
    private float _sliderHeight;
    private float _uiGap;

    // Vị trí “tâm handle” trong viewport: 0 = đáy, 0.5 = giữa, 1 = đỉnh
    [Range(0f, 1f)] public float centerBias01 = 0.5f;

    // ===== Intro/Sync guard (MERGED) =====
    private bool _introRunning;
    private bool _suppressSync;
    private UnityEngine.Events.UnityAction<float> _sliderListener;
    private bool _plannedIntro;

    [Header("cheat")]
    public GameObject cheatBanner;
    public TMP_InputField milestoneInput;

    // =================================================
    public override void OnSetup()
    {
        base.OnSetup();

        Init().Forget();
    }

    private void UpdateTileText()
    {
        curvedText.ApplyCurve();
    }
    private void CheatMilestone(string arg0)
    {
        if (int.TryParse(arg0, out var milestone))
        {
            _winstreakManager.Instance.winningInARow.Value = milestone;
            UpdateSlider(true).Forget();

            UpdateAllMilestones();
        }
    }

    public void UpdateAllMilestones()
    {
        foreach (var ui in uIWinStreakMilestones)
        {
            if (ui.targetLevel == _winstreakManager.Instance.winningInARow.Value && !_winstreakManager.Instance.CheckMilestoneClaimed(ui.Milestone))
                ui.UpdateUI(true);
            else
                ui.UpdateUI(false);
        }
    }

    public override void Open(UIData uiData)
    {
        base.Open(uiData);

        // MERGE: listener an toàn, chỉ sync khi không intro/không suppress
        _sliderListener = _ =>
        {
            if (_introRunning || _suppressSync) return;
            SyncScrollToHandleCore();
        };
        slider.onValueChanged.AddListener(_sliderListener);

        if (_winstreakManager.Instance.IsMax())
            finishedObj.SetActive(true);
        else
        {
            timeCounter.SetData(_winstreakManager.Instance.GetTimeToReset(), FinishEvent);
            finishedObj.SetActive(false);
        }

        textLives.text = $"{_winstreakManager.Instance.lives.Value}";

        // MERGE: quyết định intro 1 lần theo cờ trong manager
        _plannedIntro = _winstreakManager.Instance.ShowIntroOnOpen;

        // MERGE: chạy chuỗi sau khi layout ổn định (thay vì Invoke SyncScrollToHandle)
        AfterOpenSequence().Forget();

        Invoke(nameof(UpdateTileText), 0.05f);

        CheckOpenCheat();
    }
    private void CheckOpenCheat()
    {
        cheatBanner.SetActive(PanelManager.Instance.GetPanel<CheatPanel>() != null);

        if (cheatBanner.activeSelf)
        {
            milestoneInput.onEndEdit.RemoveAllListeners();
            milestoneInput.onEndEdit.AddListener(CheatMilestone);
        }
    }
    private async UniTaskVoid AfterOpenSequence()
    {
        // đợi layout ổn định 2 khung hình để tránh sai vị trí ban đầu
        Canvas.ForceUpdateCanvases();
        await UniTask.Yield(PlayerLoopTiming.LastPostLateUpdate);
        Canvas.ForceUpdateCanvases();
        await UniTask.Yield(PlayerLoopTiming.LastPostLateUpdate);

        if (_plannedIntro)
        {
            topBtnObj.SetActive(false);
            await RunIntroScrollSequence(); // TOP -> BOT
            _winstreakManager.Instance.ShowIntroOnOpen = false;
            _plannedIntro = false;

            ShowTut(true, () =>
            {
                topBtnObj.SetActive(true);
            });

            // sync lại theo handle thực tế sau intro
            SyncScrollToHandleCore();
        }
        else
        {
            // không intro: sync một lần
            SyncScrollToHandleCore();
        }
    }

    private void FinishEvent()
    {
        DOVirtual.DelayedCall(0.1f, () =>
        {
            finishedObj.SetActive(true);
        });
    }

    private async UniTask Init()
    {
        _sliderRT = slider.transform as RectTransform;

        _levels = _winstreakManager.Instance.config.rewardDatas
                    .Select(m => m.levelWinRequired)
                    .OrderBy(x => x)
                    .ToList();

        _sliderHeight = _sliderRT.rect.height; // ví dụ 5000
        _uiGap = (_levels.Count > 0) ? (float)_sliderHeight / _levels.Count : 0f;

        // Slider theo số milestone (N đoạn)
        slider.wholeNumbers = false;
        slider.minValue = 0f;
        slider.maxValue = _levels.Count;   // ví dụ 10
        slider.direction = Slider.Direction.BottomToTop;

        await CreateUI();
        // Chờ layout ổn định lần đầu
        Canvas.ForceUpdateCanvases();
        await UniTask.Yield();

        await UpdateSlider();
    }

    // ====== UI Milestones ======
    private async UniTask CreateUI()
    {
        for (int i = 0; i < milestoneContainer.childCount; i++)
            milestoneContainer.GetChild(i).gameObject.SetActive(false);

        for (int i = 0; i < pointStateContainer.childCount; i++)
            pointStateContainer.GetChild(i).gameObject.SetActive(false);

        uIWinStreakMilestones = new List<UIWinStreakMilestone>();

        for (int i = _levels.Count - 1; i >= 0; i--)
        {
            var data = _winstreakManager.Instance.GetMilestoneData(i);

            var posY = _uiGap * (i + 1);

            var item = poolingService.Instance.CreateObject<UIWinStreakMilestone>(milestoneContainer);
            item.SetPos(posY);

            var point = poolingService.Instance.CreateObject<UIMilestoneState>(pointStateContainer);
            point.SetText(data.levelWinRequired);
            point.SetPos(posY);

            item.SetData(i, _levels[i], data, point);

            uIWinStreakMilestones.Add(item);
        }

        await UniTask.Yield();

        CreateSliderLine();
    }

    private void CreateSliderLine()
    {
        for (int i = 0; i < lineContainer.childCount; i++)
            lineContainer.GetChild(i).gameObject.SetActive(false);

        for (int i = 0; i < _levels.Count; i++)
        {
            var max = i == 0 ? _levels[i] : _levels[i] - _levels[i - 1];

            for (int j = 1; j < max; j++)
            {
                var line = poolingService.Instance.CreateObject<RectTransform>(lineContainer);
                var posY = i * _uiGap + (j * _uiGap / max);
                Vector2 pos = line.anchoredPosition;
                pos.y = posY;
                line.anchoredPosition = pos;
            }
        }
    }

    [SerializeField] private ParticleSystem effectComplete;
    private async UniTask UpdateSlider(bool isCheat = false)
    {
        slider.DOKill();

        int fromWin = _winstreakManager.Instance.lastWinningInARow;
        int toWin = _winstreakManager.Instance.winningInARow.Value;

        float fromVal = MapWinsToSliderValue(fromWin);
        float toVal = MapWinsToSliderValue(toWin);

        if (skeletonChar != null)
        {
            if (toWin == 0 && fromWin != toWin)
                skeletonChar.Play(animStateLose.loopAnim, true);
            else
                skeletonChar.Play(animStates[0].loopAnim, true);
        }
        // Đặt slider tại vị trí from, sync ngay
        if (!isCheat)
            SetValueAndSyncImmediate(fromVal);

        if (textMilestone != null) textMilestone.text = fromWin.ToString();

        if (!Mathf.Approximately(fromVal, toVal))
        {
            UpdateAllMilestones();
            await UniTask.WaitForSeconds(0.25f);

            MySonatFramework.audioService.PlaySound(AudioId.WinStreak_Progress_Move_up_Grill_sort);

            await UniTask.WaitForSeconds(0.25f);

            await slider
                .DOValue(toVal, 0.5f)
                .SetDelay(0.2f)
                .SetEase(Ease.OutCubic)
                .OnComplete(() =>
                {
                    if (textMilestone != null) textMilestone.text = toWin.ToString();

                    int level = _winstreakManager.Instance.GetMilestoneData(_winstreakManager.Instance.GetCurrentMilestone() - 1).levelWinRequired;
               
                    //Debug.Log($"anhnt: toWin={toWin} --- GetCurrentMilestone={level}");
                    if (toWin != level || _winstreakManager.Instance.CheckMilestoneClaimed(toWin))
                        return;

                    var state = 0;

                    if (level >= _levels[^1])
                        state = 2;
                    else if (level >= _levels[^3])
                        state = 1;
                    else
                        state = 0;

                    skeletonChar.PlayOnceThenLoop(animStates[state]);

                    MySonatFramework.audioService.PlaySound(AudioId.WinStreak_Progress_Unlock_prize_Grill_sort);

                    effectComplete.Play();
                    
                })
                .AsyncWaitForCompletion();
        }
    }

    public override void Close()
    {
        base.Close();
        slider.onValueChanged.RemoveAllListeners();
    }

    // ====== Wins → Slider (phân đoạn theo milestone) ======
    private float MapWinsToSliderValue(int wins)
    {
        if (_levels == null || _levels.Count == 0) return 0f;

        if (wins <= 0) return 0f;
        if (wins >= _levels[_levels.Count - 1]) return _levels.Count;

        int seg = 0;
        int prevReq = 0;
        int currReq = _levels[0];

        for (int i = 0; i < _levels.Count; i++)
        {
            int lower = (i == 0) ? 0 : _levels[i - 1];
            int upper = _levels[i];
            if (wins <= upper)
            {
                seg = i;
                prevReq = lower;
                currReq = upper;
                break;
            }
        }

        float t = Mathf.InverseLerp(prevReq, currReq, Mathf.Clamp(wins, prevReq, currReq));
        return seg + t; // 0..N (N = số milestone)
    }

    // ===== Intro scroll: TOP(1) -> BOT(0), chống va chạm (MERGED) =====
    private async UniTask RunIntroScrollSequence()
    {
        _introRunning = true;
        _suppressSync = true;

        // Kill tween dính scroll/slider
        slider.DOKill();
        DOTween.Kill(scrollRect, complete: false);
        DOTween.Kill(scrollRect.content, complete: false);

        // Gỡ listener để intro không bị sync kéo ngược
        if (_sliderListener != null)
            slider.onValueChanged.RemoveListener(_sliderListener);

        // Tắt inertia để khỏi drift
        bool oldInertia = scrollRect.inertia;
        scrollRect.inertia = false;

        // Đặt cứng TOP (mốc cuối)
        scrollRect.verticalNormalizedPosition = 1f;

        // Đợi 1s
        await UniTask.WaitForSeconds(1f);

        // Tween TOP -> BOT
        var tcs = new Cysharp.Threading.Tasks.UniTaskCompletionSource();
        _ = DOTween.To(
            () => scrollRect.verticalNormalizedPosition,
            v => scrollRect.verticalNormalizedPosition = v,
            0f,            // BOT (mốc 0)
            2.5f           // thời lượng intro
        )
        .SetEase(Ease.InOutSine)
        .OnComplete(() => tcs.TrySetResult());

        await tcs.Task;

        // Khôi phục
        scrollRect.inertia = oldInertia;
        if (_sliderListener != null)
            slider.onValueChanged.AddListener(_sliderListener);

        _suppressSync = false;
        _introRunning = false;
    }

    /// <summary>
    /// Đồng bộ ScrollRect theo vị trí "thực" của handle trong Content.
    /// - Lấy tâm handle (world) → local content
    /// - Dùng content.rect.yMin để quy về khoảng cách từ đáy (độc lập pivot/anchor)
    /// - Tính verticalNormalizedPosition (1=top, 0=bottom)
    /// </summary>
    private void SyncScrollToHandle()
    {
        // giữ cho tương thích cũ nếu nơi khác gọi hàm này
        if (_introRunning || _suppressSync) return;
        SyncScrollToHandleCore();
    }

    private void SyncScrollToHandleCore()
    {
        if (scrollRect == null || scrollRect.content == null || slider == null || slider.handleRect == null)
            return;

        Canvas.ForceUpdateCanvases();

        var content = scrollRect.content;
        var viewport = (RectTransform)scrollRect.viewport;
        var handleRT = slider.handleRect;

        // Tâm handle: world -> local(content)
        Vector3 handleCenterWorld = handleRT.TransformPoint(handleRT.rect.center);
        Vector2 handleCenterLocal = content.InverseTransformPoint(handleCenterWorld);

        // Khoảng cách từ ĐÁY content (độc lập pivot)
        float yFromBottom = handleCenterLocal.y - content.rect.yMin;

        float contentH = content.rect.height;     // 6000
        float viewportH = viewport.rect.height;   // tuỳ layout
        float scrollable = Mathf.Max(0f, contentH - viewportH);

        // Đặt handle ở vị trí centerBias trong viewport (0=đáy, 0.5=giữa, 1=đỉnh)
        float targetBottom = yFromBottom - viewportH * centerBias01;
        float clampedBottom = Mathf.Clamp(targetBottom, 0f, scrollable);

        // TOP=1, BOTTOM=0
        float targetNormalized = (scrollable <= 0f) ? 1f : (clampedBottom / scrollable);
        scrollRect.verticalNormalizedPosition = targetNormalized;
    }

    // ====== Helpers ======
    private void SetValueAndSyncImmediate(float sliderVal)
    {
        slider.DOKill();
        DOTween.Kill(scrollRect);
        slider.SetValueWithoutNotify(sliderVal);
        Canvas.ForceUpdateCanvases();

        if (!_introRunning && !_suppressSync)
            SyncScrollToHandleCore();
    }

    public void ClickInfo()
    {
        ShowTut(false, null);
    }
    public void ShowTut(bool isAuto, Action action)
    {
        PanelManager.Instance.OpenPanelByName<Panel>("PopupTutWinstreak",
            new UIData().Add(UIDataKey.CallBackOnClose, action));

        MySonatFramework.customTrackingService.OnShowPopup(gameObject.name.ToLogString(), "widget", "non_iap", isAuto ? "auto" : "user");
    }
}
