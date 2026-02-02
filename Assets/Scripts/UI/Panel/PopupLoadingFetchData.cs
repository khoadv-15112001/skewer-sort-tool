using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using GrillSort.OnlineService;
using SonatFramework.Scripts.UIModule;
using SonatFramework.Systems;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PopupLoadingFetchData : Panel
{
    public static string LOG_TAG = "[PopupLoadingFetchData]";
    [SerializeField] private Slider slider;
    [SerializeField] private float startProgressTime = 1f;
    [SerializeField] private float endProgressTime = 0.5f;

    private Service<HybridDataService> dataSyncService = new();
    private bool isCompleted = false;

    public override void Open(UIData uiData)
    {
        base.Open(uiData);

        isCompleted = false;
        StartFetch().Forget();

        StartProgress().Forget();
    }

    private async UniTask StartFetch()
    {
        Debug.Log($"{LOG_TAG} StartFetch");
        await dataSyncService.Instance.FetchDataFromServer();
        isCompleted = true;
    }

    private async UniTask StartProgress()
    {
        await slider.DOValue(0.5f, startProgressTime);

        var loadingTime = startProgressTime;
        while (!isCompleted || loadingTime + endProgressTime < dataSyncService.Instance.config.delayCompletedLoading)
        {
            await UniTask.Delay(100);
            loadingTime += 0.1f;
            slider.value += 0.002f;
            Debug.Log($"{LOG_TAG} loadingTime: {loadingTime}, slider.value: {slider.value}");
        }
        await slider.DOValue(1, endProgressTime).OnComplete(() =>
        {
            ReloadGame().Forget();
        });
    }

    private async UniTask ReloadGame()
    {
        SceneManager.LoadScene("FetchDataFromServer");
    }
}
