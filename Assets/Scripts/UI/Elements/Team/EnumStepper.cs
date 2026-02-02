using System;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

public abstract class EnumStepper<TEnum> : MonoBehaviour where TEnum : struct, Enum
{
    [SerializeField] private OptionStepper stepper;
    [SerializeField] private bool wrap = true;
    [SerializeField] private TEnum defaultValue = default;

    [Serializable] public class EnumEvent : UnityEvent<TEnum> { }
    public EnumEvent onStateChanged;

    private TEnum[] _values;

    protected virtual string Format(TEnum v) => v.ToString();

    private void Awake()
    {
        if (!stepper) stepper = GetComponent<OptionStepper>();
        _values = (TEnum[])Enum.GetValues(typeof(TEnum));
        var names = _values.Select(Format).ToList();

        int start = Array.IndexOf(_values, defaultValue);
        if (start < 0) start = 0;

        // 1) Add listener trước
        stepper.onIndexChanged.RemoveListener(OnIndexChanged);
        stepper.onIndexChanged.AddListener(OnIndexChanged);

        // 2) Đồng bộ wrap của OptionStepper v2
        stepper.SetWrap(wrap);

        // Gọi SetOptions sẽ tự bắn onIndexChanged 1 lần
        stepper.SetOptions(names, start);
    }

    private void OnIndexChanged(int idx)
    {
        if (_values == null || _values.Length == 0) return;
        var val = _values[Mathf.Clamp(idx, 0, _values.Length - 1)];
        onStateChanged?.Invoke(val);
    }

    public TEnum Current => _values != null && _values.Length > 0
        ? _values[Mathf.Clamp(stepper.Index, 0, _values.Length - 1)]
        : default;

    public void Set(TEnum value)
    {
        int idx = Array.IndexOf(_values, value);
        if (idx >= 0) stepper.SetIndex(idx);
    }
}
