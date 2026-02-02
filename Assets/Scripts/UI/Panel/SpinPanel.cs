using DG.Tweening;
using GrillSort.EndlessTreasure;
using GrillSort.LuckySpin;
using GrillSort.RealTime;
using SonatFramework.Scripts.UIModule;
using SonatFramework.Scripts.UIModule.UIElements;
using SonatFramework.Systems;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SpinPanel : Panel
{
    [SerializeField] private UILuckySpin uILuckySpin;
    [SerializeField] private TMP_Text txtLog;
    [SerializeField] private TMP_Text txtSpin;
    [SerializeField] private Button btnSpin, btnLock;
    [SerializeField] private UITimeCounter timeCounter;
    private readonly Service<RealTimeService> realTimeService = new();
    private readonly Service<LuckySpinService> luckySpinService = new();

    public override void Open(UIData uiData)
    {
        base.Open(uiData);

        CheckCooldown();
        luckySpinService.Instance.onDataChanged += UpdateUI;
    }
    private void UpdateRemaining()
    {
        SetActiveBtn(true);

        var remainingTime = realTimeService.Instance.GetRemainingTimeInDay();
        timeCounter.timeFomat = SonatFramework.Scripts.Utils.TxtTimeFormat.ShortDay_FullTime;
        timeCounter.SetData(remainingTime, null);
    }
    private void SetActiveBtn(bool isActive)
    {
        txtSpin.color = isActive ? Color.white : Color.gray;
        btnSpin.interactable = isActive;
        btnLock.gameObject.SetActive(!isActive);
    }

    public void ClickButtonLock()
    {
        var cooldown = luckySpinService.Instance.GetEndCooldown();
        PopupToast.Cretate("Comeback after", $" {cooldown}");
    }

    private void UpdateCooldown(Action action)
    {
        SetActiveBtn(false);
        var remainingTime = (int)luckySpinService.Instance.GetCooldownTime();
        timeCounter.timeFomat = SonatFramework.Scripts.Utils.TxtTimeFormat.Smart;
        timeCounter.SetData(remainingTime, () =>
        {
            SetActiveBtn(true);
            luckySpinService.Instance.ResetCooldown();
            action?.Invoke();
        });
    }
    private void UpdateUI(LuckySpinData d)
    {
        Debug.Log("SpinPanel UpdateUI");
        CheckCooldown();
    }

    private void CheckCooldown()
    {
        if (luckySpinService.Instance.Cooldown > 0)
        {
            UpdateCooldown(UpdateRemaining);
        }
        else
        {
            UpdateRemaining();
        }
    }

    public override void Close()
    {
        // nếu đang quay mà thoát
        if (uILuckySpin.CheckSpinning())
        {
            ShowLog("The wheel is spinning, please wait...");
            return;
        }

        luckySpinService.Instance.onDataChanged -= UpdateUI;
        base.Close();
    }

    Sequence sequence;
    public void ShowLog(string log)
    {
        sequence.Kill(); sequence = DOTween.Sequence();
        txtLog.DOKill();

        // Reset trạng thái
        txtLog.text = log;
        txtLog.gameObject.SetActive(true);

        var rectTransform = txtLog.GetComponent<RectTransform>();
        rectTransform.anchoredPosition = new Vector2(0, 0);
        txtLog.color = new Color(txtLog.color.r, txtLog.color.g, txtLog.color.b, 1f); // Reset alpha về 1

        // Nhảy lên
        sequence.Append(rectTransform.DOAnchorPosY(0 + 300, 1f)
            .SetEase(Ease.OutQuad));

        // Chờ
        sequence.AppendInterval(1f);

        // Mờ dần và hoàn tất
        sequence.Append(txtLog.DOFade(0f, 0.5f)
            .OnComplete(() => txtLog.gameObject.SetActive(false)));

        // Chạy sequence
        sequence.Play();
    }
}