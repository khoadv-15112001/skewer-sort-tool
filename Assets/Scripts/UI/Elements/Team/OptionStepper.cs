using I2.Loc;
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

public class OptionStepper : MonoBehaviour
{
    public enum SourceMode { Options, Virtual }

    [Header("UI")]
    [SerializeField] private TMP_Text label;
    [SerializeField] private Localize labelLocalize;
    [SerializeField] private HoldableButton leftBtn;
    [SerializeField] private HoldableButton rightBtn;

    [Header("Behavior")]
    [Tooltip("BẬT wrap: khi vượt biên sẽ QUAY VÒNG (0→last, last→0).\nTẮT wrap: dừng ở biên (không vượt qua).")]
    [SerializeField] private bool wrap = true;

    [Header("Events")]
    public UnityEvent<int> onIndexChanged;

    // -------- internal state --------
    [SerializeField] private List<string> options = new();
    private int index;
    private SourceMode mode = SourceMode.Options;

    // Virtual mode
    private int virtualCount = 0;
    private Func<int, string> virtualRenderer;

    // Custom step provider (dir, tickCount, holdElapsedSec) -> deltaIndex
    public delegate int StepSizeProvider(int dir, int tickCount, float holdElapsedSec);
    private StepSizeProvider stepSizeProvider;

    // Hold tracking
    private int _leftTicks, _rightTicks;
    private float _leftPressedAt, _rightPressedAt;

    private void Awake()
    {
        // Hook buttons if present
        if (leftBtn)
        {
            leftBtn.onPressedOnce.AddListener(() => { _leftTicks = 0; _leftPressedAt = Time.unscaledTime; });
            leftBtn.onTick.AddListener(() =>
            {
                _leftTicks++;
                int delta = stepSizeProvider != null
                    ? Mathf.Max(1, stepSizeProvider(-1, _leftTicks, Time.unscaledTime - _leftPressedAt))
                    : 1;
                StepBy(-1, delta);
            });
        }
        if (rightBtn)
        {
            rightBtn.onPressedOnce.AddListener(() => { _rightTicks = 0; _rightPressedAt = Time.unscaledTime; });
            rightBtn.onTick.AddListener(() =>
            {
                _rightTicks++;
                int delta = stepSizeProvider != null
                    ? Mathf.Max(1, stepSizeProvider(+1, _rightTicks, Time.unscaledTime - _rightPressedAt))
                    : 1;
                StepBy(+1, delta);
            });
        }

        Refresh();
    }

    // ---------- Public API ----------
    public void SetOptions(IList<string> opts, int startIndex = 0)
    {
        mode = SourceMode.Options;
        options = new List<string>(opts ?? Array.Empty<string>());
        SetIndex(startIndex);
        Refresh();
    }

    public void SetVirtual(int count, Func<int, string> renderer, int startIndex = 0)
    {
        mode = SourceMode.Virtual;
        virtualCount = Mathf.Max(0, count);
        virtualRenderer = renderer;
        SetIndex(startIndex);
        Refresh();
    }

    public void SetIndex(int i)
    {
        int count = Count;
        if (count <= 0)
        {
            index = 0;
            Refresh();
            onIndexChanged?.Invoke(index);
            return;
        }

        if (wrap) index = ((i % count) + count) % count;
        else index = Mathf.Clamp(i, 0, count - 1);

        Refresh();
        onIndexChanged?.Invoke(index);
    }

    public void SetWrap(bool enable) => wrap = enable;

    /// <summary>
    /// Cung cấp hàm tính bước nhảy theo lần tick và thời gian giữ.
    /// Trả về deltaIndex >= 1. Truyền null để dùng bước mặc định (=1).
    /// </summary>
    public void SetStepSizeProvider(StepSizeProvider provider) => stepSizeProvider = provider;

    public int Index => index;
    public int Count => (mode == SourceMode.Options) ? (options?.Count ?? 0) : virtualCount;

    // ---------- Internal ----------
    private void StepBy(int dir, int delta) => SetIndex(index + dir * delta);

    private void Refresh()
    {
        if (!label) return;

        if (Count <= 0) { label.text = ""; return; }

        if (mode == SourceMode.Options)
        {
            int safe = Mathf.Clamp(index, 0, options.Count - 1);
            SetLabelText(options[safe]);
        }
        else
        {
            int safe = Mathf.Clamp(index, 0, virtualCount - 1);
            SetLabelText(virtualRenderer != null ? virtualRenderer(safe) : safe.ToString());
        }
    }

    private void SetLabelText(string text)
    {
        if (labelLocalize != null)
            labelLocalize.SetTerm(text);
        else
            label.text = text;
    }
}
