using Cysharp.Threading.Tasks;
using Manager;
using Sonat.GrillSort.UI;
using SonatFramework.Scripts.UIModule;
using System.Threading;
using UnityEngine;

public class HomeWidgetManager : MonoBehaviour
{
    private BlockPanel blockPanel;
    [SerializeField] private int delayProcess = 750;
    [SerializeField] private UIHomeWidget[] widgets;
    public static int homeCount = -1;
    private CancellationToken cts;
    [SerializeField] WidgetColumnAutoScaler widgetColumnAutoScaler;

    private bool isProcessTaskCompleted;
    private bool isOnFocusTaskCompleted;

    public void Setup()
    {
        isProcessTaskCompleted = false;
        isOnFocusTaskCompleted = false;

        homeCount++;
        foreach (var widget in widgets)
        {
            widget.Setup();
        }
        widgetColumnAutoScaler.RecalculateScale();
        ProcessTasks().Forget();
    }

    public void OnFocus()
    {
        foreach (var widget in widgets)
        {
            widget.OnFocus();
        }

        CheckOnFocusTask().Forget();
    }

    public void OnLoseFocus()
    {
        foreach (var widget in widgets)
        {
            widget.OnLoseFocus();
        }
    }

    public async UniTask ProcessTasks()
    {
        int level = MySonatFramework.userDataService.GetLevel();
        // if(level <= GameRemoteConfigValue.levelForceHome && GameplayController.gameplayCount > 0) return;
        BlockUI();
        await UniTask.Delay(delayProcess);
        int popupCount = 0;
        foreach (var widget in widgets)
        {
            if (!widget.forceOpen && popupCount >= GameRemoteConfigValue.maxPopupMO) continue;
            var open = await widget.ProcessTask();
            if (open)
            {
                popupCount++;
            }
        }

        isProcessTaskCompleted = true;

        await UniTask.Yield();
        await UniTask.WaitUntil(() => isOnFocusTaskCompleted);

        UnlockUI();
    }


    public void BlockUI()
    {
        if (blockPanel != null) return;
        blockPanel = PanelManager.Instance.OpenPanel<BlockPanel>();
    }

    public void UnlockUI()
    {
        if (blockPanel == null) return;
        blockPanel.Close();
        blockPanel = null;
    }

    private void OnDisable()
    {
        UnlockUI();
    }

    private async UniTask CheckOnFocusTask()
    {
        await UniTask.WaitUntil(() => isProcessTaskCompleted);

        foreach (var widget in widgets)
        {
            await widget.ProcessOnFocus();
        }

        isOnFocusTaskCompleted = true;
    }
}
