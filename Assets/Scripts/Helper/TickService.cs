using System;
using System.Collections;
using UnityEngine;

public class TickService : MonoBehaviour
{
    public enum TickPhase
    {
        Tick,           // affected by Time.timeScale
        TickRealtime,   // unaffected by Time.timeScale
        Update,
        FixedUpdate,
        LateUpdate
    }

    public static event Action OnTick;
    public static event Action OnTickRealtime;
    public static event Action OnUpdate;
    public static event Action OnFixedUpdate;
    public static event Action OnLateUpdate;

    private static TickService instance;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Init()
    {
        if (instance == null)
        {
            GameObject go = new GameObject("[TickService]");
            instance = go.AddComponent<TickService>();
            DontDestroyOnLoad(go);

            instance.Initialize();
        }
    }

    public void Initialize()
    {
        StartCoroutine(TickCoroutine());          // game time
        StartCoroutine(TickRealtimeCoroutine());  // real time
    }

    /// <summary>
    /// Tick theo game time (bị ảnh hưởng bởi Time.timeScale)
    /// </summary>
    private IEnumerator TickCoroutine()
    {
        var wait = new WaitForSeconds(1f);
        while (true)
        {
            yield return wait;
            OnTick?.Invoke();
        }
    }

    /// <summary>
    /// Tick theo real time (không bị ảnh hưởng bởi Time.timeScale)
    /// </summary>
    private IEnumerator TickRealtimeCoroutine()
    {
        var wait = new WaitForSecondsRealtime(1f);
        while (true)
        {
            yield return wait;
            OnTickRealtime?.Invoke();
        }
    }

    /// <summary>
    /// Cho phép gọi StartCoroutine từ bất kỳ đâu.
    /// </summary>
    public static Coroutine StartRoutine(IEnumerator routine)
    {
        if (instance == null)
        {
            Debug.LogError("TickService not init");
            return null;
        }

        return instance.StartCoroutine(routine);
    }

    /// <summary>
    /// Cho phép gọi StopCoroutine từ bất kỳ đâu.
    /// </summary>
    public static void StopRoutine(Coroutine routine)
    {
        if (instance != null && routine != null)
        {
            instance.StopCoroutine(routine);
        }
    }

    public static void Register(Action callback, TickPhase phase)
    {
        if (callback == null) return;

        switch (phase)
        {
            case TickPhase.Tick:
                OnTick -= callback;
                OnTick += callback;
                break;
            case TickPhase.TickRealtime:
                OnTickRealtime -= callback;
                OnTickRealtime += callback;
                break;
            case TickPhase.Update:
                OnUpdate -= callback;
                OnUpdate += callback;
                break;
            case TickPhase.FixedUpdate:
                OnFixedUpdate -= callback;
                OnFixedUpdate += callback;
                break;
            case TickPhase.LateUpdate:
                OnLateUpdate -= callback;
                OnLateUpdate += callback;
                break;
        }
    }

    public static void UnRegister(Action callback, TickPhase phase)
    {
        if (callback == null) return;

        switch (phase)
        {
            case TickPhase.Tick:
                OnTick -= callback;
                break;
            case TickPhase.TickRealtime:
                OnTickRealtime -= callback;
                break;
            case TickPhase.Update:
                OnUpdate -= callback;
                break;
            case TickPhase.FixedUpdate:
                OnFixedUpdate -= callback;
                break;
            case TickPhase.LateUpdate:
                OnLateUpdate -= callback;
                break;
        }
    }

    private void Update()
    {
        OnUpdate?.Invoke();
    }

    private void FixedUpdate()
    {
        OnFixedUpdate?.Invoke();
    }

    private void LateUpdate()
    {
        OnLateUpdate?.Invoke();
    }
}
