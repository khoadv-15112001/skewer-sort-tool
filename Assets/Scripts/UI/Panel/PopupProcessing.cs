using Cysharp.Threading.Tasks;
using SonatFramework.Scripts.UIModule;
using System;
using System.Threading;
using UnityEngine;

public class PopupProcessing : Panel
{
    [SerializeField] private float minTime = 0.3f;
    [SerializeField] private float timeout = 5f;

    private CancellationTokenSource cts;

    public override void Open(UIData uiData)
    {
        base.Open(uiData);

        if (uiData != null && uiData.TryGet("condition", out Func<bool> condition))
        {
            cts = new();
            WaitClose(condition, cts.Token).Forget();
        }
        else
        {
            Close();
        }
    }

    private async UniTaskVoid WaitClose(Func<bool> condition, CancellationToken ct)
    {
        try
        {
            if (timeout > 0f)
            {
                var flowTask = WaitMinThenCondition(condition, ct);
                var timeoutTask = UniTask.Delay(TimeSpan.FromSeconds(timeout), cancellationToken: ct);
                var index = await UniTask.WhenAny(flowTask, timeoutTask);
                
                Close();
            }
            else
            {
                await WaitMinThenCondition(condition, ct);
                Close();
            }
        }
        catch (OperationCanceledException)
        {
            
        }
        catch (Exception e)
        {
            Debug.LogError($"PopupProcessing.WaitClose error: {e}");
            Close();
        }
    }

    private async UniTask WaitMinThenCondition(Func<bool> condition, CancellationToken ct)
    {
        if (minTime > 0f)
            await UniTask.WaitForSeconds(minTime, cancellationToken: ct);

        await UniTask.WaitUntil(condition, cancellationToken: ct);
    }


    public override void Close()
    {
        base.Close();

        if (cts != null)
        {
            cts.Cancel();
            cts.Dispose();
            cts = null;
        }
    }
}
