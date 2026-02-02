using Sirenix.OdinInspector;
using System;
using UnityEngine;
using UnityEngine.Events;

public class NumberStepper : MonoBehaviour
{
    [Serializable] public class IntEvent : UnityEvent<int> { }

    [Header("Dependencies")]
    [SerializeField] private OptionStepper stepper;

    [Header("Value")]
    [SerializeField] private int value = 0;

    [Header("Constraints")]
    [SerializeField] private int minValue = 0;
    [SerializeField] private int maxValue = 100;
    [SerializeField, Min(1)] private int step = 1;
    [Tooltip("BẬT: khi đi quá Max sẽ quay về Min (và ngược lại). TẮT: chặn ở biên Min/Max.")]
    [SerializeField] private bool wrap = false;

    [Header("Display")]
    [SerializeField] private string prefix = "";
    [SerializeField] private string suffix = "";
    [SerializeField] private string numericFormat = "";

    [Header("Adaptive Hold Step")]
    [Tooltip("Nếu bật: giữ càng lâu thì bước nhảy sẽ tăng 1→10→100→1000 (nhân vào số 'bước' index).")]
    [SerializeField] private bool useAdaptiveHoldStep = false;

    [Tooltip("Mốc thời gian (giây) để chuyển 1→10")]
    [SerializeField, ShowIf("@this.useAdaptiveHoldStep == true")] private float tTo10 = 0.6f;
    [Tooltip("Mốc thời gian (giây) để chuyển 10→100")]
    [SerializeField, ShowIf("@this.useAdaptiveHoldStep == true")] private float tTo100 = 1.4f;
    [Tooltip("Mốc thời gian (giây) để chuyển 100→1000")]
    [SerializeField, ShowIf("@this.useAdaptiveHoldStep == true")] private float tTo1000 = 2.4f;

    public IntEvent onValueChanged;

    private void Awake()
    {
        if (!stepper) stepper = GetComponent<OptionStepper>();
        if (!stepper)
        {
            Debug.LogError("NumberRowStepper: OptionStepper not found.");
            enabled = false;
            return;
        }

        stepper.SetWrap(wrap);
        RebindVirtual();

        // Bật/tắt adaptive stepping
        stepper.SetStepSizeProvider(useAdaptiveHoldStep ? AdaptiveStepProvider : (OptionStepper.StepSizeProvider)null);

        onValueChanged?.Invoke(value);
    }

    // ------- Mapping index <-> value -------
    private int Count => Mathf.Max(0, 1 + (maxValue - minValue) / Mathf.Max(1, step));
    private int IndexFromValue(int v) => (v - minValue) / Mathf.Max(1, step);
    private int ValueFromIndex(int idx) => minValue + idx * Mathf.Max(1, step);

    private string RenderIndex(int idx)
    {
        int v = ValueFromIndex(idx);
        return FormatValue(v);
    }

    private string FormatValue(int v)
    {
        string core = string.IsNullOrEmpty(numericFormat) ? v.ToString() : v.ToString(numericFormat);
        if (!string.IsNullOrEmpty(prefix)) core = prefix + core;
        if (!string.IsNullOrEmpty(suffix)) core = core + suffix;
        return core;
    }

    // ------- Public API -------
    public int Value
    {
        get => value;
        set
        {
            int v = AlignToStep(value);
            v = Mathf.Clamp(v, minValue, maxValue);

            if (this.value == v) { UpdateLabelOnly(); return; }
            this.value = v;

            int idx = Mathf.Clamp(IndexFromValue(v), 0, Mathf.Max(0, Count - 1));
            stepper.SetIndex(idx);

            onValueChanged?.Invoke(this.value);
        }
    }

    public void Configure(int min, int max, int step, bool wrap)
    {
        this.minValue = min;
        this.maxValue = Mathf.Max(max, min);
        this.step = Mathf.Max(1, step);
        this.wrap = wrap;
        stepper.SetWrap(wrap);
        RebindVirtual();
    }

    public void SetPrefixSuffix(string prefix, string suffix)
    {
        this.prefix = prefix ?? "";
        this.suffix = suffix ?? "";
        UpdateLabelOnly();
    }

    public void SetNumericFormat(string fmt)
    {
        this.numericFormat = fmt ?? "";
        UpdateLabelOnly();
    }

    public void EnableAdaptiveHoldStep(bool enable) =>
        stepper.SetStepSizeProvider(enable ? AdaptiveStepProvider : (OptionStepper.StepSizeProvider)null);

    // ------- Internal -------
    private void RebindVirtual()
    {
        int cnt = Count;
        if (cnt <= 0)
        {
            Debug.LogWarning("NumberRowStepper: invalid range; ensure max >= min and step >= 1.");
            stepper.SetVirtual(0, null, 0);
            return;
        }

        value = Mathf.Clamp(AlignToStep(value), minValue, maxValue);
        int startIdx = Mathf.Clamp(IndexFromValue(value), 0, cnt - 1);

        stepper.SetVirtual(cnt, RenderIndex, startIdx);

        stepper.onIndexChanged.RemoveListener(OnStepperIndexChanged);
        stepper.onIndexChanged.AddListener(OnStepperIndexChanged);
    }

    private void OnStepperIndexChanged(int idx)
    {
        int newVal = Mathf.Clamp(ValueFromIndex(idx), minValue, maxValue);
        if (value == newVal) return;
        value = newVal;
        onValueChanged?.Invoke(value);
        // Label được OptionStepper cập nhật qua RenderIndex
    }

    private void UpdateLabelOnly() => stepper.SetIndex(stepper.Index);

    private int AlignToStep(int v)
    {
        int s = Mathf.Max(1, step);
        int delta = v - minValue;
        return minValue + (delta / s) * s;
    }

    // -------- Adaptive provider: 1 -> 10 -> 100 -> 1000 theo thời gian giữ --------
    private int AdaptiveStepProvider(int dir, int tickCount, float holdElapsedSec)
    {
        if (!useAdaptiveHoldStep) return 1; // phòng khi bị gọi dù đã tắt

        if (holdElapsedSec >= tTo1000) return 1000;
        if (holdElapsedSec >= tTo100) return 100;
        if (holdElapsedSec >= tTo10) return 10;
        return 1;
    }
}
