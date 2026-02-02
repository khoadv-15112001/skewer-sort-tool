using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using Gameplay.LevelData;
using GrillSort.ConsecutiveWin;
using Sonat.Enums;
using Spine.Unity;
using UnityEngine;
using UnityEngine.UI;

public class LoadingScreenInstance : SingletonSimple<LoadingScreenInstance>
{
    private Coroutine coroutine;
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private GameObject panel;
    [SerializeField] private float fadeDuration = 0.25f;
    //private SkeletonGraphic logoAnim;
    [SerializeField] private Slider slider;
    [SerializeField] private Image bgr;
    [SerializeField] private Sprite[] bgrSprites;
    //[SerializeField] private LocalizedEntry<SkeletonGraphic> logo;

    [SerializeField] private GameObject objNotificationConsecutiveWin;

    public bool IsShowing { get; private set; }

    protected override void OnAwake()
    {
        base.OnAwake();
        panel.SetActive(false);
    }

    public void Show(float time = 0, Action callback = null)
    {
        panel.SetActive(true);
        canvasGroup.DOKill();
        canvasGroup.alpha = 0;
        IsShowing = true;

        //SetupBackground();

        slider.value = 0;
        slider.DOValue(1, time);
        canvasGroup.DOFade(1, fadeDuration);
        if (coroutine != null)
        {
            StopCoroutine(coroutine);
            coroutine = null;
        }

        if (time > 0)
        {
            coroutine = StartCoroutine(Delay(time, callback));
        }

        if (objNotificationConsecutiveWin)
        {
            var consecutiveWinService = MySonatFramework.GetService<ConsecutiveWinService>();
            var level = MySonatFramework.userDataService.GetLevel();
            objNotificationConsecutiveWin.SetActive(consecutiveWinService.CheckStart(level));
        }
    }

    private void SetupBackground()
    {
        int level = MySonatFramework.userDataService.GetLevel();
        LevelDifficulty levelDifficulty = MySonatFramework.GetLevelDifficulty(level);
        bgr.sprite = bgrSprites[(int)levelDifficulty];
    }

    public void Hide()
    {
        //logoAnim = logo.GetLocalizedEntry();
        //IsShowing = false;
        //logoAnim.AnimationState.ClearTracks();
        //logoAnim.Initialize(true);
        //logoAnim.AnimationState.SetAnimation(0, "End", false);
        canvasGroup.DOFade(0, fadeDuration).SetDelay(0.43f).OnComplete(() => { panel.SetActive(false); });
        if (coroutine != null)
        {
            StopCoroutine(coroutine);
            coroutine = null;
        }
    }

    IEnumerator Delay(float time, Action callback = null)
    {
        yield return new WaitForSeconds(time);
        callback?.Invoke();
        coroutine = null;
        Hide();
    }
}