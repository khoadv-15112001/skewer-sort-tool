using Cysharp.Threading.Tasks;
using System;
using System.Threading;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

[RequireComponent(typeof(UnityEngine.UI.Button))]
public class HoldableButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerExitHandler
{
    public enum Mode { HoldRepeat, ClickOnlyOnDown, ClickOnlyOnUp }

    [Header("Mode")]
    [SerializeField] private Mode mode = Mode.HoldRepeat;
    [Tooltip("Only for ClickOnlyOnUp: require release inside the button area")]
    [SerializeField] private bool requireReleaseInside = true;

    [Header("Repeat / Acceleration (used when Mode = HoldRepeat)")]
    [Tooltip("Invoke once immediately when pointer goes down")]
    [SerializeField] private bool invokeImmediately = true;
    [SerializeField] private float firstDelay = 0.35f;
    [SerializeField] private float minDelay = 0.06f;
    [SerializeField] private float accelDuration = 1.25f;
    [SerializeField] private float preRepeatDelay = 0f;

    [Serializable] public class TickEvent : UnityEvent { }
    public TickEvent onTick;        // fired on click or each repeat tick
    public TickEvent onPressedOnce; // fired once at the press (or at release for ClickOnlyOnUp)

    private bool _pressing;
    private CancellationTokenSource _cts;

    public void OnPointerDown(PointerEventData eventData)
    {
        _pressing = true;

        if (mode == Mode.ClickOnlyOnDown)
        {
            onPressedOnce?.Invoke();
            onTick?.Invoke();
            return;
        }

        if (mode == Mode.HoldRepeat)
        {
            onPressedOnce?.Invoke();
            _cts?.Cancel();
            _cts = new CancellationTokenSource();
            _ = HoldLoopAsync(_cts.Token);
        }
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (mode == Mode.ClickOnlyOnUp && _pressing)
        {
            bool inside = !requireReleaseInside ||
                          RectTransformUtility.RectangleContainsScreenPoint(
                              (RectTransform)transform, eventData.position, eventData.pressEventCamera);

            if (inside)
            {
                onPressedOnce?.Invoke();
                onTick?.Invoke();
            }
        }
        StopHolding();
    }

    public void OnPointerExit(PointerEventData eventData) => StopHolding();
    private void OnDisable() => StopHolding();

    private void StopHolding()
    {
        _pressing = false;
        _cts?.Cancel();
        _cts = null;
    }

    private async UniTaskVoid HoldLoopAsync(CancellationToken token)
    {
        try
        {
            if (invokeImmediately)
                onTick?.Invoke();

            if (preRepeatDelay > 0f)
                await UniTask.Delay(TimeSpan.FromSeconds(preRepeatDelay), cancellationToken: token);

            float elapsed = 0f;
            float delay = firstDelay;

            await UniTask.Delay(TimeSpan.FromSeconds(delay), cancellationToken: token);
            while (_pressing && !token.IsCancellationRequested)
            {
                onTick?.Invoke();

                elapsed += delay;
                float t = accelDuration > 0f ? Mathf.Clamp01(elapsed / accelDuration) : 1f;
                delay = Mathf.Lerp(firstDelay, minDelay, t);

                await UniTask.Delay(TimeSpan.FromSeconds(delay), cancellationToken: token);
            }
        }
        catch (OperationCanceledException) { }
        catch (Exception ex)
        {
            Debug.LogError($"HoldableButton error: {ex.Message}");
        }
    }

    // Expose quick setters if you want to flip mode at runtime
    public void SetMode(Mode m) => mode = m;
}